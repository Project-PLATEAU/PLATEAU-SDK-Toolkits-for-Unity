using UnityEngine;

namespace PlateauToolkit.Rendering.ImageProcessing
{
    public static class PlateauRenderingFilterLibrary
    {
        static ComputeShader s_Shader;
        static int s_HighPassKernelIndex;
        static int s_ContrastKernelIndex;
        static int s_BrightnessKernelIndex;
        static int s_SharpenKernelIndex;

        public static void Initialize(ComputeShader shader)
        {
            s_Shader = shader;

            s_ContrastKernelIndex = s_Shader.FindKernel("Contrast");
            s_BrightnessKernelIndex = s_Shader.FindKernel("Brightness");
            s_HighPassKernelIndex = s_Shader.FindKernel("HighPass");
            s_SharpenKernelIndex = s_Shader.FindKernel("Sharpen");
        }

        public static RenderTexture HighPassFilterExtension(this RenderTexture source)
        {
            if (source == null)
            {
                Debug.LogError("Source RenderTexture is null!");
                return null;
            }

            if (s_Shader == null)
            {
                Debug.LogError("Compute Shader is not assigned!");
                return null;
            }

            var destination = new RenderTexture(source.width, source.height, 0, RenderTextureFormat.ARGBFloat)
            {
                enableRandomWrite = true
            };
            destination.Create();
            s_Shader.SetTexture(s_HighPassKernelIndex, "Source", source);
            s_Shader.SetTexture(s_HighPassKernelIndex, "Destination", destination);

            s_Shader.Dispatch(s_HighPassKernelIndex, Mathf.CeilToInt((float)source.width / 32),
                Mathf.CeilToInt((float)source.height / 32), 1);
            return destination;
        }

        public static RenderTexture ContrastFilterExtension(this RenderTexture source)
        {
            var destination = new RenderTexture(source.width, source.height, 0, RenderTextureFormat.ARGBFloat)
            {
                enableRandomWrite = true
            };
            destination.Create();
            s_Shader.SetTexture(s_ContrastKernelIndex, "Source", source);
            s_Shader.SetTexture(s_ContrastKernelIndex, "Destination", destination);

            s_Shader.Dispatch(s_ContrastKernelIndex, Mathf.CeilToInt((float)source.width / 32),
                Mathf.CeilToInt((float)source.height / 32), 1);
            return destination;
        }

        public static RenderTexture BrightnessFilterExtension(this RenderTexture source)
        {
            var destination = new RenderTexture(source.width, source.height, 0, RenderTextureFormat.ARGBFloat)
            {
                enableRandomWrite = true
            };
            destination.Create();
            s_Shader.SetTexture(s_BrightnessKernelIndex, "Source", source);
            s_Shader.SetTexture(s_BrightnessKernelIndex, "Destination", destination);

            s_Shader.Dispatch(s_BrightnessKernelIndex, Mathf.CeilToInt((float)source.width / 32),
                Mathf.CeilToInt((float)source.height / 32), 1);
            return destination;
        }

        public static RenderTexture SharpenessFilterExtension(this RenderTexture source)
        {
            var destination = new RenderTexture(source.width, source.height, 0, RenderTextureFormat.ARGBFloat)
            {
                enableRandomWrite = true
            };
            destination.Create();
            s_Shader.SetTexture(s_SharpenKernelIndex, "Source", source);
            s_Shader.SetTexture(s_SharpenKernelIndex, "Destination", destination);

            s_Shader.Dispatch(s_SharpenKernelIndex, Mathf.CeilToInt((float)source.width / 32),
                Mathf.CeilToInt((float)source.height / 32), 1);
            return destination;
        }
    }
}