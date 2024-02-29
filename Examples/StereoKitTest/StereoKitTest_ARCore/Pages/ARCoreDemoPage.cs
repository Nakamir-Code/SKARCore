using System;
using System.Threading;

using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;

using StereoKit;

using Google.AR.Core;

namespace StereoKit_ARCore.Pages
{
    public class ARCoreDemoPage : StereoKitPage
    {
        //NOTE: For more options as to available Android UI View classes, start writing XML in layout xml and it should suggest other Android UI, otherwise use Android resources online
        SurfaceView skSurfaceView;
        TextView debugTextView;
        Button goBackButton, button1, button2, nextActivityButton;

        //NOTE: ARCore-related stuff
        Session arCoreSession;
        //NOTE: The ARCore texture's OpenGL ID, which ARCore needs to load its data into
        int arCoreTexID = -1;

        //NOTE: The original pose of the Renderer.CameraRoot, so the ARCore pose will be relative to where the CameraRoot was placed in the 3D scene
        StereoKit.Matrix originalCamRoot;
        DemoARCore currentARCoreDemo;

        //NOTE: this is relative to the SurfaceView, we save it here to know where to draw the yellow sphere
        Vec2 touchedScreenPos;
        StereoKit.Color centerSphereColor, centerSphereColor2, touchSphereColor, touchSphereColor2;
        float centerSphereScale = 0.1f, touchSphereScale = 0.1f;

        //NOTE: This is the constructor. You can technically just load everything here, but for demo purposes and general neatness, this separation is a better option than dumping all of the code below into the constructor
        public ARCoreDemoPage(Activity androidActivity) : base(androidActivity)
        {
            centerSphereColor = new StereoKit.Color(1, 0, 0); //red
            centerSphereColor2 = new StereoKit.Color(1, 0, 1); //magenta
            touchSphereColor = new StereoKit.Color(1, 1, 0); //yellow
            touchSphereColor2 = new StereoKit.Color(0, 1, 1); //cyan
        }

        public override void OpenPage(int androidResourceID)
        {
            base.OpenPage(androidResourceID);

            //NOTE: Creates pointers for the UI elements and other page initialization 
            InitializePage();

            //NOTE: Starts SK. There can only be 1 SK instance in the entire app as SK is effectively a static class
            ((ARCoreDemoActivity)activity).RunSK(); //should be in intro page

            //NOTE: Waits until the demo opens, then sets everything. Has a failsafe timeout in case SK doesn't initialize for some reason. Needs to be on a separate thread because if you freeze it on the main thread, the UI will not be able to properly initialize, so SK won't initialize, which will cause this to fail by default. There must be some better way to do this, like have your Demo set up to alert this class of its existence
            new Thread(() =>
            {
                int failsafeTimeout = 30000; //30 seconds
                int currentTimer = 0;
                int timeoutIncrement = 100;
                while (currentTimer < failsafeTimeout)
                {
                    if (Tests.GetActiveScene() != null)
                    {

                        //TODO: do something else if it's not an arcore demo (failsafe)
                        currentARCoreDemo = (DemoARCore)Tests.GetActiveScene();
                        InitializeARCore();

                        break;
                    }
                    Thread.Sleep(timeoutIncrement);
                    currentTimer += timeoutIncrement;
                }

                if (currentARCoreDemo == null)
                {
                    Console.WriteLine("---------------------------SK did not initialize properly!");
                    activity.Finish();
                }
            }).Start();
        }

        //NOTE: This function initializes local variables pointing to various UI elements
        public override void InitializePage()
        {
            //NOTE: Finds the SurfaceView that SK will render to
            skSurfaceView = (SurfaceView)activity.FindViewById(Android.Resource.Id.Content).FindViewWithTag("@+string/surfaceview");

            //NOTE: sizes it 16:9 and fits screen. Needs to be 16:9 bc that's ARCore's frame resolution
            skSurfaceView.LayoutParameters = new RelativeLayout.LayoutParams(activity.WindowManager.DefaultDisplay.Width, (int)(activity.WindowManager.DefaultDisplay.Width * (16.0f / 9.0f)));

            SetSKWindowSize(skSurfaceView.LayoutParameters.Width, skSurfaceView.LayoutParameters.Height);

            //NOTE: Necessary for SK now that we're not letting SK absorb the screen
            skSurfaceView.Holder.AddCallback((ARCoreDemoActivity)activity);

            //NOTE: Callback for touching this surface
            skSurfaceView.Touch += TouchedSKWindow;

            debugTextView = (TextView)activity.FindViewById(Android.Resource.Id.Content).FindViewWithTag("@+string/debugtext");

            goBackButton = (Button)activity.FindViewById(Android.Resource.Id.Content).FindViewWithTag("@+string/gobackbutton");
            button1 = (Button)activity.FindViewById(Android.Resource.Id.Content).FindViewWithTag("@+string/button1");
            button2 = (Button)activity.FindViewById(Android.Resource.Id.Content).FindViewWithTag("@+string/button2");
            nextActivityButton = (Button)activity.FindViewById(Android.Resource.Id.Content).FindViewWithTag("@+string/nextactivitybutton");

            goBackButton.Click += OnClickGoBackButton;
            button1.Click += OnClickButton1;
            button2.Click += OnClickButton2;
            nextActivityButton.Click += OnClickNextActivityButton;
        }

