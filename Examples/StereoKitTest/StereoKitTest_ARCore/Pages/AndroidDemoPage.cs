using System;

using Android.App;
using Android.Views;
using Android.Widget;

namespace StereoKit_ARCore.Pages
{
    public class AndroidDemoPage : AndroidPage
    {
        LinearLayout rootLayout, leftButtonColumnLayoutList, rightButtonColumnLayoutList;
        ImageView skLogoImageView;
        TextView textView1, checkboxStatusTextView, pickerStatusTextView;
        Button goBackButton, button1, button2;

        CheckBox checkbox1, checkbox2, checkbox3, checkbox4;
        NumberPicker picker1, picker2, picker3, picker4;

        Random randomizer;

        //NOTE: This is the constructor. You can technically just load everything here, but for demo purposes and general neatness, this separation is a better option than dumping all of the code below into the constructor
        public AndroidDemoPage(Activity androidActivity) : base(androidActivity)
        {
            randomizer = new Random();
        }

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
            checkboxStatusTextView = (TextView)activity.FindViewById(Android.Resource.Id.Content).FindViewWithTag("@+string/checkboxstatus");
            pickerStatusTextView = (TextView)activity.FindViewById(Android.Resource.Id.Content).FindViewWithTag("@+string/pickerstatus");

            goBackButton = (Button)activity.FindViewById(Android.Resource.Id.Content).FindViewWithTag("@+string/gobackbutton");
            button1 = (Button)activity.FindViewById(Android.Resource.Id.Content).FindViewWithTag("@+string/button1");
            button2 = (Button)activity.FindViewById(Android.Resource.Id.Content).FindViewWithTag("@+string/button2");
            
            //NOTE: You can check for null before using these Views, but having consistent naming conventions should help avoid having too much of this nullity-checking code
            if (goBackButton != null)
            {
                //NOTE: This is how you add callbacks. Note that in VS's suggestion box that shows up when you type, there is a filter for callback functions (looks like a lightning bolt), which can make this stuff easier
                goBackButton.Click += OnClickGoBackButton;
            }

            if (button1 != null)
            {
                button1.Click += OnClickButton1;
            }

            if (button2 != null)
            {
                button2.Click += OnClickButton2;
            }

            //NOTE: This is an example of a RelativeLayout (although the 2 lists are LinearLayout). Basically, there are 2 linear layouts (which are just stacks, easy to work with) which are organized side-by-side with a RelativeLayout (which has more customizability as far as placement, but if you don't explicitly say where things are, it will just overlap them at the top)
            leftButtonColumnLayoutList = (LinearLayout)activity.FindViewById(Android.Resource.Id.Content).FindViewWithTag("@+string/leftbuttonlayoutlist");
            rightButtonColumnLayoutList = (LinearLayout)activity.FindViewById(Android.Resource.Id.Content).FindViewWithTag("@+string/rightbuttonlayoutlist");

            checkbox1 = (CheckBox)activity.FindViewById(Android.Resource.Id.Content).FindViewWithTag("@+string/checkbox1");
            checkbox2 = (CheckBox)activity.FindViewById(Android.Resource.Id.Content).FindViewWithTag("@+string/checkbox2");
            checkbox3 = (CheckBox)activity.FindViewById(Android.Resource.Id.Content).FindViewWithTag("@+string/checkbox3");
            checkbox4 = (CheckBox)activity.FindViewById(Android.Resource.Id.Content).FindViewWithTag("@+string/checkbox4");

            checkbox1.CheckedChange += OnCheckBox1Changed;
            checkbox2.CheckedChange += OnCheckBox2Changed;
            checkbox3.CheckedChange += OnCheckBox3Changed;
            checkbox4.CheckedChange += OnCheckBox4Changed;

            checkbox3.Checked = true;

            picker1 = (NumberPicker)activity.FindViewById(Android.Resource.Id.Content).FindViewWithTag("@+string/picker1");
            picker2 = (NumberPicker)activity.FindViewById(Android.Resource.Id.Content).FindViewWithTag("@+string/picker2");
            picker3 = (NumberPicker)activity.FindViewById(Android.Resource.Id.Content).FindViewWithTag("@+string/picker3");
            picker4 = (NumberPicker)activity.FindViewById(Android.Resource.Id.Content).FindViewWithTag("@+string/picker4");

            picker1.MinValue = 0;
            picker1.MaxValue = 3;

            picker2.MinValue = 4;
            picker2.MaxValue = 8;
            picker2.Value = 7;

            picker3.MinValue = 9;
            picker3.MaxValue = 20;
            picker3.Value = 15;

            picker4.MinValue = 21;
            picker4.MaxValue = 22;
            picker4.Value = 21;

            picker1.SetTextColor(Android.Graphics.Color.DarkMagenta);
            picker2.SetTextColor(Android.Graphics.Color.Fuchsia);
            picker3.SetTextColor(Android.Graphics.Color.Green);
            picker4.SetTextColor(Android.Graphics.Color.RoyalBlue);

            picker1.ValueChanged += OnPicker1Changed;
            picker2.ValueChanged += OnPicker2Changed;
            picker3.ValueChanged += OnPicker3Changed;
            picker4.ValueChanged += OnPicker4Changed;

        }

