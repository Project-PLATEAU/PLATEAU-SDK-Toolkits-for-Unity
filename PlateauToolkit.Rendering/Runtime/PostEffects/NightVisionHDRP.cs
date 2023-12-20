#if UNITY_HDRP
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
using System;

public class NightVisionHDRP: CustomPass
{
    [Serializable]
    public class NightVisonSettings
    {
        [Range(0.1f, 1)]
        public float sensitivity = 0.85f;
        public Color color = Color.green;
    }

    public  NightVisonSettings settings = new  NightVisonSettings();
    private Material material;
    private RTHandle nightVisionBuffer;

    protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
    {
        name = "NightVision";
        targetColorBuffer = TargetBuffer.Camera;
        targetDepthBuffer = TargetBuffer.None;
        clearFlags = ClearFlag.None;

        material = CoreUtils.CreateEngineMaterial("Hidden/NightVisionHDRP");

        nightVisionBuffer = RTHandles.Alloc(
            Vector2.one, TextureXR.slices, dimension: TextureXR.dimension,
            colorFormat: GraphicsFormat.B10G11R11_UFloatPack32, useDynamicScale: true, name: "Halftone", wrapMode: TextureWrapMode.Mirror
        );

        Shader.EnableKeyword("UNITY_PIPELINE_HDRP");
    }

    protected override void Execute(CustomPassContext ctx)
    {
        if (material == null)
            return;

        material.SetFloat("_Range", settings.sensitivity);
        material.SetColor("_Color", settings.color);

        ctx.cmd.Blit(ctx.cameraColorBuffer, nightVisionBuffer, 0, 0);
        material.SetTexture("_nightVisionBuffer", ctx.cameraColorBuffer);
        ctx.cmd.Blit(nightVisionBuffer, nightVisionBuffer, material, 0);
        ctx.cmd.Blit(nightVisionBuffer, ctx.cameraColorBuffer, 0, 0);
    }

    protected override void Cleanup()
    {
        CoreUtils.Destroy(material);
        nightVisionBuffer.Release();
    }
}
#endif