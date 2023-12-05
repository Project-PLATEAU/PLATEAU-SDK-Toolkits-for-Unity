#if UNITY_HDRP
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
using System;

public class HalftoneHDRP : CustomPass
{
    [Serializable]
    public class BlurSettings
    {
        [Range(0.1f, 5)]
        public float halftoneSize = 1.0f;
        [Range(0.1f, 1)]
        public float halftoneRange = 0.3f;
        public bool useColor = true;
    }

    public BlurSettings settings = new BlurSettings();
    private Material material;
    private RTHandle halftoneBuffer;

    protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
    {
        name = "Halftone";
        targetColorBuffer = TargetBuffer.Camera;
        targetDepthBuffer = TargetBuffer.None;
        clearFlags = ClearFlag.None;

        material = CoreUtils.CreateEngineMaterial("Hidden/HalftoneHDRP");

        halftoneBuffer = RTHandles.Alloc(
            Vector2.one, TextureXR.slices, dimension: TextureXR.dimension,
            colorFormat: GraphicsFormat.B10G11R11_UFloatPack32, useDynamicScale: true, name: "Halftone", wrapMode: TextureWrapMode.Mirror
        );

        Shader.EnableKeyword("UNITY_PIPELINE_HDRP");
    }

    protected override void Execute(CustomPassContext ctx)
    {
        if (material == null)
            return;

        material.SetFloat("_Size", settings.halftoneSize);
        material.SetFloat("_Range", settings.halftoneRange);
        material.SetFloat("_UseColor", settings.useColor ? 1 : 0);

        ctx.cmd.Blit(ctx.cameraColorBuffer, halftoneBuffer, 0, 0);
        material.SetTexture("_halftoneBuffer", ctx.cameraColorBuffer);
        ctx.cmd.Blit(halftoneBuffer, halftoneBuffer, material, 0);
        ctx.cmd.Blit(halftoneBuffer, ctx.cameraColorBuffer, 0, 0);
    }

    protected override void Cleanup()
    {
        CoreUtils.Destroy(material);
        halftoneBuffer.Release();
    }
}
#endif