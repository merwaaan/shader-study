﻿using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using Assimp;
using OpenTK.Graphics.OpenGL4;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

namespace Shaders
{
    /// <summary>
    ///     Data of a 3D model.
    /// </summary>
    class Model
    {
        private readonly Scene _model;

        public ushort[] Indices;
        public float[] Vertices;

        private static readonly AssimpContext Importer;

        static Model()
        {
            Importer = new AssimpContext();
        }

        public Model(string path)
        {
            _model = Importer.ImportFile(path, PostProcessPreset.TargetRealTimeMaximumQuality | PostProcessSteps.CalculateTangentSpace);

            LoadMeshData();
        }

        private void LoadMeshData()
        {
            // Position + Normal + Tangent + Bitangent + Color + UV
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
                        Indices[f++] = (ushort)index;
            }
        }

        public void Textures(string diffuse = null, string specular = null, string normal = null, string height = null)
        {

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
    }
}