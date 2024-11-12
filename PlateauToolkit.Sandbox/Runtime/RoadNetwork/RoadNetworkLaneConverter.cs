using AWSIM.TrafficSimulation;
using PLATEAU.RoadNetwork.Data;
using PLATEAU.RoadNetwork.Structure;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

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
            public RnDataRoadBase road;
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

            var parent = GameObject.Find(RoadNetworkConstants.TRAFFIC_LANE_ROOT_NAME);
            if (parent == null)
            {
                parent = new GameObject(RoadNetworkConstants.TRAFFIC_LANE_ROOT_NAME);
            }

            List<TrafficLane> allLanes = new();
            List<LaneConvertInfo> laneInfo = new();

            Dictionary<RnDataLane, TrafficLane> laneDict = new();
            Dictionary<RnDataTrack, TrafficLane> trackDict = new();

            int index = 0;

            var roadbases = getter.GetRoadBases();

            foreach (RnDataRoadBase rb in roadbases)
            {
                if (rb is RnDataRoad)
                {
                    RnDataRoad road = (RnDataRoad)rb;

                    road.TargetTran.gameObject.layer = LayerMask.NameToLayer(RoadNetworkConstants.LAYER_MASK_GROUND);

                    List<RnDataLane> lanes = road.GetMainLanes(getter);

                    foreach (var lane in lanes)
                    {
                        var points = lane.GetChildLineString(getter).GetChildPointsVector(getter);

                        //ポイント数が足りない場合は、暫定的にポイントを作成
                        if (points.Count == 0)
                        {
                            Debug.Log($"points not found");
                            points = new() { Vector3.zero, Vector3.zero };
                        }
                        else if (points.Count == 1)
                        {
                            Debug.Log($"point size is 1");
                            points.Add(points.FirstOrDefault());
                        }

                        if (lane.IsReverse)
                        {
                            points.Reverse();
                        }

                        //points = ConvertToSplinePoints(points);

                        TrafficLane trafficLane = TrafficLane.Create($"TrafficLane_Road_{rb.GetId(getter)}_{index++}", parent.transform, points.ToArray(), TrafficLane.TurnDirectionType.STRAIGHT, speedLimit);
                        trafficLane.enabled = true;
                        laneDict.Add(lane, trafficLane);

                        LaneConvertInfo info = new LaneConvertInfo();
                        info.trafficLane = trafficLane;
                        info.lane = lane;
                        info.road = rb;

                        RnDataIntersection nextIntersection = road.GetNextRoad(getter) as RnDataIntersection;
                        if (nextIntersection != null)
                        {
                            if (!nextIntersection.IsEmptyIntersection)
                            {
                                info.nextTracks = nextIntersection.GetFromTracksFromLane(getter, lane);
                            }
                            else
                            {
                                info.nextLanes = nextIntersection.GetNextLanesFromLane(getter, road, lane);
                            }
                        }

                        RnDataIntersection prevIntersection = road.GetPrevRoad(getter) as RnDataIntersection;
                        if (prevIntersection != null)
                        {
                            if (!prevIntersection.IsEmptyIntersection)
                            {
                                info.prevTracks = prevIntersection.GetToTracksFromLane(getter, lane);
                            }
                            else
                            {
                                info.prevLanes = prevIntersection.GetPrevLanesFromLane(getter, road, lane);
                            }
                        }

                        laneInfo.Add(info);
                    }
                }
                else if (rb is RnDataIntersection)
                {
                    RnDataIntersection intersection = (RnDataIntersection)rb;

                    if (intersection.IsEmptyIntersection)
                    {
                        continue;
                    }

                    intersection.TargetTran.gameObject.layer = LayerMask.NameToLayer(RoadNetworkConstants.LAYER_MASK_GROUND);

                    var tracks = intersection.Tracks;
                    foreach (RnDataTrack track in tracks)
                    {
                        List<Vector3> points = ConvertToSplinePoints(track.Spline, 5);

                        TrafficLane.TurnDirectionType turnDirType = ConvertTurnType(track.TurnType);
                        TrafficLane trafficLane = TrafficLane.Create($"TrafficLane_Intersection_{rb.GetId(getter)}_{index++}", parent.transform, points.ToArray(), turnDirType, speedLimit);
                        trafficLane.intersectionLane = true;
                        trafficLane.enabled = true;
                        trackDict.Add(track, trafficLane);

                        LaneConvertInfo info = new LaneConvertInfo();
                        info.trafficLane = trafficLane;
                        info.track = track;
                        info.road = rb;

                        info.nextLanes = intersection.GetNextLanesFromTrack(getter, track);

                        info.prevLanes = intersection.GetPrevLanesFromTrack(getter, track);

                        //Stopline
                        RnDataNeighbor nextEdge = intersection.GetEdgesFromBorder(getter, track.GetToBorder(getter)).FirstOrDefault();
                        List<Vector3> borderPoints = nextEdge.GetBorder(getter).GetChildLineString(getter).GetChildPointsVector(getter);
                        AWSIM.TrafficSimulation.StopLine.Create(borderPoints.FirstOrDefault(), borderPoints.LastOrDefault());

                        laneInfo.Add(info);
                    }
                }
            }

            //Prev/Next設定
            foreach (LaneConvertInfo info in laneInfo)
            {
                TrafficLane lane = info.trafficLane;
                lane.SetRoadNetworkData(info.road, info.lane, info.track);

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
#if UNITY_EDITOR

                GameObjectUtility.SetStaticEditorFlags(lane.gameObject, StaticEditorFlags.BatchingStatic | StaticEditorFlags.ContributeGI | StaticEditorFlags.OccluderStatic | StaticEditorFlags.OccludeeStatic);
#endif

                allLanes.Add(lane);
            }

            return allLanes;
        }
    }
}
