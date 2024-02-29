using System;

using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;

namespace StereoKit_ARCore.Pages
{
    public class DemoEntryPage : AndroidPage
    {
        LinearLayout rootLayout;
        ImageView skLogoImageView;
        TextView textView1;
        Button androidDemoButton, basicSKDemoButton, interactiveSKDemoButton, arCoreDemoButton;

        //NOTE: This is the constructor. You can technically just load everything here, but for demo purposes and general neatness, this separation is a better option than dumping all of the code below into the constructor
        public DemoEntryPage(Activity androidActivity) : base(androidActivity) { }

        public override void OpenPage(int androidResourceID)
        {
            //NOTE: This is basically a wrapper for SetContentView, which is how you open XML pages
            base.OpenPage(androidResourceID);

            //NOTE: This function initializes local variables pointing to various UI elements
            InitializePage();
        }

        //NOTE: This function initializes local variables pointing to various UI elements
        public override void InitializePage()
        {
            //NOTE: This gets the root view of the current page, which is a LinearLayout (check Resources/layout/demoentry_page.xml). Could also get it by id as shown later
            View layoutViewUncasted = activity.FindViewById(Android.Resource.Id.Content);
            ViewGroup vg = (ViewGroup)layoutViewUncasted;
            rootLayout = (LinearLayout)vg.GetChildAt(0);

            //NOTE: This is an example of loading an Drawable asset and then filling an ImageView with it
            skLogoImageView = (ImageView)activity.FindViewById(Android.Resource.Id.Content).FindViewWithTag("@+string/sklogoview");

            //NOTE: Assets MUST be added in the csproj with AndroidResource Include or other Resource class
            int skLogoID = activity.Resources.GetIdentifier("stereokit_logo", "drawable", "com.nakamir.skarcore");

            //NOTE: Must be bitmap format, so you can't use an svg :(
            Android.Graphics.Drawables.Drawable sklogo_drawable = activity.GetDrawable(skLogoID);
            skLogoImageView.SetImageDrawable(sklogo_drawable);

            //NOTE: This is the standard way of getting pointers to UI elements
            textView1 = (TextView)activity.FindViewById(Android.Resource.Id.Content).FindViewWithTag("@+string/textview1");

            androidDemoButton = (Button)activity.FindViewById(Android.Resource.Id.Content).FindViewWithTag("@+string/basicandroiddemobutton");
            basicSKDemoButton = (Button)activity.FindViewById(Android.Resource.Id.Content).FindViewWithTag("@+string/basicskdemobutton");
            interactiveSKDemoButton = (Button)activity.FindViewById(Android.Resource.Id.Content).FindViewWithTag("@+string/interactiveskdemobutton");
            arCoreDemoButton = (Button)activity.FindViewById(Android.Resource.Id.Content).FindViewWithTag("@+string/arcoredemobutton");


            //NOTE: This is how you add callbacks. Note that in VS's suggestion box that shows up when you type, there is a filter for callback functions (looks like a lightning bolt), which can make this stuff easier
            androidDemoButton.Click += OnClickAndroidDemoButton;
            basicSKDemoButton.Click += OnClickBasicSKDemoButton;
            interactiveSKDemoButton.Click += OnClickInteractiveSKDemoButton;
            arCoreDemoButton.Click += OnClickARCoreDemoButton;

        }

        private void OnClickAndroidDemoButton(object sender, EventArgs e)
        {
            OpenNewAndroidActivity(typeof(AndroidDemoActivity), 100);
        }

        private void OnClickBasicSKDemoButton(object sender, EventArgs e)
        {
            OpenNewAndroidActivity(typeof(BasicStereoKitDemoActivity), 200);
        }

        private void OnClickInteractiveSKDemoButton(object sender, EventArgs e)
        {
            OpenNewAndroidActivity(typeof(InteractiveStereoKitDemoActivity), 300);
        }

        private void OnClickARCoreDemoButton(object sender, EventArgs e)
        {
            OpenNewAndroidActivity(typeof(ARCoreDemoActivity), 400);
        }

        private void OpenNewAndroidActivity(Type activityToOpenType, int requestCode) {
            //NOTE: This is how you change Activity. While an Activity is somewhat like a different app, it's not. Changes made to static variables or to the overall Android context will still last between Activities. The other way to do it would be to call SetContentView again, but this could make garbage collection and resetting references more difficult. However, since quitting SK breaks future attempts to load SK in the same app session, making that is the better way to do it until Nick Klingensmith fixes issues with the internal GL-resetting process
            Intent i = new Intent(Android.App.Application.Context, activityToOpenType);

            //NOTE: Android will throw an error if you don't do this
            i.AddFlags(ActivityFlags.NewTask);

            //NOTE: The requestCode is just a code that gets passed to OnActivityResult, doesn't really have an inherent meaning
            activity.StartActivityForResult(i, requestCode);
        }
    }
}
