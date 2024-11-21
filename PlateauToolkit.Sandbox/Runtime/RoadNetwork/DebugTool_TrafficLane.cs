using AWSIM.TrafficSimulation;
using PLATEAU.RoadNetwork.Data;
using PLATEAU.RoadNetwork.Structure;
using PlateauToolkit.Sandbox.RoadNetwork;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[ExecuteAlways]
public class DebugTool_TrafficLane : MonoBehaviour
{
    [SerializeField]
    private TrafficLane Lane;

    [SerializeField]
    private List<TrafficLane> _nextLanes = new List<TrafficLane>();
    [SerializeField]
    private List<TrafficLane> _prevLanes = new List<TrafficLane>();

    public RoadNetworkDataGetter RnGetter
    {
        get
        {
            if (m_RoadNetworkGetter == null)
            {
                PLATEAURnStructureModel roadNetwork = GameObject.FindObjectOfType<PLATEAURnStructureModel>();
                m_RoadNetworkGetter = roadNetwork?.GetRoadNetworkDataGetter();

                if (m_RoadNetworkGetter == null)
                {
                    Debug.LogError($"RoadNetworkDataGetter is null");
                }
            }

            return m_RoadNetworkGetter;
        }
    }
    RoadNetworkDataGetter m_RoadNetworkGetter;

    private void Start()
    {
        Init();
    }

    private void OnValidate()
    {
        Init();
    }

    private void Init()
    {
        if (Lane == null)
            return;

        _nextLanes.Clear();
        _prevLanes.Clear();

        int iterations = 50;

        _nextLanes.Add(Lane);
        _prevLanes.Add(Lane);

        var current = Lane;

        //Next Lanes
        for (int i = 0; i < iterations; i++)
        //while(current?.NextLanes?.Count > 0)
        {
            if (current?.NextLanes?.Count == 0)
                break;

            //var next = current.NextLanes.FirstOrDefault();
            var next = current.NextLanes[Random.Range(0, current.NextLanes.Count)];
            if (next == null)
                break;
            _nextLanes.Add( next);
            current = next;
        }

        var lastLane = _nextLanes.LastOrDefault();
        if (lastLane != null)
        {
            if (lastLane.intersectionLane)
            {
                Debug.Log($"Last Track: {lastLane.rnTrack.GetToBorder(RnGetter).LineString.ID}");
            }
            else
            {
                Debug.Log($"Last Lane: {lastLane.rnLane.GetNextBorder(RnGetter).LineString.ID}");

                var nextIntersection = lastLane.rnRoad.GetNextRoad(RnGetter) as RnDataIntersection;
                Debug.Log($"Last Lane: {lastLane.rnLane.GetNextBorder(RnGetter).LineString.ID} hasNext{nextIntersection != null}");

                if (nextIntersection != null)
                {
                    var roads = nextIntersection.GetAllConnectedRoads(RnGetter);
                    Debug.Log($"next connected roads: {roads.Count}  {string.Join(",", roads.Select(x => x.GetId(RnGetter)))}" );

                }

            }
        }

        //Prev Lanes
        current = Lane;
        for (int i = 0; i < iterations; i++)
        {
            if (current?.PrevLanes?.Count == 0)
                break;
            var prev = current.PrevLanes[Random.Range(0, current.PrevLanes.Count)];
            if (prev == null)
                break;
            _prevLanes.Add(prev);
            current = prev;
        }
    }


    private void OnDrawGizmos()
    {
        if(_nextLanes.Count > 0)
        {
            Gizmos.color = Color.white;

            Vector3 lastPoint = Vector3.zero;

            foreach (var lane in _nextLanes)
            {
                if(lastPoint != Vector3.zero)
                {
                    var firstPoint = lane.Waypoints.FirstOrDefault();

                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(lastPoint, firstPoint);
                    //DrawArrow(lastPoint, (lastPoint - firstPoint).normalized);
                }

                Gizmos.color = Color.green;
                //Gizmos.DrawCube(lane.Waypoints.FirstOrDefault(), UnityEngine.Vector3.one);
                //Gizmos.DrawLineList(lane.Waypoints);
                for (int j = 0; j < lane.Waypoints.Length - 1; j++)
                {
                    Gizmos.DrawLine(lane.Waypoints[j], lane.Waypoints[j + 1]);
                }
                DrawArrow(lane.Waypoints.LastOrDefault(), (lane.Waypoints.LastOrDefault() - lane.Waypoints.FirstOrDefault()).normalized);

                if (lane.NextLanes?.Count == 0)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawCube(lane.Waypoints.LastOrDefault(), UnityEngine.Vector3.one);
                }

                lastPoint = lane.Waypoints.LastOrDefault();

            }
            //if (lastPoint != Vector3.zero)
            //    Handles.Label(lastPoint, $"Last {_lanes.LastOrDefault().rnRoad.GetId(RnGetter)}");
        }

        if (_prevLanes.Count > 0)
        {
            Gizmos.color = Color.white;
            Vector3 lastPoint = Vector3.zero;

            foreach (var lane in _prevLanes)
            {
                if (lastPoint != Vector3.zero)
                {
                    var firstPoint = lane.Waypoints.LastOrDefault();
                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(lastPoint, firstPoint);
                }

                Gizmos.color = Color.yellow;
                for (int j = 0; j < lane.Waypoints.Length - 1; j++)
                {
                    Gizmos.DrawLine(lane.Waypoints[j], lane.Waypoints[j + 1]);
                }
                DrawArrow(lane.Waypoints.LastOrDefault(), (lane.Waypoints.LastOrDefault() - lane.Waypoints.FirstOrDefault()).normalized);

                if (lane.PrevLanes?.Count == 0)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawCube(lane.Waypoints.FirstOrDefault(), UnityEngine.Vector3.one);
                }

                lastPoint = lane.Waypoints.FirstOrDefault();

            }
        }

        Gizmos.color = Color.white;
    }


    public void DrawArrow(Vector3 pos, Vector3 direction, float arrowHeadLength = 3.5f, float arrowHeadAngle = 20.0f)
    {
        Gizmos.DrawRay(pos, direction);

        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Gizmos.DrawRay(pos + direction, right * arrowHeadLength);
        Gizmos.DrawRay(pos + direction, left * arrowHeadLength);
    }

}
