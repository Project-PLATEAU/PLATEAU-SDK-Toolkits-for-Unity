using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Editor
{
    class SandboxAssetListState<TAsset> : IDisposable where TAsset : Component
    {
        public bool IsReady { get; private set; }
        public SandboxAsset<TAsset>[] Assets { get; private set; }
        public Vector2 ScrollPosition { get; set; }
        public SandboxAssetType SelectedAssetType { get; set; }

        public async Task PrepareAsync(CancellationToken cancellationToken)
        {
            async Task<SandboxAsset<TAsset>> WaitPreview((TAsset Asset, SandboxAssetType AssetType) asset)
            {
                while (AssetPreview.IsLoadingAssetPreview(asset.Asset.gameObject.GetInstanceID()))
                {
                    await Task.Yield();
                    cancellationToken.ThrowIfCancellationRequested();
                }

                Texture2D preview = AssetPreview.GetAssetPreview(asset.Asset.gameObject);
                if (preview != null)
                {
                    Texture2D cachedPreviewTexture = new(preview.width, preview.height);
                    cachedPreviewTexture.SetPixels(preview.GetPixels());
                    cachedPreviewTexture.Apply();
                    preview = cachedPreviewTexture;
                }

                return new SandboxAsset<TAsset>(asset.Asset, asset.AssetType, preview);
            }

            (TAsset, SandboxAssetType)[] assets = PlateauSandboxAssetUtility.FindAllAssets<TAsset>();

            AssetPreview.SetPreviewTextureCacheSize(assets.Length + 50);
            foreach ((TAsset asset, _) in assets)
            {
                // Request an asset preview
                AssetPreview.GetAssetPreview(asset.gameObject);
            }

            Assets = await Task.WhenAll(assets.Select(WaitPreview));
            IsReady = true;
        }

        public void Dispose()
        {
            if (Assets == null)
            {
                return;
            }

            foreach (SandboxAsset<TAsset> asset in Assets)
            {
                if (asset.PreviewTexture != null)
                {
                    UnityEngine.Object.DestroyImmediate(asset.PreviewTexture);
                }
            }
        }
    }
}