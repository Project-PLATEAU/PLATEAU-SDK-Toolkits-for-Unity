#if UNITY_URP
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System;

public class TiltShiftURP : ScriptableRendererFeature
{
    [Serializable]
    public class BlurSettings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
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
    private BlurPass blurPass;

    public override void Create()
    {
        blurPass = new BlurPass(settings);
        Shader.EnableKeyword("UNITY_PIPELINE_URP");
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(blurPass);
    }

    class BlurPass : ScriptableRenderPass
    {
        private Material blurMaterial;
        private int blurIterations;
        private float blurSize;
        private int blurSamples;
        private int blurBufferID;
        private int blurTempBufferID;
        private float blurStartRange;
        private RenderTargetIdentifier blurBuffer;
        private RenderTargetIdentifier blurTempBuffer;
        private RenderTargetIdentifier source;
        private Vector2Int sourceSize;

        public BlurPass(BlurSettings settings)
        {
            if (blurMaterial == null)
            {
                blurMaterial = CoreUtils.CreateEngineMaterial(Shader.Find("Hidden/TiltShiftURP"));
            }
            this.renderPassEvent = settings.renderPassEvent;
            this.blurIterations = Mathf.Max(1, settings.blurIterations);
            this.blurSize = Mathf.Max(0.1f, settings.blurSize);
            this.blurSamples = Mathf.Max(1, settings.blurSamples);
            this.blurStartRange = Mathf.Clamp01(settings.blurStartRange);
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
            sourceSize = new Vector2Int(descriptor.width, descriptor.height);
            descriptor.depthBufferBits = 0;

            blurTempBufferID = Shader.PropertyToID("_blurTempBuffer");
            cmd.GetTemporaryRT(blurTempBufferID, descriptor.width, descriptor.height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
            blurTempBuffer = new RenderTargetIdentifier(blurTempBufferID);

            blurBufferID = Shader.PropertyToID("_blurBuffer");
            cmd.GetTemporaryRT(blurBufferID, descriptor.width, descriptor.height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);

            blurBuffer = new RenderTargetIdentifier(blurBufferID);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            blurMaterial.SetFloat("_BlurSize", blurSize);
            blurSamples = blurSamples / 8 * 8;
            blurMaterial.SetInt("_Samples", blurSamples);
            blurMaterial.SetFloat("_BlurStartRange", blurStartRange);

            source = renderingData.cameraData.renderer.cameraColorTarget;
            CommandBuffer cmd = CommandBufferPool.Get("BlurPass");

            cmd.Blit(source, blurBuffer);

            for (int i = 0; i < blurIterations; i++)
            {
                cmd.Blit(blurBuffer, blurTempBuffer, blurMaterial);
                cmd.Blit(blurTempBuffer, blurBuffer);
            }

            cmd.Blit(blurTempBuffer, source);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(blurBufferID);
        }
    }
}
#endif