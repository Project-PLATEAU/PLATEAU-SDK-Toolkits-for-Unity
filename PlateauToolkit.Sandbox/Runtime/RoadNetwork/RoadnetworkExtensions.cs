using PLATEAU.RoadNetwork.Data;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using PLATEAU.RoadNetwork.Structure;
using System.Linq;
using UnityEngine;

namespace PlateauToolkit.Sandbox.RoadNetwork
{
    public static class RoadnetworkExtensions
    {
        public enum LanePosition
        {
            Center,
            Left,
            Right,
        }

        //Generic
        public static TValue TryGet<TValue>([DisallowNull] this IReadOnlyList<TValue> list, int index)
        {
            if (list.Count > index)
                return list[index];

            Debug.LogError($"index {index} is larger than list count {list.Count}");
            return default(TValue);
        }

        public static TValue TryGet<TValue>([DisallowNull] this IReadOnlyList<TValue> list, RnID<TValue> id) where TValue : IPrimitiveData
        {
            if (!id.IsValid)
            {
                //Debug.LogError($"id is not Valid ");
                return default(TValue);
            }
            int index = id.ID;
            return list.TryGet(index);
        }

        public static int IndexOf<TValue>([DisallowNull] this IReadOnlyList<TValue> list, TValue val)
        {
            var newlist = (List<TValue>)list;
            if (newlist != null)
                return newlist.IndexOf(val);
            return -1;
        }

        //RoadBase
        public static int GetId([DisallowNull] this RnDataRoadBase roadbase, RoadNetworkDataGetter getter)
        {
            List<RnDataRoadBase> roads = getter.GetRoadBases() as List<RnDataRoadBase>;
            return roads.FindIndex(x => x == roadbase);
        }

        public static bool IsValid([DisallowNull] this RnDataRoadBase roadbase, RoadNetworkDataGetter getter)
        {
            return roadbase.GetId(getter) >= 0;
        }


        //Point
        #region Point
        public static int GetId([DisallowNull] this RnDataPoint point, RoadNetworkDataGetter getter)
        {
            List<RnDataPoint> points = getter.GetPoints() as List<RnDataPoint>;
            return points.FindIndex(x => x == point);
        }

        /// <summary>
        /// RnDataPoint から RnDataLineString取得
        /// </summary>
        public static List<RnDataLineString> GetParentLineString([DisallowNull] this RnDataPoint point, RoadNetworkDataGetter getter)
        {
            int index = point.GetId(getter);
            List<RnDataLineString> lines = getter.GetLineStrings() as List<RnDataLineString>;
            return lines.Where(x => x.Points.Any(y => y.ID == index)).ToList<RnDataLineString>();
        }

        /// <summary>
        /// RnDataPoint から RnDataRoad取得
        /// </summary>
        public static List<RnDataRoad> GetRoad([DisallowNull] this RnDataPoint point, RoadNetworkDataGetter getter)
        {
            List<RnDataRoad> outRoads = new();

            List<RnDataLineString> linestrings = point.GetParentLineString(getter);
            foreach (RnDataLineString linestring in linestrings)
            {
                outRoads.AddRange(linestring.GetRoad(getter));
            }
            return outRoads;
        }
        #endregion Point

        //LineString
        #region LineString
        public static int GetId([DisallowNull] this RnDataLineString linestring, RoadNetworkDataGetter getter)
        {
            List<RnDataLineString> lines = getter.GetLineStrings() as List<RnDataLineString>;
            return lines.FindIndex(x => x == linestring);
        }

        /// <summary>
        /// RnDataLineString から RnDataWay取得
        /// </summary>
        public static List<RnDataWay> GetParentWays([DisallowNull] this RnDataLineString linestring, RoadNetworkDataGetter getter)
        {
            int index = linestring.GetId(getter);
            var ways = getter.GetWays() as List<RnDataWay>;
            return ways.Where(x => x.LineString.ID == index).ToList<RnDataWay>();
        }

        /// <summary>
        /// RnDataLineStringの距離
        /// </summary>
        public static float GetTotalDistance([DisallowNull] this RnDataLineString linestring, RoadNetworkDataGetter getter)
        {
            List<Vector3> points = linestring.GetChildPointsVector(getter);
            float distance = 0;
            for (int i = 0; i < points.Count - 1; i++)
            {
                distance += Vector3.Distance(points[i], points[i + 1]);
            }
            return distance;
        }

        /// <summary>
        /// RnDataLineStringからRnDataRoad取得
        /// </summary>
        public static List<RnDataRoad> GetRoad([DisallowNull] this RnDataLineString linestring, RoadNetworkDataGetter getter)
        {
            List<RnDataRoad> outRoads = new();

            List<RnDataWay> ways = linestring.GetParentWays(getter);
            foreach (RnDataWay way in ways)
            {
                List<RnDataLane> lanes = way.GetParentLanes(getter);
                foreach (RnDataLane lane in lanes)
                {
                    List<RnDataRoad> roads = lane.GetParentRoads(getter);
                    outRoads.AddRange(roads);
                }
            }
            return outRoads;
        }

        /// <summary>
        /// RnDataLineStringからRnDataPoint取得
        /// </summary>
        public static List<RnDataPoint> GetChildPoints([DisallowNull] this RnDataLineString linestring, RoadNetworkDataGetter getter)
        {
            List<RnDataPoint> points = new();
            foreach (var pid in linestring.Points)
            {
                if (!pid.IsValid)
                    continue;

                points.Add(getter.GetPoints()[pid.ID]);
            }

            return points;
        }

