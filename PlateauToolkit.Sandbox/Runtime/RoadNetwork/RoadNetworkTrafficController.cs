using PLATEAU.RoadNetwork.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using PLATEAU.RoadNetwork.Structure;
using static Codice.CM.Common.CmCallContext;

namespace PlateauToolkit.Sandbox.RoadNetwork
{
    [Serializable]
    public class RoadInfo
    {
        [SerializeField]
        public int m_RoadId;
        [SerializeField]
        public int m_LaneIndex;
        [SerializeField]
        public int m_TrackIndex;
        [SerializeField]
        public bool m_IsReverse;
        [SerializeField]
        public int m_VehicleID;

        [SerializeField]
        public Bounds m_Bounds;

        [SerializeField]
        public float m_CurrentProgress;

        public RoadInfo() { }
        public RoadInfo(int road, int lane)
        {
            m_RoadId = road;
            m_LaneIndex = lane;
            m_TrackIndex = 0;
            m_IsReverse = false;
            m_VehicleID = -1;
            m_CurrentProgress = 0f;
        }

        public RoadInfo Clone()
        {
            RoadInfo info = new RoadInfo();
            info.m_VehicleID = m_VehicleID;
            info.m_Bounds = m_Bounds;
            return info;
        }
    }

    [Serializable]
    public class DebugInfo
    {
        [SerializeField] public int m_NumVehiclesOnTheRoad;
        [SerializeField] public int m_NumVehiclesOnTheLane;
        [SerializeField] public int m_NumVehiclesForward;
        [SerializeField] public float m_LastCarProgress;

        [SerializeField] public int m_NumVehiclesOncominglane; //対向車(Intersection only)
        [SerializeField] public int m_NumVehiclesCrossing; //横断車(Intersection only)

        [SerializeField] public float m_Distance_from_Other;
        [SerializeField] public float m_Speed;

        [SerializeField]
        public float m_CurrentProgress;

        [SerializeField]
        public float m_RoadID;
        [SerializeField]
        public float m_LaneIndex;

        [SerializeField]
        public bool m_IsPriorityTrack;

        [SerializeField]
        public string m_DebugString;

        public DebugInfo(TrafficManager.LaneStatus info, ProgressResult prg, RoadNetworkTrafficController cont)
        {
            m_NumVehiclesOnTheRoad = info.m_NumVehiclesOnTheRoad;
            m_NumVehiclesOnTheLane = info.m_NumVehiclesOnTheLane;
            m_NumVehiclesForward = info.m_NumVehiclesForward;
            m_LastCarProgress = info.m_LastCarProgress;
            m_NumVehiclesOncominglane = info.m_NumVehiclesOncominglane;
            m_NumVehiclesCrossing = info.m_NumVehiclesCrossing;

            m_Distance_from_Other = prg.m_Distance_from_Other;
            m_Speed = prg.m_Speed;

            m_RoadID = info.m_RoadID;
            m_LaneIndex = info.m_LaneIndex;

            m_IsPriorityTrack = info.m_IsPriorityTrack;

            m_DebugString = info.m_DebugString;

            m_CurrentProgress = cont.m_RoadInfo.m_CurrentProgress;
        }
    }

    [Serializable]
    public class RoadNetworkTrafficController : IDisposable
    {

        //Debug用 ==========================================================
        //public List<RnDataWay> expectedBorders = new List<RnDataWay>();
        //public List<RnDataWay> actualBorders = new List<RnDataWay>();
        [SerializeField]
        public DebugInfo m_DebugInfo;
        //Debug用 ==========================================================


        [SerializeField]
        public RoadInfo m_RoadInfo;

        //Road (Roadの場合自動的にセット）
        [SerializeField]
        public RnDataRoad m_Road;

        //Intersection (Intersectionの場合自動的にセット）
        [SerializeField]
        public RnDataIntersection m_Intersection;

        [SerializeField]
        public RnDataWay m_FromBorder;
        [SerializeField]
        public RnDataWay m_ToBorder;

