
using OpenTK.Graphics.OpenGL;
using System;
using System.IO;

namespace Shaders
{
    // From http://deathbyalgorithm.blogspot.fr/2013/12/basic-opentk-4-window.html
    public class Shader
    {
        public int VertexShaderHandle { get; private set; } = -1;
        public int GeometryShaderHandle { get; private set; } = -1;
        public int FragmentShaderHandler { get; private set; } = -1;
        public int ProgramHandle { get; private set; } = -1;

        public int ModelMatrixLocation { get; private set; }
        public int MvpMatrixLocation { get; private set; }

        public int DiffuseMapLocation { get; private set; }
        public int SpecularMapLocation { get; private set; }
        public int NormalMapLocation { get; private set; }
        public int HeightMapLocation { get; private set; }
        public int ShadowMapLocation { get; private set; }

        public int ShadowBiasLocation { get; private set; }

        public int LightPositionLocation { get; private set; }
        public int LightSpaceMatrixLocation { get; private set; }

        public int PositionLocation { get; private set; }
        public int ColorLocation { get; private set; }
        public int NormalLocation { get; private set; }
        public int TangentLocation { get; private set; }
        public int BitangentLocation { get; private set; }
        public int TexCoordLocation { get; private set; }

        private string _vertexSource;
        private string _geometrySource;
        private string _fragmentSource;

        public Shader(App app)
        {
        }

        public Shader(App app, string name)
        {
            Vertex(name);
            Geometry(name);
            Fragment(name);
            Build();
        }

        public Shader Vertex(string name)
        {
            Console.WriteLine($"Loading vertex shader: {name}...");
            _vertexSource = ReadShaderSource($"Shaders/{name}.vert");
            return this;
        }

        public Shader Geometry(string name)
        {
            Console.WriteLine($"Loading geometry shader: {name}...");
            _geometrySource = ReadShaderSource($"Shaders/{name}.geom");
            return this;
        }

        public Shader Fragment(string name)
        {
            Console.WriteLine($"Loading fragment shader: {name}...");
            _fragmentSource = ReadShaderSource($"Shaders/{name}.frag");
            return this;
        }

        private static string ReadShaderSource(string path)
        {
            try
            {
                return File.ReadAllText(path);
            }
            catch (IOException)
            {
                Console.WriteLine($"\tCannot read shader file: {path}");
                return null;
            }
        }

        public void Build()
        {
            ProgramHandle = GL.CreateProgram();

            // Compile vertex shader
            if (_vertexSource != null)
            {
                VertexShaderHandle = GL.CreateShader(ShaderType.VertexShader);

                GL.ShaderSource(VertexShaderHandle, _vertexSource);
                GL.CompileShader(VertexShaderHandle);

                var info = GL.GetShaderInfoLog(VertexShaderHandle);
                if (!string.IsNullOrEmpty(info))
                    throw new ApplicationException(info);

                GL.AttachShader(ProgramHandle, VertexShaderHandle);
            }

            // Compile geometry shader
            if (_geometrySource != null)
            {
                GeometryShaderHandle = GL.CreateShader(ShaderType.GeometryShader);

                GL.ShaderSource(GeometryShaderHandle, _geometrySource);
                GL.CompileShader(GeometryShaderHandle);

                var info = GL.GetShaderInfoLog(GeometryShaderHandle);
                if (!string.IsNullOrEmpty(info))
                    throw new ApplicationException(info);

                GL.AttachShader(ProgramHandle, GeometryShaderHandle);
            }

            // Compile fragment shader
            if (_fragmentSource != null)
            {
                FragmentShaderHandler = GL.CreateShader(ShaderType.FragmentShader);

                GL.ShaderSource(FragmentShaderHandler, _fragmentSource);
                GL.CompileShader(FragmentShaderHandler);

                var info = GL.GetShaderInfoLog(FragmentShaderHandler);
                if (!string.IsNullOrEmpty(info))
                    throw new ApplicationException(info);

                GL.AttachShader(ProgramHandle, FragmentShaderHandler);
            }

            GL.LinkProgram(ProgramHandle);
            GL.UseProgram(ProgramHandle);

            ModelMatrixLocation = GL.GetUniformLocation(ProgramHandle, "model_matrix");
            MvpMatrixLocation = GL.GetUniformLocation(ProgramHandle, "mvp_matrix");

            DiffuseMapLocation = GL.GetUniformLocation(ProgramHandle, "diffuse_map");
            SpecularMapLocation = GL.GetUniformLocation(ProgramHandle, "specular_map");
            NormalMapLocation = GL.GetUniformLocation(ProgramHandle, "normal_map");
            HeightMapLocation = GL.GetUniformLocation(ProgramHandle, "height_map");
            ShadowMapLocation = GL.GetUniformLocation(ProgramHandle, "shadow_map");

            ShadowBiasLocation = GL.GetUniformLocation(ProgramHandle, "shadow_bias");

            LightPositionLocation = GL.GetUniformLocation(ProgramHandle, "light_position");
            LightSpaceMatrixLocation = GL.GetUniformLocation(ProgramHandle, "light_matrix");

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
                case Material.TextureType.Shadow:
                    return ShadowMapLocation;
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
