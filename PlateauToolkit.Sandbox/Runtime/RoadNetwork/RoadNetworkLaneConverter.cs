using AWSIM.TrafficSimulation;
using PLATEAU.RoadNetwork.Data;
using PLATEAU.RoadNetwork.Structure;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;
using static Codice.CM.WorkspaceServer.DataStore.WkTree.WriteWorkspaceTree;
using static PLATEAU.RoadNetwork.Util.LineCrossPointResult;


namespace PlateauToolkit.Sandbox.RoadNetwork
{
    public class RoadNetworkLaneConverter
    {
        //temporarily keeps Lane information as RoadNetwork data
        public struct LaneConvertInfo
        {
            public List<RnDataTrack> nextTracks;
            public List<RnDataTrack> prevTracks;

            public List<RnDataLane> nextLanes;
            public List<RnDataLane> prevLanes;

            public TrafficLane trafficLane;

            public RnDataTrack track;
            public RnDataLane lane;
        }



        public List<Vector3> ConvertToSplinePoints(List<Vector3> points, int numPoints = 10)
        {
            List<Vector3> outPoints = new List<Vector3>();
            var spline = SplineTool.CreateSplineFromPoints(points);
            for (int i = 0; i < numPoints; i++)
            {
                float percent = (float)i * ((float)numPoints / 1f);
                outPoints.Add(SplineTool.GetPointOnSpline(spline, percent));
            }
            return outPoints;
        }


