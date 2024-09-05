using PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib;
using System;
using UnityEngine;

namespace ProceduralToolkit
{
    /// <summary>
    /// Texture extensions
    /// </summary>
    public static class TextureE
    {
        #region DrawLine

        /// <summary>
        /// Draws a line on the texture
        /// </summary>
        public static void DrawLine(this Texture2D texture, Vector2Int v0, Vector2Int v1, Color color)
        {
            DrawLine(texture, v0.x, v0.y, v1.x, v1.y, color);
        }

        /// <summary>
        /// Draws a line on the texture
        /// </summary>
        public static void DrawLine(this Texture2D texture, int x0, int y0, int x1, int y1, Color color)
        {
            if (texture == null)
            {
                throw new ArgumentNullException(nameof(texture));
            }
            Draw.RasterLine(x0, y0, x1, y1, (x, y) => texture.SetPixel(x, y, color));
        }

        /// <summary>
        /// Draws a line on pixels
        /// </summary>
        public static void DrawLine(this Color[] pixels, int textureWidth, Vector2Int v0, Vector2Int v1, Color color)
        {
            DrawLine(pixels, textureWidth, v0.x, v0.y, v1.x, v1.y, color);
        }

        /// <summary>
        /// Draws a line on pixels
        /// </summary>
        public static void DrawLine(this Color[] pixels, int textureWidth, int x0, int y0, int x1, int y1, Color color)
        {
            if (pixels == null)
            {
                throw new ArgumentNullException(nameof(pixels));
            }
            Draw.RasterLine(x0, y0, x1, y1, (x, y) => pixels[x + y*textureWidth] = color);
        }

        #endregion DrawLine

        #region DrawAALine

        /// <summary>
        /// Draws an anti-aliased line on the texture
        /// </summary>
        public static void DrawAALine(this Texture2D texture, Vector2Int v0, Vector2Int v1, Color color)
        {
            DrawAALine(texture, v0.x, v0.y, v1.x, v1.y, color);
        }

        /// <summary>
        /// Draws an anti-aliased line on the texture
        /// </summary>
        public static void DrawAALine(this Texture2D texture, int x0, int y0, int x1, int y1, Color color)
        {
            if (texture == null)
            {
                throw new ArgumentNullException(nameof(texture));
            }
            Draw.RasterAALine(x0, y0, x1, y1, (x, y, t) => texture.SetPixel(x, y, Color.Lerp(texture.GetPixel(x, y), color, t)));
        }

        /// <summary>
        /// Draws an anti-aliased line on pixels
        /// </summary>
        public static void DrawAALine(this Color[] pixels, int textureWidth, Vector2Int v0, Vector2Int v1, Color color)
        {
            DrawAALine(pixels, textureWidth, v0.x, v0.y, v1.x, v1.y, color);
        }

        /// <summary>
        /// Draws an anti-aliased line on pixels
        /// </summary>
        public static void DrawAALine(this Color[] pixels, int textureWidth, int x0, int y0, int x1, int y1, Color color)
        {
            if (pixels == null)
            {
                throw new ArgumentNullException(nameof(pixels));
            }
            Draw.RasterAALine(x0, y0, x1, y1, (x, y, t) => pixels[x + y*textureWidth] = Color.Lerp(pixels[x + y*textureWidth], color, t));
        }

        #endregion DrawAALine

        #region DrawCircle

        /// <summary>
        /// Draws a circle on the texture
        /// </summary>
        public static void DrawCircle(this Texture2D texture, Vector2Int center, int radius, Color color)
        {
            DrawCircle(texture, center.x, center.y, radius, color);
        }

        /// <summary>
        /// Draws a circle on the texture
        /// </summary>
        public static void DrawCircle(this Texture2D texture, int centerX, int centerY, int radius, Color color)
        {
            if (texture == null)
            {
                throw new ArgumentNullException(nameof(texture));
            }
            Draw.RasterCircle(centerX, centerY, radius, (x, y) => texture.SetPixel(x, y, color));
        }

        /// <summary>
        /// Draws a circle on pixels
        /// </summary>
        public static void DrawCircle(this Color[] pixels, int textureWidth, Vector2Int center, int radius, Color color)
        {
            DrawCircle(pixels, textureWidth, center.x, center.y, radius, color);
        }

