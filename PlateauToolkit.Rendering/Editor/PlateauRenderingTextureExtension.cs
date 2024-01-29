using UnityEngine;

namespace PlateauToolkit.Rendering.ImageProcessing
{
    public static class PlateauRenderingTextureExtension
    {
        public static RenderTexture ToRenderTexture(this Texture texture)
        {
            var renderTexture = new RenderTexture(texture.width, texture.height, 0) { enableRandomWrite = true };
            renderTexture.Create();
            Graphics.Blit(texture, renderTexture);
            return renderTexture;
        }

        public static Texture2D ToTexture2D(this RenderTexture texture)
        {
            var texture2D = new Texture2D(texture.width, texture.height, TextureFormat.ARGB32, false, false);

            RenderTexture.active = texture;
            texture2D.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
            texture2D.Apply();
            RenderTexture.active = null;
            return texture2D;
        }

        public static Vector2Int GetSize(this RenderTexture texture) => new Vector2Int(texture.width, texture.height);

        public static Vector2Int GetSize(this Texture texture) => new Vector2Int(texture.width, texture.height);
    }
}