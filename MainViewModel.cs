using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Media;
using System.Windows.Controls;

namespace C_ScanGradient
{
    public class MainViewModel : INotifyPropertyChanged
    {
        //private ImageSource _image;
        private ColorSpectrum _colorSpectrum = new ColorSpectrum();
        public event PropertyChangedEventHandler PropertyChanged;

        //public ImageSource image
        //{
        //    get => _image;
        //    set
        //    {
        //        _image = value;
        //        OnPropertyChanged(nameof(image));
        //    }
        //}

        public MainViewModel()
        {
            SignalAnalyse();
        }

        public Image SignalAnalyse()
        {
            string directoryPath = @"D:\WORK\C_ScanGradient\signals";
            string[] filePaths = Directory.GetFiles(directoryPath, "*.txt");
            double[] maxValue = new double[filePaths.Length];

            for (int signalIndex = 0; signalIndex < filePaths.Length; signalIndex++)
            {
                var values = LoadDataFromFile(filePaths[signalIndex]);
                double max = 0.0;
                for (int value = 1190; value < 2210; value++)
                {
                    if (Math.Abs(values[value]) > max) max = values[value];
                }
                maxValue[signalIndex] = max + 2.0;
            }
            return _colorSpectrum.BitmapDrawer(maxValue);
        }

        private List<double> LoadDataFromFile(string filePath)
        {
            List<double> values = new List<double>();
            string[] lines = File.ReadAllLines(filePath);

            foreach (string line in lines)
            {
                string[] parts = line.Trim().Split(';');
                if (double.TryParse(parts[1],
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out double value))
                {
                    values.Add(value);
                }
            }
            return values;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}