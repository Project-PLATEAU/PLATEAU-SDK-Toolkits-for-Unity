#if UNITY_URP
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System;

public class HalftoneURP : ScriptableRendererFeature
{
    [Serializable]
    public class HalftoneSettings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
        [Range(0.1f, 5)]
        public float halftoneSize = 1.0f;
        [Range(0.1f, 1)]
        public float halftoneRange = 0.3f;
        public bool useColor = true;
    }

    public HalftoneSettings settings = new HalftoneSettings();
    private HalftonePass halftonePass;

    public override void Create()
    {
        halftonePass = new HalftonePass(settings);
        Shader.EnableKeyword("UNITY_PIPELINE_URP");
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(halftonePass);
    }

    class HalftonePass : ScriptableRenderPass
    {
        private Material material;
        private int halftoneBufferID;
        private RenderTargetIdentifier halftoneBuffer;
        private RenderTargetIdentifier source;
        private float halftoneSize;
        private float halftoneRange;
        private bool useColor;

        public HalftonePass(HalftoneSettings settings)
        {
            if (material == null)
            {
                material = CoreUtils.CreateEngineMaterial(Shader.Find("Hidden/HalftoneURP"));
            }
            this.renderPassEvent = settings.renderPassEvent;
            this.halftoneSize = Mathf.Max(0.1f, settings.halftoneSize);
            this.halftoneRange = Mathf.Max(0.0f, settings.halftoneRange);
            this.useColor = settings.useColor;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
            descriptor.depthBufferBits = 0;

            halftoneBufferID = Shader.PropertyToID("_halftoneBuffer");
            cmd.GetTemporaryRT(halftoneBufferID, descriptor.width, descriptor.height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
            halftoneBuffer = new RenderTargetIdentifier(halftoneBufferID);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            material.SetFloat("_Size", halftoneSize);
            material.SetFloat("_Range", halftoneRange);
            material.SetFloat("_UseColor", useColor ? 1.0f : 0.0f);

            source = renderingData.cameraData.renderer.cameraColorTarget;
            CommandBuffer cmd = CommandBufferPool.Get("HalftonePass");

            cmd.Blit(source, halftoneBuffer);
            cmd.Blit(halftoneBuffer, source, material, 0);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(halftoneBufferID);
        }
    }
}
#endif