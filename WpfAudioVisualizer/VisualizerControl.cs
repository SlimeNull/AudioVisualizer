using System;
using System.Buffers;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Media.Converters;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LibAudioVisualizer;
using LibAudioVisualizer.Utilities;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace WpfAudioVisualizer
{
    public class VisualizerControl : Control
    {
        static VisualizerControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VisualizerControl), new FrameworkPropertyMetadata(typeof(VisualizerControl)));
        }

        public VisualizerControl()
        {
            _capture = new WasapiLoopbackCapture();
            _visualizer = new Visualizer(512);
            _startTime = DateTime.Now;
            _doubleArrayPool = ArrayPool<double>.Create();
            _pointArrayPool = ArrayPool<Point>.Create();

            _capture.WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(8192, 1);
            _capture.DataAvailable += CaptureDataAvailable;
        }

        ~VisualizerControl()
        {
            if (_capture.CaptureState == CaptureState.Capturing)
                _capture.StopRecording();
        }

        WasapiCapture _capture;
        Visualizer _visualizer;
        DateTime _startTime;
        double[]? _spectrumData;
        ArrayPool<double> _doubleArrayPool;
        ArrayPool<Point> _pointArrayPool;

        static readonly Color[] allColors =
            ColorUtils.GetAllHsvColors();



        public int SpectrumSize
        {
            get { return (int)GetValue(SpectrumSizeProperty); }
            set { SetValue(SpectrumSizeProperty, value); }
        }

        public int SpectrumSampleRate
        {
            get { return (int)GetValue(SpectrumSampleRateProperty); }
            set { SetValue(SpectrumSampleRateProperty, value); }
        }

        public int SpectrumBlurry
        {
            get { return (int)GetValue(SpectrumBlurryProperty); }
            set { SetValue(SpectrumBlurryProperty, value); }
        }

        public double SpectrumFactor
        {
            get { return (double)GetValue(SpectrumFactorProperty); }
            set { SetValue(SpectrumFactorProperty, value); }
        }





        public bool IsRendering
        {
            get { return (bool)GetValue(IsRenderingProperty.DependencyProperty); }
            private set { SetValue(IsRenderingProperty, value); }
        }
        public bool RenderEnabled
        {
            get { return (bool)GetValue(EnableRenderingProperty); }
            set { SetValue(EnableRenderingProperty, value); }
        }

        public int RenderInterval
        {
            get { return (int)GetValue(RenderIntervalProperty); }
            set { SetValue(RenderIntervalProperty, value); }
        }

        public float ColorTransitionTime
        {
            get { return (float)GetValue(ColorTransitionTimeProperty); }
            set { SetValue(ColorTransitionTimeProperty, value); }
        }

        public float ColorGradientOffset
        {
            get { return (float)GetValue(ColorGradientOffsetProperty); }
            set { SetValue(ColorGradientOffsetProperty, value); }
        }

        public int StripCount
        {
            get { return (int)GetValue(StripCountProperty); }
            set { SetValue(StripCountProperty, value); }
        }

        public float StripSpacing
        {
            get { return (float)GetValue(StripSpacingProperty); }
            set { SetValue(StripSpacingProperty, value); }
        }

        public int CircleStripCount
        {
            get { return (int)GetValue(CircleStripCountProperty); }
            set { SetValue(CircleStripCountProperty, value); }
        }

        public float CircleStripSpacing
        {
            get { return (float)GetValue(CircleStripSpacingProperty); }
            set { SetValue(CircleStripSpacingProperty, value); }
        }

        public double CircleStripRotationSpeed
        {
            get { return (double)GetValue(CircleStripRotationSpeedProperty); }
            set { SetValue(CircleStripRotationSpeedProperty, value); }
        }


        public bool EnableCurveRendering
        {
            get { return (bool)GetValue(EnableCurveProperty); }
            set { SetValue(EnableCurveProperty, value); }
        }

        public bool EnableStripsRendering
        {
            get { return (bool)GetValue(EnableStripsProperty); }
            set { SetValue(EnableStripsProperty, value); }
        }

        public bool EnableBorderRendering
        {
            get { return (bool)GetValue(EnableBorderDrawingProperty); }
            set { SetValue(EnableBorderDrawingProperty, value); }
        }

        public bool EnableCircleStripsRendering
        {
            get { return (bool)GetValue(EnableCircleStripsRenderingProperty); }
            set { SetValue(EnableCircleStripsRenderingProperty, value); }
        }

        public static readonly DependencyProperty EnableCurveProperty =
            DependencyProperty.Register(nameof(EnableCurveRendering), typeof(bool), typeof(VisualizerControl), new PropertyMetadata(true));
        public static readonly DependencyProperty EnableStripsProperty =
            DependencyProperty.Register(nameof(EnableStripsRendering), typeof(bool), typeof(VisualizerControl), new PropertyMetadata(true));
        public static readonly DependencyProperty EnableBorderDrawingProperty =
            DependencyProperty.Register(nameof(EnableBorderRendering), typeof(bool), typeof(VisualizerControl), new PropertyMetadata(true));
        public static readonly DependencyProperty EnableCircleStripsRenderingProperty =
            DependencyProperty.Register(nameof(EnableCircleStripsRendering), typeof(bool), typeof(VisualizerControl), new PropertyMetadata(true));





        public static readonly DependencyProperty SpectrumSizeProperty =
            DependencyProperty.Register(nameof(SpectrumSize), typeof(int), typeof(VisualizerControl), new PropertyMetadata(512, SpectrumSizeChanged));
        public static readonly DependencyProperty SpectrumSampleRateProperty =
            DependencyProperty.Register(nameof(SpectrumSampleRate), typeof(int), typeof(VisualizerControl), new PropertyMetadata(8192, SpectrumSampleRateChanged));
        public static readonly DependencyProperty SpectrumBlurryProperty =
            DependencyProperty.Register(nameof(SpectrumBlurry), typeof(int), typeof(VisualizerControl), new PropertyMetadata(0));
        public static readonly DependencyProperty SpectrumFactorProperty =
            DependencyProperty.Register(nameof(SpectrumFactor), typeof(double), typeof(VisualizerControl), new PropertyMetadata(1.0));
        public static readonly DependencyPropertyKey IsRenderingProperty =
            DependencyProperty.RegisterReadOnly(nameof(IsRendering), typeof(bool), typeof(VisualizerControl), new PropertyMetadata(false));
        public static readonly DependencyProperty EnableRenderingProperty =
            DependencyProperty.Register(nameof(RenderEnabled), typeof(bool), typeof(VisualizerControl), new PropertyMetadata(false, RenderEnableChanged));
        public static readonly DependencyProperty RenderIntervalProperty =
            DependencyProperty.Register(nameof(RenderInterval), typeof(int), typeof(VisualizerControl), new PropertyMetadata(10));
        public static readonly DependencyProperty ColorTransitionTimeProperty =
            DependencyProperty.Register(nameof(ColorTransitionTime), typeof(float), typeof(VisualizerControl), new PropertyMetadata(30f));
        public static readonly DependencyProperty ColorGradientOffsetProperty =
            DependencyProperty.Register(nameof(ColorGradientOffset), typeof(float), typeof(VisualizerControl), new PropertyMetadata(.1f));



        public static readonly DependencyProperty StripCountProperty =
            DependencyProperty.Register(nameof(StripCount), typeof(int), typeof(VisualizerControl), new PropertyMetadata(128));
        public static readonly DependencyProperty StripSpacingProperty =
            DependencyProperty.Register(nameof(StripSpacing), typeof(float), typeof(VisualizerControl), new PropertyMetadata(.2f));
        public static readonly DependencyProperty CircleStripCountProperty =
            DependencyProperty.Register(nameof(CircleStripCount), typeof(int), typeof(VisualizerControl), new PropertyMetadata(128));
        public static readonly DependencyProperty CircleStripSpacingProperty =
            DependencyProperty.Register(nameof(CircleStripSpacing), typeof(float), typeof(VisualizerControl), new PropertyMetadata(.2f));
        public static readonly DependencyProperty CircleStripRotationSpeedProperty =
            DependencyProperty.Register(nameof(CircleStripRotationSpeed), typeof (double), typeof(VisualizerControl), new PropertyMetadata(.5));



        private static void SpectrumSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not VisualizerControl visualizerControl ||
                e.NewValue is not int spectrumSize)
                return;

            if (visualizerControl.IsRendering)
                throw new InvalidOperationException($"{nameof(SpectrumSize)} on only be set while not rendering");

            visualizerControl._visualizer.Size = spectrumSize * 2;
        }

        private static void SpectrumSampleRateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not VisualizerControl visualizerControl ||
                e.NewValue is not int spectrumSampleRate)
                return;

            if (visualizerControl.IsRendering)
                throw new InvalidOperationException($"{nameof(SpectrumSampleRate)} on only be set while not rendering");

            visualizerControl._capture.WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(spectrumSampleRate, 1);
        }

        private static void RenderEnableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not VisualizerControl visualizerControl ||
                e.NewValue is not bool value)
                return;
