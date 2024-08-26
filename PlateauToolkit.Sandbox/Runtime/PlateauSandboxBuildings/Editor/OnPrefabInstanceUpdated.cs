using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildings.Editor
{
    public class OnPrefabInstanceUpdatedParameter : ScriptableSingleton<OnPrefabInstanceUpdatedParameter>
    {
        public bool canUpdate = true;
    }

    [InitializeOnLoad]
    public class OnPrefabInstanceUpdated
    {
        static OnPrefabInstanceUpdated()
        {
            PrefabUtility.prefabInstanceUpdated += OnPrefabInstanceUpdate;
            OnPrefabInstanceUpdatedParameter.instance.canUpdate = true;
        }

        private static void OnPrefabInstanceUpdate(GameObject instance)
        {
            GameObject selectedGameObject = Selection.activeGameObject;
            if (instance != selectedGameObject)
            {
                return;
            }

            if (OnPrefabInstanceUpdatedParameter.instance.canUpdate == false)
            {
                OnPrefabInstanceUpdatedParameter.instance.canUpdate = true;
                return;
            }

            if (!instance.TryGetComponent(out Runtime.PlateauSandboxBuilding buildingGeneratorComponent))
            {
                return;
            }

            string meshAssetsFolderPath = BuildingMeshUtility.GetMeshAssetsFolderPath();
            if (!Directory.Exists(meshAssetsFolderPath))
            {
                Directory.CreateDirectory(meshAssetsFolderPath);
            }

            string prefabAssetsFolderPath = BuildingMeshUtility.GetPrefabAssetsFolderPath();
            if (!Directory.Exists(prefabAssetsFolderPath))
            {
                Directory.CreateDirectory(prefabAssetsFolderPath);
            }

            var lsFacadeMeshFilter = instance.transform.GetComponentsInChildren<MeshFilter>().ToList();
            if (BuildingMeshUtility.SaveMesh(lsFacadeMeshFilter, buildingGeneratorComponent.buildingName))
            {
                OnPrefabInstanceUpdatedParameter.instance.canUpdate = false;
                string prefabPath = Path.Combine(prefabAssetsFolderPath, buildingGeneratorComponent.buildingName + ".prefab").Replace("\\", "/");
                PrefabUtility.SaveAsPrefabAssetAndConnect(instance, prefabPath, InteractionMode.AutomatedAction);
                return;
            }

            EditorUtility.DisplayDialog("建築物のメッシュを保存", "建築物の保存に失敗しました。建築物を再生成して下さい。", "はい");
        }
    }
}
