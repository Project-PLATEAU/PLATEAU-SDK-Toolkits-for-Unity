using PlateauToolkit.Sandbox.RoadNetwork;
using UnityEditor;
using UnityEngine;

namespace AWSIM.TrafficSimulation
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(TrafficIntersection))]
    public class TrafficIntersectionEditor : Editor
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
                DrawPropertiesExcluding(serializedObject, "rnTrafficLightController");
            }
        }
    }
}