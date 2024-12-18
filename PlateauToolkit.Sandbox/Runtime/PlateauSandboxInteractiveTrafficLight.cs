using AWSIM;
using PlateauToolkit.Sandbox.RoadNetwork;
using UnityEngine;
using static PlateauToolkit.Sandbox.RoadNetwork.TrafficLightData;

namespace PlateauToolkit.Sandbox
{
    /// <summary>
    /// 信号機アセット
    /// </summary>
    [SelectionBase]
    public class PlateauSandboxInteractiveTrafficLight : PlateauSandboxInteractive
    {

        [SerializeField] TrafficLightAssetBulbData[] m_TrafficLightAssetBulbData = TrafficLightData.GetDefaultTrafficLightAssetBulbData();

        [SerializeField] TrafficLight m_TrafficLightController;

        public TrafficLightAssetBulbData[] TrafficLightAssetBulbData => m_TrafficLightAssetBulbData;

        public void SetTrafficLight(TrafficLight trafficLight)
        {
            m_TrafficLightController = trafficLight;
        }
    }
}