using AWSIM.TrafficSimulation;
using PLATEAU.RoadNetwork.Data;
using PLATEAU.RoadNetwork.Structure;
using PlateauToolkit.Sandbox.RoadNetwork;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;


/// <summary>
/// TrafficLaneのルート接続確認用
/// </summary>
[ExecuteAlways]
public class DebugTool_TrafficLane : MonoBehaviour
{
    [SerializeField]
    private TrafficLane lane;

    [SerializeField]
    private List<TrafficLane> nextLanes = new List<TrafficLane>();
    [SerializeField]
    private List<TrafficLane> prevLanes = new List<TrafficLane>();

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
        if (lane == null)
            return;

        nextLanes.Clear();
        prevLanes.Clear();

        int iterations = 50;

        nextLanes.Add(lane);
        prevLanes.Add(lane);

        var current = lane;

        //Next Lanes
        for (int i = 0; i < iterations; i++)
        {
            if (current?.NextLanes?.Count == 0)
                break;

            var next = current.NextLanes[Random.Range(0, current.NextLanes.Count)];
            if (next == null)
                break;
            nextLanes.Add( next);
            current = next;
        }

        var lastLane = nextLanes.LastOrDefault();
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
        current = lane;
        for (int i = 0; i < iterations; i++)
        {
            if (current?.PrevLanes?.Count == 0)
                break;
            var prev = current.PrevLanes[Random.Range(0, current.PrevLanes.Count)];
            if (prev == null)
                break;
            prevLanes.Add(prev);
            current = prev;
        }
    }

    private void OnDrawGizmos()
    {
        if(nextLanes.Count > 0)
        {
            Gizmos.color = Color.white;

            Vector3 lastPoint = Vector3.zero;

            foreach (var lane in nextLanes)
            {
                if(lastPoint != Vector3.zero)
                {
                    var firstPoint = lane.Waypoints.FirstOrDefault();

                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(lastPoint, firstPoint);
                }

                Gizmos.color = Color.green;
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
        }

        if (prevLanes.Count > 0)
        {
            Gizmos.color = Color.white;
            Vector3 lastPoint = Vector3.zero;

            foreach (var lane in prevLanes)
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
