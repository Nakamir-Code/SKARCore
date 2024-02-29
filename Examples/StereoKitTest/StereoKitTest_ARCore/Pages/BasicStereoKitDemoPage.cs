using System;

using Android.App;
using Android.Views;
using Android.Widget;

using StereoKit;

namespace StereoKit_ARCore.Pages
{
    public class BasicStereoKitDemoPage : StereoKitPage
    {
        //NOTE: For more options as to available Android UI View classes, start writing XML in layout xml and it should suggest other Android UI, otherwise use Android resources online
        SurfaceView skSurfaceView;
        Button goBackButton;

        //NOTE: This is the constructor. You can technically just load everything here, but for demo purposes and general neatness, this separation is a better option than dumping all of the code below into the constructor
        public BasicStereoKitDemoPage(Activity androidActivity) : base(androidActivity) { }

        public override void OpenPage(int androidResourceID)
        {
            base.OpenPage(androidResourceID);

            //NOTE: Creates pointers for the UI elements and other page initialization 
            InitializePage();

            //NOTE: Starts SK. There can only be 1 SK instance in the entire app as SK is effectively a static class
            ((BasicStereoKitDemoActivity)activity).RunSK(); //should be in intro page
        }

        //NOTE: This function initializes local variables pointing to various UI elements
        public override void InitializePage()
        {
            //NOTE: Finds the SurfaceView that SK will render to
            skSurfaceView = (SurfaceView)activity.FindViewById(Android.Resource.Id.Content).FindViewWithTag("@+string/surfaceview");

            //NOTE: Necessary for SK now that we're not letting SK absorb the screen
            skSurfaceView.Holder.AddCallback((BasicStereoKitDemoActivity)activity);

            goBackButton = (Button)activity.FindViewById(Android.Resource.Id.Content).FindViewWithTag("@+string/gobackbutton");
            goBackButton.Click += OnClickGoBackButton;
        }


        private void OnClickGoBackButton(object sender, EventArgs e)
        {
            Console.WriteLine("-----------OnGoBackButtonClicked clicked");
            SK.Quit();
            //NOTE: Kills this activity and either goes back to previous Activity or kills app
            activity.Finish();
        }
    }
}
