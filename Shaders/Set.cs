using System.Collections.Generic;

namespace Shaders
{
    /// <summary>
    ///     Scene containing model instances to be rendered.
    /// </summary>
    internal class Set
    {
        private readonly List<ModelInstance> _modelInstances = new List<ModelInstance>();

        public Set(params ModelInstance[] instances)
        {
            foreach (var instance in instances)
                Add(instance);
        }

        /*public void Unload()
        {
            foreach (var instance in _modelInstances)
                instance.Unload();
        }*/

        public ModelInstance Add(ModelInstance instance)
        {
            _modelInstances.Add(instance);
            return instance;
        }

        public void Draw()
        {
            foreach (var instance in _modelInstances)
                instance.Draw();
        }
    }
}
