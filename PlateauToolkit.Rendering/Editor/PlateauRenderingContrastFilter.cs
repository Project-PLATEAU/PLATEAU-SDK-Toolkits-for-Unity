using UnityEngine;

namespace PlateauToolkit.Rendering.ImageProcessing
{
    public class PlateauRenderingContrastFilter : IPlateauRenderingImageFilter
    {
        public int MinValue => -100;
        public int MaxValue => 100;
        public int DefaultValue => 0;

        static readonly int s_ShaderContrastFactorProperty = Shader.PropertyToID("contrastFactor");

        public Texture2D ApplyFilter(Texture source, ComputeShader computeShader, float contrastFactor)
        {
            PlateauRenderingFilterLibrary.Initialize(computeShader);

            RenderTexture renderTexture = source.ToRenderTexture();
            contrastFactor = Remap(contrastFactor, MinValue, MaxValue, 0.1f, 2.5f);

            computeShader.SetFloat(s_ShaderContrastFactorProperty, contrastFactor);
            RenderTexture blurredTexture = renderTexture.ContrastFilterExtension();

            Texture2D outputTexture = PlateauRenderingTextureUtilities.ConvertToTexture2D(blurredTexture);

            renderTexture.Release();

            return outputTexture;
        }

        /// <summary>
        /// Refer to this link for details https://docs.unity3d.com/ja/Packages/com.unity.shadergraph@10.0/manual/Remap-Node.html
        /// </summary>
        /// <param name="value"></param>
        /// <param name="from1"></param>
        /// <param name="to1"></param>
        /// <param name="from2"></param>
        /// <param name="to2"></param>
        /// <returns></returns>
        float Remap(float value, float from1, float to1, float from2, float to2)
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }
    }

}