using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using LibAudioVisualizer;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace WpfAudioVisualizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        WasapiCapture capture; // 音频捕获
        Visualizer visualizer; // 可视化
        Timer? dataTimer;
        Timer? drawingTimer;

        double[]? spectrumData; // 频谱数据

        Color[] allColors; // 渐变颜色

        public MainWindow()
        {
            capture = new WasapiLoopbackCapture(); // 捕获电脑发出的声音
            visualizer = new Visualizer(256); // 新建一个可视化器, 并使用 256 个采样进行傅里叶变换

            allColors = GetAllHsvColors(); // 获取所有的渐变颜色 (HSV 颜色)

            capture.WaveFormat =
                WaveFormat.CreateIeeeFloatWaveFormat(8192, 1); // 指定捕获的格式, 单声道, 32位深度, IeeeFloat 编码, 8192采样率
            capture.DataAvailable += Capture_DataAvailable; // 订阅事件

            InitializeComponent();
        }

        /// <summary>
        /// 简单的数据模糊
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="radius">模糊半径</param>
        /// <returns>结果</returns>
        private double[] MakeSmooth(double[] data, int radius)
        {
            double[] GetWeights(int radius)
            {
                double Gaussian(double x) => Math.Pow(Math.E, (-4 * x * x)); // 憨批高斯函数

                int len = 1 + radius * 2; // 长度
                int end = len - 1; // 最后的索引
                double radiusF = (double)radius; // 半径浮点数
                double[] weights = new double[len]; // 权重

                for (int i = 0; i <= radius; i++) // 先把右边的权重算出来
                    weights[radius + i] = Gaussian(i / radiusF);
                for (int i = 0; i < radius; i++) // 把右边的权重拷贝到左边
                    weights[i] = weights[end - i];

                double total = weights.Sum();
                for (int i = 0; i < len; i++) // 使权重合为 0
                    weights[i] = weights[i] / total;

                return weights;
            }

            void ApplyWeights(double[] buffer, double[] weights)
            {
                int len = buffer.Length;
                for (int i = 0; i < len; i++)
                    buffer[i] = buffer[i] * weights[i];
            }


            double[] weights = GetWeights(radius);
            double[] buffer = new double[1 + radius * 2];

            double[] result = new double[data.Length];
            if (data.Length < radius)
            {
                Array.Fill(result, data.Average());
                return result;
            }


            for (int i = 0; i < radius; i++)
            {
                Array.Fill(buffer, data[i], 0, radius + 1); // 填充缺省
                for (int j = 0; j < radius; j++) // 
                {
                    buffer[radius + 1 + j] = data[i + j];
                }

                ApplyWeights(buffer, weights);
                result[i] = buffer.Sum();
            }

            for (int i = radius; i < data.Length - radius; i++)
            {
                for (int j = 0; j < radius; j++) // 
                {
                    buffer[j] = data[i - j];
                }

                buffer[radius] = data[i];

                for (int j = 0; j < radius; j++) // 
                {
                    buffer[radius + j + 1] = data[i + j];
                }

                ApplyWeights(buffer, weights);
                result[i] = buffer.Sum();
            }

            for (int i = data.Length - radius; i < data.Length; i++)
            {
                Array.Fill(buffer, data[i], 0, radius + 1); // 填充缺省
                for (int j = 0; j < radius; j++) // 
                {
                    buffer[radius + 1 + j] = data[i - j];
                }

                ApplyWeights(buffer, weights);
                result[i] = buffer.Sum();
            }

            return result;
        }

        /// <summary>
        /// 获取 HSV 中所有的基础颜色 (饱和度和明度均为最大值)
        /// </summary>
        /// <returns>所有的 HSV 基础颜色(共 256 * 6 个, 并且随着索引增加, 颜色也会渐变)</returns>
        private Color[] GetAllHsvColors()
        {
            Color[] result = new Color[256 * 6];

            for (int i = 0; i <= 255; i++)
            {
                result[i] = Color.FromArgb(255, 255, (byte)i, 0);
            }

            for (int i = 0; i <= 255; i++)
            {
                result[256 + i] = Color.FromArgb(255, (byte)(255 - i), 255, 0);
            }

            for (int i = 0; i <= 255; i++)
            {
                result[512 + i] = Color.FromArgb(255, 0, 255, (byte)i);
            }

            for (int i = 0; i <= 255; i++)
            {
                result[768 + i] = Color.FromArgb(255, 0, (byte)(255 - i), 255);
            }

            for (int i = 0; i <= 255; i++)
            {
                result[1024 + i] = Color.FromArgb(255, (byte)i, 0, 255);
            }

            for (int i = 0; i <= 255; i++)
            {
                result[1280 + i] = Color.FromArgb(255, 255, 0, (byte)(255 - i));
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Capture_DataAvailable(object? sender, WaveInEventArgs e)
        {
            int length = e.BytesRecorded / 4; // 采样的数量 (每一个采样是 4 字节)
            double[] result = new double[length]; // 声明结果

            for (int i = 0; i < length; i++)
                result[i] = BitConverter.ToSingle(e.Buffer, i * 4); // 取出采样值

            visualizer.PushSampleData(result); // 将新的采样存储到 可视化器 中
        }

        /// <summary>
        /// 用来刷新频谱数据以及实现频谱数据缓动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataTimer_Tick(object? state)
        {
            double[] newSpectrumData = visualizer.GetSpectrumData(); // 从可视化器中获取频谱数据
            newSpectrumData = MakeSmooth(newSpectrumData, 2); // 平滑频谱数据

            if (spectrumData == null) // 如果已经存储的频谱数据为空, 则把新的频谱数据直接赋值上去
            {
                spectrumData = newSpectrumData;
                return;
            }

            for (int i = 0; i < newSpectrumData.Length; i++) // 计算旧频谱数据和新频谱数据之间的 "中间值"
            {
                double oldData = spectrumData[i];
                double newData = newSpectrumData[i];
                double lerpData =
                    oldData + (newData - oldData) * .2f; // 每一次执行, 频谱值会向目标值移动 20% (如果太大, 缓动效果不明显, 如果太小, 频谱会有延迟的感觉)
                spectrumData[i] = lerpData;
            }
        }

        /// <summary>
        /// 绘制一个渐变的 波浪
        /// </summary>
        /// <param name="g">绘图目标</param>
        /// <param name="down">下方颜色</param>
        /// <param name="up">上方颜色</param>
        /// <param name="spectrumData">频谱数据</param>
        /// <param name="pointCount">波浪中, 点的数量</param>
        /// <param name="drawingWidth">波浪的宽度</param>
        /// <param name="xOffset">波浪的起始X坐标</param>
        /// <param name="yOffset">波浪的其实Y坐标</param>
        /// <param name="scale">频谱的缩放(使用负值可以翻转波浪)</param>
        private void DrawGradient(Path g, Color down, Color up, double[] spectrumData, int pointCount,
            double drawingWidth, double xOffset, double yOffset, double scale)
        {
            Point[] points = new Point[pointCount + 2];
            for (int i = 0; i < pointCount; i++)
            {
                double x = i * drawingWidth / pointCount + xOffset;
                double y = spectrumData[i * spectrumData.Length / pointCount] * scale + yOffset;
                points[i + 1] = new Point(x, y);
            }

            points[0] = new Point(xOffset, yOffset);
            points[points.Length - 1] = new Point(xOffset + drawingWidth, yOffset);

            double upP = points.Min(v => v.Y);

            if (Math.Abs(upP - yOffset) < 1)
                return;

            g.Data = new PathGeometry()
            {
                Figures = { new PathFigure() { IsFilled = true, Segments = { new PolyLineSegment(points, false) } } }
            };
            g.Fill = new LinearGradientBrush(down, up, new Point(0, yOffset), new Point(0, upP));
        }

        /// <summary>
        /// 绘制渐变的条形
        /// </summary>
        /// <param name="g">绘图目标</param>
        /// <param name="down">下方颜色</param>
        /// <param name="up">上方颜色</param>
        /// <param name="spectrumData">频谱数据</param>
        /// <param name="stripCount">条形的数量</param>
        /// <param name="drawingWidth">绘图的宽度</param>
        /// <param name="xOffset">绘图的起始 X 坐标</param>
        /// <param name="yOffset">绘图的起始 Y 坐标</param>
        /// <param name="spacing">条形与条形之间的间隔(像素)</param>
        /// <param name="scale"></param>
        private void DrawGradientStrips(Path g, Color down, Color up, double[] spectrumData, int stripCount,
            double drawingWidth, double xOffset, double yOffset, double spacing, double scale)
        {
            double stripWidth = (drawingWidth - spacing * stripCount) / stripCount;
            Point[] points = new Point[stripCount];

            for (int i = 0; i < stripCount; i++)
            {
                double x = stripWidth * i + spacing * i + xOffset;
                double y = spectrumData[i * spectrumData.Length / stripCount] * scale; // height
                points[i] = new Point(x, y);
            }


            GeometryGroup geo = new GeometryGroup();
            for (int i = 0; i < stripCount; i++)
            {
                Point p = points[i];
                double y = yOffset;
                double height = p.Y;

                if (height < 0)
                {
                    y += height;
                    height = -height;
                }

                Point[] endPoints = new Point[]
                {
                    new Point(p.X, p.Y + yOffset),
                    new Point(p.X, p.Y + height + yOffset),
                    new Point(p.X + stripWidth, p.Y + height + yOffset),
                    new Point(p.X + stripWidth, p.Y + yOffset)
                };

                PathFigure fig = new PathFigure();

                fig.StartPoint = endPoints[0];
                fig.Segments.Add(new PolyLineSegment(endPoints, false));
                //fig.IsClosed = true;

                geo.Children.Add(new PathGeometry { Figures = { fig } });
            }

            g.Data = geo;

            Brush brush = new LinearGradientBrush(
                down, up, new Point(0, 0), new Point(0, 1)
            );
            g.Fill = brush;
        }

        /// <summary>
        /// 画曲线
        /// </summary>
        /// <param name="g"></param>
        /// <param name="brush"></param>
        /// <param name="spectrumData"></param>
        /// <param name="pointCount"></param>
        /// <param name="drawingWidth"></param>
        /// <param name="xOffset"></param>
        /// <param name="yOffset"></param>
        /// <param name="scale"></param>
        private void DrawCurve(Path g, Brush brush, double[] spectrumData, int pointCount, double drawingWidth,
            double xOffset, double yOffset, double scale)
        {
            Point[] points = new Point[pointCount];
            for (int i = 0; i < pointCount; i++)
            {
                double x = i * drawingWidth / pointCount + xOffset;
                double y = spectrumData[i * spectrumData.Length / pointCount] * scale + yOffset;
                points[i] = new Point(x, y);
            }

            PathFigure fig = new PathFigure
            {
                StartPoint = points[0]
            };
            fig.Segments.Add(new PolyLineSegment(points, true));

            g.Data = new PathGeometry { Figures = { fig } };
            g.StrokeThickness = 2;
            g.Stroke = brush;
        }

        private void DrawCircleStrips(Path g, Brush brush, double[] spectrumData, int stripCount, double xOffset,
            double yOffset, double radius, double spacing, double rotation, double scale)
        {
            double rotationAngle = Math.PI / 180 * rotation;
            double blockWidth = MathF.PI * 2 / stripCount; // angle
            double stripWidth = blockWidth - MathF.PI / 180 * spacing; // angle
            Point[] points = new Point[stripCount];

            for (int i = 0; i < stripCount; i++)
            {
                double x = blockWidth * i + rotationAngle; // angle
                double y = spectrumData[i * spectrumData.Length / stripCount] * scale; // height
                points[i] = new Point(x, y);
            }

            PathGeometry geo = new PathGeometry();

            for (int i = 0; i < stripCount; i++)
            {
                Point p = points[i];
                double sinStart = Math.Sin(p.X);
                double sinEnd = Math.Sin(p.X + stripWidth);
                double cosStart = Math.Cos(p.X);
                double cosEnd = Math.Cos(p.X + stripWidth);

                Point[] polygon = new Point[]
                {
                    new Point((cosStart * radius + xOffset), (sinStart * radius + yOffset)),
                    new Point((cosEnd * radius + xOffset), (sinEnd * radius + yOffset)),
                    new Point((cosEnd * (radius + p.Y) + xOffset), (sinEnd * (radius + p.Y) + yOffset)),
                    new Point((cosStart * (radius + p.Y) + xOffset), (sinStart * (radius + p.Y) + yOffset)),
                };

                PathFigure fig = new PathFigure();

                fig.IsFilled = true;
                fig.Segments.Add(new PolyLineSegment(polygon, false));

                geo.Figures.Add(fig);
            }

            g.Data = geo;
            g.Fill = brush;
        }

        /// <summary>
        /// 画圆环条
        /// </summary>
        /// <param name="g"></param>
        /// <param name="inner"></param>
        /// <param name="outer"></param>
        /// <param name="spectrumData"></param>
        /// <param name="stripCount"></param>
        /// <param name="xOffset"></param>
        /// <param name="yOffset"></param>
        /// <param name="radius"></param>
        /// <param name="spacing"></param>
        /// <param name="scale"></param>
        private void DrawCircleGradientStrips(Path g, Color inner, Color outer, double[] spectrumData, int stripCount,
            double xOffset, double yOffset, double radius, double spacing, double rotation, double scale)
        {
            //旋转角度转弧度
            double rotationAngle = Math.PI / 180 * rotation;

            //等分圆周，每个（竖条+空白）对应的弧度
            double blockWidth = Math.PI * 2 / stripCount; // angle

            //每个竖条对应的弧度
            double stripWidth = blockWidth - MathF.PI / 180 * spacing; // angle

            Point[] points = new Point[stripCount];

            for (int i = 0; i < stripCount; i++)
            {
                double x = blockWidth * i + rotationAngle; // angle
                double y = spectrumData[i * spectrumData.Length / stripCount] * scale; // height
                points[i] = new Point(x, y);
            }

            PathGeometry geo = new PathGeometry();

            for (int i = 0; i < stripCount; i++)
            {
                Point p = points[i];
                double sinStart = Math.Sin(p.X);
                double sinEnd = Math.Sin(p.X + stripWidth);
                double cosStart = Math.Cos(p.X);
                double cosEnd = Math.Cos(p.X + stripWidth);

                Point[] polygon = new Point[]
                {
                    new Point((cosStart * radius + xOffset), (sinStart * radius + yOffset)),
                    new Point((cosEnd * radius + xOffset), (sinEnd * radius + yOffset)),
                    new Point((cosEnd * (radius + p.Y) + xOffset), (sinEnd * (radius + p.Y) + yOffset)),
                    new Point((cosStart * (radius + p.Y) + xOffset), (sinStart * (radius + p.Y) + yOffset))
                };

                PathFigure fig = new PathFigure
                {
                    StartPoint = polygon[0],
                    IsFilled = true
                };

                fig.Segments.Add(new PolyLineSegment(polygon, false));
                geo.Figures.Add(fig);
            }

            g.Data = geo;

            double maxHeight = points.Max(v => v.Y);
            LinearGradientBrush brush = new LinearGradientBrush(
                new GradientStopCollection()
                {
                    new GradientStop(Colors.Transparent, 0),
                    new GradientStop(inner, radius / (radius + maxHeight)),
                    new GradientStop(outer, 1)
                },
                new Point(xOffset, 0),
                new Point(xOffset, 1)
            );
            g.Fill = brush;
        }

        private void DrawStrips(Path g, Brush brush, double[] spectrumData, int stripCount, int drawingWidth,
            float xOffset, float yOffset, float spacing, double scale)
        {
            float stripWidth = (drawingWidth - spacing * stripCount) / stripCount;
            Point[] points = new Point[stripCount];

            for (int i = 0; i < stripCount; i++)
            {
                double x = stripWidth * i + spacing * i + xOffset;
                double y = spectrumData[i * spectrumData.Length / stripCount] * scale; // height
                points[i] = new Point(x, y);
            }

            PathGeometry geo = new PathGeometry();

            for (int i = 0; i < stripCount; i++)
            {
                Point p = points[i];
                double y = yOffset;
                double height = p.Y;

                if (height < 0)
                {
                    y += height;
                    height = -height;
                }

                Point[] endPoints = new Point[]
                {
                    new Point(p.X, y),
                    new Point(p.X, y + height),
                    new Point(p.X + stripWidth, y + height),
                    new Point(p.X + stripWidth, y)
                };

                PathFigure fig = new PathFigure();

                fig.IsFilled = true;
                fig.Segments.Add(new PolyLineSegment(endPoints, false));

                geo.Figures.Add(fig);
            }

            g.Data = geo;
            g.Fill = brush;
        }

        private void DrawGradientBorder(
            Rectangle upBorder, Rectangle downBorder, Rectangle leftBorder, Rectangle rightBorder,
            Color inner, Color outer, double scale, double width)
        {
            int thickness = (int)(width * scale);

            upBorder.Height = thickness;
            downBorder.Height = thickness;
            leftBorder.Width = thickness;
            rightBorder.Width = thickness;

            upBorder.Fill = new LinearGradientBrush(outer, inner, 90);
            downBorder.Fill = new LinearGradientBrush(inner, outer, 90);
            leftBorder.Fill = new LinearGradientBrush(outer, inner, 0);
            rightBorder.Fill = new LinearGradientBrush(inner, outer, 0);
        }

        int colorIndex = 0;
        double rotation = 0;
        DispatcherOperation? lastInvocation;

        private void DrawingTimer_Tick(object? state)
        {
            if (spectrumData == null)
                return;
            if (lastInvocation != null &&
                lastInvocation.Status == DispatcherOperationStatus.Executing)
                return;

            lastInvocation = Dispatcher.InvokeAsync(() =>
            {
                rotation += .1;
                colorIndex++;

                Color color1 = allColors[colorIndex % allColors.Length];
                Color color2 = allColors[(colorIndex + 200) % allColors.Length];

                //音波曲线
                Brush brush = new SolidColorBrush(color1);
                DrawCurve(
                    sampleWave, brush, visualizer.SampleData, visualizer.SampleData.Length,
                    drawingPanel.ActualWidth, 0, drawingPanel.ActualHeight / 2,
                    Math.Min(drawingPanel.ActualHeight / 10, 100)
                );

                //柱状音波频谱
                DrawGradientStrips(
                    strips, color1, color2,
                    spectrumData, spectrumData.Length,
                    strips.ActualWidth, 0, strips.ActualHeight,
                    3, -strips.ActualHeight * 10
                );

                //圆形音波频谱
                double[] bassArea = Visualizer.TakeSpectrumOfFrequency(
                    spectrumData, capture.WaveFormat.SampleRate, 250
                );
                double bassScale = bassArea.Average() * 100;
                double extraScale = Math.Min(drawingPanel.ActualHeight, drawingPanel.ActualHeight) / 6;
                DrawCircleGradientStrips(
                    circle, color1, color2, spectrumData, spectrumData.Length,
                    drawingPanel.ActualWidth / 2, drawingPanel.ActualHeight / 2,
                    Math.Min(drawingPanel.ActualWidth, drawingPanel.ActualHeight) / 4 + extraScale * bassScale,
                    1, rotation, drawingPanel.ActualHeight / 6 * 10
                );

                //四周边框
                DrawGradientBorder(
                    up, down, left, right, Color.FromArgb(0, color1.R, color1.G, color1.B), color2, bassScale,
                    drawingPanel.ActualWidth / 10
                );
            });
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            capture.StartRecording();

            dataTimer = new Timer(DataTimer_Tick, null, 30, 30);
            drawingTimer = new Timer(DrawingTimer_Tick, null, 30, 30);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }
    }
}