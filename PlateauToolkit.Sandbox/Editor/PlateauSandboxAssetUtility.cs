using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Editor
{
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
        /// Find a asset by the type <see cref="TComponent" /> in assets.
        /// </summary>
        public static (TComponent, string) FindAsset<TComponent>()
            where TComponent : Component
        {
            string[] assetGuids = AssetDatabase.FindAssets("t:prefab", k_SearchInFolder);
            if (assetGuids.Length == 0)
            {
                return (null, "");
            }

            foreach (string assetGuid in assetGuids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);
                GameObject assetObject = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                if (assetObject.TryGetComponent(out TComponent component))
                {
                    return (component, assetPath);
                }
            }

            return (null, "");
        }

        /// <summary>
        /// Find all assets by the type <see cref="TComponent" /> in assets.
        /// </summary>
        internal static (TComponent, SandboxAssetType)[] FindAllAssets<TComponent>()
            where TComponent : Component
        {
            string[] assetGuids = AssetDatabase.FindAssets("t:prefab", k_SearchInFolder);

            if (assetGuids.Length == 0)
            {
                return Array.Empty<(TComponent, SandboxAssetType)>();
            }

            var assets = new List<(TComponent, SandboxAssetType)>();
            foreach (string assetGuid in assetGuids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);
                GameObject assetObject = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                if (!assetObject.TryGetComponent(out TComponent component))
                {
                    continue;
                }

                if (!assetPath.StartsWith(k_AssetSamplesFolderPath))
                {
                    assets.Add((component, SandboxAssetType.UserDefined));
                }
                else if (assetPath.StartsWith(k_AssetSamplesFolderPath))
                {
                    assets.Add((component, SandboxAssetType.Builtin));
                }
            }

            return assets.ToArray();
        }
    }
}