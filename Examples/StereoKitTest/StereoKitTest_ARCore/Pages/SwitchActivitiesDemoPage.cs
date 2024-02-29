using System;

using Android.App;
using Android.Content;
using Android.Widget;

namespace StereoKit_ARCore.Pages
{
    public class SwitchActivitiesDemoPage:AndroidPage
    {
        Button goBackButton;

        public SwitchActivitiesDemoPage(Activity androidActivity) : base(androidActivity) { }

        public override void OpenPage(int androidResourceID)
        {
            base.OpenPage(androidResourceID);
            InitializePage();
        }

        public override void InitializePage()
        {
            goBackButton = (Button)activity.FindViewById(Android.Resource.Id.Content).FindViewWithTag("@+string/switchactivitiesbutton");
            goBackButton.Click += OnClickGoBackButton;
        }

        private void OnClickGoBackButton(object sender, EventArgs e)
        {
            Intent intent = new Intent();
            activity.SetResult(Result.Ok, intent);
            activity.Finish();
        }
    }
}
