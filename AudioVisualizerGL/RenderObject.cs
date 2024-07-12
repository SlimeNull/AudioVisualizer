namespace AudioVisualizerGL
{
    public class RenderObject
    {
        public RenderObject(Mesh mesh, Shader shader)
        {
            Mesh = mesh;
            Shader = shader;
        }

        public Mesh Mesh { get; }
        public Shader Shader { get; }

        public virtual void Draw()
        {
            Mesh.Use();
            Shader.Use();
        }
    }
}