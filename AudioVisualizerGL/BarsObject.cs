namespace AudioVisualizerGL
{
    public class BarsObject : RenderObject, IDisposable
    {
        private bool _disposedValue;

        public BarsObject() : base(new BarsMesh(), new BarsShader())
        {

        }

        ~BarsObject()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: false);
        }



        public class BarsMesh : Mesh
        {
            public BarsMesh() : base(
                new float[]
                {
                    // left bar
                    -1, -1, 0,

                })
            {

            }
        }

        public class BarsShader : Shader
        {
            const string vertexShader =
                """
                #version 330 core
                layout (location = 0) in vec3 position0;

                void main()
                {
                    gl_Position = vec4(position0, 0);
                }
                """;

            const string fragmentShader =
                """
                #version 330 core
                uniform vec3 color1;
                uniform vec3 color2;
                """;

            public BarsShader() : base(
                new StringReader(vertexShader),
                new StringReader(fragmentShader))
            {

            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                Shader.Dispose();
                Mesh.Dispose();
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}