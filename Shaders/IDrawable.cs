namespace Shaders
{
    public interface IDrawable
    {
        /// <summary>
        ///     Draw the drawable from the eye's perspective, optionally with the given shader.
        /// </summary>
        /// <param name="set"></param>
        /// <param name="eye"></param>
        /// <param name="shader"></param>
        void Draw(Set set, IEye eye, Shader shader = null);

        /// <summary>
        ///     Free the drawable's resources.
        /// </summary>
        void Unload();
    }
}