        // Linestring位置パーセント( 0f - 1f )
        //[SerializeField]
        //public float m_CurrentProgress;

        [SerializeField]
        public float m_Distance;

        [SerializeField]
        public float m_Speed;

        [SerializeField]
        public int m_LastRoadId;

        public bool m_EnableRunningBackwards = false; //逆走可能・禁止

        TrafficManager m_TrafficManager;

        public bool IsRoad => m_Road != null;
        public bool IsIntersection => m_Intersection != null;

        public bool IsValid => IsRoad || IsIntersection;

        //prev/nextの切替用
        public bool IsReversed
        {
            get
            {
                return m_RoadInfo?.m_IsReverse ?? false;
            }
        }

        //LineStringの向き判定
        public bool IsLineStringReversed
        {
            get
            {
                return IsReversed && !(GetLane()?.IsReverse ?? false);
            }
        }

        public RnDataRoad GetRoad()
        {
            if (IsRoad)
            {
                if (m_Road.GetId(RnGetter) == -1)
                {
                    m_Road = RnGetter.GetRoadBases().TryGet(m_RoadInfo.m_RoadId) as RnDataRoad;
                }
                return m_Road;
            }
            return null;
        }

        public RnDataIntersection GetIntersection()
        {
            if (IsIntersection)
            {
                if (m_Intersection.GetId(RnGetter) == -1)
                    m_Intersection = RnGetter.GetRoadBases().TryGet(m_RoadInfo.m_RoadId) as RnDataIntersection;
                return m_Intersection;
            }
            return null;
        }

        RoadNetworkDataGetter RnGetter
        {
            get
            {
                return TrafficManager?.RnGetter;
            }
        }

        TrafficManager TrafficManager
        {
            get
            {
                if (m_TrafficManager == null)
                {
                    m_TrafficManager = GameObject.FindObjectOfType<TrafficManager>();
                    if (m_TrafficManager == null)
                    {
                        Debug.LogError($"TrafficManager is null");
                    }
                }
                return m_TrafficManager;
            }
        }

        public RnDataLineString GetLineString()
        {
            if (!IsRoad)
            {
                return null;
            }
            return GetRoad().GetChildLineString(RnGetter, m_RoadInfo.m_LaneIndex);
        }

        public RnDataWay GetWay()
        {
            if (!IsRoad)
            {
                return null;
            }
            return GetRoad().GetChildWay(RnGetter, m_RoadInfo.m_LaneIndex);
        }

        public RnDataLane GetLane()
        {
            if (!IsRoad)
            {
                return null;
            }
            return GetRoad().GetChildLane(RnGetter, m_RoadInfo.m_LaneIndex);
        }

        public RnDataTrack GetTrack()
        {
            if (!IsIntersection || GetIntersection()?.Tracks == null)
            {
                return null;
            }
            else if (GetIntersection().Tracks.Count < m_RoadInfo.m_TrackIndex)
            {
                return null;
            }
            return GetIntersection().Tracks[m_RoadInfo.m_TrackIndex];
        }

        public void Initialize()
        {
            TrafficManager.SetRoadInfo(m_LastRoadId, m_RoadInfo);
        }

        //初回
        public RoadNetworkTrafficController(RoadInfo roadInfo)
        {
            m_RoadInfo = roadInfo;
            SetRoadBase();
            Initialize();
            Debug.Log($"<color=yellow>roadInfo {roadInfo.m_RoadId} {roadInfo.m_LaneIndex}</color>");
        }

        public RoadNetworkTrafficController Respawn()
        {
            Debug.Log($"<color=blue>Respawn {m_RoadInfo.m_VehicleID}</color>");

            var (pos, road, lane) = m_TrafficManager.GetRandomRoad();
            //var info = new RoadInfo(
            //            road.GetId(RnGetter),
            //            road.GetLaneIndexOfMainLanes(RnGetter, lane));
            //info.m_VehicleID = m_RoadInfo.m_VehicleID;
            var info = m_RoadInfo.Clone();
            info.m_RoadId = road.GetId(RnGetter);
            info.m_LaneIndex = road.GetLaneIndexOfMainLanes(RnGetter, lane);
            return new(info);
        }

