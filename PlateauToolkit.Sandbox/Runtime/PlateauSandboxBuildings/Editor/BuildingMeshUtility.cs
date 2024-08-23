using PlateauToolkit.Sandbox.Editor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildings.Editor
{
    public static class BuildingMeshUtility
    {
        public static string GetMeshAssetsFolderPath()
        {
            return GetAssetsFolderPath("Meshes");
        }

        public static string GetPrefabAssetsFolderPath()
        {
            return GetAssetsFolderPath("Prefabs");
        }

        private static string GetAssetsFolderPath(string folderName)
        {
            bool res = PlateauSandboxAssetUtility.GetSample(out Sample sample);
            if (res == false)
            {
                Debug.LogError($"{folderName} assets folder directory is not exist.");
                return Path.Combine("Assets/Samples/PLATEAU SDK-Toolkits for Unity/0.0.0/Sample Assets", folderName).Replace("\\", "/");
            }

#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            string assetsFolderPath = Path.Combine("Assets", sample.importPath.Split(@"/Assets/")[1], $"Buildings/{folderName}").Replace("\\", "/");
#elif UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            string assetsFolderPath = Path.Combine("Assets", sample.importPath.Split(@"\Assets\")[1], $"Buildings/{folderName}").Replace("\\", "/");
#endif
            if (!Directory.Exists(assetsFolderPath))
            {
                Directory.CreateDirectory(assetsFolderPath);
            }
            return assetsFolderPath;
        }

        public static bool SaveMesh(List<MeshFilter> inLsMeshFilter, string inMeshNamePrefix)
        {
            if (inLsMeshFilter.Select(meshFilter => meshFilter.sharedMesh).Any(sharedMesh => sharedMesh == null))
            {
                Debug.LogError("MeshFilter is null");
                return false;
            }

            string meshAssetsFolderPath = GetMeshAssetsFolderPath();
            if (!Directory.Exists(meshAssetsFolderPath))
            {
                Directory.CreateDirectory(meshAssetsFolderPath);
            }

            foreach (MeshFilter meshFilter in inLsMeshFilter)
            {
                Transform lodObject = meshFilter.transform.parent;
                string lodNum = lodObject.name.Contains("LOD0") ? "LOD0" : "LOD1";
                Mesh sharedMesh = meshFilter.sharedMesh;

                if (!meshFilter.TryGetComponent(out BoxCollider boxCollider))
                {
                    boxCollider = meshFilter.gameObject.AddComponent<BoxCollider>();
                }

                boxCollider.center = sharedMesh.bounds.center;
                boxCollider.size = sharedMesh.bounds.size;

                string newMeshAssetName = inMeshNamePrefix + "_" + meshFilter.name + "Mesh_" + lodNum;
                string newMeshAssetPath = Path.Combine(meshAssetsFolderPath, newMeshAssetName + ".asset").Replace("\\", "/");
                Mesh newMeshAsset = AssetDatabase.LoadAssetAtPath<Mesh>(newMeshAssetPath);
                string oldMeshAssetPath = Path.Combine(meshAssetsFolderPath, sharedMesh.name + ".asset").Replace("\\", "/");
                Mesh oldMeshAsset = AssetDatabase.LoadAssetAtPath<Mesh>(oldMeshAssetPath);
                if (oldMeshAsset == null && newMeshAsset == null)
                {
                    AssetDatabase.CreateAsset(sharedMesh, newMeshAssetPath);
                }
                else if (oldMeshAsset == null && newMeshAsset != null)
                {
                    newMeshAsset.Clear(false);
                    EditorUtility.CopySerialized(meshFilter.sharedMesh, newMeshAsset);
                    newMeshAsset.name = newMeshAssetName;
                    EditorUtility.SetDirty(newMeshAsset);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    meshFilter.mesh = newMeshAsset;
                }
                else
                {
                    AssetDatabase.CopyAsset(oldMeshAssetPath, newMeshAssetPath);
                    newMeshAsset = AssetDatabase.LoadAssetAtPath<Mesh>(newMeshAssetPath);
                    newMeshAsset.Clear(false);
                    EditorUtility.CopySerialized(meshFilter.sharedMesh, newMeshAsset);
                    newMeshAsset.name = newMeshAssetName;
                    EditorUtility.SetDirty(newMeshAsset);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    meshFilter.mesh = newMeshAsset;
                }
            }

            return true;
        }
    }
}