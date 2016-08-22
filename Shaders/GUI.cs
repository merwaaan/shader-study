using ImGuiNET;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using OpenTK.Input;

namespace Shaders
{
    // Rendering code from https://github.com/mellinoe/ImGui.NET/blob/1e600bff5426bc59a916aed20d11485a7f4b97d6/src/ImGui.NET.SampleProgram/SampleWindow.cs
    class GUI
    {
        private readonly App _app;

        private int _fontTexture;
        private System.Numerics.Vector4 _buttonColor = new System.Numerics.Vector4(55f / 255f, 155f / 255f, 1f, 1f);
        private System.Numerics.Vector3 _positionValue = new System.Numerics.Vector3(500);

        public unsafe GUI(App app)
        {
            _app = app;

            ImGui.LoadDefaultFont();

            IO io = ImGui.GetIO();

            // Build texture atlas
            FontTextureData texData = io.FontAtlas.GetTexDataAsAlpha8();

            // Create OpenGL texture
            _fontTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, _fontTexture);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) All.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) All.Linear);
            GL.TexImage2D(
                TextureTarget.Texture2D,
                0,
                PixelInternalFormat.Alpha,
                texData.Width,
                texData.Height,
                0,
                PixelFormat.Alpha,
                PixelType.UnsignedByte,
                new IntPtr(texData.Pixels));

            // Store the texture identifier in the ImFontAtlas substructure.
            io.FontAtlas.SetTexID(_fontTexture);

            io.FontAtlas.ClearTexData();
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public unsafe void Draw(double dt)
        {
            IO io = ImGui.GetIO();
            io.DeltaTime = (float) dt;
            io.DisplaySize = new System.Numerics.Vector2(_app.Width, _app.Height);

            Build();
            Render();
        }

        internal void OnMouseMove(MouseMoveEventArgs ev)
        {
            var io = ImGui.GetIO();
            io.MousePosition = new System.Numerics.Vector2(ev.X, ev.Y);
        }

        internal void OnMouseButton(MouseButtonEventArgs ev)
        {
            var io = ImGui.GetIO();
            io.MouseDown[(int) ev.Button] = ev.IsPressed;
        }

        internal void OnKeyPress(KeyPressEventArgs ev)
        {
        }

        private void Build()
        {
            ImGui.NewFrame();

            //ImGui.GetStyle().WindowRounding = 0;

            ImGui.SetNextWindowSize(new System.Numerics.Vector2(200, 100), SetCondition.Always);
            ImGui.SetNextWindowPos(new System.Numerics.Vector2(10, 10), SetCondition.Appearing);
            ImGui.BeginWindow($"Scene #{_app.CurrentSetIndex + 1}");

            ImGui.Text("Hello,");
            ImGui.Text("Test1");
            ImGui.Text("Test2");

            ImGui.Checkbox("Orbiting light", ref _app.CurrentSet.OrbitLight);

            ImGui.EndWindow();

            /*if (ImGui.GetIO().AltPressed && ImGui.GetIO().KeysDown[(int) Key.F4])
            {
                _nativeWindow.Close();
            }*/
        }

        private unsafe void Render()
        {
            ImGui.Render();
            var data = ImGui.GetDrawData();

            // Rendering
            int display_w, display_h;
            display_w = _app.Width;
            display_h = _app.Height;

            Vector4 clear_color = new Vector4(114f / 255f, 144f / 255f, 154f / 255f, 1.0f);
            GL.Viewport(0, 0, display_w, display_h);

            // We are using the OpenGL fixed pipeline to make the example code simpler to read!
            // Setup render state: alpha-blending enabled, no face culling, no depth testing, scissor enabled, vertex/texcoord/color pointers.
            GL.PushAttrib(AttribMask.EnableBit | AttribMask.ColorBufferBit | AttribMask.TransformBit);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.DepthTest);
            GL.Enable(EnableCap.ScissorTest);
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.EnableClientState(ArrayCap.TextureCoordArray);
            GL.EnableClientState(ArrayCap.ColorArray);
            GL.Enable(EnableCap.Texture2D);

            IO io = ImGui.GetIO();

            // Setup orthographic projection matrix
            GL.MatrixMode(MatrixMode.Projection);
            GL.PushMatrix();
            GL.LoadIdentity();
            GL.Ortho(
                0.0f,
                io.DisplaySize.X / io.DisplayFramebufferScale.X,
                io.DisplaySize.Y / io.DisplayFramebufferScale.Y,
                0.0f,
                -1.0f,
                1.0f);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.PushMatrix();
            GL.LoadIdentity();

            // Render command lists

            for (int n = 0; n < data->CmdListsCount; n++)
            {
                DrawList* cmd_list = data->CmdLists[n];
                byte* vtx_buffer = (byte*) cmd_list->VtxBuffer.Data;
                ushort* idx_buffer = (ushort*) cmd_list->IdxBuffer.Data;

                DrawVert vert0 = *((DrawVert*) vtx_buffer);
                DrawVert vert1 = *(((DrawVert*) vtx_buffer) + 1);
                DrawVert vert2 = *(((DrawVert*) vtx_buffer) + 2);

                GL.VertexPointer(2, VertexPointerType.Float, sizeof(DrawVert), new IntPtr(vtx_buffer + DrawVert.PosOffset));
                GL.TexCoordPointer(2, TexCoordPointerType.Float, sizeof(DrawVert), new IntPtr(vtx_buffer + DrawVert.UVOffset));
                GL.ColorPointer(4, ColorPointerType.UnsignedByte, sizeof(DrawVert), new IntPtr(vtx_buffer + DrawVert.ColOffset));

                for (int cmd_i = 0; cmd_i < cmd_list->CmdBuffer.Size; cmd_i++)
                {
                    DrawCmd* pcmd = &(((DrawCmd*) cmd_list->CmdBuffer.Data)[cmd_i]);
                    if (pcmd->UserCallback != IntPtr.Zero)
                    {
                        throw new NotImplementedException();
                    }
                    else
                    {
                        GL.BindTexture(TextureTarget.Texture2D, pcmd->TextureId.ToInt32());
                        GL.Scissor(
                            (int) pcmd->ClipRect.X,
                            (int) (io.DisplaySize.Y - pcmd->ClipRect.W),
                            (int) (pcmd->ClipRect.Z - pcmd->ClipRect.X),
                            (int) (pcmd->ClipRect.W - pcmd->ClipRect.Y));
                        ushort[] indices = new ushort[pcmd->ElemCount];
                        for (int i = 0; i < indices.Length; i++) { indices[i] = idx_buffer[i]; }
                        GL.DrawElements(BeginMode.Triangles, (int) pcmd->ElemCount, DrawElementsType.UnsignedShort, new IntPtr(idx_buffer));
                    }
                    idx_buffer += pcmd->ElemCount;
                }
            }

            // Restore modified state
            GL.DisableClientState(ArrayCap.ColorArray);
            GL.DisableClientState(ArrayCap.TextureCoordArray);
            GL.DisableClientState(ArrayCap.VertexArray);
            GL.Disable(EnableCap.Blend);
            //GL.BindTexture(TextureTarget.Texture2D, last_texture);
            /*GL.MatrixMode(MatrixMode.Modelview);
            GL.PopMatrix();
            GL.MatrixMode(MatrixMode.Projection);
            GL.PopMatrix();*/
            GL.PopAttrib();
        }
    }
}
