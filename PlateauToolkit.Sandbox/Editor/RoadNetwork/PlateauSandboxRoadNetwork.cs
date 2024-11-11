using PLATEAU.RoadNetwork.Data;
using PLATEAU.RoadNetwork.Structure;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using PlateauToolkit.Sandbox.RoadNetwork;
using static PlateauToolkit.Sandbox.RoadNetwork.RoadnetworkExtensions;
using AWSIM.TrafficSimulation;

namespace PlateauToolkit.Sandbox.Editor
{
    public class PlateauSandboxRoadNetwork
    {
        RoadNetworkDataGetter m_RoadNetworkGetter;
        RoadNetworkTrafficManager m_RnTrafficManager;
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


        //AWSIM用
        public void PlaceVehicles(List<GameObject> vehiclePrefabs)
        {
            m_RnTrafficManager.SetPrefabs(vehiclePrefabs);

            if(!Layers.LayerExists(RoadNetworkConstants.LAYER_MASK_VEHICLES))
                Layers.CreateLayer(RoadNetworkConstants.LAYER_MASK_VEHICLES);
            if (!Layers.LayerExists(RoadNetworkConstants.LAYER_MASK_GROUND))
                Layers.CreateLayer(RoadNetworkConstants.LAYER_MASK_GROUND);

            m_RnTrafficManager.CreateSimulator();
        }

        // 交通シミュレータ配置　実行時に呼ばれる
        public void Initialize()
        {
            PLATEAURnStructureModel roadNetwork = GameObject.FindObjectOfType<PLATEAURnStructureModel>();
            m_RoadNetworkGetter = roadNetwork.GetRoadNetworkDataGetter();

            //Component attach

            GameObject managerGo = GameObject.Find("TrafficManager");
            if (managerGo == null)
            {
                managerGo = new GameObject("TrafficManager");
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
