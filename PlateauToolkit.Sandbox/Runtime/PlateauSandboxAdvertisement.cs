using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Video;

namespace PlateauToolkit.Sandbox.Runtime
{
    /// <summary>
    /// The definition of a advertisement
    /// </summary>
    [SelectionBase]
    public class PlateauSandboxAdvertisement : MonoBehaviour, IPlateauSandboxPlaceableObject
    {
        public enum AdvertisementType
        {
            Image,
            Video,
        }

        public enum FrontAxis
        {
            X,
            Z,
        }

        public float textureAspectWidth = 1;
        public float textureAspectHeight = 3;
        public int targetMaterialNumber;
        public string targetTextureProperty = "_MainTex";
        public FrontAxis frontAxis;
        public AdvertisementType advertisementType;
        public List<AdvertisementMaterials> advertisementMaterials;
        public Texture advertisementTexture;
        public VideoClip advertisementVideoClip;
        public VideoPlayer VideoPlayer { get; private set; }

        [Serializable]
        public class AdvertisementMaterials
        {
            public string gameObjectName;
            public List<Material> materials;
        }

        /// <summary>
        /// リセットするとその他の変数も初期化されるので注意
        /// </summary>
        private void Reset()
        {
            advertisementMaterials = new List<AdvertisementMaterials>();
            MeshRenderer[] lsChildMeshRender = transform.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer childMeshRender in lsChildMeshRender)
            {
                var advertisementMaterial = new AdvertisementMaterials
                {
                    gameObjectName = childMeshRender.transform.name,
                    materials = new List<Material>(childMeshRender.sharedMaterials)
                };
                advertisementMaterials.Add(advertisementMaterial);
            }

            SetTexture();
            AddVideoPlayer();
        }

        public void AddVideoPlayer()
        {
            if (!gameObject.TryGetComponent(out VideoPlayer videoPlayer))
            {
                videoPlayer = gameObject.AddComponent<VideoPlayer>();
                videoPlayer.isLooping = true;
                videoPlayer.renderMode = VideoRenderMode.RenderTexture;
                videoPlayer.clip = advertisementVideoClip;
                VideoPlayer = videoPlayer;
            }
            else
            {
                VideoPlayer = videoPlayer;
            }
        }

        public void SetTexture()
        {
            if (advertisementMaterials.Count <= 0)
            {
                return;
            }

            Material mat = advertisementMaterials[0].materials[targetMaterialNumber];
            if (mat.HasProperty(targetTextureProperty))
            {
                // RenderTextureは設定しない
                Texture texture = mat.GetTexture(targetTextureProperty);
                if (texture as RenderTexture == null)
                {
                    advertisementTexture = texture;
                }
            }
        }

        public void SetMaterials()
        {
            MeshRenderer[] lsChildMeshRender = transform.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer childMeshRender in lsChildMeshRender)
            {
                foreach (AdvertisementMaterials advertisementMaterial in advertisementMaterials.Where(advertisementMaterial => childMeshRender.transform.name == advertisementMaterial.gameObjectName))
                {
                    childMeshRender.sharedMaterials = advertisementMaterial.materials.ToArray();
                }
            }
        }

        public void SetPosition(in Vector3 position)
        {
            transform.position = position;
        }
    }
}