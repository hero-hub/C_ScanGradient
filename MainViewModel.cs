using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace C_ScanGradient
{
    public class MainViewModel : INotifyPropertyChanged
    {
        //private ImageSource _image;
        private ColorSpectrum _colorSpectrum = new ColorSpectrum();
        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand SpectrCommand { get; }

        public string FirstContrast { get; set; } = string.Empty;
        public string LastContrast { get; set; } = string.Empty;

        private Image _image;
        public Image Image
        {
            get => _image;
            set
            {
                _image = value;
                OnPropertyChanged(nameof(Image));
            }
        }
        public MainViewModel()
        {
            SpectrCommand = new RelayCommand(_ => SpectrBilding());
        }
        public void SpectrBilding()
        {
            double first = 0.0;
            double last = 0.0;
            if (double.TryParse(FirstContrast, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out double value))
            {
                first = value;
            }

            if (double.TryParse(LastContrast, System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture, out double value1))
            {
                last = value1;
            }

            Image = _colorSpectrum.BitmapDrawer(SignalAnalyse(first), first, last);
        }
        public double[] SignalAnalyse(double first)
        {
            string directoryPath = @"D:\WORK\signals";
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
                maxValue[signalIndex] = max - first;
            }
            return maxValue;
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