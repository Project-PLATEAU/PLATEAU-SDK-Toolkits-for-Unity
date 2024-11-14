using PLATEAU.RoadNetwork.Data;
using PLATEAU.RoadNetwork.Structure;
using System.Collections.Generic;
using UnityEngine;
using PlateauToolkit.Sandbox.RoadNetwork;
using AWSIM.TrafficSimulation;
using UnityEditor;

namespace PlateauToolkit.Sandbox.Editor
{
    public class PlateauSandboxRoadNetwork
    {
        RoadNetworkDataGetter m_RoadNetworkGetter;
        RoadNetworkTrafficManager m_RnTrafficManager;
        TrafficManager m_TrafficManager;

        public void ClearTrafficManager()
        {
            var manager = GameObject.Find(RoadNetworkConstants.TRAFFIC_MANAGER_NAME);
            if (manager != null)
            {
                GameObject.DestroyImmediate(manager);
            }
            var vehicles = GameObject.Find(RoadNetworkConstants.VEHICLE_ROOT_NAME);
            if (vehicles != null)
            {
                GameObject.DestroyImmediate(vehicles);
            }
            var trafficLanes = GameObject.Find(RoadNetworkConstants.TRAFFIC_LANE_ROOT_NAME);
            if (trafficLanes != null)
            {
                GameObject.DestroyImmediate(trafficLanes);
            }
            var stoplines = GameObject.Find(RoadNetworkConstants.STOPLINE_ROOT_NAME);
            if (stoplines != null)
            {
                GameObject.DestroyImmediate(stoplines);
            }
        }

        //AWSIM用
        public bool PlaceVehicles(List<GameObject> vehiclePrefabs)
        {
            if (!Layers.LayerExists(RoadNetworkConstants.LAYER_MASK_VEHICLE))
                Layers.CreateLayer(RoadNetworkConstants.LAYER_MASK_VEHICLE);
            if (!Layers.LayerExists(RoadNetworkConstants.LAYER_MASK_GROUND))
                Layers.CreateLayer(RoadNetworkConstants.LAYER_MASK_GROUND);

            try
            {
                Initialize();
                m_RnTrafficManager.SetPrefabs(vehiclePrefabs);
                m_RnTrafficManager.CreateSimulator();

                var lanes = GameObject.Find(RoadNetworkConstants.TRAFFIC_LANE_ROOT_NAME);
                for (int i = 0;  i < lanes.transform.childCount; i++)
                {
                    TrafficLaneEditor.FindAndSetRightOfWays(lanes.transform.GetChild(i).GetComponent<TrafficLane>());
                }
            }
            catch(System.Exception ex)
            {
                Debug.LogException(ex);
                EditorUtility.DisplayDialog("アセットの配置に失敗しました。", ex.Message, "OK");
                return false;
            }

            EditorUtility.DisplayDialog("成功", $"交通シミュレータが配置されました。\n{vehiclePrefabs.Count}種類のアセットが追加されました。", "OK");
            return true;
        }

        // 交通シミュレータ配置　実行時に呼ばれる
        public void Initialize()
        {
            ClearTrafficManager();

            PLATEAURnStructureModel roadNetwork = GameObject.FindObjectOfType<PLATEAURnStructureModel>();
            if (roadNetwork == null)
            {
                throw new System.Exception("道路ネットワークが見つかりませんでした。");
            }
            m_RoadNetworkGetter = roadNetwork.GetRoadNetworkDataGetter();

            //Component attach

            GameObject managerGo = GameObject.Find(RoadNetworkConstants.TRAFFIC_MANAGER_NAME);
            if (managerGo == null)
            {
                managerGo = new GameObject(RoadNetworkConstants.TRAFFIC_MANAGER_NAME);
            }

            m_RnTrafficManager = managerGo.GetComponent<RoadNetworkTrafficManager>();
            if (m_RnTrafficManager == null)
            {
                m_RnTrafficManager = managerGo.AddComponent<RoadNetworkTrafficManager>();
            }

            m_TrafficManager = managerGo.GetComponent<TrafficManager>();
            if (m_TrafficManager == null)
            {
                m_TrafficManager = managerGo.AddComponent<TrafficManager>();
            }
        }
    }
}
