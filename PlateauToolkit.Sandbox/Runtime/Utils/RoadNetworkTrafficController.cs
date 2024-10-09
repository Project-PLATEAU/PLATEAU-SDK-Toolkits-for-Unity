using PLATEAU.RoadNetwork.Data;
using PLATEAU.RoadNetwork.Structure;
using System;
using System.Linq;
using UnityEngine;
using static PlateauToolkit.Sandbox.RoadnetworkExtensions;

namespace PlateauToolkit.Sandbox
{
    [Serializable]
    public class RaodInfo
    {
        [SerializeField] public RnDataRoadBase m_RoadBase;
        [SerializeField] public int m_LaneIndex;
        [SerializeField] public bool m_IsMainLane;
        [SerializeField] public LanePosition m_LanePosition;

    }

    [Serializable]
    public class RoadNetworkTrafficController
    {
        [HideInInspector][SerializeField] RoadNetworkDataGetter m_RoadNetworkGetter;

        [SerializeField] public RaodInfo m_RoadInfo;


        [SerializeField] public RnDataRoad m_Road;
        [SerializeField] public RnDataIntersection m_Intersection;


        [SerializeField] public RnDataLineString m_LineString;

        ///[SerializeField] public int m_TrackPosition;
        [SerializeField] public RnDataTrack m_Track;

        public bool IsRoad => m_Road != null;
        public bool IsIntersection => m_Intersection != null;

        RoadNetworkDataGetter RoadNetworkGetter
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

        public RnDataWay GetWay()
        {
            if (!IsRoad)
                return null;
            return m_Road.GetChildWay(RoadNetworkGetter, m_RoadInfo.m_IsMainLane, m_RoadInfo.m_LaneIndex, m_RoadInfo.m_LanePosition);
        }

