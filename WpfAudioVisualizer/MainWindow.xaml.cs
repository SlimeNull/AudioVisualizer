using CommunityToolkit.Mvvm.Input;
using LibAudioVisualizer;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace WpfAudioVisualizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            DataContext = this;

            InitializeComponent();
        }

        [RelayCommand]
        public void Toggle()
        {
            visualizerControl.RenderEnabled ^= true;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            visualizerControl.RenderEnabled = false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            visualizerControl.RenderEnabled = true;
        }
    }
}