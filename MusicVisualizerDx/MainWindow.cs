using LibMusicVisualizer;
using NAudio.CoreAudioApi;
using NAudio.Dsp;
using NAudio.Wave;
using SharpDX.Direct2D1;
using System;
using System.Numerics;
using System.Windows.Forms;

namespace MusicVisualizerDx
{
    public partial class MainWindow : Form
    {
        WasapiCapture capture;             // 音频捕获
        Visualizer visualizer;             // 可视化
        double[]? spectrumData;            // 频谱数据

        Color[] allColors;                 // 渐变颜色

        RenderTarget rt;

        public MainWindow()
        {
            capture = new WasapiLoopbackCapture();          // 捕获电脑发出的声音
            visualizer = new Visualizer(256);               // 新建一个可视化器, 并使用 256 个采样进行傅里叶变换

            allColors = GetAllHsvColors();                  // 获取所有的渐变颜色 (HSV 颜色)

            capture.WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(8192, 1);      // 指定捕获的格式, 单声道, 32位深度, IeeeFloat 编码, 8192采样率
            capture.DataAvailable += Capture_DataAvailable;                          // 订阅事件

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
                double Gaussian(double x) => Math.Pow(Math.E, (-4 * x * x));        // 憨批高斯函数

                int len = 1 + radius * 2;                         // 长度
                int end = len - 1;                                // 最后的索引
                double radiusF = (double)radius;                    // 半径浮点数
                double[] weights = new double[len];                 // 权重

                for (int i = 0; i <= radius; i++)                 // 先把右边的权重算出来
                    weights[radius + i] = Gaussian(i / radiusF);
                for (int i = 0; i < radius; i++)                  // 把右边的权重拷贝到左边
                    weights[i] = weights[end - i];

                double total = weights.Sum();
                for (int i = 0; i < len; i++)                  // 使权重合为 0
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
                Array.Fill(buffer, data[i], 0, radius + 1);      // 填充缺省
                for (int j = 0; j < radius; j++)                 // 
                {
                    buffer[radius + 1 + j] = data[i + j];
                }

                ApplyWeights(buffer, weights);
                result[i] = buffer.Sum();
            }

            for (int i = radius; i < data.Length - radius; i++)
            {
                for (int j = 0; j < radius; j++)                 // 
                {
                    buffer[j] = data[i - j];
                }

                buffer[radius] = data[i];

                for (int j = 0; j < radius; j++)                 // 
                {
                    buffer[radius + j + 1] = data[i + j];
                }

                ApplyWeights(buffer, weights);
                result[i] = buffer.Sum();
            }

            for (int i = data.Length - radius; i < data.Length; i++)
            {
                Array.Fill(buffer, data[i], 0, radius + 1);      // 填充缺省
                for (int j = 0; j < radius; j++)                 // 
                {
                    buffer[radius + 1 + j] = data[i - j];
                }

                ApplyWeights(buffer, weights);
                result[i] = buffer.Sum();
            }

            return result;
        }

        private double[] TakeSpectrumOfFrequency(double[] spectrum, double sampleRate, double frequency)
        {
            double frequencyPerSampe = sampleRate / spectrum.Length;

            int lengthInNeed = (int)(Math.Min(frequency / frequencyPerSampe, spectrum.Length));
            double[] result = new double[lengthInNeed];
            Array.Copy(spectrum, 0, result, 0, lengthInNeed);
            return result;
        }

