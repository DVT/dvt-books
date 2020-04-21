using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace Dvt.Drawing
{
    public static class ImageHelper
    {
        public static void Resize(Stream input, Stream output, int minWidth, int minHeight, int quality)
        {
            using (var bitmap = new Bitmap(input))
            {
                int w = bitmap.Width;
                int h = bitmap.Height;

                if (w > h)
                {
                    if (h > minHeight)
                    {
                        float scale = Convert.ToSingle(minHeight) / Convert.ToSingle(h);

                        w = Convert.ToInt32(w * scale);
                        h = minHeight;
                    }
                }
                else
                {
                    if (w > minWidth)
                    {
                        float scale = Convert.ToSingle(minWidth) / Convert.ToSingle(w);

                        w = minWidth;
                        h = Convert.ToInt32(h * scale);
                    }
                }

                using (var resized = new Bitmap(w, h, PixelFormat.Format32bppArgb))
                using (var graphics = Graphics.FromImage(resized))
                {
                    graphics.CompositingQuality = CompositingQuality.HighSpeed;
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.CompositingMode = CompositingMode.SourceCopy;
                    graphics.DrawImage(bitmap, 0, 0, w, h);

                    ImageCodecInfo codec = ImageCodecInfo.GetImageDecoders().Single(x => x.FormatID == bitmap.RawFormat.Guid);

                    var encoderParameters = new EncoderParameters(1);
                    encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, quality);

                    resized.Save(output, codec, encoderParameters);
                }
            }
        }
    }
}
