namespace StereoKit_ARCore.Pages
{
    internal interface AndroidPageInterface
    {
        //NOTE: Sets content view
        public abstract void OpenPage(int androidResourceID);

        //NOTE: Sets up variables and other page setup
        public abstract void InitializePage();

        //NOTE: should update every frame, i.e. as SK stepper. Doesn't do that until you do it explicitly, e.g. make a page an IStepper and then call SK.AddStepper 
        public abstract void UpdatePage();

        //NOTE: Stops any recording, file i/o, etc.
        public abstract void DestroyPage();

    }
}
