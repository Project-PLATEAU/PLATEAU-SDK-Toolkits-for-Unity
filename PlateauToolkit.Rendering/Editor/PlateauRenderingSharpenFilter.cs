using UnityEngine;

namespace PlateauToolkit.Rendering.ImageProcessing
{
    public class PlateauRenderingSharpenFilter : IPlateauRenderingImageFilter
    {
        public int MinValue => 0;
        public int MaxValue => 100;
        public int DefaultValue => 0;

        public Texture2D ApplyFilter(Texture source, ComputeShader computeShader, float sharpnessFactor)
        {
            PlateauRenderingFilterLibrary.Initialize(computeShader);

            RenderTexture renderTexture = source.ToRenderTexture();
            sharpnessFactor = Remap(sharpnessFactor, MinValue, MaxValue, 0.0f, 1.0f);
            computeShader.SetFloat("sharpnessFactor", sharpnessFactor);
            RenderTexture blurredTexture = renderTexture.SharpenessFilterExtension();

            Texture2D outputTexture = PlateauRenderingTextureUtilities.ConvertToTexture2D(blurredTexture);

            renderTexture.Release();

            return outputTexture;
        }

        float Remap(float value, float from1, float to1, float from2, float to2)
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }
    }

}