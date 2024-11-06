using AWSIM.TrafficSimulation;
using PLATEAU.RoadNetwork.Data;
using PLATEAU.RoadNetwork.Structure;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Codice.CM.WorkspaceServer.DataStore.WkTree.WriteWorkspaceTree;


namespace PlateauToolkit.Sandbox.RoadNetwork
{
    public class RoadNetworkLaneConverter
    {

        public List<TrafficLane> Create(RoadNetworkDataGetter getter)
        {


            var parent = GameObject.Find("TrafficLanes");
            if (parent == null)
            {
                parent = new GameObject("TrafficLanes");
            }

            List<TrafficLane> allLanes = new();

            var roadbases = getter.GetRoadBases();

            foreach(var rb in roadbases)
            {
                if (rb is RnDataRoad)
                {
                    RnDataRoad road = (RnDataRoad)rb;
                    var lanes = road.GetMainLanes(getter);
                    int index = 0;
                    foreach(var lane in lanes)
                    {
                        var points = lane.GetChildLineString(getter).GetChildPointsVector(getter);
                        if (points.Count > 0)
                        {
                            TrafficLane tlane = TrafficLane.Create($"TrafficLane_Road_{index++}", parent.transform, points.ToArray(), TrafficLane.TurnDirectionType.STRAIGHT);
                            tlane.enabled = true;
                            allLanes.Add(tlane);

                            Debug.Log($"lane points {points.Count}");
                        }
                    }
                }
                else if (rb is RnDataIntersection)
                {
                    RnDataIntersection intersection = (RnDataIntersection)rb;
                    var tracks = intersection.Tracks;
                    int index = 0;
                    foreach(var track in tracks)
                    {
                        var knotsPosistions = track.Spline.Knots.Select(x => (Vector3)x.Position).ToList();
                        if (knotsPosistions.Count > 0)
                        {
                            TrafficLane.TurnDirectionType turnDirType = ConvertTurnType(track.TurnType);
                            TrafficLane lane = TrafficLane.Create($"TrafficLane_Intersection_{index++}", parent.transform, knotsPosistions.ToArray(), turnDirType);
                            lane.intersectionLane = true;
                            lane.enabled = true;
                            allLanes.Add(lane);

                            Debug.Log($"lane intersection points {knotsPosistions.Count}");
                        }
                    }
                }
            }

            return allLanes;
        }

        TrafficLane.TurnDirectionType ConvertTurnType(RnTurnType turnType)
        {
            if (turnType == RnTurnType.LeftTurn || turnType == RnTurnType.LeftFront || turnType == RnTurnType.LeftBack)
            {
                return TrafficLane.TurnDirectionType.LEFT;
            }

            if (turnType == RnTurnType.RightTurn || turnType == RnTurnType.RightFront || turnType == RnTurnType.RightBack)
            {
                return TrafficLane.TurnDirectionType.RIGHT;
            }

            return TrafficLane.TurnDirectionType.STRAIGHT;
        }
    }
}
