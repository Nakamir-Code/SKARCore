using System;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Content.Res;

using StereoKit_ARCore.Pages;


//NOTE: This MUST be set correctly or you won't be able to load this activity. Doing this adds the activity to the AndroidManifest so Android knows how to find it when you try to open it
[Activity(Label = "AndroidDemoActivity", MainLauncher = false, Exported = true)]

//NOTE: adding this here causes this to be the entry Activity, and there doesn't seem to be a good reason to have it for Android mobile anyway
//[IntentFilter(new[] { Intent.ActionMain }, Categories = new[] { "org.khronos.openxr.intent.category.IMMERSIVE_HMD", "com.oculus.intent.category.VR", Intent.CategoryLauncher })]
public class AndroidDemoActivity : Activity
{
    //NOTE: Theoretically, if you wanted to push/pop pages like a NavigationController, you would make this a stack, but the correct way seems to be to use Activities
    AndroidDemoPage currentPage;

    protected override void OnCreate(Bundle savedInstanceState)
    {
        Console.WriteLine($"--------------------AndroidDemoActivity OnCreate");
        base.OnCreate(savedInstanceState);

        //NOTE: This gets rid of the bar on top that tells you the name of the app
        this.RequestWindowFeature(WindowFeatures.NoTitle);

        AndroidResourceIDsRuntime.androidDemoPageResID = Resources.GetIdentifier("androiddemo_page", "layout", "com.nakamir.skarcore");
        currentPage = new AndroidDemoPage(this);
        currentPage.OpenPage(AndroidResourceIDsRuntime.androidDemoPageResID);
    }

    protected override void OnDestroy()
    {
        currentPage.DestroyPage();
        base.OnDestroy();
    }

    //NOTE: This function will get called with specific Android functions, e.g. pushing a new activity (StartActivityForResult), allowing you to do various things like wrap up internet features, close streams, etc. before the Activity changes. You can use different requestCodes for this to tell what is actually happening
    protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
    {
        base.OnActivityResult(requestCode, resultCode, data);
        Console.WriteLine($"--------------------AndroidDemoActivity OnActivityResult {requestCode} {resultCode}");
    }

}