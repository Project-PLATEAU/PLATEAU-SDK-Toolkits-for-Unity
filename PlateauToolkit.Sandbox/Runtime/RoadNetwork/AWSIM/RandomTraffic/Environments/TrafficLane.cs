using PLATEAU.RoadNetwork.Data;
using PLATEAU.RoadNetwork.Structure;
using System.Collections.Generic;
using UnityEngine;
using static PLATEAU.RoadNetwork.Util.LineCrossPointResult;

namespace AWSIM.TrafficSimulation
{
    /// <summary>
    /// Traffic lane component.
    /// </summary>
    public class TrafficLane : MonoBehaviour
    {
        /// <summary>
        /// Turning direction type of vehicles.
        /// </summary>
        public enum TurnDirectionType
        {
            STRAIGHT = 0,
            LEFT = 1,
            RIGHT = 2,
            NULL = 3
        }

        [SerializeField, Tooltip("Waypoints in this lane.")]
        private Vector3[] waypoints;
        [SerializeField, Tooltip("Turning direction of vehicles in the lane.")]
        private TurnDirectionType turnDirection;
        [SerializeField, Tooltip("Next lanes connected to this lane.")]
        private List<TrafficLane> nextLanes = new List<TrafficLane>();
        [SerializeField, Tooltip("Lanes leading to this lane.")]
        private List<TrafficLane> prevLanes = new List<TrafficLane>();
        [SerializeField, Tooltip("Lanes to which vehicles in this lane should yield the right of way.")]
        private List<TrafficLane> rightOfWayLanes = new List<TrafficLane>();
        [SerializeField, Tooltip("Stop line in the lane")]
        private StopLine stopLine;
        [SerializeField, Tooltip("Speed limit in m/s")]
        private float speedLimit;
        [SerializeField, Tooltip("Is intersection lane")]
        public bool intersectionLane;

        [SerializeField, Tooltip("RoadNetwork")]
        public RnDataRoad rnRoad;
        public RnDataLane rnLane;
        public RnDataIntersection rnIntersection;
        public RnDataTrack rnTrack;

        public void SetRoadNetworkData(RnDataRoadBase road, RnDataLane lane, RnDataTrack track)
        {
            if (road is RnDataRoad)
                rnRoad = road as RnDataRoad;
            else if (road is RnDataIntersection)
                rnIntersection = road as RnDataIntersection;

            rnLane = lane;
            rnTrack = track;
        }

        /// <summary>
        /// Get waypoints in this lane.
        /// </summary>
        public Vector3[] Waypoints => waypoints;

        /// <summary>
        /// Get turning direction of vehicles in the lane.
        /// </summary>
        public TurnDirectionType TurnDirection => turnDirection;

        /// <summary>
        /// Get next lanes connected to this lane.
        /// </summary>
        public List<TrafficLane> NextLanes => nextLanes;

        /// <summary>
        /// Get lanes leading to this lane.
        /// </summary>
        public List<TrafficLane> PrevLanes => prevLanes;

        /// <summary>
        /// Get lanes to which vehicles in this lane should yield the right of way.
        /// </summary>
        public List<TrafficLane> RightOfWayLanes => rightOfWayLanes;

        /// <summary>
        /// Get a stop line in the lane.
        /// </summary>
        public StopLine StopLine
        {
            get => stopLine;
            set => stopLine = value;
        }

        public Vector3 GetStopPoint(int waypointIndex = 0)
        {
            return StopLine == null ? Waypoints[waypointIndex] : StopLine.CenterPoint;
        }

        /// <summary>
        /// Get speed limit in m/s.
        /// </summary>
        public float SpeedLimit => speedLimit;

        /// <summary>
        /// Create <see cref="TrafficLane"/> instance in the scene.<br/>
        /// </summary>
        /// <param name="wayPoints"></param>
        /// <param name="speedLimit"></param>
        /// <returns><see cref="TrafficLane"/> instance.</returns>
        public static TrafficLane Create(string name, Transform parent, Vector3[] wayPoints, TurnDirectionType turnDirection, float speedLimit = 0f)
        {
            var gameObject = new GameObject(name, typeof(TrafficLane));
            gameObject.transform.position = wayPoints[0];
            gameObject.transform.SetParent(parent, true);
            var trafficLane = gameObject.GetComponent<TrafficLane>();
            trafficLane.waypoints = wayPoints;
            trafficLane.turnDirection = turnDirection;
            trafficLane.speedLimit = speedLimit;
            return trafficLane;
        }
    }
}
