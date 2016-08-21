using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Shaders
{
    /// <summary>
    ///     Visual appearance of an object.
    ///     This gathers a shader, textures and possibly additional parameters (colors, lighting response...).
    /// </summary>
    public class Material
    {
        public enum TextureType
        {
            Diffuse = TextureUnit.Texture0,
            Specular,
            Normal,
            Height
        }

        public Shader Shader { get; }

        private readonly Dictionary<TextureType, int> _textureHandles = new Dictionary<TextureType, int>();

        public Material(Shader shader)
        {
            Shader = shader;
        }

        public Material Texture(TextureType type, string path)
        {
            var handle = CreateTexture(path, (TextureUnit) type, Shader.GetTextureLocation(type));
            _textureHandles.Add(type, handle);
            return this;
        }

        private int CreateTexture(string path, TextureUnit unit, int uniformLocation)
        {
            GL.ActiveTexture(unit);

            // Generate an ID and bind the texture
            var textureHandle = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, textureHandle);
            GL.Uniform1(uniformLocation, unit - TextureUnit.Texture0); // layout(binding=?) must match in the shader

            // Load the texture data
            var fullPath = Path.Combine(App.AssetsDirectory, path);
            var bitmap = new Bitmap(fullPath);
            var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bitmapData.Width, bitmapData.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bitmapData.Scan0);
            bitmap.UnlockBits(bitmapData);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Linear);

            Console.WriteLine($"Loaded texture {fullPath} with ID {textureHandle}, uniform location {uniformLocation}, unit {unit}");

            return textureHandle;
        }

        internal void Bind()
        {
            foreach (var texture in _textureHandles)
            {
                var type = texture.Key;
                var unit = (TextureUnit) type;
                var handle = texture.Value;

                GL.ActiveTexture(unit);
                GL.BindTexture(TextureTarget.Texture2D, handle);
                GL.Uniform1(Shader.GetTextureLocation(type), unit - TextureUnit.Texture0); // layout(binding=?) must match in the shader
            }
        }
    }
}
