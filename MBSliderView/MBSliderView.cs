using System;
using System.Drawing;
using System.Text;
using MonoTouch.CoreAnimation;
using MonoTouch.CoreGraphics;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace RTodorov
{
    [Register("MBSliderView")]
    public class MBSliderView : UIView
    {
        public static readonly float FRAMES_PER_SEC = 10;
        public static readonly float gradientWidth = 0.2f;
        public static readonly float gradientDimAlpha = 0.5f;

        public event EventHandler<EventArgs> SliderDidSlide;

        UISlider _slider;
        MBSliderLabel _label;
        bool _sliding;

        // Implement the "enabled" property
        public bool Enabled
        {
            get
            {
                return _slider.Enabled;
            }
            set
            {
                _slider.Enabled = value;
                _label.Enabled = value;

                if (value)
                {
                    _slider.Value = 0;
                    _label.Alpha = 1;
                    _sliding = false;
                }

                _label.Animated = value;
            }
        }

        // Implement the "text" property
        public string Text
        {
            get
            {
                return _label.Text;
            }
            set
            {
                _label.Text = value;
            }
        }

        // Implement the "labelColor" property
        public UIColor LabelColor
        {
            get
            {
                return _label.TextColor;
            }
            set
            {
                _label.TextColor = value;
            }
        }

        public UIColor ThumbColor
        {
            set
            {
                _slider.SetThumbImage(this.ThumbWithColor(value), UIControlState.Normal);
            }
        }

        public MBSliderView (RectangleF frame) : base(frame)
        {
            if (frame.Width < 136.0)
            {
                frame.Width = 136.0f;
            }

            if (frame.Height < 44.0)
            {
                frame.Height = 44.0f;
            }

            base.Frame = frame;

            LoadContent();
        }

        public MBSliderView (NSCoder coder) : base(coder)
        {
            LoadContent();
        }

        public MBSliderView (IntPtr handle) : base(handle)
        {
            LoadContent();
        }

        public void LoadContent ()
        {
            this.BackgroundColor = UIColor.Clear;
            this.UserInteractionEnabled = true;

            if (_label == null || _slider == null)
            {
                /*foreach (UIView subview in this.Subviews)
                {
                    subview.PerformSelector(new MonoTouch.ObjCRuntime.Selector("removeFromSuperview"), subview, 0);
                }*/

                _label = new MBSliderLabel(RectangleF.Empty);
                _label.AutoresizingMask = UIViewAutoresizing.FlexibleLeftMargin | UIViewAutoresizing.FlexibleRightMargin;
                _label.TextColor = UIColor.White;
                _label.TextAlignment = UITextAlignment.Center;
                _label.BackgroundColor = UIColor.Clear;
                _label.Font = UIFont.SystemFontOfSize(24);
                _label.Text = "Slide";
                this.AddSubview(_label);
                _label.Animated = true;

                _slider = new UISlider(RectangleF.Empty);
                _slider.AutoresizingMask = UIViewAutoresizing.FlexibleWidth;
                PointF ctr = _slider.Center;
                RectangleF sliderFrame = _slider.Frame;
                sliderFrame.Width -= 4; //each "edge" of the track is 2 pixels wide
                _slider.Frame = sliderFrame;
                _slider.Center = ctr;
                _slider.BackgroundColor = UIColor.Clear;
                UIImage thumbImage = this.ThumbWithColor(UIColor.FromRGBA(200, 200, 200, 1));
                _slider.SetThumbImage(thumbImage, UIControlState.Normal);

                UIImage clearImage = this.ClearPixel();
                _slider.SetMaxTrackImage(clearImage, UIControlState.Normal);
                _slider.SetMinTrackImage(clearImage, UIControlState.Normal);

                _slider.MinValue = 0.0f;
                _slider.MaxValue = 1.0f;
                _slider.Continuous = true;
                _slider.Value = 0.0f;
                this.AddSubview (_slider);

                // Set the slider action methods
                _slider.AddTarget (sliderUp, UIControlEvent.TouchUpInside);
                _slider.AddTarget (sliderUp, UIControlEvent.TouchUpOutside);
                _slider.AddTarget (sliderDown, UIControlEvent.TouchDown);
                _slider.AddTarget (sliderChanged, UIControlEvent.ValueChanged);
            }
        }

        public override void LayoutSubviews ()
        {
            base.LayoutSubviews();

            float sliderWidth = _slider.ThumbImage (_slider.State).Size.Width;
            SizeF labelSize = _label.SizeThatFits (this.Bounds.Size);

            _label.Frame = new RectangleF (sliderWidth + 30,
                RectangleFExtensions.GetMidY(this.Bounds) - (labelSize.Height / 2),
                this.Bounds.Width - sliderWidth - 30,
                labelSize.Height
            );
            _slider.Frame = this.Bounds;
        }

        // UISlider actions
        public void sliderUp (object sender, EventArgs e)
        {
            if (_sliding) {
                _sliding = false;

                if (_slider.Value == 1.0 && SliderDidSlide != null)
                {
                    SliderDidSlide(this, null);
                }

                _slider.SetValue(0, true);
                _label.Alpha = 1;
                _label.Animated = true;
            }
        }

        public void sliderDown (object sender, EventArgs e)
        {
            if (!_sliding)
            {
                _label.Animated = false;
            }

            _sliding = true;
        }

        public void sliderChanged (object sender, EventArgs e)
        {
            _label.Alpha = Math.Max(0, 1 - (_slider.Value * 3.5f));
        }

        public UIImage ThumbWithColor (UIColor color)
        {
            float scale = UIScreen.MainScreen.Scale;
            if (scale < 1)
            {
                scale = 1;
            }

            SizeF size = new SizeF (68 * scale, 44 * scale);
            float radius = 10 * scale;
            // create a new bitmap image context
            UIGraphics.BeginImageContext(size);     

            // get context
            CGContext context = UIGraphics.GetCurrentContext();       

            // push context to make it current 
            // (need to do this manually because we are not drawing in a UIView)
            UIGraphics.PushContext(context); 

            color.SetFill();
            UIColor.Black.ColorWithAlpha(0.8f).SetStroke();

            float radiusp = radius + 0.5f;
            float wid1 = size.Width - 0.5f;
            float hei1 = size.Height - 0.5f;
            float wid2 = size.Width - radiusp;
            float hei2 = size.Height - radiusp;

            // Path
            context.MoveTo(0.5f, radiusp);
            context.AddArcToPoint(0.5f, 0.5f, radiusp, 0.5f, radius);
            context.AddLineToPoint(wid2, 0.5f);
            context.AddArcToPoint(wid1, 0.5f, wid1, radiusp, radius);
            context.AddLineToPoint(wid1, hei2);
            context.AddArcToPoint(wid1, hei1, wid2, hei1, radius);
            context.AddLineToPoint(radius, hei1);
            context.AddArcToPoint(0.5f, hei1, 0.5f, hei2, radius);
            context.ClosePath();
            context.DrawPath(CGPathDrawingMode.FillStroke);

            // Arrow
            UIColor.White.ColorWithAlpha(0.6f).SetFill();
            UIColor.Black.ColorWithAlpha(0.3f).SetStroke();

            float[] points = new float[8]
            {
                (19 * scale) + 0.5f,
                (16 * scale) + 0.5f,
                (36 * scale) + 0.5f,
                (10 * scale) + 0.5f,
                (52 * scale) + 0.5f,
                (22 * scale) + 0.5f,
                (34 * scale) + 0.5f,
                (28 * scale) + 0.5f
            };

            context.MoveTo(points[0], points[1]);
            context.AddLineToPoint(points[2], points[1]);
            context.AddLineToPoint(points[2], points[3]);
            context.AddLineToPoint(points[4], points[5]);
            context.AddLineToPoint(points[2], points[6]);
            context.AddLineToPoint(points[2], points[7]);
            context.AddLineToPoint(points[0], points[7]);
            context.ClosePath();
            context.DrawPath(CGPathDrawingMode.FillStroke); 

            // Light
            UIColor.White.ColorWithAlpha(0.2f).SetFill();

            float mid = (float)Math.Round(size.Height / 2) + 0.5f;
            context.MoveTo(0.5f, radiusp);
            context.AddArcToPoint(0.5f, 0.5f, radiusp, 0.5f, radius);
            context.AddLineToPoint(wid2, 0.5f);
            context.AddArcToPoint(wid1, 0.5f, wid1, radiusp, radius);
            context.AddLineToPoint(wid1, mid);
            context.AddLineToPoint(0.5f, mid);
            context.ClosePath(); 
            context.DrawPath(CGPathDrawingMode.FillStroke);

            // pop context 
            UIGraphics.PopContext();

            // get a UIImage from the image context
            UIImage outputImage = new UIImage(UIGraphics.GetImageFromCurrentImageContext().CGImage, scale, UIImageOrientation.Up);
            //write (debug)
            //[UIImagePNGRepresentation(outputImage) writeToFile:@"/Users/mathieu/Desktop/test.png" atomically:YES];

            // clean up drawing environment
            UIGraphics.EndImageContext();

            return outputImage;
        }

        public UIImage ClearPixel ()
        {
            RectangleF rect = new RectangleF(0, 0, 1, 1);
            UIGraphics.BeginImageContext(rect.Size);
            CGContext context = UIGraphics.GetCurrentContext();
            context.SetFillColorWithColor(new CGColor(255, 255, 255, 0)); // TODO: rever
            context.FillRect(rect);
            UIImage image = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();
            return image;
        }
    }
}