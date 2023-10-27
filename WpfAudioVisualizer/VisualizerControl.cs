using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using LibAudioVisualizer;
using LibAudioVisualizer.Utilities;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace WpfAudioVisualizer
{
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:WpfAudioVisualizer"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:WpfAudioVisualizer;assembly=WpfAudioVisualizer"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Browse to and select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:VisualizerControl/>
    ///
    /// </summary>
    public class VisualizerControl : Control
    {
        static VisualizerControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VisualizerControl), new FrameworkPropertyMetadata(typeof(VisualizerControl)));
        }

        WasapiCapture capture;
        Visualizer visualizer;

        static readonly Color[] allColors =
            ColorUtils.GetAllHsvColors();




        public int RenderInterval
        {
            get { return (int)GetValue(RenderIntervalProperty); }
            set { SetValue(RenderIntervalProperty, value); }
        }



        public int RenderMultiple
        {
            get { return (int)GetValue(RenderMultipleProperty); }
            set { SetValue(RenderMultipleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RenderMultiple.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RenderMultipleProperty =
            DependencyProperty.Register(nameof(RenderMultiple), typeof(int), typeof(VisualizerControl), new PropertyMetadata(10));



        public IEnumerable<double> SpectrumData
        {
            get { return (IEnumerable<double>)GetValue(SpectrumDataProperty); }
            set { SetValue(SpectrumDataProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RenderInterval.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RenderIntervalProperty =
            DependencyProperty.Register(nameof(RenderInterval), typeof(int), typeof(VisualizerControl), new PropertyMetadata(10));

        // Using a DependencyProperty as the backing store for SpectrumData.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SpectrumDataProperty =
            DependencyProperty.Register(nameof(SpectrumData), typeof(IEnumerable<double>), typeof(VisualizerControl), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));



        public VisualizerControl()
        {
            capture = new WasapiLoopbackCapture();
            visualizer = new Visualizer(512);

            capture.WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(8192, 1);
            capture.DataAvailable += Capture_DataAvailable;
        }

        private void Capture_DataAvailable(object? sender, WaveInEventArgs e)
        {
            int len = e.BytesRecorded / 4;
            double[] result = new double[len];

            for (int i = 0; i < len; i++)
                result[i] = BitConverter.ToSingle(e.Buffer, i * 4);

            visualizer.PushSampleData(result);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (SpectrumData == null)
                return;

            int count =
                SpectrumData.Count();
            int end = count - 1;

            double thickness = ActualWidth / count * (1 - 0.1);

            Pen pinkPen = new Pen(Brushes.Pink, thickness);

            PathFigure figure = new PathFigure()
            {
                StartPoint = new Point()
                {
                    X = 0,
                    Y = ActualHeight * (1 - SpectrumData.ElementAt(0) * 10)
                }
            };

            PathGeometry pathGeometry = new PathGeometry();

            int i = 1;
            foreach (var value in SpectrumData.Skip(1))
            {
                double y = ActualHeight * (1 - value * 10);
                double x = ((double)i / end) * ActualWidth;

                pathGeometry.Figures.Add(new PathFigure()
                {
                    StartPoint = new Point(x, ActualHeight),
                    Segments =
                    {
                        new LineSegment()
                        {
                            Point = new Point(x, y)
                        }
                    }
                });

                i++;
            }

            drawingContext.DrawGeometry(null, pinkPen, pathGeometry);
        }


        private async Task RenderLoopAsync(CancellationToken token)
        {
            capture.StartRecording();

            while (true)
            {
                if (token.IsCancellationRequested)
                    break;

                SpectrumData =
                    visualizer.GetSpectrumData();
                InvalidateVisual();

                await Task.Delay(RenderInterval);
            }

            capture.StopRecording();
        }

        public bool IsRendering => renderTask != null && !renderTask.IsCompleted;




        Task? renderTask;
        CancellationTokenSource? cancellation;
        public Task BeginRenderAsync()
        {
            if (renderTask != null)
                throw new InvalidOperationException("Render is already started");

            cancellation = new CancellationTokenSource();
            renderTask = RenderLoopAsync(cancellation.Token);

            return Task.CompletedTask;
        }

        public async Task StopRenderAsync()
        {
            cancellation?.Cancel();

            if (renderTask != null)
                await renderTask;

            renderTask = null;
        }

        public void BeginRender() => BeginRenderAsync().Wait();
        public void StopRender() => StopRenderAsync().Wait();
    }
}
