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
        public static readonly int k_PLACEMENT_MODE = 0; // (0:各Prefab, 1:各道路)
        public static readonly int k_NUM_VEHICLE_ON_PLACE = 1;

        RoadNetworkDataGetter m_RoadNetworkGetter;
        TrafficManager m_TrafficManager;

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

        public void PlaceVehicles(List<GameObject> vehiclePrefabs)
        {
            var vehicles = new List<GameObject>();

            //Prefab 単位
            if (k_PLACEMENT_MODE == 0)
            {
                foreach (var vehiclePrefab in vehiclePrefabs.Select((value, index) => new { value, index }))
                {
                    Debug.Log($"place vehicle {vehiclePrefab.value.name}");

                    (Vector3 pos, RnDataRoad road, RnDataLane lane) = m_TrafficManager.GetRandomRoad();
                    PlateauSandboxInstantiation obj = InstantiateSelectedObject(vehiclePrefab.value, pos, Quaternion.identity);

                    //IPlateauSandboxTrafficObject継承、PlateauSandboxTrafficMovementがアタッチされていない
                    if (obj.SceneObject.TryGetComponent<IPlateauSandboxTrafficObject>(out _) &&
                        !obj.SceneObject.TryGetComponent<PlateauSandboxTrafficMovement>(out _))
                    {
                        PlateauSandboxTrafficMovement trafficMovement = obj.SceneObject.AddComponent<PlateauSandboxTrafficMovement>();
                        trafficMovement.RoadInfo = new RoadInfo(
                            road.GetId(m_RoadNetworkGetter),
                            road.GetLaneIndexOfMainLanes(m_RoadNetworkGetter, lane));

                        vehicles.Add(obj.SceneObject);
                    }
                }
            }

            //各道路にPrefabを置く
            else if (k_PLACEMENT_MODE == 1)
            {
                int numVehelcesPerRoad = k_NUM_VEHICLE_ON_PLACE;
                int prefabIndex = 0;
                var roadNetworkRoads = m_RoadNetworkGetter.GetRoadBases().OfType<RnDataRoad>().ToList();

                foreach (var road in roadNetworkRoads)
                {
                    for (int i = 0; i < numVehelcesPerRoad; i++)
                    {
                        var vehiclePrefab = vehiclePrefabs[prefabIndex];
                        prefabIndex++;
                        if (prefabIndex >= vehiclePrefabs.Count)
                        {
                            prefabIndex = 0;
                        }

                        RnDataLane lane = road.GetMainLanes(m_RoadNetworkGetter).First();
                        RnDataLineString linestring = lane.GetChildLineString(m_RoadNetworkGetter, LanePosition.Center);
                        Vector3 position = linestring.GetChildPointsVector(m_RoadNetworkGetter).FirstOrDefault();

                        PlateauSandboxInstantiation obj = InstantiateSelectedObject(vehiclePrefab, position, Quaternion.identity);
                        //IPlateauSandboxTrafficObject継承、PlateauSandboxTrafficMovementがアタッチされていない
                        if (obj.SceneObject.TryGetComponent<IPlateauSandboxTrafficObject>(out _) &&
                            !obj.SceneObject.TryGetComponent<PlateauSandboxTrafficMovement>(out _))
                        {
                            PlateauSandboxTrafficMovement trafficMovement = obj.SceneObject.AddComponent<PlateauSandboxTrafficMovement>();
                            trafficMovement.RoadInfo = new RoadInfo(
                                road.GetId(m_RoadNetworkGetter),
                                road.GetLaneIndexOfMainLanes(m_RoadNetworkGetter, lane));

                            vehicles.Add(obj.SceneObject);
                        }
                    }
                }
            }

            m_TrafficManager.InitializeVehicles();
            //重なった自動車判定
            foreach (var vehicle in vehicles)
            {
                if(vehicle.TryGetComponent<PlateauSandboxTrafficMovement>(out var trafficMovement))
                {
                    var result = m_TrafficManager.GetLaneInfo(trafficMovement.RoadInfo);
                    if (result.m_NumVehiclesOnTheLane > 0)
                    {
                        //trafficMovement.m_StartOffset = result.m_NumVehiclesOnTheLane * 0.2f; // TODO: 距離で
                    }
                }
            }

        }

        // 交通シミュレータ配置　実行時に呼ばれる
        public void Initialize()
        {
            PLATEAURnStructureModel roadNetwork = GameObject.FindObjectOfType<PLATEAURnStructureModel>();
            m_RoadNetworkGetter = roadNetwork.GetRoadNetworkDataGetter();

            //Component attach
            m_TrafficManager = roadNetwork.gameObject.GetComponent<TrafficManager>();
            if (m_TrafficManager == null)
            {
                m_TrafficManager = roadNetwork.gameObject.AddComponent<TrafficManager>();
            }
        }

    }
}
