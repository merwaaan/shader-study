using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Shaders
{
    public class App : GameWindow
    {
        public static App Instance;

        public Matrix4 ViewMatrix;
        public Matrix4 ProjectionMatrix;

        private float _cameraRotation;
        private float _cameraAngularSpeed = 0.005f;
        private float _cameraDistance = 1;
        private float _cameraZoomSpeed = 0.2f;

        private Set _currentSet;

        private readonly Dictionary<string, Model> _models = new Dictionary<string, Model>();
        private readonly Dictionary<string, Shader> _shaders = new Dictionary<string, Shader>();
        private readonly Dictionary<string, Material> _materials = new Dictionary<string, Material>();
        private readonly List<Set> _sets = new List<Set>();

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
                }
            };

            // Load assets
            
            // TODO omit dir
            LoadModel("Box", "Models/box.dae");
            LoadModel("Eye", "Models/eyeball.obj");
            LoadModel("Wall", "Models/wall.obj");
            LoadModel("Rocks", "Models/wall.obj");
            LoadModel("Floor", "Models/floor.obj");
            
            LoadShader("Single color", "SingleColor");
            LoadShader("Vertex colors", "VertexColors");
            LoadShader("Texture mapping", "TextureMapping");
            LoadShader("Phong shading", "Phong");
            LoadShader("Normal mapping", "NormalMapping");
            LoadShader("Parallax mapping", "ParallaxMapping");

            CreateMaterial("Single color", _shaders["Single color"]);
            CreateMaterial("Vertex colors", _shaders["Vertex colors"]);

            CreateMaterial("Eye diffuse", _shaders["Texture mapping"])
                .Texture(Material.TextureType.Diffuse, "Models/eyeball_diffuse.png");
            CreateMaterial("Eye diffuse, specular", _shaders["Phong shading"])
                .Texture(Material.TextureType.Diffuse, "Models/eyeball_diffuse.png")
                .Texture(Material.TextureType.Specular, "Models/eyeball_specular.png");
            CreateMaterial("Eye diffuse, specular, normal", _shaders["Normal mapping"])
                .Texture(Material.TextureType.Diffuse, "Models/eyeball_diffuse.png")
                .Texture(Material.TextureType.Specular, "Models/eyeball_specular.png")
                .Texture(Material.TextureType.Normal, "Models/eyeball_normal.png");

            CreateMaterial("Rock", _shaders["Texture mapping"])
                            .Texture(Material.TextureType.Diffuse, "Models/floor_albedo_ao.png")
                            .Texture(Material.TextureType.Specular, "Models/floor_specular2.png")
                            .Texture(Material.TextureType.Normal, "Models/floor_normal.png")
                            .Texture(Material.TextureType.Height, "Models/floor_height.png");
            CreateMaterial("Rock with parallax", _shaders["Parallax mapping"])
                            .Texture(Material.TextureType.Diffuse, "Models/floor_albedo_ao.png")
                            .Texture(Material.TextureType.Specular, "Models/floor_specular2.png")
                            .Texture(Material.TextureType.Normal, "Models/floor_normal.png")
                            .Texture(Material.TextureType.Height, "Models/floor_height.png");
            
            CreateSet(new ModelInstance(_models["Box"], _materials["Single color"]));
            CreateSet(new ModelInstance(_models["Box"], _materials["Vertex colors"]));
            CreateSet(new ModelInstance(_models["Eye"], _materials["Eye diffuse"]));
            CreateSet(new ModelInstance(_models["Eye"], _materials["Eye diffuse, specular"]));
            CreateSet(new ModelInstance(_models["Eye"], _materials["Eye diffuse, specular, normal"]));
            CreateSet(new ModelInstance(_models["Rocks"], _materials["Rock"]));
            CreateSet(new ModelInstance(_models["Rocks"], _materials["Rock with parallax"]));

            /*CreateSet(
                new ModelInstance(_models["Floor"], _shaders["Single color"]).Move(0, -0.5f, 0),
                new ModelInstance(_models["Box"], _shaders["Single color"]));*/

            _currentSet = _sets.Last();
        }

        protected override void OnUnload(EventArgs e)
        {
            /*foreach (var set in _sets)
                 set.Unload();
                 */
            // TODO delete shaders
            //GL.DeleteProgram(CurrentShader.Program);
        }

        /*protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(0, 0, Width, Height);
        }*/

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            _currentSet.Draw();

            SwapBuffers();
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
