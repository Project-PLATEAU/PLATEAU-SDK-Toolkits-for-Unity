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


        [DrawGizmo(GizmoType.Selected)]
        private static void DrawGizmoSelected(TrafficLight trafficlight, GizmoType gizmoType)
        {

        }
    }
}
