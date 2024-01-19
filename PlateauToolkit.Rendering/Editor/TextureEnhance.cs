using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PlateauToolkit.Rendering.Editor
{
    internal struct ImageEditOperation
    {
        public ImageEditParameters ImageParam;
        public int ImageParamValue;
    }

    internal enum TextureDownscaleRatio
    {
        None,
        Half,
        Quarter,
        OneEighth,
        OneSixteenth
    }

    internal class TextureEnhance : IDisposable
    {
        Material m_InitialMaterial;
        Material m_MostRecentEdit;
        Material m_CurrentlyEditing;

        ImageEditParameters m_CurrentEditingParameter;

        List<ImageEditOperation> m_Operations;

        int DownscaleRatio { get; set; }

        public IReadOnlyList<ImageEditOperation> ReadOnlyOperationsList => m_Operations;

        public TextureEnhance(Material initialMat)
        {
            m_InitialMaterial = new Material(initialMat);
            m_MostRecentEdit = new Material(initialMat);
            m_CurrentlyEditing = new Material(initialMat);

            m_Operations = new List<ImageEditOperation>();
            DownscaleRatio = 1;
        }

        public void SetTexture(Material initialMat)
        {
            m_InitialMaterial = initialMat;

            m_Operations.Clear();
        }

        ~TextureEnhance()
        {
            // Finalizer calls Dispose(false)
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public Texture2D GenerateTexture(Texture baseTexture, int param)
        {
            // Example texture generation based on the slider value
            Texture2D texture = new Texture2D(baseTexture.width, baseTexture.height);

            Color color = new Color(param / 100f, param / 100f, param / 100f);
            for (int x = 0; x < baseTexture.width; x++)
            {
                for (int y = 0; y < baseTexture.height; y++)
                {
                    texture.SetPixel(x, y, color);
                }
            }

            texture.Apply();

            if (texture.width > 1024 || texture.height > 1024)
            {
                texture = TextureScaler.Resize(texture, 1024, 1024);
            }
            return texture;
        }

        /// <summary>
        /// Called when the user finished editing one image parameter, switching to another parameter or exiting Edit mode.
        /// </summary>
        /// <param name="param"></param>
        /// <param name="value"></param>
        public void SaveImageParameter(ImageEditParameters param, int value)
        {
            ImageEditOperation op = new ImageEditOperation();
            op.ImageParam = param;
            op.ImageParamValue = value;
            m_Operations.Add(op);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose managed state (managed objects)
                // If you had managed IDisposable members, you would dispose them here
            }

            // Free unmanaged resources (unmanaged objects) and override a finalizer below
            if (m_InitialMaterial != null)
            {
                UnityEngine.Object.DestroyImmediate(m_InitialMaterial);
                m_InitialMaterial = null;
            }
            if (m_MostRecentEdit != null)
            {
                UnityEngine.Object.DestroyImmediate(m_MostRecentEdit);
                m_MostRecentEdit = null;
            }
            if (m_CurrentlyEditing != null)
            {
                UnityEngine.Object.DestroyImmediate(m_CurrentlyEditing);
                m_CurrentlyEditing = null;
            }

            // Set large fields to null
            m_Operations.Clear();
        }
    }
}
