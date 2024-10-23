using PLATEAU.RoadNetwork.Data;
using PLATEAU.RoadNetwork.Structure;
using PlateauToolkit.Sandbox;
using PlateauToolkit.Sandbox.RoadNetwork;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlateauToolkit.Sandbox.RoadNetwork
{

    public class RoadStatus
    {
        public List<int> m_Vehecles = new List<int>();
    }


    //交通状況管理
    public class TrafficManager : MonoBehaviour
    {
        [SerializeField] List<RnDataRoadBase> m_RoadBases;
        [SerializeField] List<PlateauSandboxTrafficMovement> m_Vehicles;

        RoadNetworkDataGetter m_RoadNetworkGetter;

        Dictionary<int,RoadStatus> RoadStatuses = new Dictionary<int,RoadStatus>();

        RoadNetworkDataGetter RnGetter
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

        void Start()
        {
            m_RoadBases = RnGetter.GetRoadBases() as List<RnDataRoadBase>;
            m_Vehicles = new List<PlateauSandboxTrafficMovement>(GameObject.FindObjectsByType<PlateauSandboxTrafficMovement>(FindObjectsSortMode.None));
        }

        public void SetRoadInfo(RoadInfo info)
        {
            if(RoadStatuses.TryGetValue(info.m_RoadId, out var value))
            {
                if (!value.m_Vehecles.Contains(info.m_VehecleID))
                {
                    value.m_Vehecles.Add(info.m_VehecleID);
                }
            } 
            else
            {
                RoadStatus stat = new RoadStatus();
                RoadStatuses.Add(info.m_RoadId, stat);
            }
        }

    }
}
