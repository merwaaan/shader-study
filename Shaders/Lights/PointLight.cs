using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Shaders.Lights
{
    public class PointLight : ILight
    {
        public Vector3 Position { get; }

        public PointLight(Vector3 position)
        {
            Position = position;
        }

        public PointLight(float x, float y, float z)
            : this(new Vector3(x, y, z))
        {
        }

        public void Bind(Shader shader)
        {
            GL.Uniform3(shader.LightPositionLocation, Position);
        }
    }
}
