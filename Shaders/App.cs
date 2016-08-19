using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

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

        private Set _currentSet;

        private readonly Dictionary<string, Shader> _shaders = new Dictionary<string, Shader>();
        private readonly Dictionary<string, Model> _models = new Dictionary<string, Model>();
        private readonly List<Set> _sets = new List<Set>();

        public App(int width = 900, int height = 900)
            : base(width, height, OpenTK.Graphics.GraphicsMode.Default, "Shaders study")
        {
            Instance = this;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            InitGL();

            //LoadShader("Single color", "SingleColor");
            //LoadShader("Vertex colors", "VertexColors");
            //LoadShader("Texture mapping", "TextureMapping");
            //LoadShader("Phong shading", "Phong");
            //LoadShader("Normal mapping", "NormalMapping");
            LoadShader("Parallax mapping", "ParallaxMapping");

            LoadModel("Box", "Models/box.obj");
            LoadModel("Eye", "Models/eyeball.obj").Textures(diffuse: "Models/eyeball_diffuse.png", specular: "Models/eyeball_specular.png", normal: "Models/eyeball_normal.png");
            LoadModel("Wall", "Models/wall.obj").Textures(diffuse: "Models/wall_diffuse.jpg", specular: "Models/black_specular.png", normal: "Models/wall_normal.jpg", height: "Models/wall_height.jpg");
            LoadModel("Rocks", "Models/wall.obj").Textures(diffuse: "Models/floor_albedo_ao.png", specular: "Models/floor_specular2.png", normal: "Models/floor_normal.png", height: "Models/floor_height.png");

            //CreateSet(new ModelInstance(_models["Box"], _shaders["Single color"]));
            //CreateSet(new ModelInstance(_models["Box"], _shaders["Vertex colors"]));
            //CreateSet(new ModelInstance(_models["Eye"], _shaders["Texture mapping"]));
            //CreateSet(new ModelInstance(_models["Eye"], _shaders["Phong shading"]));
            //CreateSet(new ModelInstance(_models["Eye"], _shaders["Normal mapping"]));
            CreateSet(new ModelInstance(_models["Rocks"], _shaders["Parallax mapping"]));
            CreateSet(new ModelInstance(_models["Rocks"], _shaders["Parallax mapping"]));

            /*CreateSet(
                new ModelInstance(_models["Box"], _shaders["Single color"]).Move(Matrix4.CreateTranslation(5, 0, 0)),
                new ModelInstance(_models["Box"], _shaders["Single color"]).Move(Matrix4.CreateTranslation(0, 5, 0)));*/

            _currentSet = _sets.Last();
            
            ViewMatrix = Matrix4.LookAt(Vector3.UnitZ, Vector3.Zero, Vector3.UnitY);
            ProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver2, Width / (float)Height, 0.005f, 100f);

            SetupInput();
        }

        private void SetupInput()
        {
            MouseMove += (sender, ev) =>
            {
                _cameraRotation += _cameraAngularSpeed * ev.XDelta;
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
        }

        private void UpdateViewMatrix()
        {
            var eye = new Vector3(
                _cameraDistance * (float) Math.Cos(_cameraRotation),
                0,
                _cameraDistance * (float) Math.Sin(_cameraRotation));

            ViewMatrix = ViewMatrix = Matrix4.LookAt(eye, Vector3.Zero, Vector3.UnitY);
        }

        private void CycleSets(bool forward = true)
        {
            var index = _sets.IndexOf(_currentSet) + 1;

            if (index >= _sets.Count)
                index = 0;
            else if (index < 0)
                index = _sets.Count - 1;

            _currentSet = _sets[index];
        }

        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);

            foreach (var set in _sets)
                set.Unload();

            // TODO delete shaders
            //GL.DeleteProgram(CurrentShader.Program);
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

            _currentSet.Draw();

            SwapBuffers();
        }

        private void InitGL()
        {
            int major, minor;
            GL.GetInteger(GetPName.MajorVersion, out major);
            GL.GetInteger(GetPName.MinorVersion, out minor);
            Console.WriteLine($"OpenGL version: {major}.{minor}");

            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);
            GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
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

        private Set CreateSet(params ModelInstance[] instances)
        {
            var set = new Set(instances);
            _sets.Add(set);
            return set;
        }
    }
}
