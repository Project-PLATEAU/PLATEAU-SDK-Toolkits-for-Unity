using AWSIM.TrafficSimulation;
using PLATEAU.RoadNetwork.Data;
using PLATEAU.RoadNetwork.Structure;
using System.Collections.Generic;
using UnityEngine;

namespace PlateauToolkit.Sandbox.RoadNetwork
{
    //交通状況管理 (各道路の自動車）
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

        public void CreateSimulator()
        {

            GameObject vehicles = GameObject.Find(RoadNetworkConstants.VEHICLE_ROOT_NAME);
            if (vehicles == null)
                vehicles = new GameObject(RoadNetworkConstants.VEHICLE_ROOT_NAME);

            SimTrafficManager.InitParams(LayerMask.NameToLayer(RoadNetworkConstants.LAYER_MASK_VEHICLE), LayerMask.NameToLayer(RoadNetworkConstants.LAYER_MASK_GROUND), RoadNetworkConstants.NUM_MAX_VEHICLES, vehicles);

            List<TrafficLane> allLanes = new RoadNetworkLaneConverter().Create(RnGetter); //全て変換 (TrafficLane)

            //Spawn可能なTrafficLanes
            List<TrafficLane> spawnableLanes = allLanes.FindAll(x => !x.intersectionLane && x.NextLanes.Count > 0 && x.PrevLanes.Count <= 0); //交差点以外 : Prevが空でNextが存在

            RandomTrafficSimulatorConfiguration RandomTrafficSimConfig = new RandomTrafficSimulatorConfiguration();
            RandomTrafficSimConfig.maximumSpawns = 0; //0以外だとRespawnしなくなる
            RandomTrafficSimConfig.npcPrefabs = m_VehiclePrefabs.ToArray();
            RandomTrafficSimConfig.spawnableLanes = spawnableLanes.ToArray();
            RandomTrafficSimConfig.enabled = true;
            SimTrafficManager.randomTrafficSims = new RandomTrafficSimulatorConfiguration[] { RandomTrafficSimConfig };


            SimTrafficManager.Initialize();
        }
    }
}
