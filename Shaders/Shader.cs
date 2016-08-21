
using OpenTK.Graphics.OpenGL;
using System;
using System.IO;

namespace Shaders
{
    // From http://deathbyalgorithm.blogspot.fr/2013/12/basic-opentk-4-window.html
    public class Shader
    {
        public int VertexShaderHandle { get; private set; }
        public int FragmentShaderHandler { get; private set; }
        public int ProgramHandle { get; private set; }

        public int ModelMatrixLocation { get; private set; }
        public int MvpMatrixLocation { get; private set; }

        public int DiffuseMapLocation { get; private set; }
        public int SpecularMapLocation { get; private set; }
        public int NormalMapLocation { get; private set; }
        public int HeightMapLocation { get; private set; }

        public int PositionLocation { get; private set; }
        public int ColorLocation { get; private set; }
        public int NormalLocation { get; private set; }
        public int TangentLocation { get; private set; }
        public int BitangentLocation { get; private set; }
        public int TexCoordLocation { get; private set; }

        private readonly string _vertexSource;
        private readonly string _fragmentSource;

        public Shader(App window, string name)
        {
            _vertexSource = File.ReadAllText($"Shaders/{name}.vert");
            _fragmentSource = File.ReadAllText($"Shaders/{name}.frag");

            Build();
        }

        private void Build()
        {
            VertexShaderHandle = GL.CreateShader(ShaderType.VertexShader);
            FragmentShaderHandler = GL.CreateShader(ShaderType.FragmentShader);

            // Compile vertex shader
            GL.ShaderSource(VertexShaderHandle, _vertexSource);
            GL.CompileShader(VertexShaderHandle);

            var info = GL.GetShaderInfoLog(VertexShaderHandle);
            if (!string.IsNullOrEmpty(info))
                throw new ApplicationException(info);

            // Compile fragment shader
            GL.ShaderSource(FragmentShaderHandler, _fragmentSource);
            GL.CompileShader(FragmentShaderHandler);

            info = GL.GetShaderInfoLog(FragmentShaderHandler);
            if (!string.IsNullOrEmpty(info))
                throw new ApplicationException(info);

            Console.WriteLine(info);

            ProgramHandle = GL.CreateProgram();
            GL.AttachShader(ProgramHandle, FragmentShaderHandler);
            GL.AttachShader(ProgramHandle, VertexShaderHandle);

            GL.LinkProgram(ProgramHandle);
            GL.UseProgram(ProgramHandle);

            ModelMatrixLocation = GL.GetUniformLocation(ProgramHandle, "model_matrix");
            MvpMatrixLocation = GL.GetUniformLocation(ProgramHandle, "mvp_matrix");

            DiffuseMapLocation = GL.GetUniformLocation(ProgramHandle, "diffuse_map");
            SpecularMapLocation = GL.GetUniformLocation(ProgramHandle, "specular_map");
            NormalMapLocation = GL.GetUniformLocation(ProgramHandle, "normal_map");
            HeightMapLocation = GL.GetUniformLocation(ProgramHandle, "height_map");

            PositionLocation = GL.GetAttribLocation(ProgramHandle, "vertex_position");
            ColorLocation = GL.GetAttribLocation(ProgramHandle, "vertex_color");
            NormalLocation = GL.GetAttribLocation(ProgramHandle, "vertex_normal");
            TangentLocation = GL.GetAttribLocation(ProgramHandle, "vertex_tangent");
            BitangentLocation = GL.GetAttribLocation(ProgramHandle, "vertex_bitangent");
            TexCoordLocation = GL.GetAttribLocation(ProgramHandle, "vertex_texcoord");

            GL.BindAttribLocation(ProgramHandle, PositionLocation, "vertex_position");
            GL.BindAttribLocation(ProgramHandle, ColorLocation, "vertex_color");
            GL.BindAttribLocation(ProgramHandle, NormalLocation, "vertex_normal");
            GL.BindAttribLocation(ProgramHandle, TangentLocation, "vertex_tangent");
            GL.BindAttribLocation(ProgramHandle, BitangentLocation, "vertex_bitangent");
            GL.BindAttribLocation(ProgramHandle, TexCoordLocation, "vertex_texcoord");
        }

        public int GetTextureLocation(Material.TextureType type)
        {
            switch (type)
            {
                case Material.TextureType.Diffuse:
                    return DiffuseMapLocation;
                case Material.TextureType.Specular:
                    return SpecularMapLocation;
                case Material.TextureType.Normal:
                    return NormalLocation;
                case Material.TextureType.Height:
                    return HeightMapLocation;
                default:
                    throw new ArgumentException(nameof(type));
            }
        }

        public void Unload()
        {
            GL.DeleteProgram(ProgramHandle);
            GL.DeleteShader(FragmentShaderHandler);
            GL.DeleteShader(VertexShaderHandle);
        }
    }
}
