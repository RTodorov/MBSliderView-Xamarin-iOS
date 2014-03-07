using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.CoreGraphics;

namespace RTodorov
{
    public class MBSliderLabel : UILabel
    {
        NSTimer animationTimer;
        float[] gradientLocations = new float[3];
        int animationTimerCount;
        bool _animated;

        public bool Animated
        {
            get
            {
                return _animated;
            }
            set
            {
                if (_animated != value) {
                    _animated = value;
                    if (_animated) {
                        this.StartTimer();
                    } else {
                        this.StopTimer();
                    }
                }
            }
        }

        public MBSliderLabel(RectangleF frame) : base(frame)
        {
        }

        // animationTimer methods
        private void animationTimerFired ()
        {
            // Let the timer run for 2 * FPS rate before resetting.
            // This gives one second of sliding the highlight off to the right, plus one
            // additional second of uniform dimness
            if (++animationTimerCount == (2 * MBSliderView.FRAMES_PER_SEC))
            {
                animationTimerCount = 0;
            }

            // Update the gradient for the next frame
            this.SetGradientLocations(((float)animationTimerCount / MBSliderView.FRAMES_PER_SEC));
        }

        public void StartTimer ()
        {
            if (animationTimer == null)
            {
                animationTimerCount = 0;
                this.SetGradientLocations(0);
                animationTimer = NSTimer.CreateRepeatingScheduledTimer(1 / MBSliderView.FRAMES_PER_SEC, delegate {
                    animationTimerFired();
                });
            }
        }

        public void StopTimer ()
        {
            if (animationTimer != null)
            {
                animationTimer.Invalidate();
                animationTimer = null;
            }
        }

        public override void Draw (RectangleF rect)
        //(void)drawLayer:(CALayer *)theLayer inContext:(CGContextRef)theContext
        {
            CGContext theContext = UIGraphics.GetCurrentContext();

            // Note: due to use of kCGEncodingMacRoman, this code only works with Roman alphabets! 
            // In order to support non-Roman alphabets, you need to add code generate glyphs,
            // and use CGContextShowGlyphsAtPoint
            theContext.SelectFont(this.Font.Name, this.Font.PointSize, CGTextEncoding.MacRoman);

            // Set Text Matrix
            theContext.TextMatrix = new CGAffineTransform(1, 0, 0, -1, 0, 0);

            // Set Drawing Mode to clipping path, to clip the gradient created below
            theContext.SetTextDrawingMode (CGTextDrawingMode.Clip);

            // Draw the label's text
            string text = this.Text;// cStringUsingEncoding:NSMacOSRomanStringEncoding];
            theContext.ShowTextAtPoint(0, this.Font.Ascender, text, text.Length);

            // Calculate text width
            PointF textEnd = theContext.TextPosition;

            // Get the foreground text color from the UILabel.
            // Note: UIColor color space may be either monochrome or RGB.
            // If monochrome, there are 2 components, including alpha.
            // If RGB, there are 4 components, including alpha.
            CGColor textColor = this.TextColor.CGColor;
            float[] components = textColor.Components;
            int numberOfComponents = textColor.NumberOfComponents;
            bool isRGB = (numberOfComponents == 4);
            float red = components[0];
            float green = isRGB ? components[1] : components[0];
            float blue = isRGB ? components[2] : components[0];
            float alpha = isRGB ? components[3] : components[1];

            // The gradient has 4 sections, whose relative positions are defined by
            // the "gradientLocations" array:
            // 1) from 0.0 to gradientLocations[0] (dim)
            // 2) from gradientLocations[0] to gradientLocations[1] (increasing brightness)
            // 3) from gradientLocations[1] to gradientLocations[2] (decreasing brightness)
            // 4) from gradientLocations[3] to 1.0 (dim)
            int num_locations = 3;

            // The gradientComponents array is a 4 x 3 matrix. Each row of the matrix
            // defines the R, G, B, and alpha values to be used by the corresponding
            // element of the gradientLocations array
            float[] gradientComponents = new float[12];
            for (int row = 0; row < num_locations; row++)
            {
                int index = 4 * row;
                gradientComponents[index++] = red;
                gradientComponents[index++] = green;
                gradientComponents[index++] = blue;
                gradientComponents[index] = alpha * MBSliderView.gradientDimAlpha;
            }

            // If animating, set the center of the gradient to be bright (maximum alpha)
            // Otherwise it stays dim (as set above) leaving the text at uniform
            // dim brightness
            if (animationTimer != null)
            {
                gradientComponents[7] = alpha;
            }

            // Load RGB Colorspace
            CGColorSpace colorspace = CGColorSpace.CreateDeviceRGB();

            // Create Gradient
            CGGradient gradient = new CGGradient(colorspace, gradientComponents, gradientLocations);
            // Draw the gradient (using label text as the clipping path)
            theContext.DrawLinearGradient (gradient, this.Bounds.Location, textEnd, 0);

            // Cleanup
            gradient.Dispose();
            colorspace.Dispose();
        }

        public void SetGradientLocations (float leftEdge)
        {
            // Subtract the gradient width to start the animation with the brightest 
            // part (center) of the gradient at left edge of the label text
            leftEdge -= MBSliderView.gradientWidth;

            //position the bright segment of the gradient, keeping all segments within the range 0..1
            gradientLocations[0] = leftEdge < 0 ? 0 : (leftEdge > 1 ? 1 : leftEdge);
            gradientLocations[1] = Math.Min(leftEdge + MBSliderView.gradientWidth, 1);
            gradientLocations[2] = Math.Min(gradientLocations[1] + MBSliderView.gradientWidth, 1);

            // Re-render the label text
            this.Layer.SetNeedsDisplay();
        }
    }
}