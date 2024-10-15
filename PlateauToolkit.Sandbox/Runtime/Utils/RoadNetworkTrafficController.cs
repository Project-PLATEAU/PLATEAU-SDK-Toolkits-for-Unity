using PLATEAU.RoadNetwork.Data;
using PLATEAU.RoadNetwork.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Codice.CM.Common.CmCallContext;
using static Codice.CM.WorkspaceServer.DataStore.WkTree.WriteWorkspaceTree;

namespace PlateauToolkit.Sandbox
{
    [Serializable]
    public class RaodInfo
    {
        //[SerializeField] public RnDataRoadBase m_RoadBase;
        [SerializeField] public int m_RoadId;
        [SerializeField] public int m_LaneIndex;
        [SerializeField] public int m_TrackIndex;
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

        [SerializeField] public bool m_IsReverse;

        public bool IsRoad => m_Road != null;
        public bool IsIntersection => m_Intersection != null;

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

        //次のRoadBaseを取得
        public RoadNetworkTrafficController GetNextRoad()
        {
            if (IsRoad == true)
            {
                RnDataRoad road = GetRoad();
                RnDataLane lane = GetLane();

                RnDataRoadBase nextRoad = lane.IsReverse ? road.GetPrevRoad(RnGetter) : road.GetNextRoad(RnGetter);
                //RnDataRoadBase nextRoad = lane.IsReverse ? road.GetNextRoad(RoadNetworkGetter) : road.GetPrevRoad(RoadNetworkGetter);

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

                var way = m_IsReverse ? fromWay : toWay;

                var edges = intersection.GetEdgesFromBorder(RnGetter, way);

                //元の道を除外
                //edges.FindAll( x=> x.GetRoad(RoadNetworkGetter) == )

                // Edge 抽選
                RnDataNeighbor edge = edges[UnityEngine.Random.Range(0, edges.Count)];

                //絶対取得
                //if (way == null)
                //    way = !toWay.IsReversed ? fromWay : toWay;

                RnDataRoadBase nextRoad = edge.GetRoad(RnGetter);

                RoadNetworkTrafficController nextParam = new(this, nextRoad);

                Debug.Log($"<color=green>next road found {nextRoad.GetId(RnGetter)}</color>");
                return nextParam;

            }

            Debug.Log($"<color=red>next road not found </color>");
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
                Debug.Log($"<color=cyan>next is null</color>");

            RaodInfo nextRoadInfo = new();
            if (next is RnDataRoad)
            {
                // Road -> Road (あり得ない？）
                if (current.IsRoad)
                {
                    Debug.LogError($"Road -> Road");
                    //そのまま
                    nextRoadInfo = current.m_RoadInfo;
                }
                // Intersection -> Road 
                else if (current.IsIntersection)
                {
                    var nextRoad = next as RnDataRoad;
                    List<RnDataLane> lanes = new();

                    //intersection border 
                    //List<RnDataNeighbor> edges = current.GetIntersection().GetEdgesFromRoad(RoadNetworkGetter, nextRoad);
                    //List<RnDataWay> borders = edges.Select(x => x.GetBorder(RoadNetworkGetter)).ToList();

                    //foreach (RnDataWay border in borders)
                    //{
                    //    if (nextRoad.GetPrevRoad(RoadNetworkGetter) == current.GetIntersection())
                    //    {
                    //        lanes.AddRange(nextRoad.GetLanesFromNextBorder(RoadNetworkGetter, border));
                    //    }
                    //    else if (nextRoad.GetNextRoad(RoadNetworkGetter) == current.GetIntersection())
                    //    {
                    //        lanes.AddRange(nextRoad.GetLanesFromPrevBorder(RoadNetworkGetter, border));
                    //    }
                    //}

                    if (nextRoad.GetPrevRoad(RnGetter) == current.GetIntersection())
                    {
                        lanes.AddRange(nextRoad.GetLanesFromPrevTrack(RnGetter, current.GetTrack()));
                    }
                    else if (nextRoad.GetNextRoad(RnGetter) == current.GetIntersection())
                    {
                        lanes.AddRange(nextRoad.GetLanesFromNextTrack(RnGetter, current.GetTrack()));
                    }


                    if (lanes.Count > 0)
                    {
                        var laneIndex = UnityEngine.Random.Range(0, lanes.Count); //抽選 レーン
                        nextRoadInfo.m_LaneIndex = laneIndex;
                        Debug.Log($"<color=blue>lanes found {lanes.Count}</color>");
                    }
                    else
                    {
                        //取得失敗 (無理やり動かす）メインレーンから抽選して取得

                        Debug.Log($"<color=blue>lane not found from border</color>");
                        nextRoadInfo.m_LaneIndex = UnityEngine.Random.Range(0, nextRoad.MainLanes.Count); //抽選 レーン
                    }
                }
            }
            //intersection
            else if (next is RnDataIntersection)
            {
                // Road -> Intersection
                if (current.IsRoad)
                {
                    Debug.Log($"<color=cyan>Road {current.GetRoad().GetId(RnGetter)}-> Intersection {next.GetId(RnGetter)} </color>");

                    //Road way
                    var intersection = next as RnDataIntersection;

                    var currentLane = current.GetLane();
                    var fromTracks = intersection.GetFromTracksFromLane(RnGetter, currentLane);
                    var toTracks = intersection.GetToTracksFromLane(RnGetter, currentLane);
                    var reverse = currentLane.IsReverse;

                    bool useFromTrack = current.GetRoad().GetNextRoad(RnGetter) == intersection && !reverse;

                    List<RnDataTrack> tracks = fromTracks;
                    //if (current.GetRoad().GetNextRoad(RoadNetworkGetter) == intersection)
                    if (useFromTrack)
                    {
                        tracks = fromTracks;
                        //tracks.RemoveAll(x => toTracks.Contains(x)); //Uターン禁止
                    }
                    //else if (current.GetRoad().GetPrevRoad(RoadNetworkGetter) == intersection)
                    else
                    {
                        tracks = toTracks;
                        //tracks.RemoveAll(x => fromTracks.Contains(x)); //Uターン禁止
                    }

                    if (tracks.Count <= 0)
                    {
                        tracks = useFromTrack ? tracks : fromTracks;
                    }

                    //Track 抽選
                    if (tracks.Count > 0)
                    {
                        Debug.Log($"<color=green>tracks.Count {tracks.Count}</color>");

                        var track = tracks[UnityEngine.Random.Range(0, tracks.Count)];
                        if (intersection.Tracks.Count > 0)
                            nextRoadInfo.m_TrackIndex = intersection.Tracks.IndexOf(track);
                    }
                    else
                    {
                        var edges = intersection.GetEdgesFromRoad(RnGetter, current.GetRoad());
                        Debug.LogError($"edges count = {edges.Count}");
                        Debug.LogError($"fromTracks count = {fromTracks.Count}");
                        Debug.LogError($"toTracks count = {toTracks.Count}");

                        if (!currentLane.GetParentRoads(RnGetter).Contains(current.GetRoad()))
                        {
                            Debug.LogError($"Wrong Lane !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! {currentLane.GetId(RnGetter)} / {current.GetRoad().GetId(RnGetter)}");
                        }
                        else
                        {
                            //Debug.Log($"<color=yellow>Lane {currentLane.GetId(RoadNetworkGetter)} is child of {current.GetRoad().GetId(RoadNetworkGetter)} </color>");

                            var alllanes = String.Join(",", current.GetRoad().GetMainLanes(RnGetter).Select( x => x.GetId(RnGetter)));
                            Debug.Log($"<color=yellow>Road {current.GetRoad().GetId(RnGetter)} all Lanes {alllanes} </color>");

                            var allRoads = String.Join(",", currentLane.GetParentRoads(RnGetter).Select(x => x.GetId(RnGetter)));
                            Debug.Log($"<color=yellow>Lane {currentLane.GetId(RnGetter)} is child of {allRoads} </color>");

                            //var nextBorders = String.Join(",", current.GetRoad().GetAllNextBorders(RnGetter).Select(x => x.GetId(RnGetter)));
                            //var prevBorders = String.Join(",", current.GetRoad().GetAllPrevBorders(RnGetter).Select(x => x.GetId(RnGetter)));
                            //Debug.Log($"<color=yellow>Road {current.GetRoad().GetId(RnGetter)} nextBorders {nextBorders} prevBorders {prevBorders} </color>");

                            var nextBorders2 = String.Join(",", current.GetRoad().GetAllNextBorderIds(RnGetter));
                            var prevBorders2 = String.Join(",", current.GetRoad().GetAllPrevBorderIds(RnGetter));
                            Debug.Log($"<color=yellow>Road {current.GetRoad().GetId(RnGetter)} nextBorders {nextBorders2} prevBorders {prevBorders2} </color>");

                        }

                        //var toborders = String.Join(",", intersection.Tracks.Select(x => x.GetToBorder(RnGetter).GetId(RnGetter)));
                        //var fromborders = String.Join(",", intersection.Tracks.Select(x => x.GetFromBorder(RnGetter).GetId(RnGetter)));
                        //Debug.Log($"<color=red>Intersection Track ToBorders {toborders} FromBorders {fromborders} expected Lane PrevBorder{currentLane.PrevBorder.ID} NextBorder{currentLane.NextBorder.ID}</color>");

                        //普通に取得
                        var toborders2 = String.Join(",", intersection.Tracks.Select(x => x.ToBorder.ID));
                        var fromborders2 = String.Join(",", intersection.Tracks.Select(x => x.FromBorder.ID));
                        Debug.Log($"<color=red>Intersection Track ToBorders2 {toborders2} FromBorders2 {fromborders2} expected Lane PrevBorder{currentLane.PrevBorder.ID} NextBorder{currentLane.NextBorder.ID}</color>");

                        var connectedRoadId = String.Join(",", intersection.GetAllConnectedRoads(RnGetter).Select(x => x.GetId(RnGetter)));
                        Debug.Log($"<color=red>Intersection Edge Roads {connectedRoadId} expected Road {current.GetRoad().GetId(RnGetter)} </color>");

                        var edgeBorders = String.Join(",", intersection.Edges.Select(x => x.GetBorder(RnGetter).GetId(RnGetter)));
                        Debug.Log($"<color=red>Intersection Edge Borders {edgeBorders} </color>");


                        //取得失敗 (無理やり動かす）Trackの一番目を利用
                        if (intersection.Tracks.Count > 0)
                        {
                            var track = intersection.Tracks.First();
                            nextRoadInfo.m_TrackIndex = intersection.Tracks.IndexOf(track);
                        }
                        else
                            Debug.LogError($"No tracks found.");

                        //nextRoadInfo = null;
                    }

                }
                // Intersection -> Intersection (あり得ない？）
                else if (current.IsIntersection)
                {
                    Debug.LogError($"Intersection -> Intersection");
                    //そのまま
                    nextRoadInfo = current.m_RoadInfo;
                }
            }

            m_RoadInfo = nextRoadInfo;
            m_RoadInfo.m_RoadId = next.GetId(RnGetter);

            bool success = SetRoadBase();

            if (!success)
            {
                Debug.Log($"<color=cyan>SetRoadBase Failed {next.GetId(RnGetter)}</color>");
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

                m_IsReverse = GetLane().IsReverse;

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

                Debug.Log($"<color=yellow>SetRoadBase : Intersection track {m_RoadInfo.m_TrackIndex}</color>");
            }
            else
            {
                Debug.Log($"<color=yellow>SetRoadBase Failed </color>");
                return false;
            }
            return true;
        }
    }
}