        public List<Vector3> ConvertToSplinePoints(Spline spline, int numPoints = 10)
        {
            List<Vector3> outPoints = new List<Vector3>();
            for (int i = 0; i < numPoints; i++)
            {
                float percent = (float)i * ((float)numPoints / 1f);
                outPoints.Add(SplineTool.GetPointOnSpline(spline, percent));
            }
            return outPoints;
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

        public List<TrafficLane> Create(RoadNetworkDataGetter getter)
        {
            float speedLimit = 15f;

            var parent = GameObject.Find("TrafficLanes");
            if (parent == null)
            {
                parent = new GameObject("TrafficLanes");
            }

            List<TrafficLane> allLanes = new();
            List<LaneConvertInfo> laneInfo = new();

            Dictionary<RnDataLane, TrafficLane> laneDict = new();
            Dictionary<RnDataTrack, TrafficLane> trackDict = new();

            int index = 0;

            var roadbases = getter.GetRoadBases();

            foreach(var rb in roadbases)
            {
                if (rb is RnDataRoad)
                {
                    RnDataRoad road = (RnDataRoad)rb;

                    List<RnDataLane> lanes = road.GetMainLanes(getter);
                    
                    foreach(var lane in lanes)
                    {
                        var points = lane.GetChildLineString(getter).GetChildPointsVector(getter);
                        if (points.Count > 1)
                        {
                            //points = ConvertToSplinePoints(points);
                            //Debug.Log($"lane points {points.Count}");

                            TrafficLane tlane = TrafficLane.Create($"TrafficLane_Road_{index++}", parent.transform, points.ToArray(), TrafficLane.TurnDirectionType.STRAIGHT, speedLimit);
                            tlane.enabled = true;
                            //allLanes.Add(tlane);
                            laneDict.Add(lane, tlane);

                            LaneConvertInfo info = new LaneConvertInfo();
                            info.trafficLane = tlane;
                            info.lane = lane;

                            RnDataIntersection nextIntersection = road.GetNextRoad(getter) as RnDataIntersection;
                            if(nextIntersection != null)
                            {
                                if (!nextIntersection.IsEmptyIntersection)
                                {
                                    info.nextTracks = nextIntersection.GetFromTracksFromLane(getter, lane);
                                }
                                else
                                {
                                    //Next Road
                                    var edges = nextIntersection.Edges;
                                    edges.RemoveAll(e => e.Road.ID == road.GetId(getter) || !e.Road.IsValid);
                                    if (edges.Count > 0)
                                    {
                                        var nextRoad = edges.FirstOrDefault().GetRoad(getter) as RnDataRoad;
                                        info.nextLanes = nextRoad.GetLanesFromPrevBorder(getter, lane.GetNextBorder(getter));
                                    }
                                }
                            }

                            RnDataIntersection prevIntersection = road.GetPrevRoad(getter) as RnDataIntersection;
                            if(prevIntersection != null)
                            {
                                if (!prevIntersection.IsEmptyIntersection)
                                {
                                    info.prevTracks = nextIntersection.GetToTracksFromLane(getter, lane);
                                }
                                else
                                {
                                    //Prev Road
                                    var edges = prevIntersection.Edges;
                                    edges.RemoveAll(e => e.Road.ID == road.GetId(getter) || !e.Road.IsValid);
                                    if (edges.Count > 0)
                                    {
                                        var prevRoad = edges.FirstOrDefault().GetRoad(getter) as RnDataRoad;
                                        info.prevLanes = prevRoad.GetLanesFromNextBorder(getter, lane.GetPrevBorder(getter));
                                    }
                                }
                            }

                            laneInfo.Add(info);
                        }
                    }
                }
                else if (rb is RnDataIntersection)
                {
                    RnDataIntersection intersection = (RnDataIntersection)rb;

                    if (intersection.IsEmptyIntersection)
                    {
                        continue;
                    }

                    var tracks = intersection.Tracks;
                    foreach(var track in tracks)
                    {
                        //var knotsPosistions = track.Spline.Knots.Select(x => (Vector3)x.Position).ToList();
                        var points = ConvertToSplinePoints(track.Spline);
                        if (points.Count > 0)
                        {
                            //Debug.Log($"lane intersection points {points.Count}");

                            TrafficLane.TurnDirectionType turnDirType = ConvertTurnType(track.TurnType);
                            TrafficLane lane = TrafficLane.Create($"TrafficLane_Intersection_{index++}", parent.transform, points.ToArray(), turnDirType, speedLimit);
                            lane.intersectionLane = true;
                            lane.enabled = true;
                            //allLanes.Add(lane);
                            trackDict.Add(track, lane);

                            LaneConvertInfo info = new LaneConvertInfo();
                            info.trafficLane = lane;
                            info.track = track;

                            var nextEdge = intersection.GetEdgesFromBorder(getter, track.GetToBorder(getter)).FirstOrDefault();
                            var nextRoad = nextEdge.GetRoad(getter) as RnDataRoad;
                            info.nextLanes = nextRoad.GetLanesFromPrevTrack(getter, track);

                            var prevEdge = intersection.GetEdgesFromBorder(getter, track.GetFromBorder(getter)).FirstOrDefault();
                            var prevRoad = prevEdge.GetRoad(getter) as RnDataRoad;
                            info.prevLanes = prevRoad.GetLanesFromNextTrack(getter, track);

                            //Stopline
                            var borderPoints = nextEdge.GetBorder(getter).GetChildLineString(getter).GetChildPointsVector(getter);
                            AWSIM.TrafficSimulation.StopLine.Create(borderPoints.FirstOrDefault(), borderPoints.LastOrDefault());

                            laneInfo.Add(info);
                        }
                    }
                }
            }

            //Prev/Next設定
            foreach (LaneConvertInfo info in laneInfo)
            {
                TrafficLane lane = info.trafficLane;

                if (info.prevLanes != null)
                {
                    foreach(var l in info.prevLanes)
                    {
                        if (laneDict.ContainsKey(l))
                            lane.PrevLanes.Add(laneDict[l]);
                    }
                }
                if (info.nextLanes != null)
                {
                    foreach (var l in info.nextLanes)
                    {
                        if (laneDict.ContainsKey(l))
                            lane.NextLanes.Add(laneDict[l]);
                    }
                }


                if (info.prevTracks != null)
                {
                    foreach (var t in info.prevTracks)
                    {
                        if (trackDict.ContainsKey(t))
                            lane.PrevLanes.Add(trackDict[t]);
                    }
                }

                if (info.nextTracks != null)
                {
                    foreach (var t in info.nextTracks)
                    {
                        if (trackDict.ContainsKey(t))
                            lane.NextLanes.Add(trackDict[t]);
                    }
                }

                allLanes.Add(lane);
            }

            return allLanes;
        }




    }
}
