using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static System.Net.Mime.MediaTypeNames;

namespace C_ScanGradient
{
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
            MainViewModel mainVM = new MainViewModel();
            
            Grid grid = new Grid();
            grid.Children.Add(mainVM.SignalAnalyse());
            this.Content = grid;
        }
    }
}