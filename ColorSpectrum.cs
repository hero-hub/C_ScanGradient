using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;

namespace C_ScanGradient
{
    public class ColorSpectrum
    {
        public ColorSpectrum() 
        { 
            
        }
        
        public Image BitmapDrawer(double[] values)
        {
            WriteableBitmap bitmap = new WriteableBitmap(values.Length, 1, 96, 96, PixelFormats.Bgr32, null);
            byte[] pixels = new byte[values.Length * 4]; // 4 байта на пиксель (BGRA)

            for (int x = 0; x < values.Length; x++)
            {
                double blueChanel, greenChanel, redChanel;
                if (values[x] < 0)
                {
                    blueChanel = 0;
                    greenChanel = 0;
                    redChanel = 0;
                }
                else if (values[x] > 3.3)
                {
                    blueChanel = 255;
                    greenChanel = 255;
                    redChanel = 255;
                }
                else
                {
                    blueChanel = -93.66 * values[x] * values[x] + 255;
                    greenChanel = -93.66 * (values[x] - 1.65) * (values[x] - 1.65) + 255;
                    redChanel = -93.66 * (values[x] - 3.3) * (values[x] - 3.3) + 255;
                }

                byte blue = (byte)Math.Clamp(blueChanel, 0, 255);
                byte green = (byte)Math.Clamp(greenChanel, 0, 255);
                byte red = (byte)Math.Clamp(redChanel, 0, 255);

                pixels[x * 4] = blue;       // Blue
                pixels[x * 4 + 1] = green;  // Green
                pixels[x * 4 + 2] = red;    // Red
                pixels[x * 4 + 3] = 255;    // Alpha
            }

            bitmap.WritePixels(new Int32Rect(0, 0, values.Length, 1), pixels, values.Length * 4, 0);
            Image image = new Image
            {
                Source = bitmap,
                Width = 512,
                Height = 150,
                Stretch = Stretch.Fill // растягивает изображение, заполняя заданную высоту
            };

            return image;
        }

    }

}
