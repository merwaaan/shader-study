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

        private Set CurrentSet { get; set; }

        private readonly Dictionary<string, Shader> _shaders = new Dictionary<string, Shader>();
        private readonly Dictionary<string, Model> _models = new Dictionary<string, Model>();
        private readonly List<Set> _sets = new List<Set>();

        public Matrix4 ViewMatrix;
        public Matrix4 ProjectionMatrix;

        public App(int width = 900, int height = 900)
            : base(width, height, OpenTK.Graphics.GraphicsMode.Default, "Shaders study")
        {
            Instance = this;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            InitGL();

            LoadShader("Single color", "SingleColor");
            LoadShader("Vertex colors", "VertexColors");
            LoadShader("Texture mapping", "TextureMapping");
            /*LoadShader("Phong shading", "Phong");
            LoadShader("Normal mapping", "NormalMapping");
            LoadShader("Parallax mapping", "ParallaxMapping");*/

            LoadModel("Box", "Models/box.obj");
            //LoadModel("Eye", "Models/eyeball.obj");
            /*LoadModel("Wall", "Models/wall.obj").Textures(diffuse: "Models/wall_diffuse.jpg", specular: "Models/black_specular.png", normal: "Models/wall_normal.jpg", height: "Models/wall_height.jpg");
            LoadModel("Floor", "Models/floor.obj").Textures(diffuse: "Models/wall_diffuse.jpg", specular: "Models/black_specular.png", normal: "Models/wall_normal.jpg", height: "Models/wall_height.jpg");*/

            CreateSet(new ModelInstance(_models["Box"], _shaders["Single color"]));
            CreateSet(new ModelInstance(_models["Box"], _shaders["Vertex colors"]));
            //CreateSet(new ModelInstance(_models["Eye"], _shaders["Texture mapping"]));
            //CreateSet(new ModelInstance(_models["Eye"], _shaders["Single color"]));

            /*CreateSet(
                new ModelInstance(_models["Box"], _shaders["Single color"]).Move(Matrix4.CreateTranslation(5, 0, 0)),
                new ModelInstance(_models["Box"], _shaders["Single color"]).Move(Matrix4.CreateTranslation(0, 5, 0)));*/

            CurrentSet = _sets.Last();

            /*LoadTexture("Models/wall_diffuse.jpg", TextureUnit.Texture0, CurrentShader.DiffuseMapLocation);
            LoadTexture("Models/black_specular.png", TextureUnit.Texture1, CurrentShader.SpecularMapLocation);
            LoadTexture("Models/wall_normal.jpg", TextureUnit.Texture2, CurrentShader.NormalMapLocation);
            LoadTexture("Models/wall_height.jpg", TextureUnit.Texture3, CurrentShader.HeightMapLocation);*/

            ViewMatrix = Matrix4.LookAt(Vector3.UnitZ, Vector3.Zero, Vector3.UnitY);
            ProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver2, Width / (float)Height, 0.005f, 100f);

            SetupInput();
        }

        private void SetupInput()
        {
            MouseMove += (sender, ev) =>
            {
                if (Math.Abs(ev.XDelta) > 5)
                    return;

                //_modelMatrix *= Matrix4.CreateRotationY(0.005f * ev.XDelta);
                UpdateMatrices();
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

        private void CycleSets(bool forward = true)
        {
            var index = _sets.IndexOf(CurrentSet) + 1;

            if (index >= _sets.Count)
                index = 0;
            else if (index < 0)
                index = _sets.Count - 1;

            CurrentSet = _sets[index];
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

            CurrentSet.Draw();

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
