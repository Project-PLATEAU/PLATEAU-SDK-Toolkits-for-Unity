using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildings.Editor
{
    public class OnPrefabInstanceUpdatedParameter : ScriptableSingleton<OnPrefabInstanceUpdatedParameter>
    {
        public bool canUpdatePrefabInstance = true;
        public bool prefabStageDirtied;
    }

    [InitializeOnLoad]
    public class OnPrefabInstanceUpdated
    {
        static OnPrefabInstanceUpdated()
        {
            PrefabStage.prefabStageOpened += PrefabStageOnPrefabStageOpened;
            PrefabStage.prefabStageDirtied += PrefabStageOnPrefabStageDirtied;
            PrefabStage.prefabStageClosing += PrefabStageOnPrefabStageClosing;
            OnPrefabInstanceUpdatedParameter.instance.prefabStageDirtied = false;

            PrefabUtility.prefabInstanceUpdated += OnPrefabInstanceUpdate;
            OnPrefabInstanceUpdatedParameter.instance.canUpdatePrefabInstance = true;
        }

        /// <summary>
        /// Prefab ModeでPrefabを開いた時にコール
        /// </summary>
        private static void PrefabStageOnPrefabStageOpened(PrefabStage obj)
        {
            OnPrefabInstanceUpdatedParameter.instance.prefabStageDirtied = false;
        }

        /// <summary>
        /// Prefabを編集して差分が出た時にコール
        /// </summary>
        private static void PrefabStageOnPrefabStageDirtied(PrefabStage obj)
        {
            OnPrefabInstanceUpdatedParameter.instance.prefabStageDirtied = true;
        }

        /// <summary>
        /// Prefab Modeを閉じた時にコール
        /// </summary>
        private static void PrefabStageOnPrefabStageClosing(PrefabStage obj)
        {
            if (!OnPrefabInstanceUpdatedParameter.instance.prefabStageDirtied)
            {
                return;
            }

            if (!obj.prefabContentsRoot.TryGetComponent(out Runtime.PlateauSandboxBuilding buildingGeneratorComponent))
            {
                return;
            }

            SaveAssets(obj.prefabContentsRoot, obj.assetPath, buildingGeneratorComponent);
        }

        /// <summary>
        /// プレハブ更新時のコールバック
        /// </summary>
        /// <param name="instance"></param>
        private static void OnPrefabInstanceUpdate(GameObject instance)
        {
            GameObject selectedGameObject = Selection.activeGameObject;
            if (instance != selectedGameObject)
            {
                return;
            }

            // 二重コール回避
            if (OnPrefabInstanceUpdatedParameter.instance.canUpdatePrefabInstance == false)
            {
                OnPrefabInstanceUpdatedParameter.instance.canUpdatePrefabInstance = true;
                return;
            }

            if (!instance.TryGetComponent(out Runtime.PlateauSandboxBuilding buildingGeneratorComponent))
            {
                return;
            }

            Runtime.PlateauSandboxBuilding prefab = PrefabUtility.GetCorrespondingObjectFromSource(buildingGeneratorComponent);
            string prefabPath = AssetDatabase.GetAssetPath(prefab);
            SaveAssets(instance, prefabPath, buildingGeneratorComponent);
        }

        /// <summary>
        /// メッシュとプレハブを保存
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="prefabPath"></param>
        /// <param name="buildingGeneratorComponent"></param>
        private static void SaveAssets(GameObject obj, string prefabPath, Runtime.PlateauSandboxBuilding buildingGeneratorComponent)
        {
            var lsFacadeMeshFilter = obj.transform.GetComponentsInChildren<MeshFilter>().ToList();
            if (!BuildingMeshUtility.SaveMesh(prefabPath, buildingGeneratorComponent.GetBuildingName(), lsFacadeMeshFilter))
            {
                EditorUtility.DisplayDialog("建築物のメッシュ保存に失敗", "建築物の保存に失敗しました。建築物を再生成して下さい。", "はい");
                return;
            }

            // SaveAsPrefabAssetAndConnectによってOnPrefabInstanceUpdateがコールされるので二重保存を回避
            OnPrefabInstanceUpdatedParameter.instance.canUpdatePrefabInstance = false;
            PrefabUtility.SaveAsPrefabAssetAndConnect(obj, prefabPath, InteractionMode.AutomatedAction);
        }
    }
}
