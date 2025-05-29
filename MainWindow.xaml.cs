using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace C_ScanGradient
{
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();

            double[] values = new double[256];
            int count = 256;
            Random random = new Random();
            //for (int i = 0; i < count; i++)
            //{
            //    values[i] = 255 - i; // от белого до черного
            //}
            for (int i = 0; i < count; i++)
            {
                values[i] = random.NextDouble() * 3.3; // Случайное число от 0 до 3.3
            }

            WriteableBitmap bitmap = new WriteableBitmap(256, 1, 96, 96, PixelFormats.Bgr32, null);
            byte[] pixels = new byte[count * 4]; // 4 байта на пиксель (BGRA)

            for (int x = 0; x < count; x++)
            {
                //byte grayValue = (byte)values[x];
                //byte blue, green, red;

                double blueChanel = -93.66 * values[x] * values[x] + 255;
                double greenChanel = -93.66 * (values[x] - 1.65) * (values[x] - 1.65) + 255;
                double redChanel = -93.66 * (values[x] - 3.3) * (values[x] - 3.3) + 255;

                byte blue = (byte)Math.Clamp(blueChanel, 0, 255);
                byte green = (byte)Math.Clamp(greenChanel, 0, 255);
                byte red = (byte)Math.Clamp(redChanel, 0, 255);

                pixels[x * 4] = blue;       // Blue
                pixels[x * 4 + 1] = green;  // Green
                pixels[x * 4 + 2] = red;    // Red
                pixels[x * 4 + 3] = 255;    // Alpha

                //pixels[x * 4] = grayValue;     // Blue
                //pixels[x * 4 + 1] = grayValue; // Green
                //pixels[x * 4 + 2] = grayValue; // Red
                //pixels[x * 4 + 3] = 255;       // Alpha
            }

            bitmap.WritePixels(new Int32Rect(0, 0, count, 1), pixels, count * 4, 0);
            Image image = new Image
            {
                Source = bitmap,
                Width = 512,
                Height = 150,
                Stretch = Stretch.Fill // растягивает изображение, заполняя заданную высоту
            };

            Grid grid = new Grid();
            grid.Children.Add(image);
            this.Content = grid;
        }
    }
}