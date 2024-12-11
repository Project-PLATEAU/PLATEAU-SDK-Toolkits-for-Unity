using UnityEditor;
using PlateauToolkit.Sandbox.RoadNetwork;
using System.Linq;
using UnityEngine;
using PlateauToolkit.Sandbox;

namespace AWSIM.TrafficSimulation
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(PlateauSandboxTrafficMovement))]
    public class PlateauSandboxTrafficMovementEditor : Editor
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
                DrawPropertiesExcluding(serializedObject, "status");
            }
        }
    }
}
