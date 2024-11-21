using AWSIM.TrafficSimulation;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DebugTool_TrafficLane_AllConnected : MonoBehaviour
{
    [SerializeField]
    private TrafficLane Lane;

    [SerializeField]
    private List<TrafficLane> _nextLanes = new List<TrafficLane>();
    [SerializeField]
    private List<TrafficLane> _prevLanes = new List<TrafficLane>();



    // Start is called before the first frame update
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
        if (Lane == null)
            return;

        var trafficLanes = new List<TrafficLane>( GameObject.FindObjectsOfType<TrafficLane>() );
        _nextLanes = trafficLanes.FindAll(x => x.NextLanes.Contains(Lane));
        _prevLanes = trafficLanes.FindAll(x => x.PrevLanes.Contains(Lane));

    }

    private void OnDrawGizmos()
    {

        if (Lane != null)
        {
            Gizmos.color = Color.blue;
            for (int j = 0; j < Lane.Waypoints.Length - 1; j++)
            {
                Gizmos.DrawLine(Lane.Waypoints[j], Lane.Waypoints[j + 1]);
            }
        }

        if (_nextLanes.Count > 0)
        {
            foreach (var lane in _nextLanes)
            {
                Gizmos.color = Color.green;
                for (int j = 0; j < lane.Waypoints.Length - 1; j++)
                {
                    Gizmos.DrawLine(lane.Waypoints[j], lane.Waypoints[j + 1]);
                }
                DrawArrow(lane.Waypoints.LastOrDefault(), (lane.Waypoints.LastOrDefault() - lane.Waypoints.FirstOrDefault()).normalized);
            }
        }

        if (_prevLanes.Count > 0)
        {
            foreach (var lane in _prevLanes)
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
