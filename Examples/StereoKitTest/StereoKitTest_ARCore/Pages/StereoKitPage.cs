using Android.App;

namespace StereoKit_ARCore.Pages
{
    public class StereoKitPage:AndroidPage
    {
        public StereoKitPage(Activity androidActivity) : base(androidActivity) { }

        public int SKWindowWidth { get; set; }
        public int SKWindowHeight { get; set; }

        public void SetSKWindowSize(int width, int height)
        {
            SKWindowWidth = width;
            SKWindowHeight = height;
        }
    }
}
