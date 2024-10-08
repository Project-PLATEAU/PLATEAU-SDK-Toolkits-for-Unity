using PLATEAU.RoadNetwork.Data;
using PLATEAU.RoadNetwork.Structure;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;
using static Codice.CM.Common.CmCallContext;

namespace PlateauToolkit.Sandbox
{
    public static class RoadnetworkExtensions
    {
        public enum LanePosition
        {
            Center,
            Left,
            Right,
        }

        //Point
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

        //LineString
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

                points.Add(getter.GetPoints()[pid.ID].Vertex);
            }

            return points;
        }

        //Way
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
            return getter.GetLineStrings()[way.LineString.ID];
        }

        //Lane
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


        public static RnDataLineString GetChildLineString([DisallowNull] this RnDataLane lane, RoadNetworkDataGetter getter, LanePosition position)
        {
            RnDataWay way = lane.GetChildWay(getter, position);
            if (way?.LineString.IsValid == true)
                return getter.GetLineStrings()[way.LineString.ID];

            return null;
        }

        //Road
        public static List<RnDataLane> GetMainLanes([DisallowNull] this RnDataRoad road, RoadNetworkDataGetter getter)
        {
            List<RnDataLane> lanes = new();
            List<RnID<RnDataLane>> laneIds = road?.MainLanes;
            if (laneIds != null)
            {
                foreach (RnID<RnDataLane> id in laneIds)
                {
                    if (id.IsValid)
                        lanes.Add(getter.GetLanes()[id.ID]);
                }
            }
            return lanes;
        }

        public static RnDataLane GetMedianLanes([DisallowNull] this RnDataRoad road, RoadNetworkDataGetter getter)
        {
            List<RnDataLane> lanes = new();
            if (road.MedianLane.IsValid)
                return getter.GetLanes()[road.MedianLane.ID];
            return null;
        }

        public static RnDataWay GetChildWay([DisallowNull] this RnDataRoad road, RoadNetworkDataGetter getter, bool isMainLane, int laneIndex, LanePosition laneposition)
        {
            if (isMainLane)
            {
                List<RnDataLane> mainlanes = road.GetMainLanes(getter);
                if (mainlanes.Count > laneIndex)
                {
                    return road.GetMainLanes(getter)[laneIndex].GetChildWay(getter, laneposition);
                }
            }
            else
            {
                RnDataLane lane = road.GetMedianLanes(getter);
                return lane.GetChildWay(getter, laneposition);
            }

            return null;
        }
        public static RnDataRoadBase GetNextRoad([DisallowNull] this RnDataRoad road, RoadNetworkDataGetter getter)
        {
            if (road.Next.IsValid)
                return getter.GetRoadBases()[road.Next.ID];
            return null;
        }

        public static RnDataRoadBase GetPrevRoad([DisallowNull] this RnDataRoad road, RoadNetworkDataGetter getter)
        {
            if (road.Prev.IsValid)
                return getter.GetRoadBases()[road.Prev.ID];
            return null;
        }

        //Intersection
        public static List<RnDataTrack> GetFromTracks([DisallowNull] this RnDataIntersection intersection, RoadNetworkDataGetter getter, RnDataWay from)
        {
            return intersection.Tracks.FindAll(x => x.GetFromBorder(getter) == from).ToList();
        }

        public static List<RnDataTrack> GetToTracks([DisallowNull] this RnDataIntersection intersection, RoadNetworkDataGetter getter, RnDataWay to)
        {
            return intersection.Tracks.FindAll(x => x.GetToBorder(getter) == to).ToList();

        }

        //Track
        public static RnDataWay GetFromBorder([DisallowNull] this RnDataTrack track, RoadNetworkDataGetter getter)
        {
            return getter.GetWays()[track.FromBorder.ID];
        }

        public static RnDataWay GetToBorder([DisallowNull] this RnDataTrack track, RoadNetworkDataGetter getter)
        {
            return getter.GetWays()[track.ToBorder.ID];
        }

        public static RnDataLineString GetFromLineString([DisallowNull] this RnDataTrack track, RoadNetworkDataGetter getter)
        {
            var way = track.GetFromBorder(getter);
            return way.GetChildLineString(getter);
        }
        public static RnDataLineString GetToLineString([DisallowNull] this RnDataTrack track, RoadNetworkDataGetter getter)
        {
            var way = track.GetToBorder(getter);
            return way.GetChildLineString(getter);
        }

        //Edge / Neighbor
        public static RnDataNeighbor GetEdge([DisallowNull] this RnDataIntersection intersection, RoadNetworkDataGetter getter, RnDataWay to)
        {
            return intersection.Edges.Find(x => x.Border.ID == to.GetId(getter));
        }

        public static RnDataRoadBase GetRoad([DisallowNull] this RnDataNeighbor neighbor, RoadNetworkDataGetter getter)
        {
            if (neighbor.Road.IsValid)
                return getter.GetRoadBases()[neighbor.Road.ID];
            return null;
        }

    }
}
