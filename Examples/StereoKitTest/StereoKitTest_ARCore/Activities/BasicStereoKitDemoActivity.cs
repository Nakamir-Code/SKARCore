using System;
using System.Reflection;
using System.Threading;

using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Content.Res;

using StereoKit;

using StereoKit_ARCore.Pages;


//NOTE: This MUST be set correctly or you won't be able to load this activity. Doing this adds the activity to the AndroidManifest so Android knows how to find it when you try to open it
[Activity(Label = "BasicStereoKitDemoActivity", MainLauncher = false, Exported = true)]

//NOTE: adding this here causes this to be the entry Activity, and there doesn't seem to be a good reason to have it for Android mobile anyway
//[IntentFilter(new[] { Intent.ActionMain }, Categories = new[] { "org.khronos.openxr.intent.category.IMMERSIVE_HMD", "com.oculus.intent.category.VR", Intent.CategoryLauncher })]
public class BasicStereoKitDemoActivity : Activity, ISurfaceHolderCallback2 //problem with making anything else the callback is that only the Activity already implements most of the interface methods
{
    //NOTE: Theoretically, if you wanted to push/pop pages like a NavigationController, you would make this a stack
    BasicStereoKitDemoPage currentPage;

    //NOTE: This was in the original SK_NetAndroid project as "running". It's basically used to prevent accidentally activating SK multiple times
    public bool skIsRunning = false;

    //NOTE: Called when the Activity loads
    protected override void OnCreate(Bundle savedInstanceState)
    {
        Console.WriteLine($"--------------------BasicStereoKitDemoActivity OnCreate");
        base.OnCreate(savedInstanceState);

        //NOTE: This gets rid of the bar on top that tells you the name of the app
        //this.RequestWindowFeature(WindowFeatures.NoTitle);

        AndroidResourceIDsRuntime.basicStereokitDemoPageResID = Resources.GetIdentifier("basicstereokitdemo_page", "layout", "com.nakamir.skarcore");
        currentPage = new BasicStereoKitDemoPage(this);
        currentPage.OpenPage(AndroidResourceIDsRuntime.basicStereokitDemoPageResID);
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
    public void SurfaceChanged(ISurfaceHolder holder, [GeneratedEnum] Format format, int width, int height) => SK.SetWindow(holder.Surface.Handle);
    public void SurfaceCreated(ISurfaceHolder holder) => SK.SetWindow(holder.Surface.Handle);
    public void SurfaceDestroyed(ISurfaceHolder holder) => SK.SetWindow(IntPtr.Zero);
    public void SurfaceRedrawNeeded(ISurfaceHolder holder) { }
}