        private void OnClickButton1(object sender, EventArgs e)
        {
            TextView textViewButtonClicked = new TextView(activity);
            textViewButtonClicked.Text = "CLICKED BUTTON 1!";
            textViewButtonClicked.SetTextColor(Android.Graphics.Color.Argb(255, randomizer.Next(255), randomizer.Next(255), randomizer.Next(255)));

            //NOTE: This one doesn't add it to the existing layout correctly, it basically puts the new view on top. So we don't want to use this, but I left it as an example
            //activity.AddContentView(textViewButtonClicked, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent));

            //NOTE: This will add it under the button
            leftButtonColumnLayoutList.AddView(textViewButtonClicked, leftButtonColumnLayoutList.IndexOfChild(button1) + 1, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent));
        }

        private void OnClickGoBackButton(object sender, EventArgs e)
        {
            Console.WriteLine("-----------OnClickGoBackButton clicked");
            activity.Finish();
        }

        private void OnClickButton2(object sender, EventArgs e)
        {
            TextView textViewButtonClicked = new TextView(activity);
            textViewButtonClicked.Text = "CLICKED BUTTON 2!";

            //NOTE: This one doesn't add it to the existing layout correctly, it basically puts the new view on top
            //activity.AddContentView(textViewButtonClicked, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent));

            //NOTE: This will add it under the button
            rightButtonColumnLayoutList.AddView(textViewButtonClicked, rightButtonColumnLayoutList.IndexOfChild(button2) + 1, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent));
        }

        private void OnCheckBox1Changed(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            Console.WriteLine($"---------checkbox 1 now: {((CheckBox)sender).Checked} {e.IsChecked}");
            UpdateCheckboxStatus();
        }

        private void OnCheckBox2Changed(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            Console.WriteLine($"---------checkbox 1 now: {((CheckBox)sender).Checked} {e.IsChecked}");
            UpdateCheckboxStatus();
        }

        private void OnCheckBox3Changed(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            Console.WriteLine($"---------checkbox 1 now: {((CheckBox)sender).Checked} {e.IsChecked}");
            UpdateCheckboxStatus();
        }

        private void OnCheckBox4Changed(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            Console.WriteLine($"---------checkbox 1 now: {((CheckBox)sender).Checked} {e.IsChecked}");
            UpdateCheckboxStatus();
        }

        private void UpdateCheckboxStatus()
        {
            checkboxStatusTextView.Text = $"checkbox 1 = {checkbox1.Checked}, checkbox 2 = {checkbox2.Checked}, checkbox 3 = {checkbox3.Checked}, checkbox 4 = {checkbox4.Checked}";
        }

        private void OnPicker1Changed(object sender, NumberPicker.ValueChangeEventArgs e)
        {
            //demos that either way works
            Console.WriteLine($"---------picker 1 now: {((NumberPicker)sender).Value} {e.NewVal}");
            UpdatePickerStatus();
        }

        private void OnPicker2Changed(object sender, NumberPicker.ValueChangeEventArgs e)
        {
            //demos that either way works
            Console.WriteLine($"---------picker 2 now: {((NumberPicker)sender).Value} {e.NewVal}");
            UpdatePickerStatus();
        }

        private void OnPicker3Changed(object sender, NumberPicker.ValueChangeEventArgs e)
        {
            //demos that either way works
            Console.WriteLine($"---------picker 3 now: {((NumberPicker)sender).Value} {e.NewVal}");
            UpdatePickerStatus();
        }

        private void OnPicker4Changed(object sender, NumberPicker.ValueChangeEventArgs e)
        {
            //demos that either way works
            Console.WriteLine($"---------picker 4 now: {((NumberPicker)sender).Value} {e.NewVal}");
            UpdatePickerStatus();
        }

        private void UpdatePickerStatus()
        {
            pickerStatusTextView.Text = $"picker 1 = {picker1.Value}, picker 2 = {picker2.Value}, picker 3 = {picker3.Value}, picker 4 = {picker4.Value}";
        }



        //NOTE: Example of how to iterate through views and layouts... probably easier in general to simply label the elements correctly in the xml, but you may need to do something like this if populating UI elements during runtime, like a social media app
        void discoverElementsInXML()
        {

            View layoutViewUncasted = activity.FindViewById(Android.Resource.Id.Content);
            ViewGroup vg = (ViewGroup)layoutViewUncasted;

            for (int i = 0; i < vg.ChildCount; i++)
            {
                View nextChild = vg.GetChildAt(i);

                Console.WriteLine($"-----------------found a subview {nextChild} {nextChild.Class} {nextChild.Id}");

                //assuming that the root layout is linearlayout
                LinearLayout rootLayout = (LinearLayout)nextChild;

                if (rootLayout != null)
                {
                    for (int j = 0; j < rootLayout.ChildCount; j++)
                    {
                        View nextRootLayoutChild = rootLayout.GetChildAt(j);

                        Console.WriteLine($"-----------------found a rootlayout subview, info=[[[{nextRootLayoutChild}]]], class={nextRootLayoutChild.Class}, id={nextRootLayoutChild.Id}, tag={nextRootLayoutChild.Tag}");
                    }
                }

            }
        }
    }
}
