using PLATEAU.RoadNetwork.Data;
using PLATEAU.RoadNetwork.Structure;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using PlateauToolkit.Sandbox.RoadNetwork;
using static PlateauToolkit.Sandbox.RoadNetwork.RoadnetworkExtensions;

namespace PlateauToolkit.Sandbox.Editor
{
    public class PlateauSandboxRoadNetwork
    {
        RoadNetworkDataGetter m_RoadNetworkGetter;

        internal PlateauSandboxInstantiation InstantiateSelectedObject(GameObject obj, Vector3 position, Quaternion rotation, HideFlags? hideFlags = null)
        {
            string gameObjectName = GameObjectUtility.GetUniqueNameForSibling(null, obj.name);

            var gameObject = (GameObject)PrefabUtility.InstantiatePrefab(obj);
            gameObject.name = gameObjectName;
            gameObject.transform.rotation = rotation;

            if (gameObject.TryGetComponent(out IPlateauSandboxPlaceableObject placeable))
            {
                placeable.SetPosition(position);
            }
            else
            {
                gameObject.transform.position = position;
            }

            if (hideFlags != null)
            {
                gameObject.hideFlags = hideFlags.Value;
            }

            return new PlateauSandboxInstantiation(gameObject, obj);
        }

        public void PlaceVehicles(List<GameObject> vehicles)
        {
            foreach (var vehicle in vehicles.Select((value, index) => new { value, index }))
            {
                Debug.Log($"place vehicle {vehicle.value.name}");

                //(Vector3, Vector3, RnDataRoad, RnDataLineString) pos = GetPositionAndVector(vehicle.index);
                //(Vector3 pos, Vector3 vec, RnDataRoad road, RnDataLineString linestring) = GetPositionAndVector(vehicle.index);
                (Vector3 pos, RnDataRoad road, RnDataLineString linestring) = GetRandomRoad(vehicle.index);
                PlateauSandboxInstantiation obj = InstantiateSelectedObject(vehicle.value, pos, Quaternion.identity);

                //IPlateauSandboxTrafficObject継承、PlateauSandboxTrafficMovementがアタッチされていない
                if (obj.SceneObject.TryGetComponent<IPlateauSandboxTrafficObject>(out _) &&
                    !obj.SceneObject.TryGetComponent<PlateauSandboxTrafficMovement>(out _))
                {
                    PlateauSandboxTrafficMovement trafficMovement = obj.SceneObject.AddComponent<PlateauSandboxTrafficMovement>();

                    //trafficMovement.Road = road;
                    //trafficMovement.LineString = linestring;

                    // TODO: Lane情報　（暫定）
                    var info = new RaodInfo();
                    //info.m_RoadBase = road;
                    info.m_RoadId = road.GetId(m_RoadNetworkGetter);
                    info.m_LaneIndex = 0;
                    //var lane = road.GetMainLanes(m_RoadNetworkGetter)[info.m_LaneIndex];

                    trafficMovement.RoadInfo = info;
                }
            }
        }

        public (Vector3, RnDataRoad, RnDataLineString) GetRandomRoad(int index)
        {
            var roadNetworkRoads = m_RoadNetworkGetter.GetRoadBases().OfType<RnDataRoad>().ToList();
            int randValue = Random.Range(0, roadNetworkRoads.Count());

            RnDataRoad outRoad = roadNetworkRoads[randValue];
            RnDataLane lane = outRoad.GetMainLanes(m_RoadNetworkGetter).First();
            RnDataLineString outLinestring = lane.GetChildLineString(m_RoadNetworkGetter, lane.IsReverse ? LanePosition.Left : LanePosition.Right);
            Vector3 position = outLinestring.GetChildPointsVector(m_RoadNetworkGetter).FirstOrDefault();
            return (position, outRoad, outLinestring);
        }

        //public (Vector3, Vector3, RnDataRoad, RnDataLineString) GetPositionAndVector(int index)
        //{
        //    var roadNetworkRoads = m_RoadNetworkGetter.GetRoadBases();

        //    Vector3 position = Vector3.zero;

        //    RnDataRoad outRoad = null;
        //    RnDataLineString outLinestring = null;

        //    foreach (RnDataRoadBase roadbase in roadNetworkRoads)
        //    {
        //        if (roadbase is RnDataRoad)
        //        {
        //            RnDataRoad road = (RnDataRoad)roadbase;

        //            RnDataLane lane = road.GetMainLanes(m_RoadNetworkGetter).First();
        //            RnDataLineString linestring = lane.GetChildLineString(m_RoadNetworkGetter, lane.IsReverse ? LanePosition.Left : LanePosition.Right);
        //            if (linestring.Points.Count > index)
        //            {
        //                var points = linestring.GetChildPoints(m_RoadNetworkGetter);
        //                position = points[index].Vertex;

        //                outRoad = road;
        //                outLinestring = linestring;

        //                //Debug
        //                var roads = points[index].GetRoad(m_RoadNetworkGetter);
        //                foreach (var foundRoad in roads)
        //                {
        //                    Debug.Log($"point {index} road {foundRoad.TargetTran?.name}");
        //                }
        //                break;
        //            }
        //            else
        //            {
        //                index -= linestring.Points.Count;
        //            }
        //        }
        //    }

        //    return (position, Vector3.forward, outRoad, outLinestring);
        //}

        public void Initialize()
        {
            PLATEAURnStructureModel roadNetwork = GameObject.FindObjectOfType<PLATEAURnStructureModel>();
            m_RoadNetworkGetter = roadNetwork.GetRoadNetworkDataGetter();

            //var roadNetworkWays = roadNetworkGetter.GetWays();
            //var roadNetworkLanes = roadNetworkGetter.GetLanes();
            //var roadNetworkPoints = roadNetworkGetter.GetPoints();
            //var roadNetworkBlocks = roadNetworkGetter.GetBlocks();
            //var roadNetworkRoads = roadNetworkGetter.GetRoadBases();
            //var roadNetworkLineStrings = m_RoadNetworkGetter.GetLineStrings();
            //var roadNetworkNodes = roadNetworkRoads.OfType<RnDataIntersection>().ToList();
            //var roadNetworkLinks = roadNetworkRoads.OfType<RnDataRoad>().ToList();

            //Debug.Log($"roadNetworkNodes {roadNetworkNodes.Count}");
            //Debug.Log($"roadNetworkLinks {roadNetworkLinks.Count}");
            //Debug.Log($"roadNetworkWays {roadNetworkWays.Count}");
            //Debug.Log($"roadNetworkLanes {roadNetworkLanes.Count}");
            //Debug.Log($"roadNetworkPoints {roadNetworkPoints.Count}");
            //Debug.Log($"roadNetworkBlocks {roadNetworkBlocks.Count}");
            //Debug.Log($"roadNetworkLineStrings {roadNetworkLineStrings.Count}");

        }

    }
}
