using System;
using System.Reflection;
using System.Threading;
using System.IO;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Content.Res;
using Android.Opengl;
using Java.Nio;

using StereoKit;

using Google.AR.Core;
using Google.AR.Core.Exceptions;

using StereoKit_ARCore.Pages;

public struct ARState
{
    //NOTE: Found this experimentally
    public static StereoKit.Matrix arCoreRotationCorrection = StereoKit.Matrix.R(new Vec3(0, 0, 90.0f));

    //NOTE: This will be consistent throughout the entire app bc it's specific to the camera. The SK virtual camera must be set to have the same params as this one, otherwise the rendering won't look right (it might even look like AR tracking instability)
    public static float arFOV;
}

//NOTE: This MUST be set correctly or you won't be able to load this activity. Doing this adds the activity to the AndroidManifest so Android knows how to find it when you try to open it
[Activity(Label = "ARCoreDemoActivity", MainLauncher = false, Exported = true)]

//NOTE: adding this here causes this to be the entry Activity, and there doesn't seem to be a good reason to have it for Android mobile anyway
//[IntentFilter(new[] { Intent.ActionMain }, Categories = new[] { "org.khronos.openxr.intent.category.IMMERSIVE_HMD", "com.oculus.intent.category.VR", Intent.CategoryLauncher })]
public class ARCoreDemoActivity : Activity, ISurfaceHolderCallback2 //NOTE: The problem with making anything else (like the Page) the callback is that only the Activity already implements most of the interface methods
{
    //NOTE: Theoretically, if you wanted to push/pop pages like a NavigationController, you would make this a stack
    ARCoreDemoPage currentPage;

    //NOTE: This was in the original SK_NetAndroid project as "running". It's basically used to prevent accidentally activating SK multiple times
    public bool skIsRunning = false;

    //NOTE: Called when the Activity loads
    protected override void OnCreate(Bundle savedInstanceState)
    {
        Console.WriteLine($"--------------------ARCoreDemoActivity OnCreate");
        base.OnCreate(savedInstanceState);

        //NOTE: This gets rid of the bar on top that tells you the name of the app
        this.RequestWindowFeature(WindowFeatures.NoTitle);

        AndroidResourceIDsRuntime.arcoreDemoPageResID = Resources.GetIdentifier("arcoredemo_page", "layout", "com.nakamir.skarcore");
        currentPage = new ARCoreDemoPage(this);
        currentPage.OpenPage(AndroidResourceIDsRuntime.arcoreDemoPageResID);
    }