        //次のRoadBaseを取得
        public RoadNetworkTrafficController GetNextRoad()
        {
            if (IsRoad == true)
            {
                RnDataRoad road = m_Road;
                RnDataWay way = road.GetChildWay(RoadNetworkGetter, m_RoadInfo.m_IsMainLane, m_RoadInfo.m_LaneIndex, m_RoadInfo.m_LanePosition);
                if (way == null)
                    return null;

                RnDataRoadBase nextRoad = (!way.IsReversed) ? road.GetNextRoad(RoadNetworkGetter) : road.GetPrevRoad(m_RoadNetworkGetter);

                //絶対取得
                //if (nextRoad == null)
                //    nextRoad = (way.IsReversed) ? road.GetNextRoad(RoadNetworkGetter) : road.GetPrevRoad(m_RoadNetworkGetter);

                RoadNetworkTrafficController nextParam = new(this, nextRoad);

                Debug.Log($"<color=green>next road found {nextRoad.GetId(RoadNetworkGetter)} </color>");
                return nextParam;
            }
            else if (IsIntersection == true)
            {
                var intersection = m_Intersection;
                var track = m_Track;
                var toWay = track.GetToBorder(RoadNetworkGetter);
                var edges = intersection.GetEdgesFromWay(RoadNetworkGetter, toWay);

                // Edge 抽選
                var edge = edges[UnityEngine.Random.Range(0, edges.Count)];

                //絶対取得
                //if (way == null)
                //    way = !toWay.IsReversed ? fromWay : toWay;

                //RnDataNeighbor edge = intersection.GetEdgeFromWay(RoadNetworkGetter, way);
                RnDataRoadBase nextRoad = edge.GetRoad(RoadNetworkGetter);

                RoadNetworkTrafficController nextParam = new(this, nextRoad);

                Debug.Log($"<color=green>next road found {nextRoad.GetId(RoadNetworkGetter)}</color>");
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

            if (m_Road != null)
            {
                Debug.Log($"<color=yellow> Road {roadInfo.m_RoadBase.GetId(RoadNetworkGetter)} </color>");

                var road = roadInfo.m_RoadBase as RnDataRoad;
                if (roadInfo.m_IsMainLane)
                {
                    RnDataLane lane = road.GetMainLanes(RoadNetworkGetter)[roadInfo.m_LaneIndex];
                    m_LineString = lane.GetChildLineString(RoadNetworkGetter, roadInfo.m_LanePosition);
                    //lane.GetChildLineString(getter, lane.IsReverse ? LanePosition.Left : LanePosition.Right);
                }
            }
            else if (m_Intersection != null)
            {
                //intersection

                Debug.Log($"<color=yellow> TODO : Create intersection </color>");

            }
            Debug.Log($"<color=yellow>roadInfo {roadInfo.m_RoadBase} {roadInfo.m_IsMainLane} {roadInfo.m_LaneIndex} {roadInfo.m_LanePosition} </color>");
        }

        //次回作成 (Road / Road のつなぎ）
        public RoadNetworkTrafficController(RoadNetworkTrafficController current, RnDataRoadBase next)
        {
            if (next == null)
                Debug.Log($"<color=cyan>next is null</color>");

            //int trackPosition = -1;
            RnDataTrack track = null;
            RaodInfo nextRoadInfo = new();
            if (next is RnDataRoad)
            {
                // Road -> Road (あり得ない？）
                if (current.IsRoad)
                {
                    var road = next as RnDataRoad;
                    if (current.m_RoadInfo.m_IsMainLane)
                    {
                        RnDataLane lane = road.GetMainLanes(RoadNetworkGetter)[current.m_RoadInfo.m_LaneIndex];
                        m_LineString = lane.GetChildLineString(RoadNetworkGetter, current.m_RoadInfo.m_LanePosition);
                        nextRoadInfo = current.m_RoadInfo;
                    }
                }
                // Intersection -> Road 
                else if (current.IsIntersection)
                {
                    var nextRoad = next as RnDataRoad;

                    //Trackからnext roadのwayは取得できない？
                    //var edges = current.m_Intersection.GetEdgesFromRoad(RoadNetworkGetter, nextRoad);
                    //var way = edges.First().GetBorder(RoadNetworkGetter);
                    //(nextRoadInfo.m_IsMainLane, nextRoadInfo.m_LaneIndex, nextRoadInfo.m_LanePosition) = nextRoad.GetWayPosition(getter, way);


                    //暫定 (この辺の取得方法がわからない）
                    nextRoadInfo.m_IsMainLane = true;
                    //nextRoadInfo.m_LaneIndex = current.m_Intersection.Tracks.IndexOf(currentTrack);
                    nextRoadInfo.m_LaneIndex = UnityEngine.Random.Range(0, nextRoad.MainLanes.Count); //抽選 レーン

                    if (nextRoad.GetPrevRoad(RoadNetworkGetter) == current.m_Intersection)
                    {
                        nextRoadInfo.m_LanePosition = RoadnetworkExtensions.LanePosition.Left;
                    }
                    else
                    {
                        nextRoadInfo.m_LanePosition = RoadnetworkExtensions.LanePosition.Right;
                    }

                    var wayOnRoad = nextRoad.GetChildWay(RoadNetworkGetter, nextRoadInfo.m_IsMainLane, nextRoadInfo.m_LaneIndex, nextRoadInfo.m_LanePosition);

                    m_LineString = wayOnRoad.GetChildLineString(RoadNetworkGetter);
                }
            }
            //intersection
            else if (next is RnDataIntersection)
            {
                // Road -> Intersection
                if (current.IsRoad)
                {
                    Debug.Log($"<color=cyan>Road {current.m_Road.GetId(RoadNetworkGetter)}-> Intersection {next.GetId(RoadNetworkGetter)} </color>");

                    //RnDataWay from = current.m_Road.GetChildWay(RoadNetworkGetter, current.m_RoadInfo.m_IsMainLane, current.m_RoadInfo.m_LaneIndex, current.m_RoadInfo.m_LanePosition);

                    //Road way
                    var intersection = next as RnDataIntersection;

                    //var roadIDs = intersection.GetAllConnectedRoads(RoadNetworkGetter).Select(x => x.GetId(RoadNetworkGetter)).ToList();
                    //var roadIDstr = "";
                    //foreach (var id in roadIDs)
                    //    roadIDstr += id + ",";
                    //Debug.Log($"<color=cyan>Connected Road {roadIDstr} </color>");

                    // Road
                    //var tracks = intersection.GetToTracksFromRoad(getter, current.m_Road);
                    var tracks = intersection.GetFromTracksFromRoad(RoadNetworkGetter, current.m_Road);
                    if (tracks.Count <= 0)
                    {
                        tracks = intersection.GetToTracksFromRoad(RoadNetworkGetter, current.m_Road);
                    }

                    //Track 抽選
                    if (tracks.Count > 0)
                    {
                        track = tracks[UnityEngine.Random.Range(0, tracks.Count)];
                        //track = tracks[current.m_RoadInfo.m_LaneIndex];
                    }
                    else
                    {
                        var edges = intersection.GetEdgesFromRoad(RoadNetworkGetter, current.m_Road);
                        Debug.LogError($"edges count = {edges.Count}");
                        Debug.LogError("tracks count = 0");

                        //取得失敗
                        track = intersection.Tracks.First();
                    }

                }
                // Intersection -> Intersection (あり得ない？）
                else if (current.IsIntersection)
                {
                    //そのまま
                    nextRoadInfo = current.m_RoadInfo;
                    track = m_Track;
                }
            }

            m_RoadInfo = nextRoadInfo;
            m_RoadInfo.m_RoadBase = next;
            if (track != null)
            {
                m_Track = track;
            }

            bool success = SetRoadBase();

            if (!success)
            {
                var road = (RnDataRoad)next;
                var intersection = (RnDataIntersection)next;

                Debug.Log($"<color=cyan>SetRoadBase Failed {road} {intersection}</color>");
            }
        }

        private bool SetRoadBase()
        {
            if (m_RoadInfo.m_RoadBase is RnDataRoad)
            {
                m_Road = m_RoadInfo.m_RoadBase as RnDataRoad;

                Debug.Log($"<color=yellow>SetRoadBase : Road</color>");
            }
            else if (m_RoadInfo.m_RoadBase is RnDataIntersection)
            {
                m_Intersection = m_RoadInfo.m_RoadBase as RnDataIntersection;

                Debug.Log($"<color=yellow>SetRoadBase : Intersection</color>");
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
