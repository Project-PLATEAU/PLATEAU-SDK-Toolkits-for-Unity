#if UNITY_URP
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System;

public class NightVisionURP : ScriptableRendererFeature
{
    [Serializable]
    public class NightVisonSettings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
        [Range(0.1f, 1)]
        public float sensitivity = 0.3f;
        public Color color = Color.green;
    }

    public NightVisonSettings settings = new NightVisonSettings();
    private HalftonePass nightVisionPass;

    public override void Create()
    {
        nightVisionPass = new HalftonePass(settings);
        Shader.EnableKeyword("UNITY_PIPELINE_URP");
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(nightVisionPass);
    }

    class HalftonePass : ScriptableRenderPass
    {
        private Material material;
        private int nightVisonBufferID;
        private RenderTargetIdentifier nightVisonBuffer;
        private RenderTargetIdentifier source;
        private float range;
        private Color color;

        public HalftonePass(NightVisonSettings settings)
        {
            if (material == null)
            {
                material = CoreUtils.CreateEngineMaterial(Shader.Find("Hidden/NightVisionURP"));
            }
            this.renderPassEvent = settings.renderPassEvent;
            this.range = Mathf.Max(0.0f, settings.sensitivity);
            this.color = settings.color;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
            descriptor.depthBufferBits = 0;

            nightVisonBufferID = Shader.PropertyToID("_nightVisionBuffer");
            cmd.GetTemporaryRT(nightVisonBufferID, descriptor.width, descriptor.height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
            nightVisonBuffer = new RenderTargetIdentifier(nightVisonBufferID);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            material.SetFloat("_Range", range);
            material.SetColor("_Color", color);

            source = renderingData.cameraData.renderer.cameraColorTarget;
            CommandBuffer cmd = CommandBufferPool.Get("NightVisionPass");

            cmd.Blit(source, nightVisonBuffer);
            cmd.Blit(nightVisonBuffer, source, material, 0);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(nightVisonBufferID);
        }
    }
}
#endif