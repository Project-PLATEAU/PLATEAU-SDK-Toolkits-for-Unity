using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildings.Editor
{
    public static class BuildingMeshUtility
    {
        private static string GetMeshAssetsFolderPath(string prefabPath)
        {
            // ルートパスの場合はnull
            string prefabFolderPath = Path.GetDirectoryName(prefabPath);
            if (prefabFolderPath == null)
            {
                 return Path.Combine(prefabPath, "Meshes");
            }
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            int lastSlashIndex = prefabFolderPath.LastIndexOf('/');
#elif UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            int lastSlashIndex = prefabFolderPath.LastIndexOf('\\');
#endif
            string parentDirectoryName = prefabFolderPath.Substring(0, lastSlashIndex);
            string meshAssetsFolderPath = Path.Combine(parentDirectoryName, "Meshes");
            if (!Directory.Exists(meshAssetsFolderPath))
            {
                Directory.CreateDirectory(meshAssetsFolderPath);
            }

            return meshAssetsFolderPath;
        }

        public static string GetMeshNamePrefix(string prefabFolderPath, string prefixBuildingName, bool isIncrementAssetName)
        {
            string[] files = Directory.GetFiles(prefabFolderPath, prefixBuildingName + "*.prefab", SearchOption.TopDirectoryOnly);
            int count = isIncrementAssetName ? 1 + files.Length : files.Length;

            return $"{prefixBuildingName}_{count:D2}";
        }

        public static bool SaveMesh(string prefabPath, string prefixBuildingName, List<MeshFilter> inLsMeshFilter, bool isIncrementAssetName = false)
        {
            if (inLsMeshFilter.Select(meshFilter => meshFilter.sharedMesh).Any(sharedMesh => sharedMesh == null))
            {
                Debug.LogError("MeshFilter is null");
                return false;
            }

            string meshAssetsFolderPath = GetMeshAssetsFolderPath(prefabPath);
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

                string prefabFolderPath = Path.GetDirectoryName(prefabPath);
                string newMeshAssetName = GetMeshNamePrefix(prefabFolderPath, prefixBuildingName, isIncrementAssetName) + "_" + meshFilter.name + "Mesh_" + lodNum;
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