using OpenTK;
using OpenTK.Graphics.OpenGL;
using Shaders.Lights;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Shaders
{
    public class App : GameWindow
    {
        public static App Instance;

        public const string AssetsDirectory = "Models";

        // TODO create camera class
        public Matrix4 ViewMatrix;
        public Matrix4 ProjectionMatrix;
        private float _cameraRotation;
        private float _cameraAngularSpeed = 0.005f;
        private float _cameraDistance = 1;
        private float _cameraZoomSpeed = 0.2f;

        // TODO create asset handling class
        private readonly Dictionary<string, Model> _models = new Dictionary<string, Model>();
        private readonly Dictionary<string, Shader> _shaders = new Dictionary<string, Shader>();
        private readonly Dictionary<string, Material> _materials = new Dictionary<string, Material>();
        private readonly List<Set> _sets = new List<Set>();

        private GUI _gui;

        private Set _currentSet;
        private bool _orbitLight; // TODO move in set?

        public App(string name, int width = 900, int height = 900)
            : base(width, height, OpenTK.Graphics.GraphicsMode.Default, name)
        {
            Instance = this;
        }

        protected override void OnLoad(EventArgs e)
        {
            int major, minor;
            GL.GetInteger(GetPName.MajorVersion, out major);
            GL.GetInteger(GetPName.MinorVersion, out minor);
            Console.WriteLine($"OpenGL version: {major}.{minor}");

            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);
            GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);

            // Setup matrices
            ProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView((float) Math.PI / 2, Width / (float) Height, 0.005f, 100f);
            UpdateViewMatrix();

            // Setup inputs
            Mouse.Move += (sender, ev) =>
            {
                _cameraRotation += _cameraAngularSpeed * ev.XDelta;
                UpdateViewMatrix();
            };

            Mouse.WheelChanged += (sender, ev) =>
            {
                _cameraDistance += _cameraZoomSpeed * -ev.DeltaPrecise;
                UpdateViewMatrix();
            };

            KeyPress += (sender, ev) =>
            {
                switch (ev.KeyChar)
                {
                    case '7':
                        CycleSets(false);
                        break;
                    case '8':
                        CycleSets();
                        break;
                    case 'l':
                        _orbitLight = !_orbitLight;
                        break;
                }
            };

            // Load assets
            // TODO add asset handling class

            LoadModel("Box", "box.dae");
            LoadModel("Eye", "eyeball.obj");
            LoadModel("Wall", "wall.obj");
            LoadModel("Rocks", "wall.obj");
            LoadModel("Floor", "floor.obj");

            LoadShader("Single color", "SingleColor");
            LoadShader("Vertex colors", "VertexColors");
            LoadShader("Texture mapping", "TextureMapping");
            LoadShader("Phong shading", "Phong");
            LoadShader("Normal mapping", "NormalMapping");
            LoadShader("Parallax mapping", "ParallaxMapping");

            CreateMaterial("Single color", _shaders["Single color"]);
            CreateMaterial("Vertex colors", _shaders["Vertex colors"]);

            CreateMaterial("Eye diffuse", _shaders["Texture mapping"])
                .Texture(Material.TextureType.Diffuse, "eyeball_diffuse.png");
            CreateMaterial("Eye Phong", _shaders["Phong shading"])
                .Texture(Material.TextureType.Diffuse, "eyeball_diffuse.png")
                .Texture(Material.TextureType.Specular, "eyeball_specular.png");
            CreateMaterial("Eye normal", _shaders["Normal mapping"])
                .Texture(Material.TextureType.Diffuse, "eyeball_diffuse.png")
                .Texture(Material.TextureType.Specular, "eyeball_specular.png")
                .Texture(Material.TextureType.Normal, "eyeball_normal.png");

            CreateMaterial("Rock", _shaders["Texture mapping"])
                .Texture(Material.TextureType.Diffuse, "floor_albedo_ao.png");
            CreateMaterial("Rock parallax", _shaders["Parallax mapping"])
                .Texture(Material.TextureType.Diffuse, "floor_albedo_ao.png")
                .Texture(Material.TextureType.Specular, "floor_specular2.png")
                .Texture(Material.TextureType.Normal, "floor_normal.png")
                .Texture(Material.TextureType.Height, "floor_height.png");

            CreateSet(new ModelInstance(_models["Box"], _materials["Single color"]));
            CreateSet(new ModelInstance(_models["Box"], _materials["Vertex colors"]));
            CreateSet(new ModelInstance(_models["Eye"], _materials["Eye diffuse"]));
            CreateSet(new ModelInstance(_models["Eye"], _materials["Eye Phong"])).SetLight(new PointLight(5, 1, 10));
            CreateSet(new ModelInstance(_models["Eye"], _materials["Eye normal"])).SetLight(new PointLight(5, 1, 10));
            CreateSet(new ModelInstance(_models["Rocks"], _materials["Rock"]));
            CreateSet(new ModelInstance(_models["Rocks"], _materials["Rock parallax"])).SetLight(new PointLight(1, 5, -10));

            /*CreateSet(
                new ModelInstance(_models["Floor"], _shaders["Single color"]).Move(0, -0.5f, 0),
                new ModelInstance(_models["Box"], _shaders["Single color"]));*/

            _currentSet = _sets.Last();

            _gui = new GUI(this);
        }

        protected override void OnUnload(EventArgs e)
        {
            foreach (var set in _sets)
                set.Unload();

            foreach (var shader in _shaders.Values)
                shader.Unload();

            foreach (var model in _models.Values)
                model.Unload();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(0, 0, Width, Height);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            _currentSet?.Draw();
            _gui?.Draw(e.Time);

            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            // Orbit the light around the origin of the scene
            if (_orbitLight && _currentSet.Light != null)
                _currentSet.Light.Position = Vector3.Transform(_currentSet.Light.Position, Matrix4.CreateRotationY(0.01f));
        }

        private Shader LoadShader(string name, string path)
        {
            var shader = new Shader(this, path);
            _shaders.Add(name, shader);
            return shader;
        }

        private Model LoadModel(string name, string path)
        {
            var model = new Model(path);
            _models.Add(name, model);
            return model;
        }

        private Material CreateMaterial(string name, Shader shader)
        {
            var material = new Material(shader);
            _materials.Add(name, material);
            return material;
        }

        private Set CreateSet(params ModelInstance[] instances)
        {
            var set = new Set(instances);
            _sets.Add(set);
            return set;
        }

        private void CycleSets(bool forward = true)
        {
            var index = _sets.IndexOf(_currentSet) + (forward ? 1 : -1);

            if (index >= _sets.Count)
                index = 0;
            else if (index < 0)
                index = _sets.Count - 1;

            _currentSet = _sets[index];

            Console.WriteLine($"Switched to set #{index}: {_currentSet.ToString()}");
        }

        private void UpdateViewMatrix()
        {
            var eye = new Vector3(
                _cameraDistance * (float) Math.Cos(_cameraRotation),
                0,
                _cameraDistance * (float) Math.Sin(_cameraRotation));

            ViewMatrix = ViewMatrix = Matrix4.LookAt(eye, Vector3.Zero, Vector3.UnitY);
        }
    }
}
