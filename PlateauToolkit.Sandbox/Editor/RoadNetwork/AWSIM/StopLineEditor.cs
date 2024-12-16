using PLATEAU.RoadNetwork.Data;
using PlateauToolkit.Sandbox.RoadNetwork;
using UnityEditor;
using UnityEngine;

namespace AWSIM.TrafficSimulation
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(StopLine))]
    public class StopLineEditor : Editor
    {
        private void OnSceneGUI()
        {
            // var stopLine = target as StopLine;
            // TODO: Handle implementation
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (RoadNetworkConstants.SHOW_DEBUG_ROADNETWORK_INFO)
            {
                base.OnInspectorGUI();
            }
            else
            {
                DrawPropertiesExcluding(serializedObject, "rnRoad", "rnIntersection", "rnBorder");
                serializedObject.ApplyModifiedProperties();
            }
        }

        [DrawGizmo(GizmoType.InSelectionHierarchy | GizmoType.Pickable)]
        private static void DrawGizmo(StopLine stopLine, GizmoType gizmoType)
        {
            var matCache = Gizmos.matrix;
            var colorCache = Gizmos.color;

            var direction = stopLine.Points[1] - stopLine.Points[0];
            var center = (stopLine.Points[0] + stopLine.Points[1]) / 2;
            var rotation = Quaternion.LookRotation(direction);
            Gizmos.matrix = Matrix4x4.TRS(center, rotation, Vector3.one);
            Gizmos.color = stopLine.HasStopSign ? Color.red : Color.white;
            var size = new Vector3(0.3f, 1f, direction.magnitude);
            Gizmos.DrawCube(Vector3.zero, size);

            Gizmos.matrix = matCache;
            Gizmos.color = colorCache;
        }
    }
}