using Android.App;

namespace StereoKit_ARCore.Pages
{
    public class AndroidPage : AndroidPageInterface
    {
        public Activity activity;

        public AndroidPage(Activity androidActivity)
        {
            activity= androidActivity;
        }

        //NOTE: Sets page content with an xml file by its resource ID
        //NOTE: technically doesn't NEED to be made virtual here but this is a good enough way to demonstrate how the child's version of the function is called since MainActivity shouldn't be so dependent on a specific page class
        public virtual void OpenPage(int androidResourceID) => activity.SetContentView(androidResourceID);

        //NOTE: Populate variables for views
        public virtual void InitializePage() {}

        //NOTE: Should basically be an SK stepper in most cases
        public virtual void UpdatePage() { }

        //NOTE: Shut down anything with arcore and video recording, rest handled by garbage collector
        public virtual void DestroyPage() { }
        
    }
}