        public void SetDistance(float distance)
        {
            m_Distance = distance;
        }

        public void SetBounds(Bounds bounds)
        {
            m_RoadInfo.m_Bounds = bounds;
        }

        public ProgressResult SetProgress(float progress)
        {
            //m_CurrentProgress = progress;
            m_RoadInfo.m_CurrentProgress = progress;

            TrafficManager.LaneStatus info = TrafficManager.GetLaneInfo(m_RoadInfo); // Debug Lane info

            var result = new ProgressResult(this, info, RnGetter);

            m_Speed = result.m_Speed;

            DebugLaneStatus(info, result);

            return result;
        }

        void DebugLaneStatus(TrafficManager.LaneStatus info, ProgressResult prg)
        {
            if (info.m_IsValid)
            {
                m_DebugInfo = new DebugInfo(info, prg, this);
            }
        }

        //次のRoadBaseを取得
        public RoadNetworkTrafficController GetNextRoad()
        {
            if (IsRoad)
            {
                RnDataRoad road = GetRoad();
                RnDataLane lane = GetLane();
                RnDataRoadBase nextRoad = IsReversed ? road.GetPrevRoad(RnGetter) : road.GetNextRoad(RnGetter);

                if (nextRoad.IsValid(RnGetter))
                {
                    RoadNetworkTrafficController nextParam = new(this, nextRoad);

                    if (nextParam.IsValid)
                    {
                        //Debug.Log($"<color=green>next road found {nextRoad.GetId(RnGetter)} </color>");
                        return nextParam;
                    }
                }
                else
                {
                    //取得失敗
                    Debug.Log($"<color=red>Next Road is not Valid Prev{road?.Prev.ID} Next{road?.Next.ID}</color>");
                }
            }
            else if (IsIntersection)
            {
                RnDataIntersection intersection = GetIntersection();
                RnDataTrack track = GetTrack();
                RnDataWay toWay = track.GetToBorder(RnGetter);
                RnDataWay fromWay = track.GetFromBorder(RnGetter);
                RnDataWay way = IsReversed ? fromWay : toWay;
                List<RnDataNeighbor> edges = intersection.GetEdgesFromBorder(RnGetter, way);

                if (edges.Count <= 0)
                {
                    //反対のBorderもチェック
                    way = IsReversed ? toWay : fromWay;
                    edges = intersection.GetEdgesFromBorder(RnGetter, way);
                    if (edges.Count > 0)
                        Debug.Log($"<color=magenta>next road not found but found opposite border {edges.Count} </color>");
                }

                if (intersection.IsEmptyIntersection)
                {
                    edges = intersection.Edges;
                    edges.RemoveAll(e => e.Road.ID == m_LastRoadId);
                    Debug.Log($"<color=green>IsEmptyIntersection edges found {edges.Count}</color>");
                }

                if (edges.Count > 0)
                {
                    // Edge 抽選
                    RnDataNeighbor edge = edges[UnityEngine.Random.Range(0, edges.Count)];
                    RnDataRoadBase nextRoad = edge.GetRoad(RnGetter);
                    RoadNetworkTrafficController nextParam = new(this, nextRoad);

                    if (nextParam.IsValid)
                    {
                        //Debug.Log($"<color=green>next road found {nextRoad.GetId(RnGetter)}</color>");
                        return nextParam;
                    }
                }
                else
                {
                    //取得失敗
                    //Debug
                    var toborders = String.Join(",", intersection.Tracks.Select(x => x.ToBorder.ID));
                    var fromborders = String.Join(",", intersection.Tracks.Select(x => x.FromBorder.ID));
                    var connectedRoadId = String.Join(",", intersection.GetAllConnectedRoads(RnGetter).Select(x => x.GetId(RnGetter)));
                    var edgeBorders = String.Join(",", intersection.Edges.Select(x => x.GetBorder(RnGetter).GetId(RnGetter)));
                    //expectedBorders = new() { RnGetter.GetWays().TryGet(GetLane().PrevBorder), RnGetter.GetWays().TryGet(GetLane().NextBorder) };
                    Debug.Log($"<color=red>Intersection Track ToBorders {toborders} FromBorders {fromborders}</color>");
                    Debug.Log($"<color=red>Intersection Edge Roads {connectedRoadId} count {intersection.GetAllConnectedRoads(RnGetter).Count}</color>");
                    Debug.Log($"<color=red>Intersection All Edge Borders {edgeBorders} count {intersection.Edges.Count} </color>");
                    //actualBorders = intersection.Edges.Select(x => x.GetBorder(RnGetter)).ToList();
                }
            }

            Debug.Log($"<color=red>next road not found</color>");

            //return null;
            //次が見つからない場合は、初回位置に戻る

            //TLS Allocator ALLOC_TEMP_TLS, underlying allocator ALLOC_TEMP_MAIN has unfreed allocations
            return Respawn();
        }

