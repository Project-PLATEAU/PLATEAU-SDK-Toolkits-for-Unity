using AWSIM.TrafficSimulation;
using PLATEAU.RoadNetwork.Data;
using PLATEAU.RoadNetwork.Structure;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Splines;

namespace PlateauToolkit.Sandbox.RoadNetwork
{
    public class RoadNetworkLaneConverter
    {
        public static readonly bool ignoreReversedLane = false;
        public static readonly bool useSimpleSplinePoints = true; //Trackのspline形状が破綻している場合に停止するのを回避

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
                float percent = (float)i / ((float)numPoints);
                outPoints.Add(SplineTool.GetPointOnSpline(spline, percent));
            }
            return outPoints;
        }

        public List<Vector3> GetSimplePoints(Spline spline) //First/Last Points only
        {
            List<Vector3> outPoints = new List<Vector3>();
            outPoints.Add(spline.Knots.FirstOrDefault().Position);
            outPoints.Add(spline.Knots.LastOrDefault().Position);
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
            float speedLimit = RoadNetworkConstants.SPEED_LIMIT;

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
                        if (points.Count < 2)
                        {
                            Debug.Log($"point size is {points.Count}");
                            //points = new() { Vector3.zero, Vector3.zero };
                            continue;
                        }

                        if (ignoreReversedLane && lane.IsReverse)
                            points.Reverse();

                        //points = ConvertToSplinePoints(points);

                        TrafficLane trafficLane = TrafficLane.Create($"TrafficLane_Road_{rb.GetId(getter)}_{index++}", parent.transform, points.ToArray(), TrafficLane.TurnDirectionType.STRAIGHT, speedLimit);
                        trafficLane.enabled = true;
                        laneDict.Add(lane, trafficLane);

                        LaneConvertInfo info = new LaneConvertInfo();
                        info.trafficLane = trafficLane;
                        info.lane = lane;
                        info.road = rb;

                        RnDataIntersection nextIntersection = road.GetNextRoad(getter) as RnDataIntersection;
                        RnDataIntersection prevIntersection = road.GetPrevRoad(getter) as RnDataIntersection;

                        if (!ignoreReversedLane && lane.IsReverse) //反転している場合 Prev/Nextが逆になるらしい
                        {
                            (nextIntersection, prevIntersection) = (prevIntersection, nextIntersection);
                        }

                        if (nextIntersection != null)
                        {
                            if (!nextIntersection.IsEmptyIntersection)
                            {
                                var tracks = nextIntersection.GetFromTracksFromLane(getter, lane);
                                info.nextTracks = nextIntersection.FilterAvailableToTracks(getter,tracks); //一方通行侵入除外
                                //info.nextTracks = tracks;

                                Debug.Log($"<color=red>Next Tracks {tracks.Count} -> {info.nextTracks.Count}</color>");
                            }
                            else
                            {
                                info.nextLanes = nextIntersection.GetNextLanesFromLane(getter, road, lane, ignoreReversedLane);
                            }
                        }

                        if (prevIntersection != null)
                        {
                            if (!prevIntersection.IsEmptyIntersection)
                            {
                                var tracks = prevIntersection.GetToTracksFromLane(getter, lane);
                                info.prevTracks = prevIntersection.FilterAvailableFromTracks(getter, tracks); //一方通行侵入除外
                                //info.prevTracks = tracks;

                                Debug.Log($"<color=red>Prev Tracks {tracks.Count} -> {info.prevTracks.Count}</color>");
                            }
                            else
                            {
                                info.prevLanes = prevIntersection.GetPrevLanesFromLane(getter, road, lane, ignoreReversedLane);
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
                        List<Vector3> points = useSimpleSplinePoints ? GetSimplePoints(track.Spline) : ConvertToSplinePoints(track.Spline, 5);

                        TrafficLane.TurnDirectionType turnDirType = ConvertTurnType(track.TurnType);
                        TrafficLane trafficLane = TrafficLane.Create($"TrafficLane_Intersection_{rb.GetId(getter)}_{index++}", parent.transform, points.ToArray(), turnDirType, speedLimit);
                        trafficLane.intersectionLane = true;
                        trafficLane.enabled = true;
                        trackDict.Add(track, trafficLane);

                        LaneConvertInfo info = new LaneConvertInfo();
                        info.trafficLane = trafficLane;
                        info.track = track;
                        info.road = rb;

                        info.nextLanes = intersection.GetNextLanesFromTrack(getter, track, ignoreReversedLane);
                        //info.nextLanes = intersection.GetNextLanesFromTrack(getter, track);

                        info.prevLanes = intersection.GetPrevLanesFromTrack(getter, track, ignoreReversedLane);
                        //info.prevLanes = intersection.GetPrevLanesFromTrack(getter, track);

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
                        else
                            Debug.Log($"Lane not found : {info.trafficLane.name}");
                    }
                }
                if (info.nextLanes != null)
                {
                    foreach (var l in info.nextLanes)
                    {
                        if (laneDict.ContainsKey(l))
                            lane.NextLanes.Add(laneDict[l]);
                        else
                            Debug.Log($"Lane not found : {info.trafficLane.name}");
                    }
                }

                if (info.prevTracks != null)
                {
                    foreach (var t in info.prevTracks)
                    {
                        if (trackDict.ContainsKey(t))
                            lane.PrevLanes.Add(trackDict[t]);
                        else
                            Debug.Log($"Track not found : {info.trafficLane.name}");
                    }
                }

                if (info.nextTracks != null)
                {
                    foreach (var t in info.nextTracks)
                    {
                        if (trackDict.ContainsKey(t))
                            lane.NextLanes.Add(trackDict[t]);
                        else
                            Debug.Log($"Track not found : {info.trafficLane.name}");
                    }
                }

#if UNITY_EDITOR

                GameObjectUtility.SetStaticEditorFlags(lane.gameObject, StaticEditorFlags.BatchingStatic | StaticEditorFlags.ContributeGI | StaticEditorFlags.OccluderStatic | StaticEditorFlags.OccludeeStatic);
#endif

                //Next/Prev共に取得できない場合のDebug 出力
                //DebugLanes(lane, getter);

                if (lane.PrevLanes.Count > 0 || lane.NextLanes.Count > 0) //接続情報がないLaneは除外
                    allLanes.Add(lane);
            }

            //Debug.Log($"<color=green>Num not connected lanes {allLanes.FindAll(x => x.NextLanes.Count == 0 && x.PrevLanes.Count == 0).Count()}</color>");

            return allLanes;
        }

        //Next/Prev共に取得できない場合のDebug確認用
        void DebugLanes(TrafficLane lane, RoadNetworkDataGetter getter)
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

                            foreach(var roadid in roadids)
                            {
                                var borders = ((RnDataRoad)getter.GetRoadBases().TryGet(roadid)).GetLAllBorderWaysFromMainLanes(getter);
                                var borderids = ((RnDataRoad)getter.GetRoadBases().TryGet(roadid)).GetLAllBordersFromMainLanes(getter).Select(x => x.GetId(getter));
                                Debug.Log($"Next Road borders {string.Join(",", borderids)}");

                                foreach (var border in borders)
                                {
                                    Debug.Log($"Next Road border {border.LineString.ID} is same line {nextBorder.LineString.ID} {border.IsSameLine(nextBorder)} ");
                                }
                            }

                            var nextLanes = nextIntersection.GetNextLanesFromLane(getter, lane.rnRoad, lane.rnLane, ignoreReversedLane);

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
