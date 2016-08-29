using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Shaders.Lights
{
    public class PointLight : ILight
    {
        public Matrix4 Transform { get; set; }

        public Matrix4 ViewMatrix => Transform;
        public Matrix4 ProjectionMatrix { get; }

        public Matrix4 LightSpace => ViewMatrix * ProjectionMatrix;

        public PointLight(Vector3 position)
        {
            Transform = Matrix4.LookAt(position, Vector3.Zero, Vector3.UnitY);
            ProjectionMatrix = Matrix4.CreateOrthographic(4, 4, 0.005f, 100f);
        }

        public PointLight(float x, float y, float z)
            : this(new Vector3(x, y, z))
        {
        }

        public void Bind(Shader shader)
        {
            GL.Uniform3(shader.LightPositionLocation, Transform.ExtractTranslation());

            var lightSpaceMatrix = LightSpace;
            GL.UniformMatrix4(shader.LightSpaceMatrixLocation, false, ref lightSpaceMatrix);
        }
    }
}
