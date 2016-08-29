using OpenTK;

namespace Shaders
{
    public interface ITransform
    {
        Matrix4 Transform { get; set; }
    }
}
