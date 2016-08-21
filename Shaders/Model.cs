using System.Linq;
using Assimp;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Drawing.Imaging;
using System;

namespace Shaders
{
    /// <summary>
    ///     Data of a 3D model.
    /// </summary>
    class Model
    {
        public float[] Vertices { get; }
        public ushort[] Indices { get; }

        public int VboHandle { get; }
        public int EboHandle { get; }

        private readonly Scene _model;

        private static readonly AssimpContext Importer;

        static Model()
        {
            Importer = new AssimpContext();
        }

        public Model(string path)
        {
            _model = Importer.ImportFile(path, PostProcessPreset.TargetRealTimeMaximumQuality | PostProcessSteps.CalculateTangentSpace);

            // Load the mesh data
            // Layout: position + normal + tangent + bitangent + color + tex coords

            Vertices = new float[_model.Meshes.Sum(m => m.Vertices.Count) * (3 + 3 + 3 + 3 + 3 + 2)];
            Indices = new ushort[_model.Meshes.Sum(m => m.Faces.Sum(face => face.IndexCount))];

            int v = 0, f = 0;
            foreach (var mesh in _model.Meshes)
            {
                int n = 0, t = 0, c = 0, u = 0;
                foreach (var vertex in mesh.Vertices)
                {
                    // Position
                    Vertices[v++] = vertex.X / 3;
                    Vertices[v++] = vertex.Y / 3;
                    Vertices[v++] = vertex.Z / 3;

                    // Normal
                    if (mesh.HasNormals)
                    {
                        Vertices[v++] = mesh.Normals[n].X;
                        Vertices[v++] = mesh.Normals[n].Y;
                        Vertices[v++] = mesh.Normals[n].Z;
                        ++n;
                    }
                    else
                    {
                        Vertices[v++] = 0;
                        Vertices[v++] = 0;
                        Vertices[v++] = 0;
                    }

                    // Tangent + Bitangent
                    if (mesh.HasTangentBasis)
                    {
                        Vertices[v++] = mesh.Tangents[t].X;
                        Vertices[v++] = mesh.Tangents[t].Y;
                        Vertices[v++] = mesh.Tangents[t].Z;
                        Vertices[v++] = mesh.BiTangents[t].X;
                        Vertices[v++] = mesh.BiTangents[t].Y;
                        Vertices[v++] = mesh.BiTangents[t].Z;
                        ++t;
                    }
                    else
                    {
                        Vertices[v++] = 0;
                        Vertices[v++] = 0;
                        Vertices[v++] = 0;
                        Vertices[v++] = 0;
                        Vertices[v++] = 0;
                        Vertices[v++] = 0;
                    }

                    // Color
                    if (mesh.HasVertexColors(0))
                    {
                        Vertices[v++] = mesh.VertexColorChannels[0][c].R;
                        Vertices[v++] = mesh.VertexColorChannels[0][c].G;
                        Vertices[v++] = mesh.VertexColorChannels[0][c].B;
                        ++c;
                    }
                    else
                    {
                        Vertices[v++] = 0;
                        Vertices[v++] = 1;
                        Vertices[v++] = 0;
                    }

                    // Texture coordinates
                    if (mesh.HasTextureCoords(0))
                    {
                        Vertices[v++] = mesh.TextureCoordinateChannels[0][u].X;
                        Vertices[v++] = mesh.TextureCoordinateChannels[0][u].Y;
                        ++u;
                    }
                    else
                    {
                        Vertices[v++] = 0;
                        Vertices[v++] = 0;
                    }
                }

                foreach (var face in mesh.Faces)
                    foreach (var index in face.Indices)
                        Indices[f++] = (ushort) index;
            }

            // Fill the data buffers

            VboHandle = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VboHandle);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr) (Vertices.Length * sizeof(float)), Vertices, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            EboHandle = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, EboHandle);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr) (Indices.Length * sizeof(ushort)), Indices, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }

        private readonly Dictionary<string, string> _texturePaths = new Dictionary<string, string>();

        public void Textures(string diffuse = null, string specular = null, string normal = null, string height = null)
        {
            _texturePaths.Add("diffuse", diffuse);
            _texturePaths.Add("specular", specular);
            _texturePaths.Add("normal", normal);
            _texturePaths.Add("height", height);
        }

        // TODO seprate into load/bind
        // TODO only bind before rendering
        public void BindTexture(ModelInstance instance, string name, TextureUnit unit, int uniformLocation)
        {
            GL.ActiveTexture(unit);

            // Generate an ID and bind the texture
            var textureId = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, textureId);
            GL.Uniform1(uniformLocation, unit - TextureUnit.Texture0); // layout(binding=?) must match in the shader

            // Load the texture data

            string path;
            _texturePaths.TryGetValue(name, out path);
            if (path == null)
                return;

            var bitmap = new Bitmap(path);
            var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bitmapData.Width, bitmapData.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bitmapData.Scan0);
            bitmap.UnlockBits(bitmapData);

            //
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Linear);

            Console.WriteLine($"Loaded texture {path} with ID {textureId}, uniform location {uniformLocation}, unit {unit}");
        }
    }
}
