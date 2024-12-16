using AWSIM;
using UnityEngine;

namespace PlateauToolkit.Sandbox
{
    /// <summary>
    /// The definition of a StreetFurniture
    /// </summary>
    [SelectionBase]
    public class PlateauSandboxInteractive : PlateauSandboxPlaceableHandler
    {
    }

    /// <summary>
    /// 信号機アセット
    /// </summary>
    public class PlateauSandboxInteractiveTrafficLight : MonoBehaviour
    {

        [SerializeField] TrafficLight m_TrafficLight;

        public void SetTrafficLight(TrafficLight trafficLight)
        {
            m_TrafficLight = trafficLight;
        }
    }
}