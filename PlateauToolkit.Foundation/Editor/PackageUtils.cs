using UnityEditor;
using UnityEngine;

namespace PlateauToolkit.Foundation.Editor
{
    public static class PackageUtils
    {
        public static string GetPackageDirectoryPath(string packageName)
        {
            string relativePath = "Packages/" + packageName;
            string[] packageAssetPaths = AssetDatabase.FindAssets("", new[] { relativePath });

            if (packageAssetPaths.Length > 0)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(packageAssetPaths[0]);
                int index = assetPath.IndexOf(packageName) + packageName.Length;

                // Get the relative path to the package's root directory
                string relativePackagePath = assetPath.Substring(0, index);

                // Convert to full system path
                string fullPath = System.IO.Path.GetFullPath(Application.dataPath + "/../" + relativePackagePath);

                return fullPath.Replace("\\", "/");
            }
            return null;
        }
    }
}