        /// <summary>
        /// RnDataLineStringからRnDataPointのVector3座標取得
        /// </summary>
        public static List<Vector3> GetChildPointsVector([DisallowNull] this RnDataLineString linestring, RoadNetworkDataGetter getter)
        {
            if (linestring?.Points == null)
                return new List<Vector3>();

            List<Vector3> points = new();
            foreach (RnID<RnDataPoint> pid in linestring.Points)
            {
                if (!pid.IsValid)
                    continue;

                points.Add(getter.GetPoints().TryGet(pid).Vertex);
            }

            return points;
        }
        #endregion LineString

        //Way
        #region Way
        public static int GetId([DisallowNull] this RnDataWay way, RoadNetworkDataGetter getter )
        {
            List<RnDataWay> ways = getter.GetWays() as List<RnDataWay>;
            return ways.FindIndex(x => x == way);
        }

        /// <summary>
        /// RnDataWayからRnDataLane取得
        /// </summary>
        public static List<RnDataLane> GetParentLanes([DisallowNull] this RnDataWay way, RoadNetworkDataGetter getter)
        {
            int index = way.GetId(getter);
            var lanes = getter.GetLanes() as List<RnDataLane>;
            return lanes.Where(x => x.LeftWay.ID == index || x.RightWay.ID == index || x.CenterWay.ID == index).ToList<RnDataLane>();
        }

        /// <summary>
        /// RnDataWayからRnDataLineString取得
        /// </summary>
        public static RnDataLineString GetChildLineString([DisallowNull] this RnDataWay way, RoadNetworkDataGetter getter)
        {
            if (way?.LineString.IsValid == true)
                return getter.GetLineStrings()[way.LineString.ID];
            return null;
        }

        /// <summary>
        /// RnDataWayがRnDataWayのリストに含まれるか
        /// </summary>
        public static bool ContainsSameLine([DisallowNull] this RnDataWay way, List<RnDataWay> ways)
        {
            return ways.Any(x => x.IsSameLine(way));
        }

        #endregion Way

        //Lane
        #region Lane
        public static int GetId([DisallowNull] this RnDataLane lane, RoadNetworkDataGetter getter)
        {
            List<RnDataLane> ways = getter.GetLanes() as List<RnDataLane>;
            return ways.FindIndex(x => x == lane);
        }

        /// <summary>
        /// RnDataLaneからRnDataRoad取得
        /// </summary>
        public static List<RnDataRoad> GetParentRoads([DisallowNull] this RnDataLane lane, RoadNetworkDataGetter getter)
        {
            int index = lane.GetId(getter);
            var roadbases = getter.GetRoadBases() as List<RnDataRoadBase>;
            var roads = roadbases.OfType<RnDataRoad>().ToList();
            return roads.Where(x => x.MainLanes.Any(y => y.ID == index)).ToList<RnDataRoad>();
        }

        /// <summary>
        /// RnDataLaneからRnDataWay取得
        /// </summary>
        public static RnDataWay GetChildWay([DisallowNull] this RnDataLane lane, RoadNetworkDataGetter getter, LanePosition position = LanePosition.Center)
        {
            RnID<RnDataWay> wayId = lane.CenterWay;
            switch (position)
            {
                case LanePosition.Center:
                    wayId = lane.CenterWay;
                    break;
                case LanePosition.Left:
                    wayId = lane.LeftWay;
                    break;
                case LanePosition.Right:
                    wayId = lane.RightWay;
                    break;
            }
            if (wayId.IsValid)
            {
                int index = wayId.ID;
                return getter.GetWays()[index];
            }

            return null;
        }

        /// <summary>
        /// RnDataLaneからRnDataLineString取得
        /// </summary>
        public static RnDataLineString GetChildLineString([DisallowNull] this RnDataLane lane, RoadNetworkDataGetter getter, LanePosition position = LanePosition.Center)
        {
            RnDataWay way = lane.GetChildWay(getter, position);
            if (way?.LineString.IsValid == true)
                return getter.GetLineStrings().TryGet(way.LineString);

            return null;
        }

        /// <summary>
        /// RnDataLaneからNextBorder(RnDataLineString)取得
        /// </summary>
        public static RnDataWay GetNextBorder([DisallowNull] this RnDataLane lane, RoadNetworkDataGetter getter)
        {
            return getter.GetWays().TryGet(lane.NextBorder);
        }

        /// <summary>
        /// RnDataLaneからPrevBorder(RnDataLineString)取得
        /// </summary>
        public static RnDataWay GetPrevBorder([DisallowNull] this RnDataLane lane, RoadNetworkDataGetter getter)
        {
            return getter.GetWays().TryGet(lane.PrevBorder);
        }

        #endregion Lane

        //Road
        #region Road
        public static int GetId([DisallowNull] this RnDataRoad road, RoadNetworkDataGetter getter)
        {
            List<RnDataRoadBase> roads = getter.GetRoadBases() as List<RnDataRoadBase>;
            return roads.FindIndex(x => x == road);
        }

