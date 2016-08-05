using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using Assimp;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;
using PrimitiveType = OpenTK.Graphics.OpenGL4.PrimitiveType;

namespace Shaders
{
    public class Window : GameWindow
    {
        private Matrix4 _modelMatrix;
        private Matrix4 _viewMatrix;
        private Matrix4 _projectionMatrix;

        private Matrix4 _mvpMatrix;

        private Scene _model;

        private ushort[] _indices;
        private float[] _vertices;

        private uint _vbo;
        private uint _ebo;
        private uint _vao;

        private readonly Dictionary<string, Shader> _shaders = new Dictionary<string, Shader>();

        public Shader CurrentShader => _shaders.Last().Value;

        public Window(int width = 900, int height = 900)
            : base(width, height, OpenTK.Graphics.GraphicsMode.Default, "Shader tests")
        {
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            int major, minor;
            GL.GetInteger(GetPName.MajorVersion, out major);
            GL.GetInteger(GetPName.MinorVersion, out minor);
            Console.WriteLine($"OpenGL version: {major}.{minor}");

            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);
            GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);

            _modelMatrix = Matrix4.Identity;
            _viewMatrix = Matrix4.LookAt(Vector3.UnitZ, Vector3.Zero, Vector3.UnitY);
            _projectionMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver2, Width / (float)Height, 0.005f, 100f);

            //_shaders.Add("Single color", new Shader(this, "SingleColor"));
            //_shaders.Add("Vertex colors", new Shader(this, "VertexColors"));
            //_shaders.Add("Texture mapping", new Shader(this, "TextureMapping"));
            //_shaders.Add("Phong shading", new Shader(this, "Phong"));
            //_shaders.Add("Normal mapping", new Shader(this, "NormalMapping"));
            _shaders.Add("Parallax mapping", new Shader(this, "ParallaxMapping"));

            // Load the mesh data

            var importer = new AssimpContext();
            _model = importer.ImportFile("Models/wall.obj", PostProcessPreset.TargetRealTimeMaximumQuality | PostProcessSteps.CalculateTangentSpace);

            _vertices = new float[_model.Meshes.Sum(m => m.Vertices.Count) * (3 + 3 + 3 + 3 + 3 + 2)]; // Position + Normal + Tangent + Bitangent + Color + UV
            _indices = new ushort[_model.Meshes.Sum(m => m.Faces.Sum(face => face.IndexCount))];

            var v = 0;
            var f = 0;
            foreach (var mesh in _model.Meshes)
            {
                var n = 0;
                var t = 0;
                var c = 0;
                var u = 0;
                foreach (var vertex in mesh.Vertices)
                {
                    _vertices[v++] = vertex.X / 3;
                    _vertices[v++] = vertex.Y / 3;
                    _vertices[v++] = vertex.Z / 3;

                    if (mesh.HasNormals)
                    {
                        _vertices[v++] = mesh.Normals[n].X;
                        _vertices[v++] = mesh.Normals[n].Y;
                        _vertices[v++] = mesh.Normals[n].Z;
                    ++n;
                    }
                    else
                    {
                        _vertices[v++] = 0;
                        _vertices[v++] = 0;
                        _vertices[v++] = 0;
                    }

                    if (mesh.HasTangentBasis)
                    {
                        _vertices[v++] = mesh.Tangents[t].X;
                        _vertices[v++] = mesh.Tangents[t].Y;
                        _vertices[v++] = mesh.Tangents[t].Z;
                        _vertices[v++] = mesh.BiTangents[t].X;
                        _vertices[v++] = mesh.BiTangents[t].Y;
                        _vertices[v++] = mesh.BiTangents[t].Z;
                        ++t;
                    }
                    else
                    {
                        _vertices[v++] = 0;
                        _vertices[v++] = 0;
                        _vertices[v++] = 0;
                        _vertices[v++] = 0;
                        _vertices[v++] = 0;
                        _vertices[v++] = 0;
                    }

                    if (mesh.HasVertexColors(0))
                    {
                        _vertices[v++] = mesh.VertexColorChannels[0][c].R;
                        _vertices[v++] = mesh.VertexColorChannels[0][c].G;
                        _vertices[v++] = mesh.VertexColorChannels[0][c].B;
                        ++c;
                    }
                    else
                    {
                        _vertices[v++] = 0;
                        _vertices[v++] = 1;
                        _vertices[v++] = 0;
                    }

                    if (mesh.HasTextureCoords(0))
                    {
                        _vertices[v++] = mesh.TextureCoordinateChannels[0][u].X;
                        _vertices[v++] = mesh.TextureCoordinateChannels[0][u].Y;
                        ++u;
                    }
                    else
                    {
                        _vertices[v++] = 0;
                        _vertices[v++] = 0;
                    }
                }

                foreach (var face in mesh.Faces)
                    foreach (var index in face.Indices)
                        _indices[f++] = (ushort)index;
            }

            // Fill buffers

            GL.GenBuffers(1, out _vbo);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(_vertices.Length * sizeof(float)), _vertices, BufferUsageHint.StaticDraw);

            GL.GenBuffers(1, out _ebo);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(_indices.Length * sizeof(ushort)), _indices, BufferUsageHint.StaticDraw);

            // Setup attributes

            GL.GenVertexArrays(1, out _vao);
            GL.BindVertexArray(_vao);

            const int size = 17;
            GL.VertexAttribPointer(CurrentShader.PositionLocation, 3, VertexAttribPointerType.Float, false, size * sizeof(float), 0);
            GL.EnableVertexAttribArray(CurrentShader.PositionLocation);

            GL.VertexAttribPointer(CurrentShader.NormalLocation, 3, VertexAttribPointerType.Float, false, size * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(CurrentShader.NormalLocation);

            GL.VertexAttribPointer(CurrentShader.TangentLocation, 3, VertexAttribPointerType.Float, false, size * sizeof(float), 6 * sizeof(float));
            GL.EnableVertexAttribArray(CurrentShader.TangentLocation);

            GL.VertexAttribPointer(CurrentShader.BitangentLocation, 3, VertexAttribPointerType.Float, false, size * sizeof(float), 9 * sizeof(float));
            GL.EnableVertexAttribArray(CurrentShader.BitangentLocation);

            GL.VertexAttribPointer(CurrentShader.ColorLocation, 3, VertexAttribPointerType.Float, false, size * sizeof(float), 12 * sizeof(float));
            GL.EnableVertexAttribArray(CurrentShader.ColorLocation);

            GL.VertexAttribPointer(CurrentShader.TexCoordLocation, 2, VertexAttribPointerType.Float, false, size * sizeof(float), 15 * sizeof(float));
            GL.EnableVertexAttribArray(CurrentShader.TexCoordLocation);

            // Load the textures

            /*LoadTexture("Models/eyeball_diffuse.png", TextureUnit.Texture0, CurrentShader.DiffuseMapLocation);
            LoadTexture("Models/eyeball_specular.png", TextureUnit.Texture1, CurrentShader.SpecularMapLocation);
            LoadTexture("Models/eyeball_normal.png", TextureUnit.Texture2, CurrentShader.NormalMapLocation);*/

            LoadTexture("Models/floor_albedo.png", TextureUnit.Texture0, CurrentShader.DiffuseMapLocation);
            LoadTexture("Models/floor_specular2.png", TextureUnit.Texture1, CurrentShader.SpecularMapLocation);
            LoadTexture("Models/floor_normal.png", TextureUnit.Texture2, CurrentShader.NormalMapLocation);
            LoadTexture("Models/floor_height.png", TextureUnit.Texture3, CurrentShader.HeightMapLocation);

            // Setup input

            MouseMove += (sender, ev) =>
            {
                if (Math.Abs(ev.XDelta) > 5)
                    return;

                _modelMatrix *= Matrix4.CreateRotationY(0.005f * ev.XDelta);
                UpdateMatrices();
            };

            UpdateMatrices();
        }

        private void LoadTexture(string path, TextureUnit unit, int uniformLocation)
        {
            GL.ActiveTexture(unit);

            // Generate an ID and bind the texture
            var textureId = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, textureId);
            GL.Uniform1(uniformLocation, unit - TextureUnit.Texture0); // layout(binding=?) must match in the shader

            // Load the texture data
            var bitmap = new Bitmap(path);
            var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bitmapData.Width, bitmapData.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, bitmapData.Scan0);
            bitmap.UnlockBits(bitmapData);

            //
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            Console.WriteLine($"Loaded texture {path} with ID {textureId}, uniform location {uniformLocation}, unit {unit}");
        }

        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);

            GL.DeleteBuffers(1, ref _vbo);
            GL.DeleteBuffers(1, ref _ebo);
            GL.DeleteProgram(CurrentShader.Program);
            // TODO delete shaders
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(0, 0, Width, Height);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Draw(CurrentShader);

            SwapBuffers();
        }

        private void Draw(Shader shader)
        {
            GL.UseProgram(shader.Program);

            GL.BindVertexArray(_vao);
            GL.DrawElements(PrimitiveType.Triangles, _indices.Length, DrawElementsType.UnsignedShort, _indices);

            GL.UseProgram(0);
        }

        private void UpdateMatrices()
        {
            _mvpMatrix = _modelMatrix * _viewMatrix * _projectionMatrix;

            GL.UseProgram(CurrentShader.Program);
            GL.UniformMatrix4(CurrentShader.ModelMatrixLocation, false, ref _modelMatrix);
            GL.UniformMatrix4(CurrentShader.MvpMatrixLocation, false, ref _mvpMatrix);
        }
    }
}
