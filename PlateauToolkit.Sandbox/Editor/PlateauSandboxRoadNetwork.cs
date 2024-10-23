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

        public void PlaceVehicles(List<GameObject> vehicles)
        {
            foreach (var vehicle in vehicles.Select((value, index) => new { value, index }))
            {
                Debug.Log($"place vehicle {vehicle.value.name}");

                (Vector3 pos, RnDataRoad road, RnDataLane lane) = GetRandomRoad(vehicle.index);
                PlateauSandboxInstantiation obj = InstantiateSelectedObject(vehicle.value, pos, Quaternion.identity);

                //IPlateauSandboxTrafficObject継承、PlateauSandboxTrafficMovementがアタッチされていない
                if (obj.SceneObject.TryGetComponent<IPlateauSandboxTrafficObject>(out _) &&
                    !obj.SceneObject.TryGetComponent<PlateauSandboxTrafficMovement>(out _))
                {
                    PlateauSandboxTrafficMovement trafficMovement = obj.SceneObject.AddComponent<PlateauSandboxTrafficMovement>();

                    var info = new RoadInfo();
                    info.m_RoadId = road.GetId(m_RoadNetworkGetter);
                    info.m_LaneIndex = road.GetLaneIndexOfMainLanes(m_RoadNetworkGetter, lane);
                    trafficMovement.RoadInfo = info;
                    trafficMovement.VehecleID = vehicle.index;
                }
            }
        }

        //暫定　ランダムにロードを抽出
        public (Vector3, RnDataRoad, RnDataLane) GetRandomRoad(int index)
        {
            var roadNetworkRoads = m_RoadNetworkGetter.GetRoadBases().OfType<RnDataRoad>().ToList();
            int randValue = Random.Range(0, roadNetworkRoads.Count());

            RnDataRoad outRoad = roadNetworkRoads[randValue];
            RnDataLane outlane = outRoad.GetMainLanes(m_RoadNetworkGetter).First();
            RnDataLineString outLinestring = outlane.GetChildLineString(m_RoadNetworkGetter, LanePosition.Center);
            Vector3 position = outLinestring.GetChildPointsVector(m_RoadNetworkGetter).FirstOrDefault();
            return (position, outRoad, outlane);
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
