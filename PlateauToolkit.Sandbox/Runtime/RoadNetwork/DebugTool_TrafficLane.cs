using AWSIM.TrafficSimulation;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;

[ExecuteAlways]
public class DebugTool_TrafficLane : MonoBehaviour
{
    [SerializeField]
    private TrafficLane Lane;

    [SerializeField]
    private List<TrafficLane> _lanes = new List<TrafficLane>();


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

        _lanes.Clear();

        int iterations = 20;

        _lanes.Add(Lane);

        var current = Lane;
        for (int i = 0; i < iterations; i++)
        //while(current?.NextLanes?.Count > 0)
        {
            if (current?.NextLanes?.Count == 0)
                break;

            //var next = current.NextLanes.FirstOrDefault();
            var next = current.NextLanes[Random.Range(0, current.NextLanes.Count)];
            if (next == null)
                break;
            _lanes.Add( next);
            current = next;
        }

    }


    private void OnDrawGizmos()
    {
        if(_lanes.Count > 0)
        {
            Gizmos.color = Color.red;

            foreach (var lane in _lanes)
            {
                Gizmos.DrawCube(lane.Waypoints.FirstOrDefault(), UnityEngine.Vector3.one );
                //Gizmos.DrawLineList(lane.Waypoints);
                for (int j = 0; j < lane.Waypoints.Length - 1; j++)
                {
                    Gizmos.DrawLine(lane.Waypoints[j], lane.Waypoints[j + 1]);
                }
            }

            Gizmos.color = Color.white;
        }
    }




}
