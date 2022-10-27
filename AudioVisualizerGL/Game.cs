using LibAudioVisualizer;
using ManagedBass;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System.Diagnostics;
using System.Drawing;

namespace AudioVisualizerGL
{
    internal class Game : GameWindow
    {
        //WasapiCapture capture;             // 音频捕获
        Visualizer visualizer;             // 可视化
        double[]? spectrumData;            // 频谱数据

        public Game(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings)
        {
        }



        //RawColor4[] allColors;                 // 渐变颜色

        protected override void OnLoad()
        {
            GL.ClearColor(Color.MidnightBlue);
            base.OnLoad();
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            GL.Viewport(0, 0, e.Width, e.Height);
            base.OnResize(e);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);

            GL.Begin(PrimitiveType.Triangles);
            GL.Color3(Color.Red);
            GL.Vertex3(0, 0, 0);
            GL.Color3(Color.Green);
            GL.Vertex3(1, 0, 0);
            GL.Color3(Color.Blue);
            GL.Vertex3(1, 1, 0);
            GL.End();

            SwapBuffers();
        }

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            using Game game = new Game(
                new GameWindowSettings()
                {

                }, new NativeWindowSettings()
                {
                    Size = new Vector2i(800, 500)
                });

            game.Run();
        }
    }
}