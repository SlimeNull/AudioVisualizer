using CommunityToolkit.Mvvm.Input;
using LibAudioVisualizer;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
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
            visualizerControl.EnableRendering ^= true;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            visualizerControl.EnableRendering = false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            visualizerControl.EnableRendering = true;
        }

        private void Window_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }
    }
}
