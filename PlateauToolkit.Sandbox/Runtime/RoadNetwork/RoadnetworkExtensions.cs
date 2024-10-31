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

        public static List<RnDataLineString> GetParentLineString([DisallowNull] this RnDataPoint point, RoadNetworkDataGetter getter)
        {
            int index = point.GetId(getter);
            List<RnDataLineString> lines = getter.GetLineStrings() as List<RnDataLineString>;
            return lines.Where(x => x.Points.Any(y => y.ID == index)).ToList<RnDataLineString>();
        }

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
        public static List<RnDataWay> GetParentWays([DisallowNull] this RnDataLineString linestring, RoadNetworkDataGetter getter)
        {
            int index = linestring.GetId(getter);
            var ways = getter.GetWays() as List<RnDataWay>;
            return ways.Where(x => x.LineString.ID == index).ToList<RnDataWay>();
        }

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
        public static List<RnDataLane> GetParentLanes([DisallowNull] this RnDataWay way, RoadNetworkDataGetter getter)
        {
            int index = way.GetId(getter);
            var lanes = getter.GetLanes() as List<RnDataLane>;
            return lanes.Where(x => x.LeftWay.ID == index || x.RightWay.ID == index || x.CenterWay.ID == index).ToList<RnDataLane>();
        }

        public static RnDataLineString GetChildLineString([DisallowNull] this RnDataWay way, RoadNetworkDataGetter getter)
        {
            if (way?.LineString.IsValid == true)
                return getter.GetLineStrings()[way.LineString.ID];
            return null;
        }

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

        public static List<RnDataRoad> GetParentRoads([DisallowNull] this RnDataLane lane, RoadNetworkDataGetter getter)
        {
            int index = lane.GetId(getter);
            var roadbases = getter.GetRoadBases() as List<RnDataRoadBase>;
            var roads = roadbases.OfType<RnDataRoad>().ToList();
            return roads.Where(x => x.MainLanes.Any(y => y.ID == index)).ToList<RnDataRoad>();
        }

        public static RnDataWay GetChildWay([DisallowNull] this RnDataLane lane, RoadNetworkDataGetter getter, LanePosition position)
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

        public static RnDataLineString GetChildLineString([DisallowNull] this RnDataLane lane, RoadNetworkDataGetter getter, LanePosition position = LanePosition.Center)
        {
            RnDataWay way = lane.GetChildWay(getter, position);
            if (way?.LineString.IsValid == true)
                return getter.GetLineStrings().TryGet(way.LineString);

            return null;
        }

        public static RnDataWay GetNextBorder([DisallowNull] this RnDataLane lane, RoadNetworkDataGetter getter)
        {
            return getter.GetWays().TryGet(lane.NextBorder);
        }

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

        public static RnDataLane GetMedianLane([DisallowNull] this RnDataRoad road, RoadNetworkDataGetter getter)
        {
            List<RnDataLane> lanes = new();
            if (road.MedianLane.IsValid)
                return getter.GetLanes().TryGet(road.MedianLane);
            return null;
        }

        public static int GetLaneIndexOfMainLanes([DisallowNull] this RnDataRoad road, RoadNetworkDataGetter getter, RnDataLane lane)
        {
            return road.GetMainLanes(getter).IndexOf(lane);
        }

        //lane は IDではなくMainLaneのindex
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

        public static RnDataWay GetChildWay([DisallowNull] this RnDataRoad road, RoadNetworkDataGetter getter, int laneIndex , bool isMainLane = true, LanePosition laneposition = LanePosition.Center)
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

        public static RnDataLineString GetChildLineString([DisallowNull] this RnDataRoad road, RoadNetworkDataGetter getter, int laneIndex, bool isMainLane = true, LanePosition laneposition = LanePosition.Center)
        {
            RnDataWay way = road.GetChildWay(getter, laneIndex, isMainLane, laneposition);
            return way.GetChildLineString(getter);
        }

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

        public static RnDataRoadBase GetNextRoad([DisallowNull] this RnDataRoad road, RoadNetworkDataGetter getter)
        {
            if (road?.Next.IsValid ?? false)
                return getter.GetRoadBases().TryGet(road.Next);
            return null;
        }

        public static RnDataRoadBase GetPrevRoad([DisallowNull] this RnDataRoad road, RoadNetworkDataGetter getter)
        {
            if (road?.Prev.IsValid ?? false)
                return getter.GetRoadBases().TryGet(road.Prev);
            return null;
        }

        public static List<RnDataWay> GetAllNextBorders([DisallowNull] this RnDataRoad road, RoadNetworkDataGetter getter)
        {
            var borders = road.GetMainLanes(getter).Select(x => x.NextBorder).ToList();
            return borders.Select(x => getter.GetWays().TryGet(x)).ToList();
        }
        public static List<RnDataWay> GetAllPrevBorders([DisallowNull] this RnDataRoad road, RoadNetworkDataGetter getter)
        {
            var borders = road.GetMainLanes(getter).Select(x => x.PrevBorder).ToList();
            return borders.Select(x => getter.GetWays().TryGet(x)).ToList();
        }

        public static List<RnDataLane> GetLanesFromPrevBorder([DisallowNull] this RnDataRoad road, RoadNetworkDataGetter getter, RnDataWay border)
        {
            List<RnDataLane> mainLanes = road.GetMainLanes(getter);
            return mainLanes.FindAll(x => x.GetPrevBorder(getter)?.IsSameLine(border) == true);
        }

        public static List<RnDataLane> GetLanesFromNextBorder([DisallowNull] this RnDataRoad road, RoadNetworkDataGetter getter, RnDataWay border)
        {
            List<RnDataLane> mainLanes = road.GetMainLanes(getter);
            return mainLanes.FindAll(x => x.GetNextBorder(getter)?.IsSameLine(border) == true);
        }

        public static List<RnDataLane> GetLanesFromPrevTrack([DisallowNull] this RnDataRoad road, RoadNetworkDataGetter getter, RnDataTrack track)
        {
            var toBorder = track.GetToBorder(getter);
            return road.GetLanesFromPrevBorder(getter, toBorder);
        }

        public static List<RnDataLane> GetLanesFromNextTrack([DisallowNull] this RnDataRoad road, RoadNetworkDataGetter getter, RnDataTrack track)
        {
            var fromBorder = track.GetFromBorder(getter);
            return road.GetLanesFromNextBorder(getter, fromBorder);
        }

        #endregion road

        //Intersection
        #region Intersection
        public static int GetId([DisallowNull] this RnDataIntersection intersection, RoadNetworkDataGetter getter)
        {
            List<RnDataRoadBase> roads = getter.GetRoadBases() as List<RnDataRoadBase>;
            return roads.FindIndex(x => x == intersection);
        }

        public static List<RnDataTrack> GetFromTracksFromLane([DisallowNull] this RnDataIntersection intersection, RoadNetworkDataGetter getter, RnDataLane from)
        {
            return intersection.Tracks.FindAll(x => x.GetFromBorder(getter)?.IsSameLine(from.GetNextBorder(getter)) == true).ToList();
        }

        public static List<RnDataTrack> GetToTracksFromLane([DisallowNull] this RnDataIntersection intersection, RoadNetworkDataGetter getter, RnDataLane to)
        {
            return intersection.Tracks.FindAll(x => x.GetToBorder(getter)?.IsSameLine(to.GetPrevBorder(getter)) == true).ToList();
        }

        public static List<RnDataTrack> GetToTracksFromBorder([DisallowNull] this RnDataIntersection intersection, RoadNetworkDataGetter getter, RnDataWay border)
        {
            return intersection.Tracks.FindAll(x => x.GetToBorder(getter)?.IsSameLine(border) == true);
        }

        public static List<RnDataTrack> GetFromTracksFromBorder([DisallowNull] this RnDataIntersection intersection, RoadNetworkDataGetter getter, RnDataWay border)
        {
            return intersection.Tracks.FindAll(x => x.GetFromBorder(getter)?.IsSameLine(border) == true);
        }

        public static List<RnDataNeighbor> GetEdgesFromBorder([DisallowNull] this RnDataIntersection intersection, RoadNetworkDataGetter getter, RnDataWay way)
        {
            return intersection.Edges.FindAll(x => x.GetBorder(getter).IsSameLine(way));
        }

        //同一線状のEdge
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

        public static List<RnDataNeighbor> GetEdgesFromRoad([DisallowNull] this RnDataIntersection intersection, RoadNetworkDataGetter getter, RnDataRoad road)
        {
            return intersection.Edges.FindAll(x => x.Road.ID == road.GetId(getter));
        }

        public static List<RnDataRoadBase> GetAllConnectedRoads([DisallowNull] this RnDataIntersection intersection, RoadNetworkDataGetter getter)
        {
            return intersection.Edges?.FindAll(x => x.Road.IsValid).Select(x => x.GetRoad(getter)).Distinct()?.ToList();
        }

        //TurnType : Straightのtrackを渡す必要あり　（ただそこがバグってるぽい？）
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

        public static List<RnDataTrack> GetOncomingTracks([DisallowNull] this RnDataIntersection intersection, RoadNetworkDataGetter getter, RnDataTrack track)
        {
            var fromBorders = intersection.GetStraightLineEdgesFromBorder(getter, track.GetFromBorder(getter)).Select(x => x.GetBorder(getter)).ToList();
            var toBorders = intersection.GetStraightLineEdgesFromBorder(getter, track.GetToBorder(getter)).Select(x => x.GetBorder(getter)).ToList();
            return intersection.Tracks.FindAll(x => x.GetToBorder(getter).ContainsSameLine(fromBorders) && x.GetFromBorder(getter).ContainsSameLine(toBorders));
        }

        public static List<RnDataTrack> GetTraksOfSameOriginByType([DisallowNull] this RnDataIntersection intersection, RoadNetworkDataGetter getter, RnDataTrack track, RnTurnType turnType)
        {
            var fromBorder = track.GetFromBorder(getter);
            if (fromBorder != null)
                return intersection.GetFromTracksFromBorder(getter, fromBorder)?.FindAll(x => x.TurnType == turnType)?.ToList() ?? null;
            return null;
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
    }
}