        /// <summary>
        /// RnDataRoadからMainLanes(RnDataLane)取得
        /// </summary>
        public static List<RnDataLane> GetMainLanes([DisallowNull] this RnDataRoad road, RoadNetworkDataGetter getter)
        {
            if (getter == null)
                Debug.LogError("getter is null");

            List<RnDataLane> lanes = new();
            List<RnID<RnDataLane>> laneIds = road?.MainLanes;
            if (laneIds != null)
            {
                foreach (RnID<RnDataLane> id in laneIds)
                {
                    if (id.IsValid)
                        lanes.Add(getter.GetLanes().TryGet(id));
                }
            }
            return lanes;
        }

        /// <summary>
        /// RnDataRoadからMedianLane(RnDataLane)取得
        /// </summary>
        public static RnDataLane GetMedianLane([DisallowNull] this RnDataRoad road, RoadNetworkDataGetter getter)
        {
            List<RnDataLane> lanes = new();
            if (road.MedianLane.IsValid)
                return getter.GetLanes().TryGet(road.MedianLane);
            return null;
        }

        /// <summary>
        /// RnDataRoadのMainLane(RnDataLane)のIndex取得
        /// </summary>
        public static int GetLaneIndexOfMainLanes([DisallowNull] this RnDataRoad road, RoadNetworkDataGetter getter, RnDataLane lane)
        {
            return road.GetMainLanes(getter).IndexOf(lane);
        }

        /// <summary>
        /// RnDataRoadから指定した位置のRnDataLane取得
        /// </summary>
        /// <param name="laneIndex">laneIndex は IDではなくMainLaneのindex</param>
        public static RnDataLane GetChildLane([DisallowNull] this RnDataRoad road, RoadNetworkDataGetter getter, int laneIndex = -1, bool isMainLane = true)
        {
            if (isMainLane)
            {
                List<RnDataLane> mainlanes = road.GetMainLanes(getter);
                if (mainlanes.Count > laneIndex)
                {
                    return road.GetMainLanes(getter).TryGet(laneIndex);
                }
            }
            else
            {
                RnDataLane lane = road.GetMedianLane(getter);
                if (lane != null)
                {
                    return lane;
                }
            }

            return null;
        }

        /// <summary>
        /// RnDataRoadから指定した位置のRnDataWay取得
        /// </summary>
        /// <param name="laneIndex">laneIndex は IDではなくMainLaneのindex</param>
        public static RnDataWay GetChildWay([DisallowNull] this RnDataRoad road, RoadNetworkDataGetter getter, int laneIndex, bool isMainLane = true, LanePosition laneposition = LanePosition.Center)
        {
            if (isMainLane)
            {
                List<RnDataLane> mainlanes = road.GetMainLanes(getter);
                if (mainlanes.Count > laneIndex)
                {
                    return road.GetMainLanes(getter).TryGet(laneIndex).GetChildWay(getter, laneposition);
                }
            }
            else
            {
                RnDataLane lane = road.GetMedianLane(getter);
                if (lane != null)
                {
                    return lane.GetChildWay(getter, laneposition);
                }
            }

            return null;
        }

        /// <summary>
        /// RnDataRoadから指定した位置のRnDataLineString取得
        /// </summary>
        /// <param name="laneIndex">laneIndex は IDではなくMainLaneのindex</param>
        public static RnDataLineString GetChildLineString([DisallowNull] this RnDataRoad road, RoadNetworkDataGetter getter, int laneIndex, bool isMainLane = true, LanePosition laneposition = LanePosition.Center)
        {
            RnDataWay way = road.GetChildWay(getter, laneIndex, isMainLane, laneposition);
            return way.GetChildLineString(getter);
        }

        /// <summary>
        /// RnDataRoadから指定したRnDataWayの各種情報取得
        /// </summary>
        /// <returns>
        /// bool : MainLaneかどうかのフラグ
        /// int : List上のLane index
        /// LanePosition : Center , Left , Right
        /// </returns>
        public static (bool, int, LanePosition) GetWayPosition([DisallowNull] this RnDataRoad road, RoadNetworkDataGetter getter, RnDataWay way)
        {
            bool isMainLane = false;
            int laneIndex = -1;
            LanePosition lanePosition = default(LanePosition);
            var wayId = way.GetId(getter);

            if (road.MedianLane.ID != wayId)
            {
                List<RnDataLane> mainLanes = road.GetMainLanes(getter);
                foreach (var lane in mainLanes.Select((value, index) => new { value, index }))
                {
                    bool found = false;
                    if (lane.value.CenterWay.ID == way.GetId(getter))
                    {
                        lanePosition = LanePosition.Center;
                        found = true;
                    }
                    else if (lane.value.LeftWay.ID == way.GetId(getter))
                    {
                        lanePosition = LanePosition.Left;
                        found = true;
                    }
                    else if (lane.value.RightWay.ID == way.GetId(getter))
                    {
                        lanePosition = LanePosition.Right;
                        found = true;
                    }

                    if (found)
                    {
                        laneIndex = lane.index;
                        isMainLane = true;
                        break;
                    }
                }
            }
            return (isMainLane, laneIndex, lanePosition);
        }

        /// <summary>
        /// RnDataRoadのNextRoad(RnDataRoadBase)取得
        /// </summary>
        public static RnDataRoadBase GetNextRoad([DisallowNull] this RnDataRoad road, RoadNetworkDataGetter getter)
        {
            if (road?.Next.IsValid ?? false)
                return getter.GetRoadBases().TryGet(road.Next);
            return null;
        }

