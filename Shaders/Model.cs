using System.Linq;
using Assimp;
using OpenTK.Graphics.OpenGL;
using System;
using System.IO;

namespace Shaders
{
    /// <summary>
    ///     Data of a 3D model.
    /// </summary>
    public class Model
    {
        public float[] Vertices { get; }
        public ushort[] Indices { get; }

        public int VboHandle => _vboHandle;
        public int EboHandle => _eboHandle;

        private readonly Scene _model;

        private int _vboHandle;
        private int _eboHandle;

        private static readonly AssimpContext Importer;

        static Model()
        {
            Importer = new AssimpContext();
        }

        public Model(string path)
        {
            var fullPath = Path.Combine(App.AssetsDirectory, path);
            _model = Importer.ImportFile(fullPath, PostProcessPreset.TargetRealTimeMaximumQuality | PostProcessSteps.CalculateTangentSpace);

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

            _vboHandle = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VboHandle);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr) (Vertices.Length * sizeof(float)), Vertices, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            _eboHandle = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, EboHandle);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr) (Indices.Length * sizeof(ushort)), Indices, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }
        
        public void Unload()
        {
            GL.DeleteBuffers(1, ref _vboHandle);
            GL.DeleteBuffers(1, ref _eboHandle);
            _vboHandle = _eboHandle = -1;
        }
    }
}
