using UnityEngine;

namespace PlateauToolkit.Rendering.ImageProcessing
{
    public static class PlateauRenderingTextureUtilities
    {
        public static Texture2D ConvertToTexture2D(RenderTexture rTex)
        {
            Texture2D tex = new Texture2D(rTex.width, rTex.height, TextureFormat.RGBA32, false);
            RenderTexture.active = rTex;
            tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
            tex.Compress(true);
            tex.Apply();
            RenderTexture.active = null;
            return tex;
        }

        /// <summary>
        /// This method returns a new instance of a texture after converting it to a Texture2D, if the texture is already Texture2D
        /// it will create a new instance and copy the pixel information from the original Texture.
        /// The caller should manage the texture lifecycle.
        /// </summary>
        /// <param name="texture"></param>
        /// <returns></returns>
        public static Texture2D ConvertToTexture2D(Texture texture)
        {
            Texture2D newTex;

            if (texture is Texture2D texture2D)
            {
                // Create a new Texture2D instance with the same dimensions and format
                newTex = new Texture2D(texture2D.width, texture2D.height, texture2D.format, texture2D.mipmapCount > 1);

                // Copy the pixels from the existing texture2D to the newTex
                Graphics.CopyTexture(texture2D, newTex);
            }
            else
            {
                RenderTexture tempRenderTexture = RenderTexture.GetTemporary(
                    texture.width,
                    texture.height,
                    0,
                    RenderTextureFormat.Default,
                    RenderTextureReadWrite.Linear);

                RenderTexture previous = RenderTexture.active;
                RenderTexture.active = tempRenderTexture;

                Graphics.Blit(texture, tempRenderTexture);

                newTex = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
                newTex.ReadPixels(new Rect(0, 0, tempRenderTexture.width, tempRenderTexture.height), 0, 0);
                newTex.Apply();

                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(tempRenderTexture);
            }

            return newTex;
        }

    }
}
