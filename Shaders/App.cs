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

        public Set CurrentSet;
        public int CurrentSetIndex => _sets.IndexOf(CurrentSet);

        public int ShadowMapHandle { get; private set; }
        public float ShadowBias;

        // TODO create asset handling class
        private readonly Dictionary<string, Model> _models = new Dictionary<string, Model>();
        private readonly Dictionary<string, Shader> _shaders = new Dictionary<string, Shader>();
        private readonly Dictionary<string, Material> _materials = new Dictionary<string, Material>();
        private readonly List<Set> _sets = new List<Set>();

        private int _shadowFboHandle;

        private Camera _camera;

        private GUI _gui;

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

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);

            // Setup shadow mapping

            ShadowMapHandle = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, ShadowMapHandle);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent, 1000, 1000, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int) TextureWrapMode.ClampToBorder);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int) TextureWrapMode.ClampToBorder);

            _shadowFboHandle = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _shadowFboHandle);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, ShadowMapHandle, 0);
            GL.DrawBuffer(DrawBufferMode.None);
            GL.ReadBuffer(ReadBufferMode.None);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            
            // Setup inputs

            Mouse.Move += (sender, ev) =>
            {
                _camera?.OnMouseMove(ev);
                _gui?.OnMouseMove(ev);
            };

            Mouse.ButtonDown += (sender, ev) =>
            {
                _gui?.OnMouseButton(ev);
            };

            Mouse.ButtonUp += (sender, ev) =>
            {
                _gui?.OnMouseButton(ev);
            };

            Mouse.WheelChanged += (sender, ev) =>
            {
                _camera?.OnMouseWheel(ev);
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
                        if (CurrentSet != null)
                            CurrentSet.OrbitLight = !CurrentSet.OrbitLight;
                        break;
                }
                
                _gui?.OnKeyPress(ev);
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
            LoadShader("Depth map", "DepthMap");
            LoadShader("Shadow mapping", "ShadowMapping");
            LoadShader("Points to quads")
                .Vertex("SingleColor")
                .Geometry("PointsToQuad")
                .Fragment("SingleColor")
                .Build();
            LoadShader("Points to textured quads")
                .Vertex("TextureMapping")
                .Geometry("PointsToQuad")
                .Fragment("TextureMapping")
                .Build();

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

            CreateMaterial("Brick", _shaders["Shadow mapping"])
                .Texture(Material.TextureType.Diffuse, "brick_diffuse.jpg")
                .Texture(Material.TextureType.Normal, "brick_normal.jpg")
                .Texture(Material.TextureType.Height, "brick_height.jpg");

            CreateMaterial("Points to quads", _shaders["Points to quads"]);
            CreateMaterial("Points to textured quads", _shaders["Points to textured quads"])
                .Texture(Material.TextureType.Diffuse, "star.png");

            CreateSet(new ModelInstance(_models["Box"], _materials["Single color"])).SetLight(new PointLight(5, 1, 10));
            CreateSet(new ModelInstance(_models["Box"], _materials["Vertex colors"]));
            CreateSet(new ModelInstance(_models["Eye"], _materials["Eye diffuse"]));
            CreateSet(new ModelInstance(_models["Eye"], _materials["Eye Phong"])).SetLight(new PointLight(5, 1, 10));
            CreateSet(new ModelInstance(_models["Eye"], _materials["Eye normal"])).SetLight(new PointLight(5, 1, 10));
            CreateSet(new ModelInstance(_models["Rocks"], _materials["Rock"]));
            CreateSet(new ModelInstance(_models["Rocks"], _materials["Rock parallax"])).SetLight(new PointLight(1, 5, -10));

            CreateSet(
                new ModelInstance(_models["Eye"], _materials["Eye diffuse"]).Move(0, 0.25f, 0),
                new ModelInstance(_models["Floor"], _materials["Brick"]).Move(0, -0.5f, 0))
                .SetLight(new PointLight(0, 3, 0.1f));

            CreateSet(new ParticleSystem(_materials["Points to quads"]).Add(500));
            CreateSet(new ParticleSystem(_materials["Points to textured quads"]).Add(500));

            CurrentSet = _sets.Last();

            _camera = new Camera(this);

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

            RenderShadowMap();
            RenderScene();

            _gui?.Draw(e.Time);

            SwapBuffers();
        }

        private void RenderShadowMap()
        {
            if (CurrentSet?.Light == null)
                return;

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, _shadowFboHandle);
            
            GL.Clear(ClearBufferMask.DepthBufferBit);
            GL.Viewport(0, 0, 1000, 1000);
            
            // Render the scene from the light's perspective, with the shadow map shader
            CurrentSet?.Draw(CurrentSet.Light, _shaders["Depth map"]);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        private void RenderScene()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Viewport(0, 0, Width, Height);

            CurrentSet?.Draw(_camera);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            CurrentSet?.Update((float)e.Time);

            // Orbit the light around the origin of the scene
            if (CurrentSet?.Light != null && CurrentSet.OrbitLight)
                CurrentSet.Light.Transform = CurrentSet.Light.Transform * Matrix4.CreateRotationY(0.01f);
        }

        private Shader LoadShader(string name, string path)
        {
            var shader = new Shader(this, path);
            _shaders.Add(name, shader);
            return shader;
        }

        private Shader LoadShader(string name)
        {
            var shader = new Shader(this);
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

        private Set CreateSet(params IDrawable[] instances)
        {
            var set = new Set(instances);
            _sets.Add(set);
            return set;
        }

        private void CycleSets(bool forward = true)
        {
            var index = _sets.IndexOf(CurrentSet) + (forward ? 1 : -1);

            if (index >= _sets.Count)
                index = 0;
            else if (index < 0)
                index = _sets.Count - 1;

            CurrentSet = _sets[index];

            Console.WriteLine($"Switched to set #{index}: {CurrentSet.ToString()}");
        }
    }
}
