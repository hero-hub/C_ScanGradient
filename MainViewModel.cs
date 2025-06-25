using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using OxyPlot;
using OxyPlot.Series;

namespace C_ScanGradient
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private PlotModel _plotModel;
        private ColorSpectrum _colorSpectrum = new ColorSpectrum();
        private Image _image;
        private int _sliderValue;
        private string[] _filePaths;
        private string swapPhase = "";

        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand SpectrCommand { get; }

        public string FirstContrast { get; set; } = string.Empty;
        public string LastContrast { get; set; } = string.Empty;

        public Image Image
        {
            get => _image;
            set
            {
                _image = value;
                OnPropertyChanged(nameof(Image));
            }
        }
        public PlotModel PlotModel
        {
            get => _plotModel;
            set
            {
                _plotModel = value;
                OnPropertyChanged(nameof(PlotModel));
            }
        }
        public int SliderValue
        {
            get => _sliderValue;
            set
            {
                _sliderValue = value;
                OnPropertyChanged(nameof(SliderValue));
                if (_filePaths != null && _sliderValue >= 0 && _sliderValue < _filePaths.Length)
                {
                    PlotSignal(LoadDataFromFile(_filePaths[_sliderValue]), _sliderValue);
                }
            }
        }
        public string SwapPhase
        {
            get => swapPhase;
            set
            {
                swapPhase = value;
                OnPropertyChanged(nameof(SwapPhase));
            }
        }

        public MainViewModel()
        {
            SetupPlot();
            Task.Run(()=>DeterminePhaseAsync());
            string directoryPath = @"D:\WORK\signals";
            _filePaths = Directory.GetFiles(directoryPath, "*.txt");

            PlotSignal(LoadDataFromFile(_filePaths[_sliderValue]), _sliderValue);
            SpectrCommand = new RelayCommand(async _ => await SpectrBilding());
        }
        public async Task SpectrBilding()
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
            await Task.Delay(10);
        }
        public double[] SignalAnalyse(double first)
        {
            string directoryPath = @"D:\WORK\signals";
            string[] filePaths = Directory.GetFiles(directoryPath, "*.txt");
            double[] maxValue = new double[filePaths.Length];

            for (int signalIndex = 0; signalIndex < filePaths.Length; signalIndex++)
            {
                var values = LoadDataFromFile(filePaths[signalIndex]);

                //нахождение максимума
                double max = 0.0;

                for (int value = 510; value < 850; value++)
                {
                    if (Math.Abs(values[value]) > max) max = values[value];
                }
                maxValue[signalIndex] = max - first;
            }
            return maxValue;
        }
        // Метод для определения фазы сигнала
        private async Task DeterminePhaseAsync()
        {
            const int startIndex = 510;
            const int endIndex = 850;
            const double strob_A = -0.01;
            const double  strob_B = 0.01;
            int phase = 0;

            string directoryPath = @"D:\WORK\signals";
            string[] filePaths = Directory.GetFiles(directoryPath, "*.txt");

            for (int signalIndex = 1200; signalIndex < filePaths.Length; signalIndex++)
            {
                var values = LoadDataFromFile(filePaths[signalIndex]);
                int lowerCrossings = await CountCrossings(values, strob_A, startIndex, endIndex);
                int upperCrossings = await CountCrossings(values, strob_B, startIndex, endIndex);

                int signalPhase = 0;
                //два пика по верхнему стробу
                if (upperCrossings == 2 && lowerCrossings >= 1)
                {
                    signalPhase = 1;
                }
                //два пика по нижнему стробу
                if (lowerCrossings == 2 && upperCrossings >= 1)
                {
                    signalPhase = 2;
                }

                if (phase != signalPhase)
                {
                    SwapPhase+=signalIndex + "\n";
                }
                phase = signalPhase;
            }
        }
        private static Task<int> CountCrossings(List<double> signal, double level, int startIndex, int endIndex)
        {
            int crossings = 0;
            for (int i = startIndex; i < endIndex && i < signal.Count - 1; i++)
            {
                if ((signal[i] - level) * (signal[i + 1] - level) < 0)
                {
                    crossings++;
                }
            }
            return Task.FromResult(crossings);
        }
        private void PlotSignal(List<double> values, int signalIndex)
        {
            PlotModel.Series.Clear();
            LineSeries lineSeries = new LineSeries
            {
                Title = $"Сигнал {signalIndex + 1}",
                StrokeThickness = 1
            };

            double totalTime = 94.09;
            double timeStep = totalTime / (values.Count - 1);

            for (int i = 0; i < values.Count; i++)
            {
                double time = i * timeStep;
                lineSeries.Points.Add(new DataPoint(time, values[i]));
            }

            PlotModel.Series.Add(lineSeries);
            PlotModel.InvalidatePlot(true);
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
        private void SetupPlot()
        {
            PlotModel = new PlotModel
            {
                Title = "Анализ дефектоскопии",
                DefaultColors = new List<OxyColor> { OxyColors.Blue },
                IsLegendVisible = true
            };

            PlotModel.Legends.Add(new OxyPlot.Legends.Legend
            {
                LegendPosition = OxyPlot.Legends.LegendPosition.RightTop,
                LegendPlacement = OxyPlot.Legends.LegendPlacement.Outside,
                LegendOrientation = OxyPlot.Legends.LegendOrientation.Vertical
            });

            PlotModel.Axes.Add(new OxyPlot.Axes.LinearAxis
            {
                Position = OxyPlot.Axes.AxisPosition.Left,
                Title = "дБ",
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot
            });

            PlotModel.Axes.Add(new OxyPlot.Axes.LinearAxis
            {
                Position = OxyPlot.Axes.AxisPosition.Bottom,
                Title = "Время (с)"
            });
        }
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}