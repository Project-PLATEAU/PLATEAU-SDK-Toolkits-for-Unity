using UnityEngine;

namespace PlateauToolkit.Rendering.ImageProcessing
{
    public class PlateauRenderingBrightnessFilter : IPlateauRenderingImageFilter
    {
        public int MinValue => -100;
        public int MaxValue => 100;
        public int DefaultValue => 0;

        public Texture2D ApplyFilter(Texture source, ComputeShader computeShader, float brightnessFactor)
        {
            PlateauRenderingFilterLibrary.Initialize(computeShader);

            RenderTexture renderTexture = source.ToRenderTexture();
            brightnessFactor = Remap(brightnessFactor, MinValue, MaxValue, -0.999f, 0.9f);
            computeShader.SetFloat("brightnessFactor", brightnessFactor);
            RenderTexture blurredTexture = renderTexture.BrightnessFilterExtension();

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