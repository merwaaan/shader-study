using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;

namespace Shaders
{
    /// <summary>
    ///     Instance of a 3D model associating graphics data with a shader and a world transform.
    /// </summary>
    public class ParticleSystem : ITransform, IDrawable, IUpdateable
    {
        public const int MaxParticleCount = 1000;

        public Matrix4 Transform { get; set; }

        public Material Material { get; }

        private int _activeParticleCount;
        private Vector3[] _particlePositions = new Vector3[MaxParticleCount];
        private Vector3[] _particleVelocities = new Vector3[MaxParticleCount];

        private float _time;

        private readonly Random _random = new Random();

        private readonly int _vaoHandle;
        private readonly int _vboHandle;

        public ParticleSystem(Material material)
        {
            Material = material;

            Transform = Matrix4.Identity;

            _vboHandle = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vboHandle);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(MaxParticleCount * 3 * sizeof(float)), IntPtr.Zero, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            _vaoHandle = GL.GenVertexArray();
            GL.BindVertexArray(_vaoHandle);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vboHandle);

            GL.EnableVertexAttribArray(Material.Shader.PositionLocation);
            GL.VertexAttribPointer(Material.Shader.PositionLocation, 3, VertexAttribPointerType.Float, false, 0, IntPtr.Zero);

            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }

        public void Unload()
        {
            //TODO GL.DeleteBuffers(1, ref _vaoHandle);
        }

        private Vector3 RandomVector()
        {
            return new Vector3(
                (float)_random.NextDouble() * 2 - 1,
                (float)_random.NextDouble() * 5,
                (float)_random.NextDouble() * 2 - 1);
        }

        public ParticleSystem Add(int n)
        {
            for (var i = 0; i < n; ++i)
            {
                // Spawn each particle with a random initial velocity
                _particleVelocities[_activeParticleCount] = RandomVector();
                ++_activeParticleCount;
            }

            return this;
        }

        public unsafe void Update(float dt)
        {
            _time += dt;

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vboHandle);
            var particleData = GL.MapBuffer(BufferTarget.ArrayBuffer, BufferAccess.ReadWrite);

            for (var p = 0; p < _activeParticleCount; ++p)
            {
                var index = p * 3;

                // Euler integration
                _particleVelocities[p] += -Vector3.UnitY * dt;
                _particlePositions[p] += _particleVelocities[p] * dt;

                // Reset particles that fell too far
                if (_particlePositions[p].Y < -1)
                {
                    _particlePositions[p] = Vector3.Zero;
                    _particleVelocities[p] = RandomVector();
                }

                // Update graphics data
                var mem = (float*)particleData.ToPointer();
                mem[index] = _particlePositions[p].X;
                mem[index + 1] = _particlePositions[p].Y;
                mem[index + 2] = _particlePositions[p].Z;
            }

            GL.UnmapBuffer(BufferTarget.ArrayBuffer);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        // TODO pass lights only, no sets?
        public void Draw(Set set, IEye eye, Shader shader = null)
        {
            GL.UseProgram(Material.Shader.ProgramHandle);

            // Update the matrix uniforms
            var t = Transform;
            var mvp = t * eye.ViewMatrix * eye.ProjectionMatrix;
            GL.UniformMatrix4(Material.Shader.ModelMatrixLocation, false, ref t);
            GL.UniformMatrix4(Material.Shader.MvpMatrixLocation, false, ref mvp);

            // Update the material parameters
            Material.Bind();

            GL.BindVertexArray(_vaoHandle);
            GL.DrawArrays(BeginMode.Points, 0, _activeParticleCount);
            GL.BindVertexArray(0);

            GL.UseProgram(0);
        }
    }
}