        //次回作成 (Road / Road のつなぎ）
        public RoadNetworkTrafficController(RoadNetworkTrafficController current, RnDataRoadBase next)
        {
            bool success = false;
            if (next == null)
            {
                TrafficManager.RemoveRoadInfo(m_LastRoadId, current.m_RoadInfo.m_VehicleID);
                Debug.LogError($"<color=cyan>next is null</color>");
                return;
            }

            RoadInfo nextRoadInfo = current.m_RoadInfo.Clone();
            if (current.IsIntersection && next is RnDataRoad)
            {
                // Intersection -> Road 
                Debug.Log($"<color=cyan>Intersection {current.GetIntersection().GetId(RnGetter)}-> Road {next.GetId(RnGetter)} </color>");

                var nextRoad = next as RnDataRoad;
                List<RnDataLane> lanes = new();

                RnDataWay nextBorder = current.IsReversed ? current.m_FromBorder : current.m_ToBorder;

                //Linestringから取得
                lanes.AddRange(nextRoad.GetLanesFromPrevBorder(RnGetter, nextBorder));
                //Debug.Log($"<color=green>Lanes From LineString {nextBorder.LineString.ID} Count {lanes.Count}</color>");

                if (lanes.Count <= 0 && m_EnableRunningBackwards) //逆走
                {
                    lanes.AddRange(nextRoad.GetLanesFromNextBorder(RnGetter, nextBorder));
                    if (lanes.Count > 0 )
                    {
                        //expectedBorders = new() { nextBorder };
                        //Debug.Log($"<color=green>Lanes From LineString Reverse {nextBorder.LineString.ID} Count {lanes.Count}</color>");
                        nextRoadInfo.m_IsReverse = true;
                    }
                }

                if (lanes.Count <= 0)
                {
                    Debug.LogError($"<color=red>Failed to get Lanes from Last To Border {nextBorder.GetId(RnGetter)}</color>");
                }

                if (lanes.Count > 0)
                {
                    RnDataLane lane = TrafficManager.GetLaneByLottery(nextRoad, lanes); //抽選 レーン
                    nextRoadInfo.m_LaneIndex = nextRoad.GetMainLanes(RnGetter).IndexOf(lane);

                    //Debug.Log($"<color=green>Lane Next Border {lane.GetNextBorder(RnGetter).LineString.ID} Prev Border {lane.GetPrevBorder(RnGetter).LineString.ID}</color>");
                    success = true;
                }
                else
                {
                    //取得失敗
                    //expectedBorders = new() { current.GetTrack().GetToBorder(RnGetter) };
                    var actualBorders = nextRoad.GetAllNextBorders(RnGetter);
                    actualBorders.AddRange(nextRoad.GetAllPrevBorders(RnGetter));
                    var actual = String.Join(",", actualBorders.Select(x => x.GetId(RnGetter)));
                    Debug.LogError($"<color=magenta>lane not found from border exptected : {current.GetTrack().ToBorder.ID} actual {actual}</color>");
                }
            }
            //intersection
            else if (current.IsRoad && next is RnDataIntersection)
            {
                // Road -> Intersection
                Debug.Log($"<color=cyan>Road {current.GetRoad().GetId(RnGetter)}-> Intersection {next.GetId(RnGetter)} </color>");

                //Road way
                var intersection = next as RnDataIntersection;

                //Borderから取得
                RnDataWay fromBorder = IsReversed ? current.m_FromBorder : current.m_ToBorder;
                List<RnDataTrack> tracks = intersection.GetFromTracksFromBorder(RnGetter, fromBorder);

                if (tracks.Count <= 0 && m_EnableRunningBackwards) //逆走
                {
                    tracks = intersection.GetToTracksFromBorder(RnGetter, fromBorder);
                    if (tracks.Count > 0)
                    {
                        nextRoadInfo.m_IsReverse = true;
                    }
                }

                if (tracks.Count <= 0 && m_EnableRunningBackwards)
                {
                    Debug.Log($"<color=red>Failed to get Tracks from Last To Border {current.m_ToBorder.GetId(RnGetter)}</color>");
                    //逆Border から取得できるかTry？？？
                    fromBorder = IsReversed ? current.m_ToBorder : current.m_FromBorder;
                    tracks = intersection.GetFromTracksFromBorder(RnGetter, fromBorder);

                    if (tracks.Count > 0)
                        Debug.Log($"<color=magenta>Failed to get from ToBorder but found from FromBorder : tracks.Count {tracks.Count}</color>");
                    else
                    {
                        tracks = intersection.GetToTracksFromBorder(RnGetter, fromBorder);
                        if (tracks.Count > 0)
                            Debug.Log($"<color=magenta>Failed to get from ToBorder but found from FromBorder : tracks.Count {tracks.Count}</color>");
                    }
                    //取得できちゃった場合はとりあえず無理やり書き換え
                    if (tracks.Count > 0)
                        nextRoadInfo.m_IsReverse = true;
                }

                //Track 抽選
                if (tracks.Count > 0)
                {
                    //Debug.Log($"<color=green>tracks.Count {tracks.Count}</color>");

                    var track = TrafficManager.GetTrackByLottery(intersection, tracks); //抽選 Track
                    nextRoadInfo.m_TrackIndex = intersection.Tracks.IndexOf(track);
                    nextRoadInfo.m_IsReverse = current.GetLane().IsReverse;
                    success = true;
                }
                else
                {
                    //失敗
                    //Debug
                    var toborders = String.Join(",", intersection.Tracks.Select(x => x.ToBorder.ID));
                    var fromborders = String.Join(",", intersection.Tracks.Select(x => x.FromBorder.ID));
                    var connectedRoadId = String.Join(",", intersection.GetAllConnectedRoads(RnGetter).Select(x => x.GetId(RnGetter)));
                    var edgeBorders = String.Join(",", intersection.Edges.Select(x => x.GetBorder(RnGetter).GetId(RnGetter)));
                    //expectedBorders = new() { RnGetter.GetWays().TryGet(current.GetLane().PrevBorder), RnGetter.GetWays().TryGet(current.GetLane().NextBorder) };
                    //actualBorders = intersection.Edges.Select(x => x.GetBorder(RnGetter)).ToList();

                    Debug.LogError($"<color=red>Intersection Track ToBorders {toborders} FromBorders {fromborders} expected Lane PrevBorder{current.GetLane().PrevBorder.ID} NextBorder{current.GetLane().NextBorder.ID}</color>");
                    Debug.LogError($"<color=red>Intersection Edge Roads {connectedRoadId} expected Road {current.GetRoad().GetId(RnGetter)} </color>");
                    Debug.LogError($"<color=red>Intersection Edge Borders {edgeBorders} </color>");
                    Debug.LogError($"No tracks found.");
                }
            }
            else if (next is RnDataRoad && current.IsRoad) //Road->Road(あり得ない？）
            {
                Debug.LogError($"Road {current.m_Road.GetId(RnGetter)} -> Road {next.GetId(RnGetter)} : {current.m_RoadInfo.m_VehicleID}");
                //そのまま
                //nextRoadInfo = current.m_RoadInfo;
                //nextRoadInfo.m_RoadId = next.GetId(RnGetter);
                //success = true;

                //Debug.Break();
            }
            else if (next is RnDataIntersection && current.IsIntersection) //Intersection -> Intersection (あり得ない？）
            {
                Debug.LogError($"Intersection {current.m_Intersection.GetId(RnGetter)} -> Intersection {next.GetId(RnGetter)} : {current.m_RoadInfo.m_VehicleID}");
                //そのまま
                //nextRoadInfo = current.m_RoadInfo;
                //nextRoadInfo.m_RoadId = next.GetId(RnGetter);
                //success = true;

                //Debug.Break();
            }

            if (success)
            {
                m_LastRoadId = current.m_RoadInfo.m_RoadId;
                m_RoadInfo = nextRoadInfo;
                m_RoadInfo.m_RoadId = next.GetId(RnGetter);
                //m_RoadInfo.m_VehicleID = current.m_RoadInfo.m_VehicleID;
                //expectedBorders = new() { current.m_ToBorder };
                success = SetRoadBase();

                TrafficManager.SetRoadInfo(m_LastRoadId, m_RoadInfo);
                SetProgress(0f);
            }
            if (!success)
            {
                TrafficManager.RemoveRoadInfo(m_LastRoadId, current.m_RoadInfo.m_VehicleID);
                Debug.LogError($"<color=cyan>SetRoadBase Failed {next.GetId(RnGetter)}</color>");
            }
        }

