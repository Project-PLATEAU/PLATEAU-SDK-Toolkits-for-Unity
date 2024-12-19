using UnityEditor;
using PlateauToolkit.Sandbox.RoadNetwork;
using System.Linq;
using UnityEngine;
using PlateauToolkit.Sandbox;
using UnityEngine.Profiling;

namespace AWSIM.TrafficSimulation
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(PlateauSandboxTrafficManager))]
    public class PlateauSandboxTrafficManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (PlateauSandboxTrafficManagerConstants.TRAFFIC_LIGHT_ASSET_REPLACEABLE)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(PlateauSandboxTrafficManager.m_TrafficLightPrefab)));
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(PlateauSandboxTrafficManager.m_GreenRedInterval)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(PlateauSandboxTrafficManager.m_YellowInterval)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(PlateauSandboxTrafficManager.m_ExtraRedInterval)));

            if (GUILayout.Button("Update Traffic Lights"))
            {
                if (Application.isPlaying)
                {
                    EditorUtility.DisplayDialog("エラー", "プレイモードを終了してから実行してください。", "OK");
                    return;
                }
                PlateauSandboxTrafficManager manager = target as PlateauSandboxTrafficManager;
                manager.UpdateTrafficLightSequences();
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(PlateauSandboxTrafficManager.m_ShowTrafficLightGizmos)));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