    //NOTE: If any Demo wants ARCore, it should call this. This must be executed on main thread (because only calls to the main thread GL instance will actually share context with the SK scene), should check if this is the case (e.g. checking the thread id that SK is currently on where it says new Thread(InvokeStereoKit)
    //NOTE: In terms of software design, the reason I put the AR stuff in the Activity instead of the Page is because ARCore and its updating pattern is more associated with SK than the Page. I think the Page class should contain functionality specific to how the page works. Theoretically, you could make the ARCore-related functions static because there can only be 1 Session running at once, but then that leads to the slippery slope of trying to make everything ARCore-related static (i.e. why do we need to have an ARSession object at all? why not just rewrite the ARCore code). So these software engineering decisions are left to the reader
    public void InitializeARCore(out Session arCoreSession, ref int arCoreTexID)
    {
        //NOTE: I created textures with GL directly bc trying to do this through an SK.Tex didn't work, even with heavy modifications to the internal sk_gpu code. It seems like the SK.Tex's are just too managed for this to work correctly, and SK.Tex probably also binds to GlTexture2D during a rendering pass but it MUST be bound to OES--there are fundamental formatting differences that GL will complain about. Ultimately, it was much easier to simply generate the textures manually like in other ARCore demos
        var textures = new int[1];

        //NOTE: GL must generate the IDs for it to know which one is which. If you print this out, you'll see a high texID because SK has already generated many textures on this main thread already
        GLES32.GlGenTextures(1, textures, 0);
        arCoreTexID = textures[0];

        //NOTE: This makes any future texture-related calls to GLES11Ext.GlTextureExternalOes be on the arCoreTexID. You might wonder why you can't just do things on the arCoreTexID itself. It's because GL was originally structured like a low-level programming language like x86 where operations are on registers, then every engine call is static and sequential. Only in newer GL versions can you call functions directly on the tex ids.
        GLES32.GlBindTexture(GLES11Ext.GlTextureExternalOes, arCoreTexID);

        //NOTE: These are probably not necessary but they are part of basic texture setup--deciding what happens when the texture is not the same size as whatever it's being rendered to (what happens when it crosses 0-1 on UV map, and how does it interpolate
        GLES32.GlTexParameteri(GLES11Ext.GlTextureExternalOes, GLES32.GlTextureWrapS, GLES32.GlClampToEdge);
        GLES32.GlTexParameteri(GLES11Ext.GlTextureExternalOes, GLES32.GlTextureWrapT, GLES32.GlClampToEdge);
        GLES32.GlTexParameteri(GLES11Ext.GlTextureExternalOes, GLES32.GlTextureMinFilter, GLES32.GlNearest);
        GLES32.GlTexParameteri(GLES11Ext.GlTextureExternalOes, GLES32.GlTextureMagFilter, GLES32.GlNearest);


        //NOTE: Taken from the Google HelloAR example
        Java.Lang.Exception exception = null;
        string message = null;
        arCoreSession = null;

        try
        {
            arCoreSession = new Session(this);
        }
        catch (UnavailableArcoreNotInstalledException e)
        {
            message = "Please install ARCore";
            exception = e;
        }
        catch (UnavailableApkTooOldException e)
        {
            message = "Please update ARCore (Google Play Services for AR--search on Google). The app requires at least 1.29.";
            exception = e;
        }
        catch (UnavailableSdkTooOldException e)
        {
            message = "Please update this app";
            exception = e;
        }
        catch (Java.Lang.Exception e)
        {
            exception = e;
            message = "This device does not support AR";
        }

        if (message != null)
        {
            Console.WriteLine($"-------------------------ERROR IN INITIALIZING ARCORE: {message}");
            //Toast.MakeText(this, message, ToastLength.Long).Show();
            return;
        }

        // Create default config, check is supported, create session from that config.
        var config = new Google.AR.Core.Config(arCoreSession);

        //NOTE: You can add features for world tracking, depth, etc. 
        if (!arCoreSession.IsSupported(config))
        {
            //Toast.MakeText(this, "This device does not support AR", ToastLength.Long).Show();

            Console.WriteLine($"-------------------------ERROR IN INITIALIZING ARCORE: {message}");

            //NOTE: This kills this Activity and either goes back to the previous one, or if there is none, then it kills the app
            this.Finish();
            return;
        }

        arCoreSession.Configure(config);

        //NOTE: ARCore fills the arCoreTexID, but this doesn't update the OES buffer, so you need to bind the OES buffer again to update its contents with updated texture info
        arCoreSession.SetCameraTextureName(arCoreTexID);

        //NOTE: Doesn't seem to matter what resolution ARCore thinks the screen is, the output image is still the size of the viewport (or 640x480 if you copy original to cpu). the only reason I even do this is to stop it from complaining in the console, but not setting this doesn't actually break anything...
        //NOTE: Rotation doesn't seem to be doing anything?
        arCoreSession.SetDisplayGeometry(0, 480, 640); 

        //NOTE: In the Xamarin HelloAR example, they handled this in OnResume, which might be better design because it helps AR survive switching app and Activites. Unfortunately, since SK breaks when you Quit and restart anyway, it doesn't matter here
        arCoreSession.Resume();
    }

    //NOTE: Sets up AR Background shader and any other info needed to render properly
    public void SetUpARDisplay(ref Session arCoreSession, out StereoKit.Shader arCoreShader)
    {
        //NOTE: This is basically a dummy shader and its only value is being named sk/ar_background internally. I later end up replacing the GLSL code when this gets to sk_gpu.h, which is not great, but no form of "FromGLSL" ended up working thus far
        byte[] shaderBytesARCore = Platform.ReadFileBytes("ARBackground.hlsl.sks");
        arCoreShader = StereoKit.Shader.FromMemory(shaderBytesARCore);
        
        //NOTE: Need to start camera device somehow to get its intrinsics
        Frame arCoreFrame = arCoreSession.Update();
        Google.AR.Core.Camera arCoreCamera = arCoreFrame.Camera;

        //https://stackoverflow.com/questions/52828668/how-to-calculate-field-of-view-in-arcore
        var imageIntrinsics = arCoreCamera.ImageIntrinsics;

        var focalLength = imageIntrinsics.GetFocalLength()[0];
        var size = imageIntrinsics.GetImageDimensions();
        var w = size[0];
        var h = size[1];

        float fovW = (float)(2 * Math.Atan(w / (focalLength * 2.0f))) * UtilFunctions.Rad2Deg;
        float fovH = (float)(2 * Math.Atan(h / (focalLength * 2.0f))) * UtilFunctions.Rad2Deg;
        float fovD = MathF.Sqrt(MathF.Pow((float)fovW, 2) + MathF.Pow((float)fovH, 2));

        //NOTE: fovW appears to be the right one based on testing. might be different if you need to support multiple orientations. 
        ARState.arFOV = fovW;

        //NOTE: The SK camera must have the same FOV as the device camera for rendering to look right
        Renderer.SetFOV(ARState.arFOV); 
    }


