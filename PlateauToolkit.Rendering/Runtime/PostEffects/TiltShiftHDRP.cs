#if UNITY_HDRP
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
using System;

public class TiltShiftHDRP : CustomPass
{
    [Serializable]
    public class BlurSettings
    {
        [Range(0, 2)]
        public float blurSize = 0.3f;
        [Range(0, 1)]
        public float blurStartRange = 0.2f;
        [Range(1, 4)]
        public int blurIterations = 2;
        [Range(8, 128)]
        public int blurSamples = 32;
    }

    public BlurSettings settings = new BlurSettings();
    private Material blurMaterial;
    private RTHandle tempRT1;
    private RTHandle tempRT2;

    protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
    {
        name = "TiltShift/ToyCamera";
        targetColorBuffer = TargetBuffer.Camera;
        targetDepthBuffer = TargetBuffer.None;
        clearFlags = ClearFlag.None;

        blurMaterial = CoreUtils.CreateEngineMaterial("Hidden/TiltShiftHDRP");

        tempRT1 = RTHandles.Alloc(
            Vector2.one, TextureXR.slices, dimension: TextureXR.dimension,
            colorFormat: GraphicsFormat.B10G11R11_UFloatPack32, useDynamicScale: true, name: "TempBlur1", wrapMode: TextureWrapMode.Mirror
        );

        tempRT2 = RTHandles.Alloc(
            Vector2.one, TextureXR.slices, dimension: TextureXR.dimension,
            colorFormat: GraphicsFormat.B10G11R11_UFloatPack32, useDynamicScale: true, name: "TempBlur2", wrapMode: TextureWrapMode.Mirror
        );

        Shader.EnableKeyword("UNITY_PIPELINE_HDRP");
    }

    protected override void Execute(CustomPassContext ctx)
    {
        if (blurMaterial == null) return;

        blurMaterial.SetFloat("_BlurSize", settings.blurSize);
        blurMaterial.SetFloat("_BlurStartRange", settings.blurStartRange);
        blurMaterial.SetInt("_Samples", settings.blurSamples);

        ctx.cmd.Blit(ctx.cameraColorBuffer, tempRT1, 0, 0);
        blurMaterial.SetTexture("_colorBuffer", ctx.cameraColorBuffer);

        for (int i = 0; i < settings.blurIterations; i++)
        {
            blurMaterial.SetTexture("_blurBuffer", tempRT1);
            ctx.cmd.Blit(tempRT1, tempRT2, blurMaterial, 0);
            ctx.cmd.Blit(tempRT2, tempRT1, 0, 0);
        }
        ctx.cmd.Blit(tempRT1, ctx.cameraColorBuffer, 0, 0);
    }

    protected override void Cleanup()
    {
        CoreUtils.Destroy(blurMaterial);
        tempRT1.Release();
        tempRT2.Release();
    }
}
#endif