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
        private int _signalCount;
        private string swapPhase = "";
        private string directoryPath = @"C:\SavedSignals\Lud\defect";
        //private string directoryPath = @"D:\WORK\signals";

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
        public int SignalCount
        {
            get => _signalCount;
            set
            {
                _signalCount = value;
                OnPropertyChanged(nameof(SignalCount));
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
            _filePaths = Directory.GetFiles(directoryPath, "*.txt");
            _signalCount = _filePaths.Length;

            Task.Run(()=>DeterminePhaseAsync(_filePaths));
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
            //string[] filePaths = Directory.GetFiles(directoryPath, "*.txt");
            double[] maxValue = new double[_filePaths.Length];

            for (int signalIndex = 0; signalIndex < _filePaths.Length; signalIndex++)
            {
                var values = LoadDataFromFile(_filePaths[signalIndex]);

                //нахождение максимума
                double max = 0.0;

                for (int value = 3740; value < 4029; value++)
                {
                    if (Math.Abs(values[value]) > max) max = values[value];
                }
                maxValue[signalIndex] = max - first;
            }
            return maxValue;
        }
        // Метод для определения фазы сигнала
        private async Task DeterminePhaseAsync(string[] filePaths)
        {
            const int startIndex = 3740;//22mсs
            const int endIndex = 4029;//23.7mсs
            //const double strob_A = -0.15;//нижний порог
            //const double  strob_B = 0.15;//верхний порог
            string previousPhase = "";

            for (int signalIndex = 0; signalIndex < _filePaths.Length; signalIndex++)
            {
                var values = LoadDataFromFile(_filePaths[signalIndex]);
                var phaseCrossings = await CountCrossings(values, startIndex, endIndex);

                string currentPhase = phaseCrossings.Aggregate("", (current, crossing) =>
                    current + (crossing.IsStrobA ? "A" : "B"));

                // Проверяем смену фазы
                if (previousPhase != "" && currentPhase != previousPhase)
                {
                    SwapPhase += $"{signalIndex + 1}: {previousPhase} → {currentPhase} \n";
                }
                previousPhase = currentPhase;

                await Task.Delay(1);
            }
        }
        private static async Task<List<(bool IsStrobA, int Index, double Value)>> CountCrossings(List<double> signal, int startIndex, int endIndex)
        {
            var peaks = new List<(bool IsStrobA, int Index, double Value)>();

            for (int i = startIndex; i < endIndex; i++)
            {
                if (signal[i] < 0 && signal[i - 1] > signal[i] && signal[i] < signal[i + 1])
                {
                    peaks.Add((true, i, signal[i]));
                }
                else if (signal[i] > 0 && signal[i - 1] < signal[i] && signal[i] > signal[i + 1])
                {
                    peaks.Add((false, i, signal[i]));
                }
            }

            peaks.Sort((a, b) => Math.Abs(b.Value).CompareTo(Math.Abs(a.Value)));

            var selectedPeaks = new List<(bool IsStrobA, int Index, double Value)>();
            for (int i = 0; i < peaks.Count && selectedPeaks.Count < 3; i++)
            {
                if (selectedPeaks.Count == 0 || selectedPeaks.Last().IsStrobA != peaks[i].IsStrobA)
                {
                    selectedPeaks.Add(peaks[i]);
                }
            }

            selectedPeaks.Sort((a, b) => a.Index.CompareTo(b.Index));

            return await Task.FromResult(selectedPeaks);
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
                Title = "Время (мкс)"
            });
        }
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}