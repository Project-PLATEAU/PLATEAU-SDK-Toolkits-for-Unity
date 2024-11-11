using AWSIM.TrafficSimulation;
using PLATEAU.RoadNetwork.Data;
using PLATEAU.RoadNetwork.Structure;
using PLATEAU.Util;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Splines;
using static PlateauToolkit.Sandbox.RoadNetwork.RoadnetworkExtensions;

namespace PlateauToolkit.Sandbox.RoadNetwork
{
    //交通状況管理 (各道路の自動車）
    public class RoadNetworkTrafficManager : MonoBehaviour
    {

        public static readonly int NUM_MAX_VEHICLES = 60;

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

            GameObject vehicles = GameObject.Find("Vehicles");
            if (vehicles == null)
                vehicles = new GameObject("Vehicles");

            SimTrafficManager.InitParams(LayerMask.NameToLayer(RoadNetworkConstants.LAYER_MASK_VEHICLES), LayerMask.NameToLayer(RoadNetworkConstants.LAYER_MASK_GROUND), NUM_MAX_VEHICLES, vehicles);

            List<TrafficLane> allLanes = new RoadNetworkLaneConverter().Create(RnGetter); //全て変換 (TrafficLane)

            RandomTrafficSimulatorConfiguration RandomTrafficSimConfig = new RandomTrafficSimulatorConfiguration();
            RandomTrafficSimConfig.maximumSpawns = 0; //0以外だとRespawnしなくなる
            RandomTrafficSimConfig.npcPrefabs = m_VehiclePrefabs.ToArray();
            RandomTrafficSimConfig.spawnableLanes = allLanes.FindAll(x => !x.intersectionLane).ToArray(); //交差点以外
            RandomTrafficSimConfig.enabled = true;
            SimTrafficManager.randomTrafficSims = new RandomTrafficSimulatorConfiguration[] { RandomTrafficSimConfig };

            SimTrafficManager.Initialize();
        }
    }
}
