using LibAudioVisualizer;
using ManagedBass;
using OpenTK;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System.Diagnostics;
using System.Drawing;

namespace AudioVisualizerGL
{

    public class AppWindow : GameWindow
    {


        public float BarThickness { get; set; } = 0.1f;
        public float CircleRadius { get; set; } = 0.16f;
        public Vector3 Color1 { get; set; } = new Vector3(245 / 255f, 3 / 255f, 54 / 255f);
        public Vector3 Color2 { get; set; } = new Vector3(23 / 255f, 0 / 255f, 244 / 255f);

        public AppWindow(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings)
        {
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);
        }
    }
}