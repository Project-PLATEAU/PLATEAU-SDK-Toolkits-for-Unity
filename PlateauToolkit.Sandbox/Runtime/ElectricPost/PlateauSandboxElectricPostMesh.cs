using System.Collections.Generic;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Runtime.ElectricPost
{
    /// <summary>
    /// 電柱のメッシュ操作
    /// </summary>
    public class PlateauSandboxElectricPostMesh
    {
        private const string k_PostMeshName = "Electric_Post";
        private readonly List<MeshRenderer> m_MeshRenderers = new();

        private Color m_HighlightColor = new Color(1f, 190f / 255f, 0f, 1f);

        public PlateauSandboxElectricPostMesh(GameObject root)
        {
            foreach (Transform child in root.transform)
            {
                if (!child.gameObject.name.Contains(k_PostMeshName))
                {
                    continue;
                }

                if (child.gameObject.TryGetComponent<MeshRenderer>(out var meshRenderer))
                {
                    m_MeshRenderers.Add(meshRenderer);
                }
            }
        }

        public void SetHighLight(bool isShow)
        {
            foreach (var meshRenderer in m_MeshRenderers)
            {
                var material = new Material(meshRenderer.sharedMaterial);
                material.SetColor("_BaseColor", isShow ? m_HighlightColor : Color.white);
                meshRenderer.material = material;
            }
        }
    }
}