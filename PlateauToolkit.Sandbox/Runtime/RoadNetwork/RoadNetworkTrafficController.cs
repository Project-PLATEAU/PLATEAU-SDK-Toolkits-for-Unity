using PLATEAU.RoadNetwork.Data;
using PLATEAU.RoadNetwork.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;
using static Codice.CM.Common.CmCallContext;
using static Codice.CM.WorkspaceServer.DataStore.WkTree.WriteWorkspaceTree;
using static PLATEAU.RoadNetwork.Structure.RnRoadEx.LaneIntersectionResult;

namespace PlateauToolkit.Sandbox.RoadNetwork
{
    [Serializable]
    public class RaodInfo
    {
        //[SerializeField] public RnDataRoadBase m_RoadBase;
        [SerializeField] public int m_RoadId;
        [SerializeField] public int m_LaneIndex;
        [SerializeField] public int m_TrackIndex;
        [SerializeField] public bool m_IsReverse;
    }

    [Serializable]
    public class RoadNetworkTrafficController
    {
        [HideInInspector][SerializeField] RoadNetworkDataGetter m_RoadNetworkGetter;

        [SerializeField] public RaodInfo m_RoadInfo;

        //Road (Roadの場合自動的にセット）
        [SerializeField] public RnDataRoad m_Road;

        //Intersection (Intersectionの場合自動的にセット）
        [SerializeField] public RnDataIntersection m_Intersection;

        [SerializeField] public RnDataWay m_FromBorder;
        [SerializeField] public RnDataWay m_ToBorder;

        [SerializeField] public int m_LastRoadId;

        [SerializeField] public RnDataLane m_Lane;
        [SerializeField] public RnDataTrack m_Track;

        //Debug用
        public List<RnDataWay> expectedBorders = new List<RnDataWay>();
        public List<RnDataWay> actualBorders = new List<RnDataWay>();

        public string LastError = string.Empty;

        public bool IsRoad => m_Road != null;
        public bool IsIntersection => m_Intersection != null;

