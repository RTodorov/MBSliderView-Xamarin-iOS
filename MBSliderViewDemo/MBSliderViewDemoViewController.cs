using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using RTodorov;

namespace MBSliderViewDemo
{
    public partial class MBSliderViewDemoViewController : UIViewController
    {
        public MBSliderViewDemoViewController() : base("MBSliderViewDemoViewController", null)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
		
            // Programmatically
            MBSliderView s1 = new MBSliderView(new RectangleF(20, 40, View.Bounds.Width - 40, 44));
            s1.ThumbColor = UIColor.Blue;
            s1.Text = "Slide from code";

            // Loaded from nib
            s2.ThumbColor = UIColor.Brown;
            s2.Text = "Slide from XIB";

            // sliderDidSlide event
            s1.SliderDidSlide += SliderDidSlide;
            s2.SliderDidSlide += SliderDidSlide;

            View.AddSubview(s1);
        }

        public void SliderDidSlide(object sender, EventArgs e)
        {
            Console.WriteLine("Slider did slide!");

            ((MBSliderView)sender).ThumbColor = RandomColor();
            ((MBSliderView)sender).LabelColor = RandomColor();
        }

        private UIColor RandomColor ()
        {
            Random random = new Random();

            int r = random.Next(0, 256);
            int g = random.Next(0, 256);
            int b = random.Next(0, 256);
         
            return UIColor.FromRGB(r, g, b);
        }
    }
}