        /// <summary>
        /// RnDataRoadのPrevRoad(RnDataRoadBase)取得
        /// </summary>
        public static RnDataRoadBase GetPrevRoad([DisallowNull] this RnDataRoad road, RoadNetworkDataGetter getter)
        {
            if (road?.Prev.IsValid ?? false)
                return getter.GetRoadBases().TryGet(road.Prev);
            return null;
        }

        /// <summary>
        /// RnDataRoadの全NextBorder(RnDataWay)取得
        /// </summary>
        public static List<RnDataWay> GetAllNextBorders([DisallowNull] this RnDataRoad road, RoadNetworkDataGetter getter)
        {
            var borders = road.GetMainLanes(getter).Select(x => x.NextBorder).ToList();
            return borders.Select(x => getter.GetWays().TryGet(x)).ToList();
        }

        /// <summary>
        /// RnDataRoadの全PrevBorder(RnDataWay)取得
        /// </summary>
        public static List<RnDataWay> GetAllPrevBorders([DisallowNull] this RnDataRoad road, RoadNetworkDataGetter getter)
        {
            var borders = road.GetMainLanes(getter).Select(x => x.PrevBorder).ToList();
            return borders.Select(x => getter.GetWays().TryGet(x)).ToList();
        }

        /// <summary>
        /// RnDataRoadのPrevBorderからLanes(RnDataLane)取得
        /// Reverse無視フラグ対応
        /// </summary>
        public static List<RnDataLane> GetLanesFromPrevBorder([DisallowNull] this RnDataRoad road, RoadNetworkDataGetter getter, RnDataWay border, bool ignoreReversedLane)
        {
            if(!ignoreReversedLane)
                return GetLanesFromAllBorders(road, getter, border);

            List<RnDataLane> mainLanes = road.GetMainLanes(getter);
            return mainLanes.FindAll(x => x.GetPrevBorder(getter)?.IsSameLine(border) == true);
        }

        /// <summary>
        /// RnDataRoadのNextBorderからLanes(RnDataLane)取得
        /// Reverse無視フラグ対応
        /// </summary>
        public static List<RnDataLane> GetLanesFromNextBorder([DisallowNull] this RnDataRoad road, RoadNetworkDataGetter getter, RnDataWay border, bool ignoreReversedLane)
        {
            if (!ignoreReversedLane)
                return GetLanesFromAllBorders(road, getter, border);

            List<RnDataLane> mainLanes = road.GetMainLanes(getter);
            return mainLanes.FindAll(x => x.GetNextBorder(getter)?.IsSameLine(border) == true);
        }

        /// <summary>
        /// RnDataRoadのPrevBorderからLanes(RnDataLane)取得
        /// </summary>
        public static List<RnDataLane> GetLanesFromPrevBorder([DisallowNull] this RnDataRoad road, RoadNetworkDataGetter getter, RnDataWay border)
        {
            List<RnDataLane> mainLanes = road.GetMainLanes(getter);
            var prevLanes = mainLanes.FindAll(x => x.GetPrevBorder(getter)?.IsSameLine(border) == true);
            if (prevLanes.Count > 0)
                return prevLanes;

            //反転している場合は逆にする
            return mainLanes.FindAll(x => x.IsReverse && x.GetNextBorder(getter)?.IsSameLine(border) == true);
        }

        /// <summary>
        /// RnDataRoadのNextBorderからLanes(RnDataLane)取得
        /// </summary>
        public static List<RnDataLane> GetLanesFromNextBorder([DisallowNull] this RnDataRoad road, RoadNetworkDataGetter getter, RnDataWay border)
        {
            List<RnDataLane> mainLanes = road.GetMainLanes(getter);
            var nextLanes = mainLanes.FindAll(x => x.GetNextBorder(getter)?.IsSameLine(border) == true);
            if (nextLanes.Count > 0)
                return nextLanes;

            //反転している場合は逆にする
            return mainLanes.FindAll(x => x.IsReverse && x.GetPrevBorder(getter)?.IsSameLine(border) == true);
        }

        /// <summary>
        /// RnDataRoadのNext/PrevBorderからLanes(RnDataLane)取得
        /// Next/Prev両方から取得
        /// </summary>
        public static List<RnDataLane> GetLanesFromAllBorders([DisallowNull] this RnDataRoad road, RoadNetworkDataGetter getter, RnDataWay border)
        {
            List<RnDataLane> mainLanes = road.GetMainLanes(getter);
            return mainLanes.FindAll(x => x.GetNextBorder(getter)?.IsSameLine(border) == true || x.GetPrevBorder(getter)?.IsSameLine(border) == true);
        }

        /// <summary>
        /// RnDataRoadのPrevRoadのRnDataTrackと接続するRnDataLane取得
        /// </summary>
        public static List<RnDataLane> GetLanesFromPrevTrack([DisallowNull] this RnDataRoad road, RoadNetworkDataGetter getter, RnDataTrack track)
        {
            var toBorder = track.GetToBorder(getter);
            return road.GetLanesFromPrevBorder(getter, toBorder);
        }

        /// <summary>
        /// RnDataRoadのNextRoadのRnDataTrackと接続するRnDataLane取得
        /// </summary>
        public static List<RnDataLane> GetLanesFromNextTrack([DisallowNull] this RnDataRoad road, RoadNetworkDataGetter getter, RnDataTrack track)
        {
            var fromBorder = track.GetFromBorder(getter);
            return road.GetLanesFromNextBorder(getter, fromBorder);
        }