#if DEBUG
            if (DesignerProperties.GetIsInDesignMode(d))
                return;
#endif

            if (value)
                visualizerControl.StartRenderAsync();
            else
                visualizerControl.StopRendering();
        }

        private void CaptureDataAvailable(object? sender, WaveInEventArgs e)
        {
            int len = e.BytesRecorded / 4;
            double[] result = _doubleArrayPool.Rent(len);

            for (int i = 0; i < len; i++)
                result[i] = BitConverter.ToSingle(e.Buffer, i * 4);

            _visualizer.PushSampleData(result, len);

            _doubleArrayPool.Return(result);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (_spectrumData == null)
                return;

            TimeSpan elapsedTime = DateTime.Now - _startTime;

            if (EnableCurveRendering)
                DrawCurve(drawingContext, _visualizer.SampleData);

            if (EnableStripsRendering)
                DrawStrips(drawingContext, _spectrumData);

            if (EnableBorderRendering)
                DrawBorder(drawingContext, _spectrumData);

            if (EnableCircleStripsRendering)
                DrawCircleStrips(drawingContext, _spectrumData, elapsedTime.TotalSeconds);
        }

        private void DrawStrips(DrawingContext drawingContext, double[] spectrumData)
        {
            int stripCount = StripCount;
            double thickness = ActualWidth / StripCount * (1 - StripSpacing);

            if (thickness < 0)
                thickness = 1;

            double minY = ActualHeight;

            PathGeometry pathGeometry = new PathGeometry();

            int end = stripCount - 1;
            for (int i = 0; i < stripCount; i++)
            {
                double value = spectrumData[i * spectrumData.Length / stripCount];
                double y = ActualHeight * (1 - value * 10);
                double x = ((double)i / end) * ActualWidth;

                if (y < minY)
                    minY = y;

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
            }

            GetCurrentColor(out var color1, out var color2);

            GradientBrush brush = new LinearGradientBrush(color1, color2, new Point(0, 1), new Point(0, 0));
            Pen pen = new Pen(brush, thickness);

            drawingContext.DrawGeometry(null, pen, pathGeometry);
        }

        private void DrawBorder(DrawingContext drawingContext, double[] spectrumData)
        {
            double[] bassArea = Visualizer.TakeSpectrumOfFrequency(spectrumData, _capture.WaveFormat.SampleRate, 250);
            double bass = bassArea.Average() * 100;
            double thickness = ActualWidth / 10 * bass;

            if (thickness <= 0)
                return;

            GetCurrentColor(out var color1, out var color2);
            color2.A = 0;

            LinearGradientBrush brush1 = new LinearGradientBrush(color1, color2, new Point(0, 0), new Point(0, 1));
            drawingContext.DrawRectangle(brush1, null, new Rect(new Point(0, 0), new Size(ActualWidth, thickness)));

            LinearGradientBrush brush2 = new LinearGradientBrush(color1, color2, new Point(0, 0), new Point(1, 0));
            drawingContext.DrawRectangle(brush2, null, new Rect(new Point(0, 0), new Size(thickness, ActualHeight)));

            LinearGradientBrush brush3 = new LinearGradientBrush(color1, color2, new Point(1, 0), new Point(0, 0));
            drawingContext.DrawRectangle(brush3, null, new Rect(new Point(ActualWidth - thickness, 0), new Size(thickness, ActualHeight)));

            LinearGradientBrush brush4 = new LinearGradientBrush(color1, color2, new Point(0, 1), new Point(0, 0));
            drawingContext.DrawRectangle(brush4, null, new Rect(new Point(0, ActualHeight - thickness), new Size(ActualWidth, thickness)));
        }

        private void DrawCircleStrips(DrawingContext drawingContext, double[] spectrumData, double time)
        {
            double[] bassArea = Visualizer.TakeSpectrumOfFrequency(spectrumData, _capture.WaveFormat.SampleRate, 250);
            double bassScale = bassArea.Average() * 100;
            double extraScale = Math.Min(ActualWidth, ActualHeight) / 6;

            int stripCount = CircleStripCount;
            double xOffset = ActualWidth / 2;
            double yOffset = ActualHeight / 2;
            double radius = Math.Min(ActualWidth, ActualHeight) / 4 + extraScale * bassScale;
            double spacing = CircleStripSpacing;
            double rotation = CircleStripRotationSpeed * time % (Math.PI * 2);
            double scale = ActualWidth / 6 * 10;

            double rotationAngle = Math.PI / 180 * rotation;
            double blockWidth = Math.PI * 2 / stripCount;
            double stripWidth = blockWidth * (1 - spacing);
            Point[] points = _pointArrayPool.Rent(stripCount);

            for (int i = 0; i < stripCount; i++)
            {
                double x = blockWidth * i + rotationAngle;
                double y = spectrumData[i * spectrumData.Length / stripCount] * scale;
                points[i] = new Point(x, y);
            }

            double maxHeight = points.Max(v => v.Y);
            double outerRadius = radius + maxHeight;

            double gradientStart = radius / outerRadius;

            GetCurrentColor(out var color1, out var color2);
            RadialGradientBrush brush = new RadialGradientBrush(
                new GradientStopCollection()
                {
                    new GradientStop(color1, 0),
                    new GradientStop(color1, gradientStart),
                    new GradientStop(color2, 1),
                });

            PathGeometry pathGeometry = new PathGeometry();

            for (int i = 0; i < stripCount; i++)
            {
                Point p = points[i];
                double cosStart = Math.Cos(p.X);
                double sinStart = Math.Sin(p.X);
                double cosEnd = Math.Cos(p.X + stripWidth);
                double sinEnd = Math.Sin(p.X + stripWidth);

                Point
                    p0 = new Point(cosStart * radius + xOffset, sinStart * radius + yOffset),
                    p1 = new Point(cosEnd * radius + xOffset, sinEnd * radius + yOffset),
                    p2 = new Point(cosEnd * (radius + p.Y) + xOffset, sinEnd * (radius + p.Y) + yOffset),
                    p3 = new Point(cosStart * (radius + p.Y) + xOffset, sinStart * (radius + p.Y) + yOffset);

                pathGeometry.Figures.Add(
                    new PathFigure()
                    {
                        StartPoint = p0,
                        Segments =
                        {
                            new LineSegment() { Point = p1 },
                            new LineSegment() { Point = p2 },
                            new LineSegment() { Point = p3 },
                        },
                    });
            }

            _pointArrayPool.Return(points);
            drawingContext.DrawGeometry(brush, null, pathGeometry);
        }

        private void DrawCurve(DrawingContext drawingContext, double[] spectrumData)
        {
            double yBase = ActualHeight / 2;
            double scale = Math.Min(ActualHeight / 10, 100);
            double drawingWidth = ActualWidth;

            Point[] points = new Point[spectrumData.Length];
            for (int i = 0; i < points.Length; i++)
            {
                double x = i * drawingWidth / points.Length;
                double y = spectrumData[i] * scale + yBase;
                points[i] = new Point(x, y);
            }

            PathFigure figure = new PathFigure();
            figure.StartPoint = points[0];
            for (int i = 0; i < points.Length; i++)
                figure.Segments.Add(new LineSegment() { Point = points[i] });

            PathGeometry pathGeometry = new PathGeometry()
            {
                Figures =
                {
                    figure
                }
            };

            drawingContext.DrawGeometry(null, new Pen(Brushes.Cyan, 1), pathGeometry);
        }


        private async Task RenderLoopAsync(CancellationToken token)
        {
            IsRendering = true;
            _capture.StartRecording();

            while (true)
            {
                if (token.IsCancellationRequested)
                    break;

                _spectrumData = _visualizer.GetSpectrumData();

                if (SpectrumBlurry is int blurry and not 0)
                    _spectrumData = Visualizer.GetBlurry(_spectrumData, blurry);

                if (SpectrumFactor is double factor and not 1.0)
                    for (int i = 0; i < _spectrumData.Length; i++)
                        _spectrumData[i] *= factor;

                InvalidateVisual();

                await Task.Delay(RenderInterval);
            }

            _capture.StopRecording();
            IsRendering = false;
        }

        Task? renderTask;
        CancellationTokenSource? cancellation;

        private void StartRenderAsync()
        {
            if (renderTask != null)
                return;

            cancellation = new CancellationTokenSource();
            renderTask = RenderLoopAsync(cancellation.Token);
        }

        private void StopRendering()
        {
            cancellation?.Cancel();
            renderTask = null;
        }

        private Color GetColorFromRate(double rate)
        {
            if (rate < 0)
                rate = rate % 1 + 1;
            else
                rate = rate % 1;

            int maxIndex = allColors.Length - 1;
            return allColors[(int)(maxIndex * rate)];
        }

        private void GetCurrentColor(out Color color1, out Color color2)
        {
            double time = (DateTime.Now - _startTime).TotalSeconds;
            double rate = time / ColorTransitionTime;

            color1 = GetColorFromRate(rate);
            color2 = GetColorFromRate(rate + ColorGradientOffset);
        }
    }
}
