using OpenTK;

namespace Shaders.Lights
{
    public interface ILight
    {
        Vector3 Position { get; set; }

        /// <summary>
        ///     Update the shader's uniforms with the light's parameters.
        /// </summary>
        void Bind(Shader shader);
    }
}