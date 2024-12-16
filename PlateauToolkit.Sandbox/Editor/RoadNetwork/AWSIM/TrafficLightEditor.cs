using UnityEditor;
using PlateauToolkit.Sandbox.RoadNetwork;
using System.Linq;
using UnityEngine;

namespace AWSIM.TrafficSimulation
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(TrafficLight))]
    public class TrafficLightEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            if (RoadNetworkConstants.SHOW_DEBUG_ROADNETWORK_INFO)
            {
                base.OnInspectorGUI();
            }
            else
            {
                DrawPropertiesExcluding(serializedObject, "rnTrafficLight");
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
