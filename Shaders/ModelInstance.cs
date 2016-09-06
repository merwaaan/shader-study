using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;

namespace Shaders
{
    /// <summary>
    ///     Instance of a 3D model associating graphics data with a shader and a world transform.
    /// </summary>
    public class ModelInstance : ITransform, IDrawable
    {
        public Matrix4 Transform { get; set; }

        public Model Model { get; }
        public Material Material { get; }

        private int _vaoHandle;

        public ModelInstance(Model model, Material material)
        {
            Model = model;
            Material = material;

            Transform = Matrix4.Identity;

            SetupAttributes();
        }

        private void SetupAttributes()
        {
            _vaoHandle = GL.GenVertexArray();
            GL.BindVertexArray(_vaoHandle);

            GL.BindBuffer(BufferTarget.ArrayBuffer, Model.VboHandle);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, Model.EboHandle);

            const int size = 17;

            GL.EnableVertexAttribArray(Material.Shader.PositionLocation);
            GL.VertexAttribPointer(Material.Shader.PositionLocation, 3, VertexAttribPointerType.Float, false, size * sizeof(float), IntPtr.Zero);

            GL.EnableVertexAttribArray(Material.Shader.NormalLocation);
            GL.VertexAttribPointer(Material.Shader.NormalLocation, 3, VertexAttribPointerType.Float, false, size * sizeof(float), 3 * sizeof(float));

            GL.EnableVertexAttribArray(Material.Shader.TangentLocation);
            GL.VertexAttribPointer(Material.Shader.TangentLocation, 3, VertexAttribPointerType.Float, false, size * sizeof(float), 6 * sizeof(float));

            GL.EnableVertexAttribArray(Material.Shader.BitangentLocation);
            GL.VertexAttribPointer(Material.Shader.BitangentLocation, 3, VertexAttribPointerType.Float, false, size * sizeof(float), 9 * sizeof(float));

            GL.EnableVertexAttribArray(Material.Shader.ColorLocation);
            GL.VertexAttribPointer(Material.Shader.ColorLocation, 3, VertexAttribPointerType.Float, false, size * sizeof(float), 12 * sizeof(float));

            GL.EnableVertexAttribArray(Material.Shader.TexCoordLocation);
            GL.VertexAttribPointer(Material.Shader.TexCoordLocation, 2, VertexAttribPointerType.Float, false, size * sizeof(float), 15 * sizeof(float));

            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }

        public void Unload()
        {
            GL.DeleteBuffers(1, ref _vaoHandle);
        }

        public void Draw(Set set, IEye eye, Shader shader = null)
        {
            // Use either the provided shader of the object's own shader
            shader = shader ?? Material.Shader;

            GL.UseProgram(shader.ProgramHandle);

            // Update the matrix uniforms
            var t = Transform;
            var mvp = t * eye.ViewMatrix * eye.ProjectionMatrix;
            GL.UniformMatrix4(shader.ModelMatrixLocation, false, ref t);
            GL.UniformMatrix4(shader.MvpMatrixLocation, false, ref mvp);

            // Update the light parameters
            set.Light?.Bind(shader);

            // Update the material parameters
            Material.Bind();

            GL.BindVertexArray(_vaoHandle);
            GL.DrawElements(BeginMode.Triangles, Model.Indices.Length, DrawElementsType.UnsignedShort, IntPtr.Zero);
            GL.BindVertexArray(0);

            GL.UseProgram(0);
        }

        public ModelInstance Move(float x, float y, float z)
        {
            Transform = Matrix4.CreateTranslation(x, y, z);
            return this;
        }

        public ModelInstance Scale(float s)
        {
            Transform = Matrix4.CreateScale(s);
            return this;
        }
    }
}