        /// <summary>
        /// Draws a circle on pixels
        /// </summary>
        public static void DrawCircle(this Color[] pixels, int textureWidth, int centerX, int centerY, int radius, Color color)
        {
            if (pixels == null)
            {
                throw new ArgumentNullException(nameof(pixels));
            }
            Draw.RasterCircle(centerX, centerY, radius, (x, y) => pixels[x + y*textureWidth] = color);
        }

        #endregion DrawCircle

        #region DrawFilledCircle

        /// <summary>
        /// Draws a filled circle on the texture using Bresenham's algorithm
        /// </summary>
        public static void DrawFilledCircle(this Texture2D texture, Vector2Int center, int radius, Color color)
        {
            DrawFilledCircle(texture, center.x, center.y, radius, color);
        }

        /// <summary>
        /// Draws a filled circle on the texture using Bresenham's algorithm
        /// </summary>
        public static void DrawFilledCircle(this Texture2D texture, int centerX, int centerY, int radius, Color color)
        {
            if (texture == null)
            {
                throw new ArgumentNullException(nameof(texture));
            }
            Draw.RasterFilledCircle(centerX, centerY, radius, (x, y) => texture.SetPixel(x, y, color));
        }

        /// <summary>
        /// Draws a filled circle on pixels using Bresenham's algorithm
        /// </summary>
        public static void DrawFilledCircle(this Color[] pixels, int textureWidth, Vector2Int center, int radius, Color color)
        {
            DrawFilledCircle(pixels, textureWidth, center.x, center.y, radius, color);
        }

        /// <summary>
        /// Draws a filled circle on pixels using Bresenham's algorithm
        /// </summary>
        public static void DrawFilledCircle(this Color[] pixels, int textureWidth, int centerX, int centerY, int radius, Color color)
        {
            if (pixels == null)
            {
                throw new ArgumentNullException(nameof(pixels));
            }
            Draw.RasterFilledCircle(centerX, centerY, radius, (x, y) => pixels[x + y*textureWidth] = color);
        }

        #endregion DrawFilledCircle

        #region DrawRect

        /// <summary>
        /// Draws a filled rectangle on the texture
        /// </summary>
        public static void DrawRect(this Texture2D texture, RectInt rect, Color color)
        {
            DrawRect(texture, rect.x, rect.y, rect.width, rect.height, color);
        }

        /// <summary>
        /// Draws a filled rectangle on the texture
        /// </summary>
        public static void DrawRect(this Texture2D texture, int x, int y, int blockWidth, int blockHeight, Color color)
        {
            if (texture == null)
            {
                throw new ArgumentNullException(nameof(texture));
            }
            var pixels = new Color[blockWidth*blockHeight];
            for (int _y = 0; _y < blockHeight; _y++)
            {
                for (int _x = 0; _x < blockWidth; _x++)
                {
                    pixels[_x + _y*blockWidth] = color;
                }
            }
            texture.SetPixels(x, y, blockWidth, blockHeight, pixels);
        }

        /// <summary>
        /// Draws a filled rectangle on pixels
        /// </summary>
        public static void DrawRect(this Color[] pixels, int textureWidth, RectInt rect, Color color)
        {
            DrawRect(pixels, textureWidth, rect.x, rect.y, rect.width, rect.height, color);
        }

        /// <summary>
        /// Draws a filled rectangle on pixels
        /// </summary>
        public static void DrawRect(this Color[] pixels, int textureWidth, int x, int y, int blockWidth, int blockHeight, Color color)
        {
            if (pixels == null)
            {
                throw new ArgumentNullException(nameof(pixels));
            }
            for (int _y = y; _y < y + blockHeight; _y++)
            {
                for (int _x = x; _x < x + blockWidth; _x++)
                {
                    pixels[_x + _y*textureWidth] = color;
                }
            }
        }

        #endregion DrawRect

        #region DrawGradient

        /// <summary>
        /// Fills the texture with a gradient
        /// </summary>
        public static void DrawGradient(this Texture2D texture, Gradient gradient, Directions direction)
        {
            DrawGradient(texture, 0, 0, texture.width, texture.height, gradient, direction);
        }

