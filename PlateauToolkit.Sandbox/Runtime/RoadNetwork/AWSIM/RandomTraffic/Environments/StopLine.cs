using PLATEAU.RoadNetwork.Data;
using PLATEAU.RoadNetwork.Structure;
using PlateauToolkit.Sandbox.RoadNetwork;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AWSIM.TrafficSimulation
{
    /// <summary>
    /// Stop line component.
    /// </summary>
    public class StopLine : MonoBehaviour
    {
        [SerializeField, Tooltip("Line data consists of 2 points.")]
        private Vector3[] points = new Vector3[2];
        [SerializeField, Tooltip("Indicates whether the stop sign exists.")] 
        private bool hasStopSign = false;
        [SerializeField, Tooltip("Traffic light ")]
        private TrafficLight trafficLight;

        [Header("RoadNetwork")]
        public RnDataRoad rnRoad;
        public RnDataIntersection rnIntersection;
        public RnDataWay rnBorder;

        /// <summary>
        /// Get line data consists of 2 points.
        /// </summary>
        public Vector3[] Points => points;

        /// <summary>
        /// Get center point of the stop line.
        /// </summary>
        public Vector3 CenterPoint => (points[0] + points[1]) / 2f;

        public TrafficLight TrafficLight
        {
            get => trafficLight;
            //set => trafficLight = value;
            set
            {
                trafficLight = value;
                trafficLight.AddStopLine(this);
            }
        }

        public bool HasStopSign
        {
            get => hasStopSign;
            set => hasStopSign = value;
        }

        public static StopLine Create(Vector3 p1, Vector3 p2, Transform parent)
        {
#if UNITY_EDITOR
            var gameObjectName = GameObjectUtility.GetUniqueNameForSibling(parent, "StopLine");
#else
            var gameObjectName = "StopLine";
#endif
            var gameObject = new GameObject(gameObjectName, typeof(StopLine));
            gameObject.transform.SetParent(parent.transform, false);
            gameObject.transform.position = p1;
            var stopLine = gameObject.GetComponent<StopLine>();
            stopLine.points[0] = p1;
            stopLine.points[1] = p2;
            return stopLine;
        }

        public void SetRoadNetworkData(RnDataRoad road, RnDataIntersection intersection, RnDataWay border)
        {
            rnRoad = road;
            rnIntersection = intersection;
            rnBorder = border;
        }
    }
}
