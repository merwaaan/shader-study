using System;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace Shaders
{
    /// <summary>
    ///     Instance of a 3D model associating graphics data with a shader and a world transform.
    /// </summary>
    class ModelInstance
    {
        public Matrix4 Transform => _transform;

        public Model Model { get; }

        public Shader Shader { get; set; }

        // TODO
        public uint VertexArrayObject;
        public uint VertexBufferObject;
        public uint ElementBufferObject;

        private Matrix4 _transform;

        public ModelInstance(Model model, Shader shader)
        {
            Model = model;
            Shader = shader;

            _transform = Matrix4.Identity;

            FillBuffers();
        }

        public void Unload()
        {
            GL.DeleteBuffers(1, ref VertexArrayObject);
            GL.DeleteBuffers(1, ref VertexBufferObject);
            GL.DeleteBuffers(1, ref ElementBufferObject);
        }

        private void FillBuffers()
        {
            // Fill buffers

            GL.GenVertexArrays(1, out VertexArrayObject);
            GL.GenBuffers(1, out VertexBufferObject);
            GL.GenBuffers(1, out ElementBufferObject);

            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferObject);
            GL.BindVertexArray(VertexArrayObject);

            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(Model.Vertices.Length * sizeof(float)), Model.Vertices, BufferUsageHint.StaticDraw);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(Model.Indices.Length * sizeof(ushort)), Model.Indices, BufferUsageHint.StaticDraw);

            // Setup attributes

            const int size = 17;

            GL.EnableVertexAttribArray(Shader.PositionLocation);
            GL.VertexAttribPointer(Shader.PositionLocation, 3, VertexAttribPointerType.Float, false, size * sizeof(float), 0);

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
            
            //GL.BindVertexArray(0);
        }

        public void Draw()
        {
            GL.BindVertexArray(VertexArrayObject);
            GL.UseProgram(Shader.Program);

            // Update the matrix uniforms
            var mvp = _transform * App.Instance.ViewMatrix * App.Instance.ProjectionMatrix;
            GL.UniformMatrix4(Shader.ModelMatrixLocation, false, ref _transform);
            GL.UniformMatrix4(Shader.MvpMatrixLocation, false, ref mvp);

            GL.DrawElements(PrimitiveType.Triangles, Model.Indices.Length, DrawElementsType.UnsignedShort, Model.Indices);

            GL.UseProgram(0);
            GL.BindVertexArray(0);
        }

        public ModelInstance Move(Matrix4 transform)
        {
            _transform = transform;
            return this;
        }
    }
}