    public void UpdateARCoreTextureAndPose(ref Session arCoreSession, int arCoreTexID, StereoKit.Matrix originalCamRoot)
    {

        if (arCoreSession == null) return;

        Frame arCoreFrame;

        //NOTE: You need to handle this because it will crash the app otherwise when you're switching Activities (unless you're really good at disconnecting everything ARCore-related before this point--it's much easier to just do this than to handle specific edge cases where this function happens to get called after the ARCore session is paused)
        try
        {
            arCoreFrame = arCoreSession.Update();
        }
        catch
        {
            Console.WriteLine($"---------------error in UpdateARCoreTextureAndPose: ar session is probably dead");
            arCoreSession = null;
            return;
        }

        Google.AR.Core.Camera arCoreCamera = arCoreFrame.Camera;

        //NOTE: This is needed every frame or else the texture doesn't update (OES buffer has old data)
        GLES32.GlBindTexture(GLES11Ext.GlTextureExternalOes, arCoreTexID);

        //NOTE: IDK why it's this specific axis configuration, found by trying every combination. Theoretically, ARCore SHOULD use the same coordinate system as SK but for whatever reason, it doesn't
        StereoKit.Matrix rotationMatrix = StereoKit.Matrix.R(new Quat(arCoreCamera.Pose.Qy(), -arCoreCamera.Pose.Qx(), arCoreCamera.Pose.Qz(), arCoreCamera.Pose.Qw()));
        StereoKit.Matrix modifiedRotationMatrix = rotationMatrix * ARState.arCoreRotationCorrection;
        StereoKit.Matrix newPose = modifiedRotationMatrix * StereoKit.Matrix.T(arCoreCamera.Pose.Tx(), arCoreCamera.Pose.Ty(), arCoreCamera.Pose.Tz());

        //NOTE: Preserves any initial camera root pose (e.g. if your scene is set up with the camera having some offset from the origin)
        Renderer.CameraRoot = newPose * originalCamRoot; 

        //NOTE: ARCore may eventually throw an error if you don't do this bc of memory allocation
        arCoreCamera.Dispose();
    }

    protected override void OnDestroy()
    {
        // Quit, but not if Destroy is just a rotation or resize
        if (IsChangingConfigurations == false)
            SK.Quit();

        currentPage.DestroyPage();

        base.OnDestroy();
    }

    public void RunSK()
    {
        //NOTE: This failsafe helps avoid restarting SK on accident but it also means that we need to set running to false before exiting if we want SK to work again on return
        if (skIsRunning) return;
        skIsRunning = true;

        // Before anything else, give StereoKit the Activity. This should
        // be set before any other SK calls, otherwise native library
        // loading may fail.
        SK.AndroidActivity = this;

        // Task.Run will eat exceptions, but Thread.Start doesn't seem to.
        new Thread(InvokeStereoKit).Start();
    }

