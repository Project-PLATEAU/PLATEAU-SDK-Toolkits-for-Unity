using PLATEAU.RoadNetwork.Data;
using PLATEAU.RoadNetwork.Structure;
using PlateauToolkit.Sandbox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



//åí èÛãµä«óù
public class TrafficManager : MonoBehaviour
{

    [HideInInspector][SerializeField] RoadNetworkDataGetter m_RoadNetworkGetter;

    RoadNetworkDataGetter RoadNetworkGetter
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

    [SerializeField] List<RnDataRoadBase> m_RoadBases;
    [SerializeField] List<PlateauSandboxTrafficMovement> m_Vehicles;

    void Start()
    {
        m_RoadBases = RoadNetworkGetter.GetRoadBases() as List<RnDataRoadBase>;
        m_Vehicles = new List<PlateauSandboxTrafficMovement>(GameObject.FindObjectsByType<PlateauSandboxTrafficMovement>(FindObjectsSortMode.None));
    }








}
