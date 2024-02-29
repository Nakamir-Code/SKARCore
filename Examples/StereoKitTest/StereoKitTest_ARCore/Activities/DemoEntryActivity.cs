using System;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Content.Res;

using StereoKit_ARCore.Pages;

//NOTE: Any Android resources should be static bc they are consistent throughout the app, we just don't know them beforehand. This is consistent with other Android APIs like GLES, although it does this as a class instead of struct (probably bc it has a lot of functions in it and this doesn't)
public struct AndroidResourceIDsRuntime
{
    public static int demoEntryPageResID;
    public static int androidDemoPageResID;
    public static int basicStereokitDemoPageResID;
    public static int interactiveStereokitDemoPageResID;
    public static int arcoreDemoPageResID;
    public static int switchActivitiesPageResID;
}

//NOTE: this metadata is very important for the compiler to properly populate the AndroidManifest
[Activity(Label = "@string/app_name", MainLauncher = true, Exported = true)]
[IntentFilter(new[] { Intent.ActionMain }, Categories = new[] { "org.khronos.openxr.intent.category.IMMERSIVE_HMD", "com.oculus.intent.category.VR", Intent.CategoryLauncher })]
public class DemoEntryActivity : Activity
{
    //NOTE: Storing page logic and members inside Page classes lets you work in a way closer to MAUI/Xamarin instead of trying to do everything in the activity. There's nothing technically wrong with doing that, but this way seems a bit closer to how APIs like MAUI and Xamarin prefer to do things, and will probably end up more organized, especially if you prefer switching Views to switching Activities, or otherwise need to call SetContentView multiple times in the same Activity.
    DemoEntryPage page;
    
    //NOTE: Entry point for Activities
    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(null);

        //NOTE: Asks for camera permission for the ARCore demo. This should be done better, e.g. waiting on the camera permission before initializing SK.
        if (!(CheckSelfPermission(Android.Manifest.Permission.Camera) == Android.Content.PM.Permission.Granted))
        {
            RequestPermissions(new string[] { Android.Manifest.Permission.Camera }, 0);
        }

        //NOTE: This gets rid of the bar on top that tells you the name of the app
        this.RequestWindowFeature(WindowFeatures.NoTitle);

        //NOTE: This is the standard way of finding IDs during runtime. There is no practical way to find them beforehand.
        //NOTE: the parameters are: name of file without extension (i.e. notice that .xml is missing from here, folder (Android has predetermine folder names, you can't just arbitrarily create them. Most assets will end up in Resources/raw), and bundle ID 
        //NOTE: Note that you're note supposed to give it the file extension 
        AndroidResourceIDsRuntime.demoEntryPageResID = Resources.GetIdentifier("demoentry_page", "layout", "com.nakamir.skarcore");

        //NOTE: The page itself handles finding its Views and other initialization so it doesn't look as messy in the Activity.
        page = new DemoEntryPage(this);

        //NOTE: Basically a wrapper for SetContentView (how you load the XML file for the page) + initialization
        page.OpenPage(AndroidResourceIDsRuntime.demoEntryPageResID);
    }

    //NOTE: This function will get called with specific Android functions, e.g. pushing a new activity (StartActivityForResult), allowing you to do various things like wrap up internet features, close streams, etc. before the Activity changes. You can use different requestCodes for this to tell what is actually happening
    protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
    {
        base.OnActivityResult(requestCode, resultCode, data);
        Console.WriteLine($"--------------------MainActivity OnActivityResult {requestCode} {resultCode}");

        if (requestCode == 100) 
        {
            Console.WriteLine($"--------------------MainActivity OnActivityResult requestCode = {requestCode} which indicates that you transitioned from the Main page to the Android demo.");
        } 
        else if (requestCode == 200)
        {
            Console.WriteLine($"--------------------MainActivity OnActivityResult requestCode = {requestCode} which indicates that you transitioned from the Main page to the Basic StereoKit demo.");
        }
        else if (requestCode == 300)
        {
            Console.WriteLine($"--------------------MainActivity OnActivityResult requestCode = {requestCode} which indicates that you transitioned from the Main page to the Interactive ARCore demo.");
        }
        else if (requestCode == 400)
        {
            Console.WriteLine($"--------------------MainActivity OnActivityResult requestCode = {requestCode} which indicates that you transitioned from the Main page to the ARCore demo.");
        }
    }
}