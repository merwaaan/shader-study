using OpenTK;

namespace Shaders.Lights
{
    public interface ILight : ITransform, IEye
    {
        /// <summary>
        ///     Update the shader's uniforms with the light's parameters.
        /// </summary>
        void Bind(Shader shader);
    }
}