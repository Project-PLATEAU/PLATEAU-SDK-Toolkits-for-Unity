using UnityEditor;
using PlateauToolkit.Sandbox.RoadNetwork;
using System.Linq;
using UnityEngine;
using PlateauToolkit.Sandbox;

namespace AWSIM.TrafficSimulation
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(RoadNetworkTrafficManager))]
    public class RoadNetworkTrafficManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (RoadNetworkConstants.TRAFFIC_LIGHT_ASSET_REPLACEABLE)
            {
                base.OnInspectorGUI();
            }
            else
            {
                DrawPropertiesExcluding(serializedObject, "m_TrafficLightPrefab");
                serializedObject.ApplyModifiedProperties();
            }

            if (GUILayout.Button("Update Traffic Lights"))
            {
                if (Application.isPlaying)
                {
                    return;
                }

                RoadNetworkTrafficManager manager = target as RoadNetworkTrafficManager;
                manager.UpdateTrafficLightSequences();
            }
        }
    }
}
