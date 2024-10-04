using PLATEAU.RoadNetwork.Data;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Editor
{

    public static class RoadnetworkExtensions
    {
        //Point
        public static int GetId(this RnDataPoint point, RoadNetworkDataGetter getter)
        {
            List<RnDataPoint> points = getter.GetPoints() as List<RnDataPoint>;
            return points.FindIndex(x => x == point);
        }

        public static List<RnDataLineString> GetParentLineString(this RnDataPoint point, RoadNetworkDataGetter getter)
        {
            int index = point.GetId(getter);
            List<RnDataLineString> lines = getter.GetLineStrings() as List<RnDataLineString>;
            return lines.Where(x => x.Points.Any(y => y.ID == index)).ToList<RnDataLineString>();
        }

        public static List<RnDataRoad> GetRoad(this RnDataPoint point, RoadNetworkDataGetter getter)
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
        public static int GetId(this RnDataLineString linestring, RoadNetworkDataGetter getter)
        {
            List<RnDataLineString> lines = getter.GetLineStrings() as List<RnDataLineString>;
            return lines.FindIndex(x => x == linestring);
        }
        public static List<RnDataWay> GetParentWays(this RnDataLineString linestring, RoadNetworkDataGetter getter)
        {
            int index = linestring.GetId(getter);
            var ways = getter.GetWays() as List<RnDataWay>;
            return ways.Where(x => x.LineString.ID == index).ToList<RnDataWay>();
        }

        public static List<RnDataRoad> GetRoad(this RnDataLineString linestring, RoadNetworkDataGetter getter)
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

        public static List<RnDataPoint> GetChildPoints(this RnDataLineString linestring, RoadNetworkDataGetter getter )
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

        public static List<Vector3> GetChildPointsVector(this RnDataLineString linestring, RoadNetworkDataGetter getter)
        {
            List<Vector3> points = new();
            foreach (var pid in linestring.Points)
            {
                if (!pid.IsValid)
                    continue;

                points.Add(getter.GetPoints()[pid.ID].Vertex);
            }

            return points;
        }

        //Way
        public static int GetId(this RnDataWay way, RoadNetworkDataGetter getter )
        {
            List<RnDataWay> ways = getter.GetWays() as List<RnDataWay>;
            return ways.FindIndex(x => x == way);
        }
        public static List<RnDataLane> GetParentLanes(this RnDataWay way, RoadNetworkDataGetter getter)
        {
            int index = way.GetId(getter);
            var lanes = getter.GetLanes() as List<RnDataLane>;
            return lanes.Where(x => x.LeftWay.ID == index || x.RightWay.ID == index || x.CenterWay.ID == index).ToList<RnDataLane>();
        }

        //Lane
        public static int GetId(this RnDataLane lane, RoadNetworkDataGetter getter)
        {
            List<RnDataLane> ways = getter.GetLanes() as List<RnDataLane>;
            return ways.FindIndex(x => x == lane);
        }

        public static List<RnDataRoad> GetParentRoads(this RnDataLane lane, RoadNetworkDataGetter getter)
        {
            int index = lane.GetId(getter);
            var roadbases = getter.GetRoadBases() as List<RnDataRoadBase>;
            var roads = roadbases.OfType<RnDataRoad>().ToList();
            return roads.Where(x => x.MainLanes.Any(y => y.ID == index)).ToList<RnDataRoad>();
        }

        public static RnDataWay GetChildWay(this RnDataLane lane, RoadNetworkDataGetter getter, int position) //position 0:center , 1:left, 2:right
        {
            RnID<RnDataWay> wayId = lane.CenterWay;
            switch (position)
            {
                case 0:
                    wayId = lane.CenterWay;
                    break;
                case 1:
                    wayId = lane.LeftWay;
                    break;
                case 2:
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


        public static RnDataLineString GetChildLineString(this RnDataLane lane, RoadNetworkDataGetter getter, int position) //position 0:center , 1:left, 2:right
        {
            //RnID<RnDataWay> wayId = lane.CenterWay;
            //switch (position)
            //{
            //    case 0:
            //        wayId = lane.CenterWay;
            //        break;
            //    case 1:
            //        wayId = lane.LeftWay;
            //        break;
            //    case 2:
            //        wayId = lane.RightWay;
            //        break;
            //}

            //if (wayId.IsValid)
            //{
            //    int index = wayId.ID;
            //    RnDataWay way = getter.GetWays()[index];

            //    if (way.LineString.IsValid)
            //        return getter.GetLineStrings()[way.LineString.ID];
            //}

            RnDataWay way = lane.GetChildWay(getter, position);
            if (way.LineString.IsValid)
                return getter.GetLineStrings()[way.LineString.ID];

            return null;
        }

        //Road
        public static List<RnDataLane> GetMainLanes(this RnDataRoad road, RoadNetworkDataGetter getter)
        {
            List<RnDataLane> lanes = new();
            List<RnID<RnDataLane>> laneIds = road.MainLanes;
            foreach (RnID<RnDataLane> id in laneIds)
            {
                if(id.IsValid)
                    lanes.Add(getter.GetLanes()[id.ID]);
            }
            return lanes;
        }

        public static RnDataLane GetMedianLanes(this RnDataRoad road, RoadNetworkDataGetter getter)
        {
            List<RnDataLane> lanes = new();
            if (road.MedianLane.IsValid)
                return getter.GetLanes()[road.MedianLane.ID];
            return null;
        }

        ////bottomup search
        //public static List<RnDataLineString> GetLineStringsFromPointId(this RoadNetworkDataGetter getter, int index)
        //{
        //    List<RnDataLineString> lines = getter.GetLineStrings() as List<RnDataLineString>;
        //    return lines.Where(x => x.Points.Any(y => y.ID == index)).ToList<RnDataLineString>();
        //}

        //public static List<RnDataWay> GetWaysFromLineStringId(this RoadNetworkDataGetter getter, int index)
        //{
        //    var ways = getter.GetWays() as List<RnDataWay>;
        //    return ways.Where(x => x.LineString.ID == index).ToList<RnDataWay>();
        //}

        //public static List<RnDataLane> GetLanesFromWayId(this RoadNetworkDataGetter getter, int index)
        //{
        //    var lanes = getter.GetLanes() as List<RnDataLane>;
        //    return lanes.Where(x => x.LeftWay.ID == index || x.RightWay.ID == index || x.CenterWay.ID == index).ToList<RnDataLane>();
        //}

        //public static List<RnDataRoad> GetRoadsFromLaneId(this RoadNetworkDataGetter getter, int index)
        //{
        //    var roadbases = getter.GetRoadBases() as List<RnDataRoadBase>;
        //    var roads = roadbases.OfType<RnDataRoad>().ToList();
        //    return roads.Where(x => x.MainLanes.Any(y => y.ID == index)).ToList<RnDataRoad>();
        //}

        ////topdwon search
        //public static List<RnDataLane> GetMainLanesFromRoad(this RoadNetworkDataGetter getter, RnDataRoad road)
        //{
        //    List<RnDataLane> lanes = new();
        //    List<RnID<RnDataLane>> laneIds = road.MainLanes;
        //    foreach (RnID<RnDataLane> id in laneIds)
        //    {
        //        lanes.Add(getter.GetLanes()[id.ID]);
        //    }
        //    return lanes;
        //}

        //public static RnDataLineString GetLineStringFromLane(this RoadNetworkDataGetter getter, RnDataLane lane, int position) //position 0:center , 1:left, 2:right
        //{
        //    RnID<RnDataWay> wayId = lane.CenterWay;
        //    switch (position)
        //    {
        //        case 0:
        //            wayId = lane.CenterWay;
        //            break;
        //        case 1:
        //            wayId = lane.LeftWay;
        //            break;
        //        case 2:
        //            wayId = lane.RightWay;
        //            break;
        //    }

        //    if (wayId.IsValid)
        //    {
        //        int index = wayId.ID;
        //        RnDataWay way = getter.GetWays()[index];

        //        if (way.LineString.IsValid)
        //            return getter.GetLineStrings()[way.LineString.ID];
        //    }

        //    return null;
        //}

        //public static List<Vector3> GetPointsFromLineString(this RoadNetworkDataGetter getter, RnDataLineString linestring)
        //{
        //    List<Vector3> points = new();
        //    foreach (var pid in linestring.Points)
        //    {
        //        if (!pid.IsValid)
        //            continue;

        //        points.Add(getter.GetPoints()[pid.ID].Vertex);
        //    }

        //    return points;
        //}


        ////Get Index
        //public static int GetPointId(this RoadNetworkDataGetter getter, RnDataPoint point)
        //{
        //    List<RnDataPoint> points = getter.GetPoints() as List<RnDataPoint>;
        //    return points.FindIndex(x => x == point);
        //}

        //public static int GetLineStringId(this RoadNetworkDataGetter getter, RnDataLineString linestring)
        //{
        //    List<RnDataLineString> lines = getter.GetLineStrings() as List<RnDataLineString>;
        //    return lines.FindIndex(x => x == linestring);
        //}

        //public static int GetWayId(this RoadNetworkDataGetter getter, RnDataWay way)
        //{
        //    List<RnDataWay> ways = getter.GetWays() as List<RnDataWay>;
        //    return ways.FindIndex(x => x == way);
        //}

        //public static int GetLaneId(this RoadNetworkDataGetter getter, RnDataLane lane)
        //{
        //    List<RnDataLane> ways = getter.GetLanes() as List<RnDataLane>;
        //    return ways.FindIndex(x => x == lane);
        //}

        //public static int GetRoadId(this RoadNetworkDataGetter getter, RnDataRoadBase road)
        //{
        //    List<RnDataRoadBase> roads = getter.GetRoadBases() as List<RnDataRoadBase>;
        //    return roads.FindIndex(x => x == road);
        //}
    }
}
