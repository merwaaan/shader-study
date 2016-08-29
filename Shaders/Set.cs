﻿using Shaders.Lights;
using System.Collections.Generic;

namespace Shaders
{
    /// <summary>
    ///     Scene containing model instances to be rendered.
    /// </summary>
    public class Set
    {
        public ILight Light { get; private set; }

        public bool OrbitLight;

        private readonly List<ModelInstance> _modelInstances = new List<ModelInstance>();

        public Set(params ModelInstance[] instances)
        {
            foreach (var instance in instances)
                _modelInstances.Add(instance);
        }

        public void Unload()
        {
            foreach (var instance in _modelInstances)
                instance.Unload();
        }

        public void Draw(IEye eye, Shader shader = null)
        {
            foreach (var instance in _modelInstances)
                instance.Draw(this, eye, shader);
        }

        public Set SetLight(ILight light)
        {
            Light = light;
            return this;
        }
    }
}