        /// <summary>
        /// RnDataRoadのPrevRoadのRnDataTrackと接続するRnDataLane取得
        /// Reverse無視フラグ対応
        /// </summary>
        public static List<RnDataLane> GetLanesFromPrevTrack([DisallowNull] this RnDataRoad road, RoadNetworkDataGetter getter, RnDataTrack track, bool ignoreReversedLane)
        {
            var toBorder = track.GetToBorder(getter);
            return road.GetLanesFromPrevBorder(getter, toBorder, ignoreReversedLane);
        }

        /// <summary>
        /// RnDataRoadのNextRoadのRnDataTrackと接続するRnDataLane取得
        /// Reverse無視フラグ対応
        /// </summary>
        public static List<RnDataLane> GetLanesFromNextTrack([DisallowNull] this RnDataRoad road, RoadNetworkDataGetter getter, RnDataTrack track, bool ignoreReversedLane)
        {
            var fromBorder = track.GetFromBorder(getter);
            return road.GetLanesFromNextBorder(getter, fromBorder, ignoreReversedLane);
        }

        /// <summary>
        /// RnDataRoadの全Border(RnDataLineString)取得
        /// </summary>
        public static List<RnDataLineString> GetLAllBordersFromMainLanes([DisallowNull] this RnDataRoad road, RoadNetworkDataGetter getter)
        {
            List<RnDataLane> mainLanes = road.GetMainLanes(getter);
            var borders = mainLanes.Select(x => x.GetNextBorder(getter).GetChildLineString(getter)).ToList();
            var prevBorders = mainLanes.Select(x => x.GetPrevBorder(getter).GetChildLineString(getter)).ToList();
            borders.AddRange(prevBorders);
            return borders;
        }

        /// <summary>
        /// RnDataRoadの全Border(RnDataWay)取得
        /// </summary>
        public static List<RnDataWay> GetLAllBorderWaysFromMainLanes([DisallowNull] this RnDataRoad road, RoadNetworkDataGetter getter)
        {
            List<RnDataLane> mainLanes = road.GetMainLanes(getter);
            var borders = mainLanes.Select(x => x.GetNextBorder(getter)).ToList();
            var prevBorders = mainLanes.Select(x => x.GetPrevBorder(getter)).ToList();
            borders.AddRange(prevBorders);
            return borders;
        }

        #endregion road

        //Intersection
        #region Intersection
        public static int GetId([DisallowNull] this RnDataIntersection intersection, RoadNetworkDataGetter getter)
        {
            List<RnDataRoadBase> roads = getter.GetRoadBases() as List<RnDataRoadBase>;
            return roads.FindIndex(x => x == intersection);
        }

        /// <summary>
        /// RnDataIntersectionのfrom Border(RnDataLane)に接続するRnDataTrackを取得
        /// </summary>
        public static List<RnDataTrack> GetFromTracksFromLane([DisallowNull] this RnDataIntersection intersection, RoadNetworkDataGetter getter, RnDataLane from)
        {
            return intersection.Tracks.FindAll(x => x.GetFromBorder(getter)?.IsSameLine(from.GetNextBorder(getter)) == true).ToList();
        }

        /// <summary>
        /// RnDataIntersectionのto Border(RnDataLane)に接続するRnDataTrackを取得
        /// </summary>
        public static List<RnDataTrack> GetToTracksFromLane([DisallowNull] this RnDataIntersection intersection, RoadNetworkDataGetter getter, RnDataLane to)
        {
            return intersection.Tracks.FindAll(x => x.GetToBorder(getter)?.IsSameLine(to.GetPrevBorder(getter)) == true).ToList();
        }

        /// <summary>
        /// RnDataIntersectionのto Border(RnDataWay)に接続するRnDataTrackを取得
        /// </summary>
        public static List<RnDataTrack> GetToTracksFromBorder([DisallowNull] this RnDataIntersection intersection, RoadNetworkDataGetter getter, RnDataWay border)
        {
            return intersection.Tracks.FindAll(x => x.GetToBorder(getter)?.IsSameLine(border) == true);
        }

        /// <summary>
        /// RnDataIntersectionのfrom Border(RnDataWay)に接続するRnDataTrackを取得
        /// </summary>
        public static List<RnDataTrack> GetFromTracksFromBorder([DisallowNull] this RnDataIntersection intersection, RoadNetworkDataGetter getter, RnDataWay border)
        {
            return intersection.Tracks.FindAll(x => x.GetFromBorder(getter)?.IsSameLine(border) == true);
        }

        /// <summary>
        /// RnDataIntersectionのBorder(RnDataWay)に接続する全Edge(RnDataNeighbor)を取得
        /// </summary>
        public static List<RnDataNeighbor> GetEdgesFromBorder([DisallowNull] this RnDataIntersection intersection, RoadNetworkDataGetter getter, RnDataWay way)
        {
            return intersection.Edges.FindAll(x => x.GetBorder(getter).IsSameLine(way));
        }

        /// <summary>
        /// RnDataIntersectionのRnDataTrackから接続する次のRoadのRnDataLaneを取得
        /// </summary>
        public static List<RnDataLane> GetNextLanesFromTrack([DisallowNull] this RnDataIntersection intersection, RoadNetworkDataGetter getter, RnDataTrack track)
        {
            var nextEdge = intersection.GetEdgesFromBorder(getter, track.GetToBorder(getter)).FirstOrDefault();
            var nextRoad = nextEdge.GetRoad(getter) as RnDataRoad;
            return nextRoad.GetLanesFromPrevTrack(getter, track);
        }

