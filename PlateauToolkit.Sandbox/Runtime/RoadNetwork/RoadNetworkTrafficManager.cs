using AWSIM.TrafficSimulation;
using PLATEAU.RoadNetwork.Data;
using PLATEAU.RoadNetwork.Structure;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PlateauToolkit.Sandbox.RoadNetwork
{
    /// <summary>
    /// Traffic Simulator / RoadNetwork 総合管理
    /// </summary>
    public class RoadNetworkTrafficManager : MonoBehaviour
    {
        RoadNetworkDataGetter m_RoadNetworkGetter;
        [SerializeField]
        List<GameObject> m_VehiclePrefabs;

        public TrafficManager SimTrafficManager
        {
            get
            {
                if (TryGetComponent<TrafficManager>(out TrafficManager tManager))
                {
                    return tManager;
                }

                return gameObject.AddComponent<TrafficManager>();
            }
        }

        public RoadNetworkDataGetter RnGetter
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

        public void SetPrefabs(List<GameObject> prefabs)
        {
            m_VehiclePrefabs = prefabs;
        }

        public int GetNumMaxVehicles()
        {
            int numRoads = RnGetter.GetRoadBases().OfType<RnDataRoad>().Count();
            return (int)Mathf.Min(numRoads / 2, RoadNetworkConstants.NUM_MAX_VEHICLES); // 交差点以外の道路数の半分 or 最大車輛数
        }

        public void CreateSimulator()
        {
            GameObject vehicles = GameObject.Find(RoadNetworkConstants.VEHICLE_ROOT_NAME);
            if (vehicles == null)
                vehicles = new GameObject(RoadNetworkConstants.VEHICLE_ROOT_NAME);

            SimTrafficManager.InitParams(LayerMask.NameToLayer(RoadNetworkConstants.LAYER_MASK_VEHICLE), LayerMask.NameToLayer(RoadNetworkConstants.LAYER_MASK_GROUND), GetNumMaxVehicles(), vehicles, true);

            List<TrafficLane> allLanes = new RoadNetworkLaneConverter().Create(RnGetter); //全て変換 (TrafficLane)

            //初期Spawn可能なTrafficLanes
            List<TrafficLane> spawnableLanes = allLanes.FindAll(x => !x.intersectionLane); //交差点以外

            //ReSpawn可能なTrafficLanes
            List<TrafficLane> respawnableLanes = allLanes.FindAll(x => !x.intersectionLane && x.rnRoad.Next.IsValid && !x.rnRoad.Prev.IsValid); //交差点以外 : Prevが空でNextが存在 (RoadNetworkで判定）

            spawnableLanes.RemoveAll(x => respawnableLanes.Contains(x)); //重複を除去

            //初期Spawn
            RandomTrafficSimulatorConfiguration randomTrafficSimConfigInitial = new RandomTrafficSimulatorConfiguration();
            randomTrafficSimConfigInitial.maximumSpawns = GetNumMaxVehicles(); // 初回用　Respawn禁止
            randomTrafficSimConfigInitial.npcPrefabs = m_VehiclePrefabs.ToArray();
            randomTrafficSimConfigInitial.spawnableLanes = spawnableLanes.ToArray();
            randomTrafficSimConfigInitial.enabled = true;

            RandomTrafficSimulatorConfiguration randomTrafficSimConfig = new RandomTrafficSimulatorConfiguration();
            randomTrafficSimConfig.maximumSpawns = 0; //0:Respawn可能
            randomTrafficSimConfig.npcPrefabs = m_VehiclePrefabs.ToArray();
            randomTrafficSimConfig.spawnableLanes = respawnableLanes.ToArray();
            randomTrafficSimConfig.enabled = true;
            SimTrafficManager.randomTrafficSims = new RandomTrafficSimulatorConfiguration[] { randomTrafficSimConfigInitial, randomTrafficSimConfig };

            SimTrafficManager.Initialize();
        }
    }
}