        public bool IsReversed
        {
            get{
                if (IsIntersection)
                    return m_RoadInfo?.m_IsReverse ?? false;
                else if (IsRoad)
                {
                    bool bLaneIsReverse = GetLane().IsReverse;
                    bool bInfoIsReverse = m_RoadInfo?.m_IsReverse ?? false;
                    //if (bLaneIsReverse && bInfoIsReverse)
                    //    return false;
                    return bLaneIsReverse || bInfoIsReverse;
                }
                return false;
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

        //public static RnDataWay FindSameBorder(RnDataWay expected, List<RnDataWay> actual)
        //{
        //    return actual.Find(x => x?.IsSameLine(expected) == true);
        //}

        //次のRoadBaseを取得
        public RoadNetworkTrafficController GetNextRoad()
        {
            if (IsRoad == true)
            {
                RnDataRoad road = GetRoad();
                RnDataLane lane = GetLane();
                RnDataRoadBase nextRoad = lane.IsReverse ? road.GetPrevRoad(RnGetter) : road.GetNextRoad(RnGetter);

                //Lane Connect Check ==========================
                bool laneIsConnected = false;
                if (nextRoad is RnDataIntersection)
                {
                    var intersection = (RnDataIntersection)nextRoad;
                    if (intersection.GetFromTracksFromLane(RnGetter, lane).Count > 0 ||
                        intersection.GetToTracksFromLane(RnGetter, lane).Count > 0)
                        laneIsConnected = true;
                }

                if (!laneIsConnected)
                {
                    //nextRoad = lane.IsReverse ? road.GetNextRoad(RoadNetworkGetter) : road.GetPrevRoad(RoadNetworkGetter);
                    Debug.LogError($"Lane border is not connected to Next Road");
                    LastError = $"Lane border is not connected to Next Road";
                    return null;
                }
                // =============================================

                //絶対取得
                //if (nextRoad == null)
                //    nextRoad = (way.IsReversed) ? road.GetNextRoad(RoadNetworkGetter) : road.GetPrevRoad(RoadNetworkGetter);

                RoadNetworkTrafficController nextParam = new(this, nextRoad);
                Debug.Log($"<color=green>next road found {nextRoad.GetId(RnGetter)} </color>");
                return nextParam;
            }
            else if (IsIntersection == true)
            {
                var intersection = GetIntersection();

                var track = GetTrack();
                var toWay = track.GetToBorder(RnGetter);
                var fromWay = track.GetFromBorder(RnGetter);
                var way = m_RoadInfo.m_IsReverse ? fromWay : toWay;
                var edges = intersection.GetEdgesFromBorder(RnGetter, way);

                //元の道を除外
                //edges.FindAll( x=> x.GetRoad(RoadNetworkGetter) == )

                //if (fromWay == toWay) //From / To Borders が同じ場合は異なる道を選ぶ

                if (fromWay == toWay && !intersection.IsEmptyIntersection)
                {
                    Debug.LogError($"<color=red>From / To Borders are the same!!!!! (not EmptyIntersection)</color>");
                }

                if (intersection.IsEmptyIntersection)
                {
                    //Debug.LogError($"From / To Borders are the same!!!!!");
                    edges = intersection.Edges;
                    edges.RemoveAll(e => e.Road.ID == m_LastRoadId);
                    Debug.Log($"<color=green>IsEmptyIntersection edges found {edges.Count}</color>");
                }

                //絶対取得
                //if (way == null)
                //    way = !toWay.IsReversed ? fromWay : toWay;

                if (edges.Count > 0)
                {
                    // Edge 抽選
                    RnDataNeighbor edge = edges[UnityEngine.Random.Range(0, edges.Count)];

                    RnDataRoadBase nextRoad = edge.GetRoad(RnGetter);

                    RoadNetworkTrafficController nextParam = new(this, nextRoad);

                    Debug.Log($"<color=green>next road found {nextRoad.GetId(RnGetter)}</color>");
                    return nextParam;
                }
            }

            Debug.LogError($"<color=red>next road not found </color>");
            LastError = $"next road not found ";
            return null;
        }

        //初回
        public RoadNetworkTrafficController(RaodInfo roadInfo)
        {
            m_RoadInfo = roadInfo;
            SetRoadBase();
            Debug.Log($"<color=yellow>roadInfo {roadInfo.m_RoadId} {roadInfo.m_LaneIndex}</color>");
        }

        //次回作成 (Road / Road のつなぎ）
        public RoadNetworkTrafficController(RoadNetworkTrafficController current, RnDataRoadBase next)
        {
            if (next == null)
                Debug.LogError($"<color=cyan>next is null</color>");

            bool success = false;

            RaodInfo nextRoadInfo = new();
            if (current.IsIntersection && next is RnDataRoad)
            {
                // Intersection -> Road 
                Debug.Log($"<color=cyan>Intersection {current.GetIntersection().GetId(RnGetter)}-> Road {next.GetId(RnGetter)} </color>");

                var nextRoad = next as RnDataRoad;
                List<RnDataLane> lanes = new();

                //var nextBorder = current.IsReversed ? current.m_FromBorder : current.m_ToBorder;
                var nextBorder = current.m_ToBorder;

                //Linestringから取得
                //var fromLineString = nextBorder.GetChildLineString(RnGetter);
                //lanes.AddRange(nextRoad.GetLanesFromNextBorderLineString(RnGetter, fromLineString));
                lanes.AddRange(nextRoad.GetLanesFromNextBorder(RnGetter, nextBorder));
                //lanes.AddRange(nextRoad.GetLanesFromPrevBorder(RnGetter, nextBorder));
                Debug.Log($"<color=green>Lanes From LineString {nextBorder.LineString.ID} Count {lanes.Count}</color>");

                if (lanes.Count <= 0)
                {
                    //この処理で道路が反転してしまう
                    //lanes.AddRange(nextRoad.GetLanesFromPrevBorderLineString(RnGetter, fromLineString));
                    lanes.AddRange(nextRoad.GetLanesFromPrevBorder(RnGetter, nextBorder));

                    //var oppositeLanes = nextRoad.GetLanesFromPrevBorder(RnGetter, nextBorder);
                    //lanes = nextRoad.GetMainLanes(RnGetter);
                    //lanes.RemoveAll(x => oppositeLanes.Contains(x)); //反対車線以外全て

                    if (lanes.Count > 0)
                    {
                        expectedBorders = new() { nextBorder };
                        //actualBorders = nextRoad.GetAllNextBorders(RnGetter);
                        Debug.LogError($"<color=green>Lanes From LineString Reverse {nextBorder.LineString.ID} Count {lanes.Count}</color>");
                        //nextRoadInfo.m_IsReverse = true;
                    }
                }

                if (lanes.Count <= 0)
                {
                    Debug.LogError($"<color=red>Failed to get Lanes from Last To Border {nextBorder.GetId(RnGetter)}</color>");

                    //var toBorder = current.m_RoadInfo.m_IsReverse ? current.GetTrack().GetFromBorder(RnGetter) : current.GetTrack().GetToBorder(RnGetter);
                    //lanes.AddRange(nextRoad.GetLanesFromNextBorder(RnGetter, toBorder));
                    //lanes.AddRange(nextRoad.GetLanesFromPrevBorder(RnGetter, toBorder));
                }

                if (lanes.Count > 0)
                {
                    var laneIndex = UnityEngine.Random.Range(0, lanes.Count); //抽選 レーン
                    nextRoadInfo.m_LaneIndex = laneIndex;

                    var lane = lanes.TryGet(laneIndex);
                    Debug.Log($"<color=green>Lane Next Border {lane.GetNextBorder(RnGetter).LineString.ID} Prev Border {lane.GetPrevBorder(RnGetter).LineString.ID}</color>");
                    if (lane.GetNextBorder(RnGetter).IsSameLine(nextBorder))
                    {
                        nextRoadInfo.m_IsReverse = true;
                    }

                    Debug.Log($"<color=blue>lanes found {lanes.Count}</color>");
                    success = true;
                }
                else
                {
                    //失敗

                    //debug
                    expectedBorders = new() { current.GetTrack().GetToBorder(RnGetter) };
                    actualBorders = nextRoad.GetAllNextBorders(RnGetter);
                    actualBorders.AddRange(nextRoad.GetAllPrevBorders(RnGetter));
                    //取得失敗 (無理やり動かす）メインレーンから抽選して取得
                    Debug.LogError($"<color=magenta>lane not found from border exptected : {current.GetTrack().ToBorder.ID}</color>");
                    var actual = String.Join(",", actualBorders.Select(x => x.GetId(RnGetter)));
                    Debug.LogError($"<color=magenta>actual {actual}</color>");

                    //nextRoadInfo.m_LaneIndex = UnityEngine.Random.Range(0, nextRoad.MainLanes.Count); //抽選 レーン
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
                var fromLineString = current.m_ToBorder.GetChildLineString(RnGetter);
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

                    //var currentLane = current.GetLane();
                    //var fromTracks = intersection.GetFromTracksFromLane(RnGetter, currentLane);
                    //var toTracks = intersection.GetToTracksFromLane(RnGetter, currentLane);
                    //var reverse = currentLane.IsReverse;

                    //bool useFromTrack = current.GetRoad().GetNextRoad(RnGetter) == intersection && !reverse;

                    //tracks = useFromTrack ? fromTracks : toTracks;
                    //if (tracks.Count <= 0)
                    //    tracks = useFromTrack ? tracks : fromTracks;
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
                    var toborders2 = String.Join(",", intersection.Tracks.Select(x => x.ToBorder.ID));
                    var fromborders2 = String.Join(",", intersection.Tracks.Select(x => x.FromBorder.ID));
                    Debug.Log($"<color=red>Intersection Track ToBorders2 {toborders2} FromBorders2 {fromborders2} expected Lane PrevBorder{current.GetLane().PrevBorder.ID} NextBorder{current.GetLane().NextBorder.ID}</color>");
                    expectedBorders = new() { RnGetter.GetWays().TryGet(current.GetLane().PrevBorder), RnGetter.GetWays().TryGet(current.GetLane().NextBorder) };
                    var connectedRoadId = String.Join(",", intersection.GetAllConnectedRoads(RnGetter).Select(x => x.GetId(RnGetter)));
                    Debug.LogError($"<color=red>Intersection Edge Roads {connectedRoadId} expected Road {current.GetRoad().GetId(RnGetter)} </color>");

                    var edgeBorders = String.Join(",", intersection.Edges.Select(x => x.GetBorder(RnGetter).GetId(RnGetter)));
                    Debug.LogError($"<color=red>Intersection Edge Borders {edgeBorders} </color>");
                    actualBorders = intersection.Edges.Select(x => x.GetBorder(RnGetter)).ToList();

                    //取得失敗 (無理やり動かす）Trackの一番目を利用
                    if (intersection.Tracks.Count <= 0)
                    {
                        //var track = intersection.Tracks.First();
                        //nextRoadInfo.m_TrackIndex = intersection.Tracks.IndexOf(track);
                        //m_IsReverse = current.GetLane().IsReverse;
                        //Debug.LogError($"No tracks found.");
                    }

                    Debug.LogError($"No tracks found.");
                    LastError = $"No tracks found. ";
                }
            }
            else if(next is RnDataRoad && current.IsRoad) //Road->Road(あり得ない？）
            {
                Debug.LogError($"Road -> Road");
                LastError = $"Road -> Road";
                //そのまま
                nextRoadInfo = current.m_RoadInfo;
            }
            else if (next is RnDataIntersection && current.IsIntersection) //Intersection -> Intersection (あり得ない？）
            {
                Debug.LogError($"Intersection -> Intersection");
                LastError = $"Intersection -> Intersection";
                //そのまま
                nextRoadInfo = current.m_RoadInfo;
            }

            if (success)
            {
                m_LastRoadId = current.m_RoadInfo.m_RoadId;

                m_RoadInfo = nextRoadInfo;
                m_RoadInfo.m_RoadId = next.GetId(RnGetter);

                //expectedBorders = new() { current.m_ToBorder };

                success = SetRoadBase();
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
                m_RoadInfo.m_IsReverse = GetLane()?.IsReverse ?? false;

                m_Lane = GetLane();

                var prevBorder = GetLane().GetPrevBorder(RnGetter);
                var nextBorder = GetLane().GetNextBorder(RnGetter);
                m_FromBorder = m_RoadInfo.m_IsReverse ? nextBorder : prevBorder;
                m_ToBorder = m_RoadInfo.m_IsReverse ? prevBorder : nextBorder;

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
                m_RoadInfo.m_IsReverse = GetLane()?.IsReverse ?? false;

                m_Track = GetTrack();

                var fromBorder = GetTrack().GetFromBorder(RnGetter);
                var toBorder = GetTrack().GetToBorder(RnGetter);
                m_FromBorder = m_RoadInfo.m_IsReverse ? toBorder : fromBorder;
                m_ToBorder = m_RoadInfo.m_IsReverse ? fromBorder : toBorder;

                Debug.Log($"<color=yellow>SetRoadBase : Intersection track {m_RoadInfo.m_TrackIndex}</color>");
            }
            else
            {
                Debug.Log($"<color=yellow>SetRoadBase Failed </color>");
                LastError = $"SetRoadBase Failed ";
                return false;
            }
            return true;
        }
    }
}
