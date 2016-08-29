using OpenTK;

namespace Shaders
{
    /// <summary>
    ///     Interface for objects that can act as a point of view (eg. camera for rendering, light for the shadom mapping).
    /// </summary>
    public interface IEye
    {
        Matrix4 ViewMatrix { get; }
        Matrix4 ProjectionMatrix { get; }
    }
}
