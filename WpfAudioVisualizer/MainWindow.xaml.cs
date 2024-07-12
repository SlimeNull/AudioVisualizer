using CommandLine;
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
using System.Windows.Shell;
using System.Windows.Threading;

namespace WpfAudioVisualizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(App.AppOptions o)
        {
            DataContext = this;

            InitializeComponent();
            ProcessCommandLine(o);
        }

        void ProcessCommandLine(App.AppOptions o)
        {
            if (o.FullScreen)
            {
                Topmost = true;
                WindowStyle = WindowStyle.None;
                ResizeMode = ResizeMode.NoResize;
                WindowState = WindowState.Maximized;
                Background = Brushes.Transparent;
                Opacity = o.Opacity;

                AllowsTransparency = true;
                IsHitTestVisible = false;
                //WindowChrome.SetWindowChrome(this, new WindowChrome()
                //{
                //    GlassFrameThickness = new Thickness(-1)
                //});


                visualizerControl.EnableCurveRendering = false;
                visualizerControl.EnableCircleStripsRendering = false;
            }


            if (o.DisableCurve)
                visualizerControl.EnableCurveRendering = false;
            if (o.DisableStrips)
                visualizerControl.EnableStripsRendering = false;
            if (o.DisableBorder)
                visualizerControl.EnableBorderRendering = false;
            if (o.DisableCircleStripsRendering)
                visualizerControl.EnableCircleStripsRendering = false;


            if (o.EnableCurve)
                visualizerControl.EnableCurveRendering = true;
            if (o.EnableStrips)
                visualizerControl.EnableStripsRendering = true;
            if (o.EnableBorder)
                visualizerControl.EnableBorderRendering = true;
            if (o.EnableCircleStripsRendering)
                visualizerControl.EnableCircleStripsRendering = true;

            if (o.SpectrumSize.HasValue)
                visualizerControl.SpectrumSize = o.SpectrumSize.Value;
            if (o.SpectrumSampleRate.HasValue)
                visualizerControl.SpectrumSampleRate = o.SpectrumSampleRate.Value;
            if (o.SpectrumBlurry.HasValue)
                visualizerControl.SpectrumBlurry = o.SpectrumBlurry.Value;
            if (o.SpectrumFactor.HasValue)
                visualizerControl.SpectrumFactor = o.SpectrumFactor.Value;

            if (o.StripCount.HasValue)
                visualizerControl.StripCount = o.StripCount.Value;
            if (o.StripSpacing.HasValue)
                visualizerControl.StripSpacing = o.StripSpacing.Value;
            if (o.CircleStripCount.HasValue)
                visualizerControl.CircleStripCount = o.CircleStripCount.Value;
            if (o.CircleStripSpacing.HasValue)
                visualizerControl.CircleStripSpacing = o.CircleStripSpacing.Value;
            if (o.CircleStripRotationSpeed.HasValue)
                visualizerControl.CircleStripRotationSpeed = o.CircleStripRotationSpeed.Value;
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