        /// <summary>
        /// RnDataIntersectionのRnDataTrackから接続する前のRoadのRnDataLaneを取得
        /// </summary>
        public static List<RnDataLane> GetPrevLanesFromTrack([DisallowNull] this RnDataIntersection intersection, RoadNetworkDataGetter getter, RnDataTrack track)
        {
            var prevEdge = intersection.GetEdgesFromBorder(getter, track.GetFromBorder(getter)).FirstOrDefault();
            var prevRoad = prevEdge.GetRoad(getter) as RnDataRoad;
            return prevRoad.GetLanesFromNextTrack(getter, track);
        }

        /// <summary>
        /// RnDataIntersectionのRnDataTrackから接続する次のRoadのRnDataLaneを取得
        /// Reverse無視フラグ対応
        /// </summary>
        public static List<RnDataLane> GetNextLanesFromTrack([DisallowNull] this RnDataIntersection intersection, RoadNetworkDataGetter getter, RnDataTrack track, bool ignoreReversedLane)
        {
            var nextEdge = intersection.GetEdgesFromBorder(getter, track.GetToBorder(getter)).FirstOrDefault();
            var nextRoad = nextEdge.GetRoad(getter) as RnDataRoad;
            return nextRoad.GetLanesFromPrevTrack(getter, track, ignoreReversedLane);
        }

        /// <summary>
        /// RnDataIntersectionのRnDataTrackから接続する前のRoadのRnDataLaneを取得
        /// Reverse無視フラグ対応
        /// </summary>
        public static List<RnDataLane> GetPrevLanesFromTrack([DisallowNull] this RnDataIntersection intersection, RoadNetworkDataGetter getter, RnDataTrack track, bool ignoreReversedLane)
        {
            var prevEdge = intersection.GetEdgesFromBorder(getter, track.GetFromBorder(getter)).FirstOrDefault();
            var prevRoad = prevEdge.GetRoad(getter) as RnDataRoad;
            return prevRoad.GetLanesFromNextTrack(getter, track, ignoreReversedLane);
        }

        /// <summary>
        /// RnDataIntersectionの指定と反対側の道路のEdgeを取得
        /// Empty Intersection用
        /// </summary>
        public static List<RnDataNeighbor> GetOppositeSideEdgesFromRoad([DisallowNull] this RnDataIntersection intersection, int roadId)
        {
            var edges = new List<RnDataNeighbor>(intersection.Edges);
            edges.RemoveAll(e => e.Road.ID == roadId || !e.Road.IsValid);
            return edges;
        }

        //Empty Intersectionの場合
        /// <summary>
        /// Empty Intersectionをはさんだ手間の道路の接続するRnDataLane取得
        /// </summary>
        public static List<RnDataLane> GetPrevLanesFromLane([DisallowNull] this RnDataIntersection intersection, RoadNetworkDataGetter getter, RnDataRoad currentRoad, RnDataLane currentLane)
        {
            var edge = intersection.GetOppositeSideEdgesFromRoad(currentRoad.GetId(getter)).FirstOrDefault();
            if (edge != null)
            {
                var prevRoad = edge.GetRoad(getter) as RnDataRoad;
                return prevRoad.GetLanesFromNextBorder(getter, currentLane.GetPrevBorder(getter));
            }
            return null;
        }

        /// <summary>
        /// Empty Intersectionをはさんだ次の道路の接続するRnDataLane取得
        /// </summary>
        public static List<RnDataLane> GetNextLanesFromLane([DisallowNull] this RnDataIntersection intersection, RoadNetworkDataGetter getter, RnDataRoad currentRoad, RnDataLane currentLane)
        {
            var edge = intersection.GetOppositeSideEdgesFromRoad(currentRoad.GetId(getter)).FirstOrDefault();
            if (edge != null)
            {
                var nextRoad = edge.GetRoad(getter) as RnDataRoad;
                return nextRoad.GetLanesFromPrevBorder(getter, currentLane.GetNextBorder(getter));
            }
            return null;
        }

        /// <summary>
        /// Empty Intersectionをはさんだ手間の道路の接続するRnDataLane取得
        /// Reverse無視フラグ対応
        /// </summary>
        public static List<RnDataLane> GetPrevLanesFromLane([DisallowNull] this RnDataIntersection intersection, RoadNetworkDataGetter getter, RnDataRoad currentRoad, RnDataLane currentLane, bool ignoreReversedLane)
        {
            var edge = intersection.GetOppositeSideEdgesFromRoad(currentRoad.GetId(getter)).FirstOrDefault();
            if (edge != null)
            {
                var prevRoad = edge.GetRoad(getter) as RnDataRoad;
                return prevRoad.GetLanesFromNextBorder(getter, currentLane.GetPrevBorder(getter), ignoreReversedLane);
            }
            return null;
        }

        /// <summary>
        /// Empty Intersectionをはさんだ次の道路の接続するRnDataLane取得
        /// Reverse無視フラグ対応
        /// </summary>
        public static List<RnDataLane> GetNextLanesFromLane([DisallowNull] this RnDataIntersection intersection, RoadNetworkDataGetter getter, RnDataRoad currentRoad, RnDataLane currentLane, bool ignoreReversedLane)
        {
            var edge = intersection.GetOppositeSideEdgesFromRoad(currentRoad.GetId(getter)).FirstOrDefault();
            if (edge != null)
            {
                var nextRoad = edge.GetRoad(getter) as RnDataRoad;
                return nextRoad.GetLanesFromPrevBorder(getter, currentLane.GetNextBorder(getter), ignoreReversedLane);
            }
            return null;
        }

