using PlateauToolkit.Sandbox.RoadNetwork;
using UnityEngine;
using UnityEngine.Rendering;
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

        public void CreateBulbDataAuto()
        {
            bool isHDRP = false;
            if (GraphicsSettings.defaultRenderPipeline != null)
            {
                if (QualitySettings.renderPipeline == null)
                {
                    isHDRP = GraphicsSettings.defaultRenderPipeline.name.Contains("HDRP");
                }
                else
                {
                    isHDRP = QualitySettings.renderPipeline.name.Contains("HDRP");
                }

                if (isHDRP)
                {
                    CreateHDRPBulbData();
                }
                else
                {
                    CreateURPBulbData();
                }
            }
        }

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