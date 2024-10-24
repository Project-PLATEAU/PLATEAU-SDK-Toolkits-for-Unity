using PLATEAU.RoadNetwork.Data;
using PLATEAU.RoadNetwork.Structure;
using PlateauToolkit.Sandbox;
using PlateauToolkit.Sandbox.RoadNetwork;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static PlateauToolkit.Sandbox.RoadNetwork.RoadnetworkExtensions;

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

        void Start()
        {
            m_RoadBases = RnGetter.GetRoadBases() as List<RnDataRoadBase>;
            m_Vehicles = new List<PlateauSandboxTrafficMovement>(GameObject.FindObjectsByType<PlateauSandboxTrafficMovement>(FindObjectsSortMode.None));
        }

        //暫定　ランダムにロードを抽出
        public (Vector3, RnDataRoad, RnDataLane) GetRandomRoad()
        {
            var roadNetworkRoads = RnGetter.GetRoadBases().OfType<RnDataRoad>().ToList();
            int randValue = Random.Range(0, roadNetworkRoads.Count());

            RnDataRoad outRoad = roadNetworkRoads[randValue];
            RnDataLane outlane = outRoad.GetMainLanes(m_RoadNetworkGetter).First();
            RnDataLineString outLinestring = outlane.GetChildLineString(m_RoadNetworkGetter, LanePosition.Center);
            Vector3 position = outLinestring.GetChildPointsVector(m_RoadNetworkGetter).FirstOrDefault();
            return (position, outRoad, outlane);
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
