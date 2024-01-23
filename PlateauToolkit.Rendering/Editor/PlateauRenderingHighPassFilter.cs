using UnityEngine;

namespace PlateauToolkit.Rendering.ImageProcessing
{
    public class PlateauRenderingHighPassFilter : IPlateauRenderingImageFilter
    {
        public int MinValue => 0;
        public int MaxValue => 100;
        public int DefaultValue => 0;

        static readonly int s_ShaderRadiusProperty = Shader.PropertyToID("radius");

        public Texture2D ApplyFilter(Texture source, ComputeShader computeShader, float radius)
        {
            PlateauRenderingFilterLibrary.Initialize(computeShader);

            RenderTexture renderTexture = source.ToRenderTexture();

            computeShader.SetFloat(s_ShaderRadiusProperty, radius);
            RenderTexture blurredTexture = renderTexture.HighPassFilterExtension();

            Texture2D outputTexture = PlateauRenderingTextureUtilities.ConvertToTexture2D(blurredTexture);

            renderTexture.Release();

            return outputTexture;
        }
    }
}