        /// <summary>
        /// 获取 HSV 中所有的基础颜色 (饱和度和明度均为最大值)
        /// </summary>
        /// <returns>所有的 HSV 基础颜色(共 256 * 6 个, 并且随着索引增加, 颜色也会渐变)</returns>
        private Color[] GetAllHsvColors()
        {
            Color[] result = new Color[256 * 6];

            for (int i = 0; i < 256; i++)
            {
                result[i] = Color.FromArgb(255, i, 0);
            }

            for (int i = 0; i < 256; i++)
            {
                result[256 + i] = Color.FromArgb(255 - i, 255, 0);
            }

            for (int i = 0; i < 256; i++)
            {
                result[512 + i] = Color.FromArgb(0, 255, i);
            }

            for (int i = 0; i < 256; i++)
            {
                result[768 + i] = Color.FromArgb(0, 255 - i, 255);
            }

            for (int i = 0; i < 256; i++)
            {
                result[1024 + i] = Color.FromArgb(i, 0, 255);
            }

            for (int i = 0; i < 256; i++)
            {
                result[1280 + i] = Color.FromArgb(255, 0, 255 - i);
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
            int length = e.BytesRecorded / 4;           // 采样的数量 (每一个采样是 4 字节)
            double[] result = new double[length];       // 声明结果

            for (int i = 0; i < length; i++)
                result[i] = BitConverter.ToSingle(e.Buffer, i * 4);      // 取出采样值

            visualizer.PushSampleData(result);          // 将新的采样存储到 可视化器 中
        }

        /// <summary>
        /// 用来刷新频谱数据以及实现频谱数据缓动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataTimer_Tick(object? sender, EventArgs e)
        {
            double[] newSpectrumData = visualizer.GetSpectrumData();         // 从可视化器中获取频谱数据
            newSpectrumData = MakeSmooth(newSpectrumData, 2);                // 平滑频谱数据

            if (spectrumData == null)                                        // 如果已经存储的频谱数据为空, 则把新的频谱数据直接赋值上去
            {
                spectrumData = newSpectrumData;
                return;
            }

            for (int i = 0; i < newSpectrumData.Length; i++)                 // 计算旧频谱数据和新频谱数据之间的 "中间值"
            {
                double oldData = spectrumData[i];
                double newData = newSpectrumData[i];
                double lerpData = oldData + (newData - oldData) * .2f;            // 每一次执行, 频谱值会向目标值移动 20% (如果太大, 缓动效果不明显, 如果太小, 频谱会有延迟的感觉)
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
        private void DrawGradient(RenderTarget g, Color down, Color up, double[] spectrumData, int pointCount, int drawingWidth, float xOffset, float yOffset, double scale)
        {
            GraphicsPath path = new GraphicsPath();

            PointF[] points = new PointF[pointCount + 2];
            for (int i = 0; i < pointCount; i++)
            {
                double x = i * drawingWidth / pointCount + xOffset;
                double y = spectrumData[i * spectrumData.Length / pointCount] * scale + yOffset;
                points[i + 1] = new PointF((float)x, (float)y);
            }

            points[0] = new PointF(xOffset, yOffset);
            points[points.Length - 1] = new PointF(xOffset + drawingWidth, yOffset);

            path.AddCurve(points);

            float upP = (float)points.Min(v => v.Y);

            if (Math.Abs(upP - yOffset) < 1)
                return;

            using Brush brush = new LinearGradientBrush(new PointF(0, yOffset), new PointF(0, upP), down, up);
            g.FillPath(brush, path);
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
        private void DrawGradientStrips(RenderTarget g, Color down, Color up, double[] spectrumData, int stripCount, int drawingWidth, float xOffset, float yOffset, float spacing, double scale)
        {
            float stripWidth = (drawingWidth - spacing * stripCount) / stripCount;
            PointF[] points = new PointF[stripCount];

            for (int i = 0; i < stripCount; i++)
            {
                double x = stripWidth * i + spacing * i + xOffset;
                double y = spectrumData[i * spectrumData.Length / stripCount] * scale;   // height
                points[i] = new PointF((float)x, (float)y);
            }

            float upP = (float)points.Min(v => v.Y < 0 ? yOffset + v.Y : yOffset);
            float downP = (float)points.Max(v => v.Y < 0 ? yOffset : yOffset + v.Y);

            if (downP < yOffset)
                downP = yOffset;

            if (Math.Abs(upP - downP) < 1)
                return;

            using Brush brush = new LinearGradientBrush(new PointF(0, downP), new PointF(0, upP), down, up);

            for (int i = 0; i < stripCount; i++)
            {
                PointF p = points[i];
                float y = yOffset;
                float height = p.Y;

                if (height < 0)
                {
                    y += height;
                    height = -height;
                }

                g.FillRectangle(brush, new RectangleF(p.X, y, stripWidth, height));
            }
        }

        /// <summary>
        /// 画曲线
        /// </summary>
        /// <param name="g"></param>
        /// <param name="pen"></param>
        /// <param name="spectrumData"></param>
        /// <param name="pointCount"></param>
        /// <param name="drawingWidth"></param>
        /// <param name="xOffset"></param>
        /// <param name="yOffset"></param>
        /// <param name="scale"></param>
        private void DrawCurve(RenderTarget g, Pen pen, double[] spectrumData, int pointCount, int drawingWidth, double xOffset, double yOffset, double scale)
        {
            PointF[] points = new PointF[pointCount];
            for (int i = 0; i < pointCount; i++)
            {
                double x = i * drawingWidth / pointCount + xOffset;
                double y = spectrumData[i * spectrumData.Length / pointCount] * scale + yOffset;
                points[i] = new PointF((float)x, (float)y);
            }

            g.DrawCurve(pen, points);
        }

        private void DrawCircleStrips(RenderTarget g, Brush brush, double[] spectrumData, int stripCount, double xOffset, double yOffset, double radius, double spacing, double rotation, double scale)
        {
            double rotationAngle = Math.PI / 180 * rotation;
            double blockWidth = MathF.PI * 2 / stripCount;           // angle
            double stripWidth = blockWidth - MathF.PI / 180 * spacing;                // angle
            PointF[] points = new PointF[stripCount];

            for (int i = 0; i < stripCount; i++)
            {
                double x = blockWidth * i + rotationAngle;      // angle
                double y = spectrumData[i * spectrumData.Length / stripCount] * scale;   // height
                points[i] = new PointF((float)x, (float)y);
            }

            for (int i = 0; i < stripCount; i++)
            {
                PointF p = points[i];
                double sinStart = Math.Sin(p.X);
                double sinEnd = Math.Sin(p.X + stripWidth);
                double cosStart = Math.Cos(p.X);
                double cosEnd = Math.Cos(p.X + stripWidth);

                PointF[] polygon = new PointF[]
                {
                    new PointF((float)(cosStart * radius + xOffset), (float)(sinStart * radius + yOffset)),
                    new PointF((float)(cosEnd * radius + xOffset), (float)(sinEnd * radius + yOffset)),
                    new PointF((float)(cosEnd * (radius + p.Y) + xOffset), (float)(sinEnd * (radius + p.Y) + yOffset)),
                    new PointF((float)(cosStart * (radius + p.Y) + xOffset), (float)(sinStart * (radius + p.Y) + yOffset)),
                };

                g.FillPolygon(brush, polygon);
            }
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
        private void DrawCircleGradientStrips(RenderTarget g, Color inner, Color outer, double[] spectrumData, int stripCount, double xOffset, double yOffset, double radius, double spacing, double rotation, double scale)
        {
            double rotationAngle = Math.PI / 180 * rotation;
            double blockWidth = Math.PI * 2 / stripCount;           // angle
            double stripWidth = blockWidth - MathF.PI / 180 * spacing;                // angle
            PointF[] points = new PointF[stripCount];

            for (int i = 0; i < stripCount; i++)
            {
                double x = blockWidth * i + rotationAngle;      // angle
                double y = spectrumData[i * spectrumData.Length / stripCount] * scale;   // height
                points[i] = new PointF((float)x, (float)y);
            }

            double maxHeight = points.Max(v =>  v.Y);
            double outerRadius = radius + maxHeight;

            PointF[] polygon = new PointF[4];
            for (int i = 0; i < stripCount; i++)
            {
                PointF p = points[i];
                double sinStart = Math.Sin(p.X);
                double sinEnd = Math.Sin(p.X + stripWidth);
                double cosStart = Math.Cos(p.X);
                double cosEnd = Math.Cos(p.X + stripWidth);

                PointF
                    p1 = new PointF((float)(cosStart * radius + xOffset),(float)(sinStart * radius + yOffset)),
                    p2 = new PointF((float)(cosEnd * radius + xOffset),(float)(sinEnd * radius + yOffset)),
                    p3 = new PointF((float)(cosEnd * (radius + p.Y) + xOffset), (float)(sinEnd * (radius + p.Y) + yOffset)),
                    p4 = new PointF((float)(cosStart * (radius + p.Y) + xOffset), (float)(sinStart * (radius + p.Y) + yOffset));

                polygon[0] = p1;
                polygon[1] = p2;
                polygon[2] = p3;
                polygon[3] = p4;


                PointF innerP = new PointF((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2);
                PointF outerP = new PointF((p3.X + p4.X) / 2, (p3.Y + p4.Y) / 2);

                Vector2 offset = new Vector2(outerP.X - innerP.X, outerP.Y - innerP.Y);
                if (MathF.Sqrt(offset.X * offset.X + offset.Y * offset.Y) < 3)
                    continue;

                LinearGradientBrush brush = new LinearGradientBrush(innerP, outerP, inner, outer);
                g.FillPolygon(brush, polygon);

                brush.Dispose();
            }
        }

        private void DrawStrips(RenderTarget g, Brush brush, double[] spectrumData, int stripCount, int drawingWidth, float xOffset, float yOffset, float spacing, double scale)
        {
            float stripWidth = (drawingWidth - spacing * stripCount) / stripCount;
            PointF[] points = new PointF[stripCount];

            for (int i = 0; i < stripCount; i++)
            {
                double x = stripWidth * i + spacing * i + xOffset;
                double y = spectrumData[i * spectrumData.Length / stripCount] * scale;   // height
                points[i] = new PointF((float)x, (float)y);
            }

            for (int i = 0; i < stripCount; i++)
            {
                PointF p = points[i];
                float y = yOffset;
                float height = p.Y;

                if (height < 0)
                {
                    y += height;
                    height = -height;
                }

                g.FillRectangle(brush, new RectangleF(p.X, y, stripWidth, height));
            }
        }

        private void DrawGradientBorder(RenderTarget g, Color inner, Color outer, Rectangle area, double scale, float width)
        {
            int thickness = (int)(width * scale);
            if (thickness < 1)
                return;

            Rectangle rect = new Rectangle(area.X, area.Y, area.Width, area.Height);

            Rectangle up = new Rectangle(rect.Location, new Size(rect.Width, thickness));
            Rectangle down = new Rectangle(new Point(rect.X, (int)(rect.X + rect.Height - scale * width)), new Size(rect.Width, thickness));
            Rectangle left = new Rectangle(rect.Location, new Size(thickness, rect.Height));
            Rectangle right = new Rectangle(new Point((int)(rect.X + rect.Width - scale * width), rect.Y), new Size(thickness, rect.Height));

            LinearGradientBrush upB = new LinearGradientBrush(up, outer, inner, LinearGradientMode.Vertical);
            LinearGradientBrush downB = new LinearGradientBrush(down, inner, outer, LinearGradientMode.Vertical);
            LinearGradientBrush leftB = new LinearGradientBrush(left, outer, inner, LinearGradientMode.Horizontal);
            LinearGradientBrush rightB = new LinearGradientBrush(right, inner, outer, LinearGradientMode.Horizontal);

            upB.WrapMode = downB.WrapMode = leftB.WrapMode = rightB.WrapMode = WrapMode.TileFlipXY;

            g.FillRectangle(upB, up);
            g.FillRectangle(downB, down);
            g.FillRectangle(leftB, left);
            g.FillRectangle(rightB, right);
        }

        int colorIndex = 0;
        double rotation = 0;
        BufferedGraphics? oldBuffer;
        private void DrawingTimer_Tick(object? sender, EventArgs e)
        {
            if (spectrumData == null)
                return;

            rotation += .1;
            colorIndex++;

            Color color1 = allColors[colorIndex % allColors.Length];
            Color color2 = allColors[(colorIndex + 200) % allColors.Length];

            double[] bassArea = TakeSpectrumOfFrequency(spectrumData, capture.WaveFormat.SampleRate, 250);
            double bassScale = bassArea.Average() * 100;
            double extraScale = Math.Min(drawingPanel.Width, drawingPanel.Height) / 6;

            Rectangle border = new Rectangle(Point.Empty, drawingPanel.Size - new Size(1, 1));

            BufferedGraphics buffer = BufferedGraphicsManager.Current.Allocate(drawingPanel.CreateGraphics(), drawingPanel.ClientRectangle);

            if (oldBuffer != null)
            {
                oldBuffer.Render(buffer.Graphics);
                oldBuffer.Dispose();
            }

            using Pen pen = new Pen(Color.Pink);
            using Brush brush = new SolidBrush(Color.Purple);
            //using Brush brush1 = new PathGradientBrush()

            //using Brush brush = new SolidBrush(Color.FromArgb(50, drawingPanel.BackColor));

            buffer.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            buffer.Graphics.Clear(drawingPanel.BackColor);
            DrawGradientBorder(buffer.Graphics, Color.FromArgb(0, color1), color2, border, bassScale, drawingPanel.Width / 10);
            //buffer.Graphics.FillRectangle(brush, drawingPanel.ClientRectangle);

            //DrawCurve(buffer.Graphics, new Pen(Brushes.Cyan), half, half.Length, drawingPanel.Width, 0, drawingPanel.Height, -100);
            //DrawGradient(buffer.Graphics, Color.Purple, Color.Cyan, half, half.Length, drawingPanel.Width, 0, drawingPanel.Height, -100);
            //DrawStrips(buffer.Graphics, Brushes.Purple, half, half.Length, drawingPanel.Width, 0, drawingPanel.Height, 3, -100);
            DrawGradientStrips(buffer.Graphics, color1, color2, spectrumData, spectrumData.Length, drawingPanel.Width, 0, drawingPanel.Height, 3, -drawingPanel.Height * 10);
            DrawCircleGradientStrips(buffer.Graphics, color1, color2, spectrumData, spectrumData.Length, drawingPanel.Width / 2, drawingPanel.Height / 2, MathF.Min(drawingPanel.Width, drawingPanel.Height) / 4 + extraScale * bassScale, 1, rotation, drawingPanel.Width / 6 * 10);

            DrawCurve(buffer.Graphics, pen, visualizer.SampleData, visualizer.SampleData.Length, drawingPanel.Width, 0, drawingPanel.Height / 2, MathF.Min(drawingPanel.Height / 10, 100));

            buffer.Render();

            oldBuffer = buffer;
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            capture.StartRecording();
            dataTimer.Start();
            drawingTimer.Start();
        }

        private void MainWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}