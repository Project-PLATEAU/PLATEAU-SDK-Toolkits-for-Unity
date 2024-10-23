using PLATEAU.RoadNetwork.Data;
using PLATEAU.RoadNetwork.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
        public int m_VehecleID;

        public RoadInfo Clone()
        {
            RoadInfo info = new RoadInfo();
            info.m_RoadId = m_RoadId;
            info.m_LaneIndex = m_LaneIndex;
            info.m_TrackIndex = m_TrackIndex;
            info.m_IsReverse = m_IsReverse;
            info.m_VehecleID = m_VehecleID;
            return info;
        }
    }

    [Serializable]
    public class RoadNetworkTrafficController : IDisposable
    {
        //[HideInInspector][SerializeField]
        RoadNetworkDataGetter m_RoadNetworkGetter;

        TrafficManager m_TrafficManager;

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

        [SerializeField]
        public int m_LastRoadId;

        //Debug用
        public List<RnDataWay> expectedBorders = new List<RnDataWay>();
        public List<RnDataWay> actualBorders = new List<RnDataWay>();

        public bool IsRoad => m_Road != null;
        public bool IsIntersection => m_Intersection != null;

        public bool IsValid => IsRoad || IsIntersection;

        //prev/nextの切替用
        public bool IsReversed
        {
            get {
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
                    m_Road = RnGetter.GetRoadBases().TryGet(m_RoadInfo.m_RoadId) as RnDataRoad;
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
                return null;
            return GetRoad().GetChildLineString(RnGetter, m_RoadInfo.m_LaneIndex);
        }
        public RnDataWay GetWay()
        {
            if (!IsRoad)
                return null;
            return GetRoad().GetChildWay(RnGetter, m_RoadInfo.m_LaneIndex);
        }

        public RnDataLane GetLane()
        {
            if (!IsRoad)
                return null;
            return GetRoad().GetChildLane(RnGetter, m_RoadInfo.m_LaneIndex);
        }

        public RnDataTrack GetTrack()
        {
            if (!IsIntersection)
                return null;

            if (GetIntersection().Tracks.Count < m_RoadInfo.m_TrackIndex)
                return null;

            return GetIntersection().Tracks[m_RoadInfo.m_TrackIndex];
        }

        //次のRoadBaseを取得
        public RoadNetworkTrafficController GetNextRoad()
        {
            if (IsRoad == true)
            {
                RnDataRoad road = GetRoad();
                RnDataLane lane = GetLane();
                RnDataRoadBase nextRoad = IsReversed ? road.GetPrevRoad(RnGetter) : road.GetNextRoad(RnGetter);

                if(nextRoad.IsValid(RnGetter))
                {
                    RoadNetworkTrafficController nextParam = new(this, nextRoad);

                    if (nextParam.IsValid)
                    {
                        Debug.Log($"<color=green>next road found {nextRoad.GetId(RnGetter)} </color>");
                        return nextParam;
                    }
                }
                else
                {
                    //取得失敗
                    Debug.Log($"<color=red>Next Road is not Valid Prev{road.Prev.ID} Next{road.Next.ID}</color>");
                }
            }
            else if (IsIntersection == true)
            {
                var intersection = GetIntersection();
                var track = GetTrack();
                var toWay = track.GetToBorder(RnGetter);
                var fromWay = track.GetFromBorder(RnGetter);
                var way = IsReversed ? fromWay : toWay;
                var edges = intersection.GetEdgesFromBorder(RnGetter, way);

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
                        Debug.Log($"<color=green>next road found {nextRoad.GetId(RnGetter)}</color>");
                        return nextParam;
                    }
                }
                else
                {
                    //取得失敗
                    //Debug
                    var toborders = String.Join(",", intersection.Tracks.Select(x => x.ToBorder.ID));
                    var fromborders = String.Join(",", intersection.Tracks.Select(x => x.FromBorder.ID));
                    Debug.Log($"<color=red>Intersection Track ToBorders {toborders} FromBorders {fromborders}</color>");
                    //expectedBorders = new() { RnGetter.GetWays().TryGet(GetLane().PrevBorder), RnGetter.GetWays().TryGet(GetLane().NextBorder) };
                    var connectedRoadId = String.Join(",", intersection.GetAllConnectedRoads(RnGetter).Select(x => x.GetId(RnGetter)));
                    Debug.Log($"<color=red>Intersection Edge Roads {connectedRoadId} count {intersection.GetAllConnectedRoads(RnGetter).Count}</color>");
                    var edgeBorders = String.Join(",", intersection.Edges.Select(x => x.GetBorder(RnGetter).GetId(RnGetter)));
                    Debug.Log($"<color=red>Intersection All Edge Borders {edgeBorders} count {intersection.Edges.Count} </color>");
                    actualBorders = intersection.Edges.Select(x => x.GetBorder(RnGetter)).ToList();
                }
            }

            Debug.LogError($"<color=red>next road not found</color>");

            return null;
        }

        //初回
        public RoadNetworkTrafficController(RoadInfo roadInfo)
        {
            m_RoadInfo = roadInfo;
            SetRoadBase();
            Debug.Log($"<color=yellow>roadInfo {roadInfo.m_RoadId} {roadInfo.m_LaneIndex}</color>");
        }

        //次回作成 (Road / Road のつなぎ）
        public RoadNetworkTrafficController(RoadNetworkTrafficController current, RnDataRoadBase next)
        {
            bool success = false;
            if (next == null)
            {
                Debug.LogError($"<color=cyan>next is null</color>");
                return;
            }

            RoadInfo nextRoadInfo = new();
            if (current.IsIntersection && next is RnDataRoad)
            {
                // Intersection -> Road 
                Debug.Log($"<color=cyan>Intersection {current.GetIntersection().GetId(RnGetter)}-> Road {next.GetId(RnGetter)} </color>");

                var nextRoad = next as RnDataRoad;
                List<RnDataLane> lanes = new();

                var nextBorder = current.IsReversed ? current.m_FromBorder : current.m_ToBorder;
                //var nextBorder = current.m_ToBorder;

                //Linestringから取得
                lanes.AddRange(nextRoad.GetLanesFromPrevBorder(RnGetter, nextBorder));
                Debug.Log($"<color=green>Lanes From LineString {nextBorder.LineString.ID} Count {lanes.Count}</color>");

                if (lanes.Count <= 0)
                {
                    lanes.AddRange(nextRoad.GetLanesFromNextBorder(RnGetter, nextBorder));
                    if (lanes.Count > 0)
                    {
                        expectedBorders = new() { nextBorder };
                        Debug.Log($"<color=green>Lanes From LineString Reverse {nextBorder.LineString.ID} Count {lanes.Count}</color>");
                        nextRoadInfo.m_IsReverse = true;
                    }
                }

                if (lanes.Count <= 0)
                {
                    Debug.LogError($"<color=red>Failed to get Lanes from Last To Border {nextBorder.GetId(RnGetter)}</color>");
                }

                if (lanes.Count > 0)
                {
                    var laneIndex = UnityEngine.Random.Range(0, lanes.Count); //抽選 レーン
                    var lane = lanes.TryGet(laneIndex);
                    nextRoadInfo.m_LaneIndex = nextRoad.GetMainLanes(RnGetter).IndexOf(lane);

                    Debug.Log($"<color=green>Lane Next Border {lane.GetNextBorder(RnGetter).LineString.ID} Prev Border {lane.GetPrevBorder(RnGetter).LineString.ID}</color>");
                    success = true;
                }
                else
                {
                    //取得失敗
                    expectedBorders = new() { current.GetTrack().GetToBorder(RnGetter) };
                    actualBorders = nextRoad.GetAllNextBorders(RnGetter);
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

                //Linestringから取得
                ///var fromLineString = current.m_ToBorder.GetChildLineString(RnGetter);
                var fromLineString = IsReversed ? current.m_FromBorder.GetChildLineString(RnGetter) : current.m_ToBorder.GetChildLineString(RnGetter);


                Debug.Log($"<color=green>Tracks From LineString {fromLineString.GetId(RnGetter)} Count {intersection.GetFromTracksFromBorderLineString(RnGetter, fromLineString).Count}</color>");
                List<RnDataTrack> tracks = intersection.GetFromTracksFromBorderLineString(RnGetter, fromLineString);
                if (tracks.Count <= 0)
                {
                    tracks = intersection.GetToTracksFromBorderLineString(RnGetter, fromLineString);
                    if(tracks.Count > 0)
                    {
                        nextRoadInfo.m_IsReverse = true;
                    }
                }

                if (tracks.Count <= 0)
                {
                    Debug.LogError($"<color=red>Failed to get Tracks from Last To Border {current.m_ToBorder.GetId(RnGetter)}</color>");
                    //逆Border から取得できるかTry？？？
                    fromLineString = IsReversed ? current.m_ToBorder.GetChildLineString(RnGetter) : current.m_FromBorder.GetChildLineString(RnGetter);
                    tracks = intersection.GetFromTracksFromBorderLineString(RnGetter, fromLineString);
                    if (tracks.Count > 0)
                        Debug.Log($"<color=magenta>Failed to get from ToBorder but found from FromBorder : tracks.Count {tracks.Count}</color>");
                    else
                    {
                        tracks = intersection.GetToTracksFromBorderLineString(RnGetter, fromLineString);
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
                    Debug.Log($"<color=green>tracks.Count {tracks.Count}</color>");

                    var track = tracks[UnityEngine.Random.Range(0, tracks.Count)];
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
                    expectedBorders = new() { RnGetter.GetWays().TryGet(current.GetLane().PrevBorder), RnGetter.GetWays().TryGet(current.GetLane().NextBorder) };
                    var connectedRoadId = String.Join(",", intersection.GetAllConnectedRoads(RnGetter).Select(x => x.GetId(RnGetter)));
                    var edgeBorders = String.Join(",", intersection.Edges.Select(x => x.GetBorder(RnGetter).GetId(RnGetter)));
                    actualBorders = intersection.Edges.Select(x => x.GetBorder(RnGetter)).ToList();

                    Debug.LogError($"<color=red>Intersection Track ToBorders {toborders} FromBorders {fromborders} expected Lane PrevBorder{current.GetLane().PrevBorder.ID} NextBorder{current.GetLane().NextBorder.ID}</color>");
                    Debug.LogError($"<color=red>Intersection Edge Roads {connectedRoadId} expected Road {current.GetRoad().GetId(RnGetter)} </color>");
                    Debug.LogError($"<color=red>Intersection Edge Borders {edgeBorders} </color>");
                    Debug.LogError($"No tracks found.");
                }
            }
            else if(next is RnDataRoad && current.IsRoad) //Road->Road(あり得ない？）
            {
                Debug.LogError($"Road -> Road");
                //そのまま
                nextRoadInfo = current.m_RoadInfo;
            }
            else if (next is RnDataIntersection && current.IsIntersection) //Intersection -> Intersection (あり得ない？）
            {
                Debug.LogError($"Intersection -> Intersection");
                //そのまま
                nextRoadInfo = current.m_RoadInfo;
            }

            if (success)
            {
                m_LastRoadId = current.m_RoadInfo.m_RoadId;
                m_RoadInfo = nextRoadInfo;
                m_RoadInfo.m_RoadId = next.GetId(RnGetter);
                m_RoadInfo.m_VehecleID = current.m_RoadInfo.m_VehecleID;
                //expectedBorders = new() { current.m_ToBorder };
                success = SetRoadBase();

                TrafficManager.SetRoadInfo(m_RoadInfo);
            }
            if (!success)
            {
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

        public void Dispose()
        {
            m_RoadNetworkGetter = null;
            m_RoadInfo = null;
            m_Road = null;
            m_Intersection = null;
            m_FromBorder = null;
            m_ToBorder = null;

            //Debug用
            expectedBorders?.Clear();
            actualBorders?.Clear();
        }
    }
}
