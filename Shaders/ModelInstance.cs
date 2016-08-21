using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;

namespace Shaders
{
    /// <summary>
    ///     Instance of a 3D model associating graphics data with a shader and a world transform.
    /// </summary>
    class ModelInstance
    {
        public Matrix4 Transform => _transform;

        public Model Model { get; }
        public Material Material { get; }

        private Matrix4 _transform;

        private int _vaoHandle;

        public ModelInstance(Model model, Material material)
        {
            Model = model;
            Material = material;

            _transform = Matrix4.Identity;

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

        public void Draw(Set set)
        {
            GL.UseProgram(Material.Shader.ProgramHandle);

            // Update the matrix uniforms
            var mvp = _transform * App.Instance.ViewMatrix * App.Instance.ProjectionMatrix;
            GL.UniformMatrix4(Material.Shader.ModelMatrixLocation, false, ref _transform);
            GL.UniformMatrix4(Material.Shader.MvpMatrixLocation, false, ref mvp);

            // Update the light parameters
            set.Light?.Bind(Material.Shader);

            // Update the material parameters
            Material.Bind();

            GL.BindVertexArray(_vaoHandle);
            GL.DrawElements(BeginMode.Triangles, Model.Indices.Length, DrawElementsType.UnsignedShort, IntPtr.Zero);
            GL.BindVertexArray(0);

            GL.UseProgram(0);
        }

        public ModelInstance Move(float x, float y, float z)
        {
            _transform = Matrix4.CreateTranslation(x, y, z);
            return this;
        }
    }
}
