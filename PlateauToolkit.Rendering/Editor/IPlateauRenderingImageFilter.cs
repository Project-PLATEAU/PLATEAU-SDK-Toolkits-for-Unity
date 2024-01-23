using UnityEngine;

namespace PlateauToolkit.Rendering.ImageProcessing
{
    public interface IPlateauRenderingImageFilter
    {
        int MinValue { get; }
        int MaxValue { get; }

        int DefaultValue { get; }

        Texture2D ApplyFilter(Texture source, ComputeShader computeShader, float radius);
    }
}