using Shaders.Lights;
using System.Collections.Generic;

namespace Shaders
{
    /// <summary>
    ///     Scene containing model instances to be rendered.
    /// </summary>
    public class Set : IUpdateable
    {
        public ILight Light { get; private set; }

        public bool OrbitLight;

        private readonly List<IDrawable> _drawables = new List<IDrawable>();

        public Set(params IDrawable[] instances)
        {
            foreach (var instance in instances)
                _drawables.Add(instance);
        }

        public void Unload()
        {
            foreach (var instance in _drawables)
                instance.Unload();
        }

        public void Draw(IEye eye, Shader shader = null)
        {
            foreach (var instance in _drawables)
                instance.Draw(this, eye, shader);
        }

        public Set SetLight(ILight light)
        {
            Light = light;
            return this;
        }

        public void Update(float dt)
        {
            foreach (var drawable in _drawables)
                (drawable as IUpdateable)?.Update(dt);
        }
    }
}
