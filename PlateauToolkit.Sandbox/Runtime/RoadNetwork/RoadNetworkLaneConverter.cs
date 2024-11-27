using AWSIM.TrafficSimulation;
using PLATEAU.CityInfo;
using PLATEAU.RoadNetwork.Data;
using PLATEAU.RoadNetwork.Structure;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

namespace PlateauToolkit.Sandbox.RoadNetwork
{
    /// <summary>
    /// RoadNetworkからAWSIM TrafficLaneを生成
    /// </summary>
    public class RoadNetworkLaneConverter
    {

        public static readonly bool SHOW_DEBUG_INFO = false;

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

            public AWSIM.TrafficSimulation.StopLine stopline;
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

        void SetAsGroundLayer(List<PLATEAUCityObjectGroup> cityObjs)
        {
            if (cityObjs?.Count > 0)
            {
                foreach (var item in cityObjs)
                {
                    item.gameObject.layer = LayerMask.NameToLayer(RoadNetworkConstants.LAYER_MASK_GROUND);
                }
            }
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

                    SetAsGroundLayer(road.TargetTrans);

                    List<RnDataLane> lanes = road.GetMainLanes(getter);

                    foreach (var lane in lanes)
                    {
                        var points = lane.GetChildLineString(getter).GetChildPointsVector(getter);

                        //ポイント数が足りない場合
                        if (points.Count < 2)
                        {
                            DebugLog($"point size is {points.Count}");
                            continue;
                        }

                        if (RoadNetworkConstants.IGNORE_REVERSED_LANE && lane.IsReverse)
                            points.Reverse();

                        if (RoadNetworkConstants.USE_SIMPLE_LINESTRINGS && points.Count > 3)
                            points = ConvertToSplinePoints(points, RoadNetworkConstants.SPLINE_POINTS); //平滑化

                        TrafficLane trafficLane = TrafficLane.Create($"TrafficLane_Road_{rb.GetId(getter)}_{index++}", parent.transform, points.ToArray(), TrafficLane.TurnDirectionType.STRAIGHT, speedLimit);
                        trafficLane.enabled = true;
                        laneDict.Add(lane, trafficLane);

                        LaneConvertInfo info = new LaneConvertInfo();
                        info.trafficLane = trafficLane;
                        info.lane = lane;
                        info.road = rb;

                        RnDataIntersection nextIntersection = road.GetNextRoad(getter) as RnDataIntersection;
                        RnDataIntersection prevIntersection = road.GetPrevRoad(getter) as RnDataIntersection;

                        if (!RoadNetworkConstants.IGNORE_REVERSED_LANE && lane.IsReverse) //反転している場合 Prev/Nextを逆に
                        {
                            (nextIntersection, prevIntersection) = (prevIntersection, nextIntersection);
                        }

                        if (nextIntersection != null)
                        {
                            if (!nextIntersection.IsEmptyIntersection)
                            {
                                var tracks = nextIntersection.GetFromTracksFromLane(getter, lane);
                                info.nextTracks = nextIntersection.FilterAvailableToTracks(getter,tracks); //一方通行侵入除外

                                //Stopline
                                if (RoadNetworkConstants.ADD_STOPLINES)
                                {
                                    var border = lane.GetNextBorder(getter);
                                    List<Vector3> borderPoints = border.GetChildLineString(getter).GetChildPointsVector(getter);
                                    info.stopline = AWSIM.TrafficSimulation.StopLine.Create(borderPoints.FirstOrDefault(), borderPoints.LastOrDefault());
                                }
                            }
                            else
                            {
                                info.nextLanes = nextIntersection.GetNextLanesFromLane(getter, road, lane, RoadNetworkConstants.IGNORE_REVERSED_LANE);
                            }
                        }

                        if (prevIntersection != null)
                        {
                            if (!prevIntersection.IsEmptyIntersection)
                            {
                                var tracks = prevIntersection.GetToTracksFromLane(getter, lane);
                                info.prevTracks = prevIntersection.FilterAvailableFromTracks(getter, tracks); //一方通行侵入除外
                            }
                            else
                            {
                                info.prevLanes = prevIntersection.GetPrevLanesFromLane(getter, road, lane, RoadNetworkConstants.IGNORE_REVERSED_LANE);
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

                    SetAsGroundLayer(intersection.TargetTrans);

                    var tracks = intersection.Tracks;
                    foreach (RnDataTrack track in tracks)
                    {
                        List<Vector3> points = RoadNetworkConstants.USE_SIMPLE_SPLINE_POINTS ? GetSimplePoints(track.Spline) : ConvertToSplinePoints(track.Spline, RoadNetworkConstants.SPLINE_POINTS);

                        TrafficLane.TurnDirectionType turnDirType = ConvertTurnType(track.TurnType);
                        TrafficLane trafficLane = TrafficLane.Create($"TrafficLane_Intersection_{rb.GetId(getter)}_{index++}", parent.transform, points.ToArray(), turnDirType, speedLimit);
                        trafficLane.intersectionLane = true;
                        trafficLane.enabled = true;
                        trackDict.Add(track, trafficLane);

                        LaneConvertInfo info = new LaneConvertInfo();
                        info.trafficLane = trafficLane;
                        info.track = track;
                        info.road = rb;

                        info.nextLanes = intersection.GetNextLanesFromTrack(getter, track, RoadNetworkConstants.IGNORE_REVERSED_LANE);
  
                        info.prevLanes = intersection.GetPrevLanesFromTrack(getter, track, RoadNetworkConstants.IGNORE_REVERSED_LANE);

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
                            DebugLog($"Lane not found : {info.trafficLane.name}");
                    }
                }
                if (info.nextLanes != null)
                {
                    foreach (var l in info.nextLanes)
                    {
                        if (laneDict.ContainsKey(l))
                            lane.NextLanes.Add(laneDict[l]);
                        else
                            DebugLog($"Lane not found : {info.trafficLane.name}");
                    }
                }

                if (info.prevTracks != null)
                {
                    foreach (var t in info.prevTracks)
                    {
                        if (trackDict.ContainsKey(t))
                            lane.PrevLanes.Add(trackDict[t]);
                        else
                            DebugLog($"Track not found : {info.trafficLane.name}");
                    }
                }

                if (info.nextTracks != null)
                {
                    foreach (var t in info.nextTracks)
                    {
                        if (trackDict.ContainsKey(t))
                            lane.NextLanes.Add(trackDict[t]);
                        else
                            DebugLog($"Track not found : {info.trafficLane.name}");
                    }
                }

                if (info.stopline != null)
                {
                    lane.StopLine = info.stopline;
                }

#if UNITY_EDITOR

                GameObjectUtility.SetStaticEditorFlags(lane.gameObject, StaticEditorFlags.BatchingStatic | StaticEditorFlags.ContributeGI | StaticEditorFlags.OccluderStatic | StaticEditorFlags.OccludeeStatic);
#endif

                //Next/Prev共に取得できない場合のDebug 出力
                if (SHOW_DEBUG_INFO)
                    DebugLaneConverter.DebugLanes(lane, getter);

                if (lane.PrevLanes.Count > 0 || lane.NextLanes.Count > 0) //接続情報がないLaneは除外
                    allLanes.Add(lane);
            }

            return allLanes;
        }

        void DebugLog(string debugstr)
        {
            if (SHOW_DEBUG_INFO)
                Debug.Log(debugstr);
        }
    }
}