    static void InvokeStereoKit()
    {
        Type entryClass = typeof(Program);
        MethodInfo entryPoint = entryClass?.GetMethod("Main", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

        // There are a number of potential method signatures for Main, so
        // we need to check each one, and give it the correct values.
        //
        // Converting MethodInfo into an Action instead of calling Invoke on
        // it allows for exceptions to properly bubble up to the IDE.
        ParameterInfo[] entryParams = entryPoint?.GetParameters();
        if (entryParams == null || entryParams.Length == 0)
        {
            Action Program_Main = (Action)Delegate.CreateDelegate(typeof(Action), entryPoint);
            Program_Main();
        }
        else if (entryParams?.Length == 1 && entryParams[0].ParameterType == typeof(string[]))
        {
            Action<string[]> Program_Main = (Action<string[]>)Delegate.CreateDelegate(typeof(Action<string[]>), entryPoint);
            Program_Main(new string[] { });
        }
        else throw new Exception("Couldn't invoke Program.Main!");


        //NOTE: the SK_NetAndroid example code had this before. This is for killing the main process instead of this specific thread , bc it probably shouldn't need to kill this thread explicitly anyway. threads die by themselves, and I verified that it died. android is returning the PID for the entire app, which can have multiple threads. Nick Klingensmith probably intended for the app to die when SK did bc previously it was a Quest app whose only element was the SK instance. but for app purposes, this will kill everything
        //Process.KillProcess(Process.MyPid());
    }

    // Events related to surface state changes
    public void SurfaceChanged(ISurfaceHolder holder, [GeneratedEnum] Format format, int width, int height)
    {
        SK.SetWindow(holder.Surface.Handle);
        Console.WriteLine($"----------------width and height {width} {height}");
    }

    public void SurfaceCreated(ISurfaceHolder holder) => SK.SetWindow(holder.Surface.Handle);
    public void SurfaceDestroyed(ISurfaceHolder holder) => SK.SetWindow(IntPtr.Zero);
    public void SurfaceRedrawNeeded(ISurfaceHolder holder) { }

    //NOTE: This function will get called with specific Android functions, e.g. pushing a new activity (StartActivityForResult), allowing you to do various things like wrap up internet features, close streams, etc. before the Activity changes. You can use different requestCodes for this to tell what is actually happening
    protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
    {
        base.OnActivityResult(requestCode, resultCode, data);
        Console.WriteLine($"--------------------ARCoreDemoActivity OnActivityResult {requestCode} {resultCode}");
    }


    //NOTE: These are various lifecycle functions that you can use for different things, in my case, I just need them to know when we're returning TO SK from a different page (which doesn't work correctly rn simply bc of GL stuff not quite being cleaned up correctly internally)
    protected override void OnResume() => base.OnResume();

    protected override void OnPause() => base.OnPause();

    protected override void OnStop() => base.OnStop();

    protected override void OnRestart()
    {
        base.OnRestart();

        //NOTE: This is only called when a child activity finished and returns to this one, so here we can safely start SK again (in my case, I'm being lazy and just replacing the page)
        Console.WriteLine($"--------------------ARCoreDemoActivity OnRestart");

        currentPage = new ARCoreDemoPage(this);
        currentPage.OpenPage(AndroidResourceIDsRuntime.arcoreDemoPageResID);
    }
}

//NOTE: This is one option for Android-specific utility functions. You can't really put them anywhere in SK because it will mess up the SK dependencies (SK cannot be dependent on Android--there is too much multiplatform code. Much easier to simply handle everything Android-related in these Android-specific classes)
public static class UtilFunctions
{

    //NOTE: This is mostly for if you want to take the arcore image (e.g. from frame.AcquireImage) and write to file. doesn't get used in this demo but might be helpful for computer vision applications. I did verify that this works
    //NOTE: Required you to call AcquireImage on the ARCore frame. Android camera images are YUV format thus this conversion is necessary
    //https://learn.microsoft.com/en-us/answers/questions/1196366/converting-from-android-media-image-to-android-gra
    public static Bitmap ToBitmap(this Android.Media.Image img)
    {
        ByteBuffer ybuffer = img.GetPlanes()[0].Buffer;
        ByteBuffer vubuffer = img.GetPlanes()[2].Buffer;
        ybuffer.Position(0);
        vubuffer.Position(0);
        int ysize = ybuffer.Remaining();
        int vusize = vubuffer.Remaining();
        byte[] nv21 = new byte[ysize + vusize];
        ybuffer.Get(nv21, 0, ysize);
        vubuffer.Get(nv21, ysize, vusize);
        YuvImage yuvimage = new YuvImage(nv21, ImageFormatType.Nv21, img.Width, img.Height, null);
        MemoryStream outstream = new MemoryStream();
        yuvimage.CompressToJpeg(new Android.Graphics.Rect(0, 0, yuvimage.Width, yuvimage.Height), 50, outstream);
        byte[] imagebytes = outstream.ToArray();
        return BitmapFactory.DecodeByteArray(imagebytes, 0, imagebytes.Length, null);
    }

    public static float Rad2Deg = 180.0f / MathF.PI;

}