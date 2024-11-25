using AWSIM.TrafficSimulation;
using PLATEAU.RoadNetwork.Data;
using System.Linq;
using UnityEngine;

namespace PlateauToolkit.Sandbox.RoadNetwork
{
    public class DebugLaneConverter
    {
        //Next/Prev共に取得できない場合のDebug確認用
        public static void DebugLanes(TrafficLane lane, RoadNetworkDataGetter getter)
        {
            if (lane.PrevLanes.Count == 0 && lane.NextLanes.Count == 0)
            {
                //Debug.LogError($"Lane has no connection {lane.name}");
                if (lane.rnRoad != null)
                {
                    var nextBorder = lane.rnLane.GetNextBorder(getter);
                    var prevBorder = lane.rnLane.GetPrevBorder(getter);
                    var nextIntersection = lane.rnRoad.GetNextRoad(getter) as RnDataIntersection;
                    var prevIntersection = lane.rnRoad.GetPrevRoad(getter) as RnDataIntersection;
                    if (nextIntersection != null)
                    {
                        var linestrings = nextIntersection.Edges.Select(x => x.GetBorder(getter).LineString.ID);
                        Debug.Log($"Next Intersection Borders {string.Join(",", linestrings)} : nextBorder {nextBorder.LineString.ID} {lane.rnLane.IsReverse} {nextIntersection.IsEmptyIntersection} {lane.name} ");

                        if (nextIntersection.IsEmptyIntersection)
                        {
                            var edges = nextIntersection.GetOppositeSideEdgesFromRoad(lane.rnRoad.GetId(getter));

                            var roadids = edges.Select(x => x.Road.ID).ToList();
                            Debug.Log($"Next Roads {string.Join(",", roadids)}");

                            foreach (var roadid in roadids)
                            {
                                var borders = ((RnDataRoad)getter.GetRoadBases().TryGet(roadid)).GetLAllBorderWaysFromMainLanes(getter);
                                var borderids = ((RnDataRoad)getter.GetRoadBases().TryGet(roadid)).GetLAllBordersFromMainLanes(getter).Select(x => x.GetId(getter));
                                Debug.Log($"Next Road borders {string.Join(",", borderids)}");

                                foreach (var border in borders)
                                {
                                    Debug.Log($"Next Road border {border.LineString.ID} is same line {nextBorder.LineString.ID} {border.IsSameLine(nextBorder)} ");
                                }
                            }

                            var nextLanes = nextIntersection.GetNextLanesFromLane(getter, lane.rnRoad, lane.rnLane, RoadNetworkConstants.IGNORE_REVERSED_LANE);

                            Debug.Log($"nextLanes {nextLanes.Count}");

                        }
                    }
                    if (prevIntersection != null)
                    {
                        var linestrings = prevIntersection.Edges.Select(x => x.GetBorder(getter).LineString.ID);
                        Debug.Log($"Prev Intersection Borders {string.Join(",", linestrings)} : prevBorder {prevBorder.LineString.ID}  {lane.rnLane.IsReverse} {prevIntersection.IsEmptyIntersection} {lane.name}");
                    }
                }
                else if (lane.rnIntersection != null)
                {
                    var toBorder = lane.rnTrack.GetToBorder(getter);
                    var fromBorder = lane.rnTrack.GetFromBorder(getter);
                    var nextRoad = lane.rnIntersection.Edges.Find(x => x.GetBorder(getter).IsSameLine(toBorder)).GetRoad(getter) as RnDataRoad;
                    var prevRoad = lane.rnIntersection.Edges.Find(x => x.GetBorder(getter).IsSameLine(fromBorder)).GetRoad(getter) as RnDataRoad;
                    if (nextRoad != null)
                    {
                        var nextBorderlinestrings = nextRoad.GetMainLanes(getter).Select(x => x.GetNextBorder(getter).LineString.ID);
                        var prevBorderlinestrings = nextRoad.GetMainLanes(getter).Select(x => x.GetPrevBorder(getter).LineString.ID);
                        Debug.Log($"Next Road next {string.Join(",", nextBorderlinestrings)} prev {string.Join(",", prevBorderlinestrings)} : toBorder {toBorder.LineString.ID} ");
                    }
                    if (prevRoad != null)
                    {
                        var nextBorderlinestrings = prevRoad.GetMainLanes(getter).Select(x => x.GetNextBorder(getter).LineString.ID);
                        var prevBorderlinestrings = prevRoad.GetMainLanes(getter).Select(x => x.GetPrevBorder(getter).LineString.ID);
                        Debug.Log($"Prev Road next {string.Join(",", nextBorderlinestrings)} prev {string.Join(",", prevBorderlinestrings)} : fromBorder {fromBorder.LineString.ID} ");
                    }
                }
            }
        }

    }
}
