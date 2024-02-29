using Android.App;
using Android.OS;
using Android.Content.Res;

using StereoKit_ARCore.Pages;

//this MUST be set correctly or you won't be able to load this activity. doing this adds the activity to the AndroidManifest so Android knows how to find it
[Activity(Label = "SwitchActivitiesDemoActivity", MainLauncher = false, Exported = true)]

//adding this seems to break something
//[IntentFilter(new[] { Intent.ActionMain }, Categories = new[] { "org.khronos.openxr.intent.category.IMMERSIVE_HMD", "com.oculus.intent.category.VR", Intent.CategoryLauncher })]
public class SwitchActivitiesDemoActivity : Activity
{
    SwitchActivitiesDemoPage page;
    protected override void OnCreate(Bundle savedInstanceState)
	{
		base.OnCreate(null);

        //this gets rid of the bar on top that tells you the name of the app
        //this.RequestWindowFeature(WindowFeatures.NoTitle);

        AndroidResourceIDsRuntime.switchActivitiesPageResID = Resources.GetIdentifier("switchactivitiesdemo_page", "layout", "com.nakamir.skarcore");
        page = new SwitchActivitiesDemoPage(this);
        page.OpenPage(AndroidResourceIDsRuntime.switchActivitiesPageResID);
    }
}