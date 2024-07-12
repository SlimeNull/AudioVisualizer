using OpenTK.Graphics.OpenGL;

namespace AudioVisualizerGL
{
    public class Mesh : IDisposable
    {
        int _buffer;
        int _vertexArray;
        private bool _disposedValue;

        public Mesh(float[] vertices)
        {
            _buffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _buffer);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            _vertexArray = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArray);

            GL.VertexAttribPointer(0, sizeof(float), VertexAttribPointerType.Float, false, sizeof(float) * 3, 0);
            GL.EnableVertexAttribArray(0);
        }

        public Mesh(float[] vertices, float[] colors)
        {
            _buffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _buffer);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            _vertexArray = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArray);

            GL.VertexAttribPointer(0, sizeof(float), VertexAttribPointerType.Float, false, sizeof(float) * 6, 0);
            GL.EnableVertexAttribArray(0);
        }


        ~Mesh()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: false);
        }

        public virtual void Use()
        {
            GL.BindVertexArray(_vertexArray);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                GL.DeleteBuffer(_buffer);
                GL.DeleteVertexArray(_vertexArray);
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