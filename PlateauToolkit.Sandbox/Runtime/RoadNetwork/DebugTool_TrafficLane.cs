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
    private List<TrafficLane> _lanes = new List<TrafficLane>();

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

        _lanes.Clear();

        int iterations = 50;

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


        var lastLane = _lanes.LastOrDefault();
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
    }


    private void OnDrawGizmos()
    {
        if(_lanes.Count > 0)
        {
            Gizmos.color = Color.white;

            Vector3 lastPoint = Vector3.zero;

            foreach (var lane in _lanes)
            {
                if(lastPoint != Vector3.zero)
                {
                    var firstPoint = lane.Waypoints.FirstOrDefault();

                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(lastPoint, firstPoint);
                }

                Gizmos.color = Color.green;
                Gizmos.DrawCube(lane.Waypoints.FirstOrDefault(), UnityEngine.Vector3.one);
                //Gizmos.DrawLineList(lane.Waypoints);
                for (int j = 0; j < lane.Waypoints.Length - 1; j++)
                {
                    Gizmos.DrawLine(lane.Waypoints[j], lane.Waypoints[j + 1]);
                }

                if(lane.NextLanes?.Count == 0)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawCube(lane.Waypoints.LastOrDefault(), UnityEngine.Vector3.one);
                }

                lastPoint = lane.Waypoints.LastOrDefault();

                
            }
            //if (lastPoint != Vector3.zero)
            //    Handles.Label(lastPoint, $"Last {_lanes.LastOrDefault().rnRoad.GetId(RnGetter)}");

            Gizmos.color = Color.white;
        }
    }




}
