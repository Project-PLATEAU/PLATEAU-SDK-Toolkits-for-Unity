using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Editor
{
    [Flags]
    enum PlateauSandboxAssetType
    {
        /// <summary>Assets defined by users</summary>
        UserDefined = 1 << 0,

        /// <summary>Sample assets included in the package</summary>
        Builtin = 1 << 1,

        /// <summary>All assets</summary>
        All = UserDefined | Builtin,
    }

    /// <summary>
    /// Sandbox Asset Definition.
    /// </summary>
    /// <typeparam name="TComponent"></typeparam>
    readonly struct PlateauSandboxAsset<TComponent> where TComponent : Component
    {
        /// <summary>
        /// The prefab of <see cref="TComponent" />
        /// </summary>
        public TComponent Asset { get; }

        public PlateauSandboxAsset(TComponent asset)
        {
            Asset = asset;
        }
    }

    /// <summary>
    /// Operate assets for Sandbox Toolkit.
    /// </summary>
    public static class PlateauSandboxAssetUtility
    {
        const string k_PlateauToolkitPackageName = "com.unity.plateautoolkit";

        static readonly string[] k_SearchInFolder = { "Assets" };
        const string k_AssetSamplesFolderPath = "Assets/Samples";
        const string k_TargetSampleName =
#if UNITY_URP
            "URP Sample Assets";
#elif UNITY_HDRP
            "HDRP Sample Assets";
#else
            null;
#endif

        static Sample? s_Sample;

        /// <summary>
        /// Get <see cref="Sample" /> for PLATEAU Toolkit.
        /// </summary>
        /// <remarks>
        /// If the result hasn't been cached, find the sample from the package.
        /// Otherwise, returns the cache.
        /// </remarks>
        public static bool GetSample(out Sample sample)
        {
            if (s_Sample != null)
            {
                sample = s_Sample.Value;
                return true;
            }

            IEnumerable<Sample> samples = Sample.FindByPackage(k_PlateauToolkitPackageName, null);
            foreach (Sample s in samples)
            {
                if (s.displayName == k_TargetSampleName)
                {
                    s_Sample = s;
                    sample = s;
                    return true;
                }
            }

            sample = default;
            return false;
        }

        /// <summary>
        /// Find all assets by the type <see cref="TComponent" /> in assets.
        /// </summary>
        internal static PlateauSandboxAsset<TComponent>[] FindAllAssets<TComponent>(PlateauSandboxAssetType assetType)
            where TComponent : Component
        {
            string[] assetGuids = AssetDatabase.FindAssets("t:prefab", k_SearchInFolder);

            if (assetGuids.Length == 0)
            {
                return Array.Empty<PlateauSandboxAsset<TComponent>>();
            }

            var assets = new List<PlateauSandboxAsset<TComponent>>();
            foreach (string assetGuid in assetGuids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);
                GameObject assetObject = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                if (!assetObject.TryGetComponent(out TComponent component))
                {
                    continue;
                }

                if (assetType.HasFlag(PlateauSandboxAssetType.UserDefined) &&
                    !assetPath.StartsWith(k_AssetSamplesFolderPath))
                {
                    assets.Add(new(component));
                    continue;
                }

                bool useBuiltin = assetType.HasFlag(PlateauSandboxAssetType.Builtin);
                useBuiltin &= assetPath.StartsWith(k_AssetSamplesFolderPath);
                if (useBuiltin)
                {
                    assets.Add(new(component));
                }
            }

            return assets.ToArray();
        }
    }
}