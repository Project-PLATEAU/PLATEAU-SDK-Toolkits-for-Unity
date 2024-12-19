using PlateauToolkit.Sandbox.RoadNetwork;
using UnityEngine;
using static PlateauToolkit.Sandbox.RoadNetwork.TrafficLightData;

namespace PlateauToolkit.Sandbox
{
    /// <summary>
    /// 信号機アセット
    /// </summary>
    [SelectionBase]
    public class PlateauSandboxInteractiveTrafficLight : PlateauSandboxPlaceableHandler
    {

        [SerializeField] TrafficLightAssetBulbData[] m_TrafficLightAssetBulbData;

        public TrafficLightAssetBulbData[] TrafficLightAssetBulbData => m_TrafficLightAssetBulbData;


        [ContextMenu("Create default URP Bulb Data")]
        void CreateURPBulbData()
        {
            m_TrafficLightAssetBulbData = TrafficLightData.GetDefaultTrafficLightAssetBulbData(false);
        }

        [ContextMenu("Create default HDRP Bulb Data")]
        void CreateHDRPBulbData()
        {
            m_TrafficLightAssetBulbData = TrafficLightData.GetDefaultTrafficLightAssetBulbData(true);
        }
    }
}