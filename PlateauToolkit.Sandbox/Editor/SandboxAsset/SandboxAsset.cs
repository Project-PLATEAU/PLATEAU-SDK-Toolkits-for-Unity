using UnityEngine;

namespace PlateauToolkit.Sandbox.Editor
{
    /// <summary>
    /// Sandbox Asset Definition.
    /// </summary>
    /// <typeparam name="TComponent"></typeparam>
    class SandboxAsset<TComponent> where TComponent : Component
    {
        /// <summary>
        /// The prefab of <see cref="TComponent" />
        /// </summary>
        public TComponent Asset { get; }

        /// <summary>
        /// Asset type
        /// </summary>
        public SandboxAssetType AssetType { get; }

        /// <summary>
        /// <see cref="Texture2D" /> of a preview of the asset.
        /// </summary>
        public Texture2D PreviewTexture { get; set; }

        public SandboxAsset(TComponent asset, SandboxAssetType assetType, Texture2D previewTexture)
        {
            Asset = asset;
            AssetType = assetType;
            PreviewTexture = previewTexture;
        }
    }
}