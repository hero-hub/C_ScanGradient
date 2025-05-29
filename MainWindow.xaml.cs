using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace C_ScanGradient
{
    public partial class MainWindow : Window
    {
        private int[] values = new int[256];

        public MainWindow()
        {
            InitializeComponent();
            for (int i = 0; i < 256; i++)
            {
                values[i] = 255 - i; // от белого до черного
            }

            WriteableBitmap bitmap = new WriteableBitmap(256, 1, 96, 96, PixelFormats.Bgr32, null);
            byte[] pixels = new byte[256 * 4]; // 4 байта на пиксель (BGRA)

            for (int x = 0; x < 256; x++)
            {
                byte grayValue = (byte)values[x];
                pixels[x * 4] = grayValue;     // Blue
                pixels[x * 4 + 1] = grayValue; // Green
                pixels[x * 4 + 2] = grayValue; // Red
                pixels[x * 4 + 3] = 255;       // Alpha
            }

            bitmap.WritePixels(new Int32Rect(0, 0, 256, 1), pixels, 256 * 4, 0);
            Image image = new Image { Source = bitmap, Width = 256, Height = 1 };
            this.Content = image;
        }
    }
}