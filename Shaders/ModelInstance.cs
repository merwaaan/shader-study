using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Runtime.InteropServices;

namespace Shaders
{
    /// <summary>
    ///     Instance of a 3D model associating graphics data with a shader and a world transform.
    /// </summary>
    class ModelInstance
    {
        public Matrix4 Transform => _transform;

        public Model Model { get; }
        //public Material Material {get;}
        public Shader Shader { get; }

        private Matrix4 _transform;

        private int _vaoHandle;

        public ModelInstance(Model model, Shader shader)
        {
            Model = model;
            Shader = shader;

            _transform = Matrix4.Identity;

            BindAttributes();

            Model.BindTexture(this, "diffuse", TextureUnit.Texture0, Shader.DiffuseMapLocation);
            Model.BindTexture(this, "specular", TextureUnit.Texture1, Shader.SpecularMapLocation);
            Model.BindTexture(this, "normal", TextureUnit.Texture2, Shader.NormalMapLocation);
            Model.BindTexture(this, "height", TextureUnit.Texture3, Shader.HeightMapLocation);
        }

        private void BindAttributes()
        {            
            _vaoHandle = GL.GenVertexArray();
            GL.BindVertexArray(_vaoHandle);

            GL.BindBuffer(BufferTarget.ArrayBuffer, Model.VboHandle);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, Model.EboHandle);

            const int size = 17;

            GL.EnableVertexAttribArray(Shader.PositionLocation);
            GL.VertexAttribPointer(Shader.PositionLocation, 3, VertexAttribPointerType.Float, false, size * sizeof(float), IntPtr.Zero);

            GL.EnableVertexAttribArray(Shader.NormalLocation);
            GL.VertexAttribPointer(Shader.NormalLocation, 3, VertexAttribPointerType.Float, false, size * sizeof(float), 3 * sizeof(float));

            GL.EnableVertexAttribArray(Shader.TangentLocation);
            GL.VertexAttribPointer(Shader.TangentLocation, 3, VertexAttribPointerType.Float, false, size * sizeof(float), 6 * sizeof(float));

            GL.EnableVertexAttribArray(Shader.BitangentLocation);
            GL.VertexAttribPointer(Shader.BitangentLocation, 3, VertexAttribPointerType.Float, false, size * sizeof(float), 9 * sizeof(float));

            GL.EnableVertexAttribArray(Shader.ColorLocation);
            GL.VertexAttribPointer(Shader.ColorLocation, 3, VertexAttribPointerType.Float, false, size * sizeof(float), 12 * sizeof(float));

            GL.EnableVertexAttribArray(Shader.TexCoordLocation);
            GL.VertexAttribPointer(Shader.TexCoordLocation, 2, VertexAttribPointerType.Float, false, size * sizeof(float), 15 * sizeof(float));

            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }

        /*public void Unload()
        {
            GL.DeleteBuffers(1, ref VertexArrayObject);
            GL.DeleteBuffers(1, ref VertexBufferObject);
            GL.DeleteBuffers(1, ref ElementBufferObject);
        }*/

        public void Draw() {
            GL.UseProgram(Shader.ProgramHandle);

            // Update the matrix uniforms
            var mvp = _transform * App.Instance.ViewMatrix * App.Instance.ProjectionMatrix;
            GL.UniformMatrix4(Shader.ModelMatrixLocation, false, ref _transform);
            GL.UniformMatrix4(Shader.MvpMatrixLocation, false, ref mvp);

            GL.BindVertexArray(_vaoHandle);
            GL.DrawElements(BeginMode.Triangles, Model.Indices.Length, DrawElementsType.UnsignedShort, IntPtr.Zero);
        }

        public ModelInstance Move(float x, float y, float z)
        {
            _transform = Matrix4.CreateTranslation(x, y, z);
            return this;
        }
    }
}