        bool SetRoadBase()
        {
            var roadBase = RnGetter.GetRoadBases().TryGet(m_RoadInfo.m_RoadId);
            if (roadBase is RnDataRoad)
            {
                if (m_RoadInfo.m_LaneIndex < 0)
                {
                    Debug.LogError($"m_RoadInfo.m_LaneIndex not set {m_RoadInfo.m_LaneIndex} ");
                    return false;
                }
                m_Road = roadBase as RnDataRoad;

                var reverse = GetLane()?.IsReverse ?? false;

                //laneのReverse判定はここで行う
                if (reverse)
                    m_RoadInfo.m_IsReverse = !m_RoadInfo.m_IsReverse;

                var prevBorder = GetLane().GetPrevBorder(RnGetter);
                var nextBorder = GetLane().GetNextBorder(RnGetter);

                m_FromBorder = prevBorder;
                m_ToBorder = nextBorder;

                Debug.Log($"<color=yellow>SetRoadBase : Road lane {m_RoadInfo.m_LaneIndex}</color>");
            }
            else if (roadBase is RnDataIntersection)
            {
                if (m_RoadInfo.m_TrackIndex < 0)
                {
                    Debug.LogError($"m_RoadInfo.m_TrackIndex not set {m_RoadInfo.m_TrackIndex} ");
                    return false;
                }

                m_Intersection = roadBase as RnDataIntersection;
                var reverse = GetLane()?.IsReverse ?? false;
                m_RoadInfo.m_IsReverse = reverse;

                var fromBorder = GetTrack().GetFromBorder(RnGetter);
                var toBorder = GetTrack().GetToBorder(RnGetter);
                m_FromBorder = reverse ? toBorder : fromBorder;
                m_ToBorder = reverse ? fromBorder : toBorder;

                Debug.Log($"<color=yellow>SetRoadBase : Intersection track {m_RoadInfo.m_TrackIndex}</color>");
            }
            else
            {
                Debug.Log($"<color=yellow>SetRoadBase Failed </color>");
                return false;
            }
            return true;
        }

        //不要
        public void Dispose()
        {
            //m_RoadNetworkGetter = null;
            m_TrafficManager = null;
            m_RoadInfo = null;
            m_Road = null;
            m_Intersection = null;
            m_FromBorder = null;
            m_ToBorder = null;

            //Debug用
            //expectedBorders?.Clear();
            //actualBorders?.Clear();
        }
    }
}
