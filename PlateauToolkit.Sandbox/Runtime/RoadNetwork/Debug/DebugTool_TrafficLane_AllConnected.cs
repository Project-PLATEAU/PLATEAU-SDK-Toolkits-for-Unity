using AWSIM.TrafficSimulation;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// TrafficLaneの前後の接続確認用
/// </summary>
public class DebugTool_TrafficLane_AllConnected : MonoBehaviour
{
    [SerializeField]
    private TrafficLane lane;

    [SerializeField]
    private List<TrafficLane> nextLanes = new List<TrafficLane>();
    [SerializeField]
    private List<TrafficLane> prevLanes = new List<TrafficLane>();

    void Start()
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

        var trafficLanes = new List<TrafficLane>( GameObject.FindObjectsOfType<TrafficLane>() );
        nextLanes = trafficLanes.FindAll(x => x.NextLanes.Contains(lane));
        prevLanes = trafficLanes.FindAll(x => x.PrevLanes.Contains(lane));

    }

    private void OnDrawGizmos()
    {
        if (lane != null)
        {
            Gizmos.color = Color.blue;
            for (int j = 0; j < lane.Waypoints.Length - 1; j++)
            {
                Gizmos.DrawLine(lane.Waypoints[j], lane.Waypoints[j + 1]);
            }
        }

        if (nextLanes.Count > 0)
        {
            foreach (var lane in nextLanes)
            {
                Gizmos.color = Color.green;
                for (int j = 0; j < lane.Waypoints.Length - 1; j++)
                {
                    Gizmos.DrawLine(lane.Waypoints[j], lane.Waypoints[j + 1]);
                }
                DrawArrow(lane.Waypoints.LastOrDefault(), (lane.Waypoints.LastOrDefault() - lane.Waypoints.FirstOrDefault()).normalized);
            }
        }

        if (prevLanes.Count > 0)
        {
            foreach (var lane in prevLanes)
            {
                Gizmos.color = Color.yellow;
                for (int j = 0; j < lane.Waypoints.Length - 1; j++)
                {
                    Gizmos.DrawLine(lane.Waypoints[j], lane.Waypoints[j + 1]);
                }
                DrawArrow(lane.Waypoints.LastOrDefault(), (lane.Waypoints.LastOrDefault() - lane.Waypoints.FirstOrDefault()).normalized);
            }
        }
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
