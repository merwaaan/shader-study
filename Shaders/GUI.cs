using ImGuiNET;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using OpenTK.Input;

namespace Shaders
{
    // Rendering code from https://github.com/mellinoe/ImGui.NET/blob/1e600bff5426bc59a916aed20d11485a7f4b97d6/src/ImGui.NET.SampleProgram/SampleWindow.cs
    internal class GUI
    {
        private readonly App _app;

        private string ShadowBiasAsText => _app.ShadowBias.ToString();

        public unsafe GUI(App app)
        {
            _app = app;

            ImGui.LoadDefaultFont();

            IO io = ImGui.GetIO();

            // Build texture atlas
            FontTextureData texData = io.FontAtlas.GetTexDataAsAlpha8();

            // Create OpenGL texture
            var fontTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, fontTexture);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
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
            io.FontAtlas.SetTexID(fontTexture);

            io.FontAtlas.ClearTexData();
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public void Draw(double dt)
        {
            var io = ImGui.GetIO();
            io.DeltaTime = (float)dt;
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
            io.MouseDown[(int)ev.Button] = ev.IsPressed;
        }

        internal void OnKeyPress(KeyPressEventArgs ev)
        {
        }

        private void Build()
        {
            ImGui.NewFrame();

            ImGui.SetNextWindowSize(new System.Numerics.Vector2(300, 100), SetCondition.Always);
            ImGui.SetNextWindowPos(new System.Numerics.Vector2(10, 10), SetCondition.Appearing);
            ImGui.BeginWindow($"Scene #{_app.CurrentSetIndex + 1}");

            ImGui.Checkbox("Rotate light", ref _app.CurrentSet.OrbitLight);

            // Scale the values up because imgui behaves weirdly with very small values
            var scaledBias = _app.ShadowBias * 10000;
            if (ImGui.SliderFloat("Shadow bias", ref scaledBias, 0, 10, ShadowBiasAsText, 1))
            {
                _app.ShadowBias = scaledBias / 10000f;
            }

            ImGui.EndWindow();
        }

        private unsafe void Render()
        {
            ImGui.Render();
            var data = ImGui.GetDrawData();

            var displayW = _app.Width;
            var displayH = _app.Height;

            GL.Viewport(0, 0, displayW, displayH);

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
                var cmdList = data->CmdLists[n];
                var vtxBuffer = (byte*)cmdList->VtxBuffer.Data;
                var idxBuffer = (ushort*)cmdList->IdxBuffer.Data;

                GL.VertexPointer(2, VertexPointerType.Float, sizeof(DrawVert), new IntPtr(vtxBuffer + DrawVert.PosOffset));
                GL.TexCoordPointer(2, TexCoordPointerType.Float, sizeof(DrawVert), new IntPtr(vtxBuffer + DrawVert.UVOffset));
                GL.ColorPointer(4, ColorPointerType.UnsignedByte, sizeof(DrawVert), new IntPtr(vtxBuffer + DrawVert.ColOffset));

                for (int i = 0; i < cmdList->CmdBuffer.Size; i++)
                {
                    var pcmd = &(((DrawCmd*)cmdList->CmdBuffer.Data)[i]);
                    if (pcmd->UserCallback != IntPtr.Zero)
                        throw new NotImplementedException();

                    GL.BindTexture(TextureTarget.Texture2D, pcmd->TextureId.ToInt32());
                    GL.Scissor(
                        (int)pcmd->ClipRect.X,
                        (int)(io.DisplaySize.Y - pcmd->ClipRect.W),
                        (int)(pcmd->ClipRect.Z - pcmd->ClipRect.X),
                        (int)(pcmd->ClipRect.W - pcmd->ClipRect.Y));

                    var indices = new ushort[pcmd->ElemCount];
                    for (var j = 0; j < indices.Length; j++)
                        indices[j] = idxBuffer[j];

                    GL.DrawElements(BeginMode.Triangles, (int)pcmd->ElemCount, DrawElementsType.UnsignedShort, new IntPtr(idxBuffer));

                    idxBuffer += pcmd->ElemCount;
                }
            }

            // Restore modified state
            GL.DisableClientState(ArrayCap.ColorArray);
            GL.DisableClientState(ArrayCap.TextureCoordArray);
            GL.DisableClientState(ArrayCap.VertexArray);
            GL.Disable(EnableCap.Blend);
            GL.PopAttrib();
        }
    }
}