        /// <summary>
        /// RnDataIntersectionのBorderと同一ライン上にあるEdge(RnDataNeighbor)を取得
        /// </summary>
        public static List<RnDataNeighbor> GetStraightLineEdgesFromBorder([DisallowNull] this RnDataIntersection intersection, RoadNetworkDataGetter getter, RnDataWay way)
        {
            var edges = intersection.Edges.FindAll(x => x.GetBorder(getter).IsSameLine(way));
            var roads = edges.Select(x => x.GetRoad(getter)).OfType<RnDataRoad>().ToList();
            List<RnDataNeighbor> outEdges = new List<RnDataNeighbor>();
            foreach (var road in roads)
            {
                outEdges.AddRange(intersection.GetEdgesFromRoad(getter, road));
            }
            return outEdges;
        }

        /// <summary>
        /// RnDataIntersectionと隣接する道路に接するEdge(RnDataNeighbor)取得
        /// </summary>
        public static List<RnDataNeighbor> GetEdgesFromRoad([DisallowNull] this RnDataIntersection intersection, RoadNetworkDataGetter getter, RnDataRoad road)
        {
            return intersection.Edges.FindAll(x => x.Road.ID == road.GetId(getter));
        }

        /// <summary>
        /// RnDataIntersectionと隣接する道路を全て取得
        /// </summary>
        public static List<RnDataRoadBase> GetAllConnectedRoads([DisallowNull] this RnDataIntersection intersection, RoadNetworkDataGetter getter)
        {
            return intersection.Edges?.FindAll(x => x.Road.IsValid).Select(x => x.GetRoad(getter)).Distinct()?.ToList();
        }

        /// <summary>
        /// RnDataIntersectionのRnDataTrackに対して交差するTrackを取得
        /// TurnType : Straightのtrackを渡す
        /// </summary>
        public static List<RnDataTrack> GetCrossingTracks([DisallowNull] this RnDataIntersection intersection, RoadNetworkDataGetter getter, RnDataTrack track)
        {
            var fromBorders = intersection.GetStraightLineEdgesFromBorder(getter, track.GetFromBorder(getter)).Select(x => x.GetBorder(getter)).ToList();
            var toBorders = intersection.GetStraightLineEdgesFromBorder(getter, track.GetToBorder(getter)).Select(x => x.GetBorder(getter)).ToList();
            fromBorders.AddRange(toBorders); //両方
            List<RnDataTrack> outTracks = new List<RnDataTrack>();
            foreach (RnDataTrack tr in intersection.Tracks)
            {
                if (!tr.GetToBorder(getter).ContainsSameLine(fromBorders) && !tr.GetFromBorder(getter).ContainsSameLine(fromBorders))
                {
                    outTracks.Add(tr);
                }
            }
            return outTracks;
        }

        /// <summary>
        /// RnDataIntersectionのRnDataTrackに対して対向車線となるTrackを取得
        /// TurnType : Straightのtrackを渡す
        /// </summary>
        public static List<RnDataTrack> GetOncomingTracks([DisallowNull] this RnDataIntersection intersection, RoadNetworkDataGetter getter, RnDataTrack track)
        {
            var fromBorders = intersection.GetStraightLineEdgesFromBorder(getter, track.GetFromBorder(getter)).Select(x => x.GetBorder(getter)).ToList();
            var toBorders = intersection.GetStraightLineEdgesFromBorder(getter, track.GetToBorder(getter)).Select(x => x.GetBorder(getter)).ToList();
            return intersection.Tracks.FindAll(x => x.GetToBorder(getter).ContainsSameLine(fromBorders) && x.GetFromBorder(getter).ContainsSameLine(toBorders));
        }

        /// <summary>
        /// RnDataIntersectionのRnDataTrackと同一のFromBorderを持つTrackを取得
        /// TurnType : StraightのTrack取得用
        /// </summary>
        public static List<RnDataTrack> GetTraksOfSameOriginByType([DisallowNull] this RnDataIntersection intersection, RoadNetworkDataGetter getter, RnDataTrack track, RnTurnType turnType)
        {
            var fromBorder = track.GetFromBorder(getter);
            if (fromBorder != null)
                return intersection.GetFromTracksFromBorder(getter, fromBorder)?.FindAll(x => x.TurnType == turnType)?.ToList() ?? null;
            return null;
        }

        /// <summary>
        /// 境界線borderに対応する辺を取得
        /// </summary>
        /// <param name="self"></param>
        /// <param name="border"></param>
        /// <param name="getter"></param>
        /// <returns></returns>
        public static RnDataNeighbor GetEdgeByBorder([DisallowNull] this RnDataIntersection self,
            RoadNetworkDataGetter getter, RnDataWay border)
        {
            if (border == null)
                return null;
            return self.Edges.FirstOrDefault(x => border.IsSameLine(getter.GetWay(x.Border)));
        }

