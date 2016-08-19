using System;
using System.IO;
using OpenTK.Graphics.OpenGL4;

namespace Shaders
{
    // From http://deathbyalgorithm.blogspot.fr/2013/12/basic-opentk-4-window.html
    public class Shader
    {
        public string VertexSource { get; }
        public string FragmentSource { get; }

        public int VertexId { get; private set; }
        public int FragmentId { get; private set; }

        public int Program { get; private set; }

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

        public Shader(App window, string name)
        {
            VertexSource = File.ReadAllText($"Shaders/{name}.vert");
            FragmentSource = File.ReadAllText($"Shaders/{name}.frag");

            Build();
        }

        private void Build()
        {
            int statusCode;
            string info;

            VertexId = GL.CreateShader(ShaderType.VertexShader);
            FragmentId = GL.CreateShader(ShaderType.FragmentShader);

            // Compile vertex shader
            GL.ShaderSource(VertexId, VertexSource);
            GL.CompileShader(VertexId);
            GL.GetShaderInfoLog(VertexId, out info);
            GL.GetShader(VertexId, ShaderParameter.CompileStatus, out statusCode);

            if (statusCode != 1)
                throw new ApplicationException(info);

            // Compile fragment shader
            GL.ShaderSource(FragmentId, FragmentSource);
            GL.CompileShader(FragmentId);
            GL.GetShaderInfoLog(FragmentId, out info);
            GL.GetShader(FragmentId, ShaderParameter.CompileStatus, out statusCode);

            if (statusCode != 1)
                throw new ApplicationException(info);

            Console.WriteLine(info);

            Program = GL.CreateProgram();
            GL.AttachShader(Program, FragmentId);
            GL.AttachShader(Program, VertexId);

            GL.LinkProgram(Program);
            GL.UseProgram(Program);

            ModelMatrixLocation = GL.GetUniformLocation(Program, "model_matrix");
            MvpMatrixLocation = GL.GetUniformLocation(Program, "mvp_matrix");

            DiffuseMapLocation = GL.GetUniformLocation(Program, "diffuse_map");
            SpecularMapLocation = GL.GetUniformLocation(Program, "specular_map");
            NormalMapLocation = GL.GetUniformLocation(Program, "normal_map");
            HeightMapLocation = GL.GetUniformLocation(Program, "height_map");

            PositionLocation = GL.GetAttribLocation(Program, "vertex_position");
            ColorLocation = GL.GetAttribLocation(Program, "vertex_color");
            NormalLocation = GL.GetAttribLocation(Program, "vertex_normal");
            TangentLocation = GL.GetAttribLocation(Program, "vertex_tangent");
            BitangentLocation = GL.GetAttribLocation(Program, "vertex_bitangent");
            TexCoordLocation = GL.GetAttribLocation(Program, "vertex_texcoord");

            GL.BindAttribLocation(Program, PositionLocation, "vertex_position");
            GL.BindAttribLocation(Program, ColorLocation, "vertex_color");
            GL.BindAttribLocation(Program, NormalLocation, "vertex_normal");
            GL.BindAttribLocation(Program, TangentLocation, "vertex_tangent");
            GL.BindAttribLocation(Program, BitangentLocation, "vertex_bitangent");
            GL.BindAttribLocation(Program, TexCoordLocation, "vertex_texcoord");

            GL.UseProgram(0);
        }

        public void Dispose()
        {
            if (Program != 0)
                GL.DeleteProgram(Program);

            if (FragmentId != 0)
                GL.DeleteShader(FragmentId);

            if (VertexId != 0)
                GL.DeleteShader(VertexId);
        }
    }
}