        /// <summary>
        /// Draws a gradient rectangle on the texture
        /// </summary>
        public static void DrawGradient(this Texture2D texture, RectInt rect, Gradient gradient, Directions direction)
        {
            DrawGradient(texture, rect.x, rect.y, rect.width, rect.height, gradient, direction);
        }

        /// <summary>
        /// Draws a gradient rectangle on the texture
        /// </summary>
        public static void DrawGradient(this Texture2D texture, int x, int y, int blockWidth, int blockHeight, Gradient gradient,
            Directions direction)
        {
            if (texture == null)
            {
                throw new ArgumentNullException(nameof(texture));
            }

            var pixels = new Color[blockWidth*blockHeight];
            Gradient(pixels, blockWidth, 0, 0, blockWidth, blockHeight, gradient, direction);
            texture.SetPixels(x, y, blockWidth, blockHeight, pixels);
        }

        /// <summary>
        /// Fills pixels with a gradient
        /// </summary>
        public static void DrawGradient(this Color[] pixels, int textureWidth, int textureHeight, Gradient gradient, Directions direction)
        {
            DrawGradient(pixels, textureWidth, 0, 0, textureWidth, textureHeight, gradient, direction);
        }

        /// <summary>
        /// Draws a gradient rectangle on pixels
        /// </summary>
        public static void DrawGradient(this Color[] pixels, int textureWidth, RectInt rect, Gradient gradient, Directions direction)
        {
            DrawGradient(pixels, textureWidth, rect.x, rect.y, rect.width, rect.height, gradient, direction);
        }

        /// <summary>
        /// Draws a gradient rectangle on pixels
        /// </summary>
        public static void DrawGradient(this Color[] pixels, int textureWidth, int x, int y, int blockWidth, int blockHeight, Gradient gradient,
            Directions direction)
        {
            if (pixels == null)
            {
                throw new ArgumentNullException(nameof(pixels));
            }
            Gradient(pixels, textureWidth, x, y, blockWidth, blockHeight, gradient, direction);
        }

        private static void Gradient(Color[] pixels, int textureWidth, int x, int y, int blockWidth, int blockHeight, Gradient gradient,
            Directions direction)
        {
            Func<int, int, Color> getColor;
            switch (direction)
            {
                case Directions.Left:
                    getColor = (_x, _y) => gradient.Evaluate(1 - (float) (_x - x)/(float) blockWidth);
                    break;
                case Directions.Right:
                    getColor = (_x, _y) => gradient.Evaluate((float) (_x - x)/(float) blockWidth);
                    break;
                case Directions.Down:
                    getColor = (_x, _y) => gradient.Evaluate(1 - (float) (_y - y)/(float) blockHeight);
                    break;
                case Directions.Up:
                    getColor = (_x, _y) => gradient.Evaluate((float) (_y - y)/(float) blockHeight);
                    break;
                default:
                    throw new ArgumentException("Not supported direction: " + direction, nameof(direction));
            }

            for (int _y = y; _y < y + blockHeight; _y++)
            {
                for (int _x = x; _x < x + blockWidth; _x++)
                {
                    pixels[_x + _y*textureWidth] = getColor(_x, _y);
                }
            }
        }

        #endregion DrawGradient

        #region Clear

        /// <summary>
        /// Fills the texture with white color
        /// </summary>
        public static void Clear(this Texture2D texture)
        {
            Clear(texture, Color.white);
        }

        /// <summary>
        /// Fills the texture with specified color
        /// </summary>
        public static void Clear(this Texture2D texture, Color color)
        {
            if (texture == null)
            {
                throw new ArgumentNullException(nameof(texture));
            }
            var pixels = new Color[texture.width*texture.height];
            for (var i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }
            texture.SetPixels(pixels);
        }

        /// <summary>
        /// Fills pixels with white color
        /// </summary>
        public static void Clear(this Color[] pixels)
        {
            Clear(pixels, Color.white);
        }

        /// <summary>
        /// Fills pixels with specified color
        /// </summary>
        public static void Clear(this Color[] pixels, Color color)
        {
            if (pixels == null)
            {
                throw new ArgumentNullException(nameof(pixels));
            }
            for (var i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }
        }

        #endregion Clear
    }
}