        /// <summary>
        /// selfに対して, 利用可能なトラックかどうか
        /// </summary>
        /// <param name="self"></param>
        /// <param name="track"></param>
        /// <param name="getter"></param>
        /// <returns></returns>
        public static bool IsAvailableToTrack([DisallowNull] this RnDataIntersection self, RoadNetworkDataGetter getter, RnDataTrack track)
        {
            if (self.Tracks.Contains(track) == false)
                return false;

            // 行き先の道路を求める
            var toEdge = self.GetEdgeByBorder(getter, getter.GetWay(track.ToBorder));
            if (toEdge == null)
                return false;
            // 行先の無いトラックは無効
            var rb = getter.GetRoadBase(toEdge.Road);
            if (rb == null)
                return false;
            // 行き先が道の時, そのレーンがこちらに進入してくるレーンの場合は無効
            // 道路ネットワーク生成で１車線道路は両方向扱いにしているのでそれ対策)
            if (rb is RnDataRoad road)
            {
                // PrevBorderで繋がっているレーンがあるかどうか
                return road.GetLanesFromPrevTrack(getter, track, true).Any();
            }
            // 行き先が交差点の場合はOK
            return true;
        }

        public static bool IsAvailableFromTrack([DisallowNull] this RnDataIntersection self, RoadNetworkDataGetter getter, RnDataTrack track)
        {
            if (self.Tracks.Contains(track) == false)
                return false;

            // 行き先の道路を求める
            var fromEdge = self.GetEdgeByBorder(getter, getter.GetWay(track.FromBorder));
            if (fromEdge == null)
                return false;
            // 行先の無いトラックは無効
            var rb = getter.GetRoadBase(fromEdge.Road);
            if (rb == null)
                return false;
            // 行き先が道の時, そのレーンがこちらに進入してくるレーンの場合は無効
            // 道路ネットワーク生成で１車線道路は両方向扱いにしているのでそれ対策)
            if (rb is RnDataRoad road)
            {
                // PrevBorderで繋がっているレーンがあるかどうか
                return road.GetLanesFromNextTrack(getter, track, true).Any();
            }
            // 行き先が交差点の場合はOK
            return true;
        }
        public static List<RnDataTrack> FilterAvailableToTracks([DisallowNull] this RnDataIntersection self, RoadNetworkDataGetter getter, List<RnDataTrack> tracks)
        {
            return tracks.FindAll(x => self.IsAvailableToTrack(getter, x));
        }

        public static List<RnDataTrack> FilterAvailableFromTracks([DisallowNull] this RnDataIntersection self, RoadNetworkDataGetter getter, List<RnDataTrack> tracks)
        {
            return tracks.FindAll(x => self.IsAvailableFromTrack(getter, x));
        }

        #endregion Intersection

        //Track
        #region Track
        public static RnDataWay GetFromBorder([DisallowNull] this RnDataTrack track, RoadNetworkDataGetter getter)
        {
            return getter.GetWays().TryGet(track.FromBorder);
        }

        public static RnDataWay GetToBorder([DisallowNull] this RnDataTrack track, RoadNetworkDataGetter getter)
        {
            return getter.GetWays().TryGet(track.ToBorder);
        }

        #endregion Track

        //Edge / Neighbor
        #region Edge
        public static RnDataRoadBase GetRoad([DisallowNull] this RnDataNeighbor neighbor, RoadNetworkDataGetter getter)
        {
            if (neighbor.Road.IsValid)
                return getter.GetRoadBases().TryGet(neighbor.Road);
            return null;
        }

        public static RnDataWay GetBorder([DisallowNull] this RnDataNeighbor neighbor, RoadNetworkDataGetter getter)
        {
            if (neighbor.Border.IsValid)
                return getter.GetWays().TryGet(neighbor.Border);
            return null;
        }

        #endregion Edge

        #region RoadNetworkDataGetter

        /// <summary>
        /// idで指定したRnDataLineStringを取得. 存在しない場合はnullを返す
        /// </summary>
        /// <param name="self"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static RnDataWay GetWay([DisallowNull] this RoadNetworkDataGetter self, RnID<RnDataWay> id)
        {
            return self.GetWays().TryGet(id);
        }

        /// <summary>
        /// idで指定したRnDataLineStringを取得. 存在しない場合はnullを返す
        /// </summary>
        /// <param name="self"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static RnDataLineString GetLineString([DisallowNull] this RoadNetworkDataGetter self, RnID<RnDataLineString> id)
        {
            return self.GetLineStrings().TryGet(id);
        }

        /// <summary>
        /// idで指定したRnDataPointを取得. 存在しない場合はnullを返す
        /// </summary>
        /// <param name="self"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static RnDataPoint GetPoint([DisallowNull] this RoadNetworkDataGetter self, RnID<RnDataPoint> id)
        {
            return self.GetPoints().TryGet(id);
        }

        /// <summary>
        /// idで指定したRnDataRoadBaseを取得. 存在しない場合はnullを返す
        /// </summary>
        /// <param name="self"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static RnDataRoadBase GetRoadBase([DisallowNull] this RoadNetworkDataGetter self, RnID<RnDataRoadBase> id)
        {
            return self.GetRoadBases().TryGet(id);
        }

        /// <summary>
        /// idで指定したRnDataLaneを取得. 存在しない場合はnullを返す
        /// </summary>
        /// <param name="self"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static RnDataLane GetLane([DisallowNull] this RoadNetworkDataGetter self, RnID<RnDataLane> id)
        {
            return self.GetLanes().TryGet(id);
        }
        #endregion RoadNetworkStorage
    }
}
