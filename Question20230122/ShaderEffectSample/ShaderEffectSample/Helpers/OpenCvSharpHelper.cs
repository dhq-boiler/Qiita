using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ShaderEffectSample.Helpers
{
    static class OpenCvSharpHelper
    {
        [Conditional("DEBUG")]
        public static void ImShow(string windowTitle, BitmapSource rtb)
        {
            FormatConvertedBitmap newFormatedBitmapSource = new FormatConvertedBitmap();
            newFormatedBitmapSource.BeginInit();
            newFormatedBitmapSource.Source = rtb;
            newFormatedBitmapSource.DestinationFormat = PixelFormats.Bgr24;
            newFormatedBitmapSource.EndInit();

            var mat = OpenCvSharp.WpfExtensions.BitmapSourceConverter.ToMat(newFormatedBitmapSource);
            OpenCvSharp.Cv2.ImShow(windowTitle, mat);
        }

        [Conditional("DEBUG")]
        public static void ImShow(string windowTitle, System.Windows.Media.Visual visual, int width, int height)
        {
            var rtb = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(visual);
            //rtb.Freeze();
            ImShow(windowTitle, rtb);
        }

        [Conditional("DEBUG")]
        public static void ImShow(string windowTitle, VisualBrush brush, int width, int height)
        {
            var rtb = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            DrawingVisual visual = new DrawingVisual();
            using (DrawingContext context = visual.RenderOpen())
            {
                context.DrawRectangle(brush, null, new System.Windows.Rect(0, 0, width, height));
            }
            rtb.Render(visual);
            //rtb.Freeze();
            ImShow(windowTitle, rtb);
        }
    }
}