        //NOTE: The DemoARCore calls this every frame. The better way to do this is with an SK IStepper, but idk SK well enough yet, and it can also conflict with other Android app design choices (e.g. the Activity then won't have any real control over when and how this Page gets instantiated and run
        public override void UpdatePage()
        {
            //NOTE: The Activity will update the AR background texture and the camera pose. That was done in the Activity because for design reasons, it made more sense to keep Page-specific stuff inside the Page
            ((ARCoreDemoActivity)activity).UpdateARCoreTextureAndPose(ref arCoreSession, arCoreTexID, originalCamRoot);

            //NOTE: Red sphere in the middle. Can use this if you want to raycast to the center of the screen like with HoloLens 1
            Mesh.Sphere.Draw(Material.Default, StereoKit.Matrix.TS(Input.Head.position + Input.Head.Forward, centerSphereScale), centerSphereColor);

            float percentWidth = touchedScreenPos.x / SKWindowWidth;
            float percentHeight = touchedScreenPos.y / SKWindowHeight;

            float screenPosFOVW = ARState.arFOV * percentWidth / 2.0f;
            float screenPosFOVH = ARState.arFOV * percentHeight; //NOTE: idk why this is the case (no division by 2), I think it might even be wrong and only looks right bc the screen height is roughly 2*width. good enough for the demo tho. also, testing it with a strange screen resolution also worked so who knows

            Vec3 touchedVector = (StereoKit.Matrix.R(new Vec3(screenPosFOVH, screenPosFOVW, 0)) * StereoKit.Matrix.R(Renderer.CameraRoot.Rotation)).TransformNormal(Vec3.Forward).Normalized; //there's probably some simpler way to do this but this works well enough

            //NOTE: Yellow sphere. used to allow user to interact with things in the 3D scene by touching them.
            float distanceFromCamera = 1.0f; //1 meter
            Mesh.Sphere.Draw(Material.Default, StereoKit.Matrix.TS(Input.Head.position + (touchedVector * distanceFromCamera), touchSphereScale), touchSphereColor);
        }

        public void InitializeARCore()
        {
            //NOTE: This setup requires calling GL directly, which requires being on the GL thread (same thread that SK renders with, thus ExecuteOnMain)
            SK.ExecuteOnMain(() =>
            {
                ((ARCoreDemoActivity)activity).InitializeARCore(out arCoreSession, ref arCoreTexID);

                StereoKit.Shader arCoreShader;
                ((ARCoreDemoActivity)activity).SetUpARDisplay(ref arCoreSession, out arCoreShader);
                Material nickUnlitMat = new Material(arCoreShader);
                currentARCoreDemo.arCoreMaterial = nickUnlitMat;

                currentARCoreDemo.UpdatePageMethod = UpdatePage;

                originalCamRoot = Renderer.CameraRoot;

            });
        }

        //NOTE: Shut down anything with arcore and video recording, rest handled by garbage collector
        public override void DestroyPage()
        {
            //NOTE: ARCore will keep running if not turned off, even when you switch Activity
            if (arCoreSession!=null)
            {
                arCoreSession.Pause();
                arCoreSession.Close();
            }
        }

        //NOTE: Touch event for the surface itself
        private void TouchedSKWindow(object sender, View.TouchEventArgs touchEventArgs)
        {
            touchedScreenPos = new Vec2((SKWindowWidth / 2.0f) - touchEventArgs.Event.GetX(), (SKWindowHeight / 2.0f) - touchEventArgs.Event.GetY());
        }

        private void OnClickGoBackButton(object sender, EventArgs e)
        {
            Console.WriteLine("-----------OnGoBackButtonClicked clicked");
            SK.Quit();

            //NOTE: ARCore will keep running if not turned off, even when you switch Activity
            if (arCoreSession != null) {
                arCoreSession.Pause();
                arCoreSession.Close();
            }

            //NOTE: Kills this activity and either goes back to previous Activity or kills app
            activity.Finish();
        }

        private void OnClickButton1(object sender, EventArgs e)
        {
            //NOTE: this doesn't seem to actually open
            Toast.MakeText(activity, "Button 1", ToastLength.Short).Show();
            debugTextView.Text = "Clicked button 1!";
            debugTextView.SetTextColor(Android.Graphics.Color.Chocolate);
            centerSphereScale += 0.1f;
            centerSphereColor = centerSphereColor2;
        }

        //NOTE: The 2 click buttons both working as intended proves that higher-level SK stuff doesn't need to be on the Main thread like GL stuff does
        private void OnClickButton2(object sender, EventArgs e)
        {
            SK.ExecuteOnMain(() =>
            {
                debugTextView.Text = "Clicked button 2!";
                debugTextView.SetTextColor(Android.Graphics.Color.Coral);
                //Toast.MakeText(activity, "Button 2", ToastLength.Short).Show(); //throws an error bc SK is not the UI thread
                touchSphereScale += 0.1f;
                touchSphereColor = touchSphereColor2;
            });
        }

        private void OnClickNextActivityButton(object sender, EventArgs e)
        {
            //NOTE: Leaving an Activity doesn't seem to stop ARCore from running, so you need to close it explicitly
            arCoreSession.Pause();
            arCoreSession.Close();
            SK.Quit();

            //NOTE: Clears the interface. Probably isn't necessary but I reload the page later and it doesn't seem to SetContentView properly if the same XML file is already loaded
            activity.SetContentView(new View(activity));

            //NOTE: not doing this prevent SK from reinitializing bc of Nick Klingensmith's failsafe in the Activity
            ((ARCoreDemoActivity)activity).skIsRunning = false;

            Intent i = new Intent(Android.App.Application.Context, typeof(SwitchActivitiesDemoActivity));
            i.AddFlags(ActivityFlags.NewTask);
            activity.StartActivityForResult(i, 100);
        }
    }
}
