using PLATEAU.RoadNetwork.Data;
using PLATEAU.RoadNetwork.Structure;
using UnityEngine;

namespace PlateauToolkit.Sandbox.RoadNetwork
{
    public struct ProgressResult
    {
        public float m_Speed;

        public ProgressResult(RoadNetworkTrafficController controller, TrafficManager.LaneStatus status, RoadNetworkDataGetter getter)
        {
            if (status.m_NumVehicles > 0)
            {
                if (status.m_NumVehiclesForward > 0)
                {
                    var boundsOffset = controller.m_Distance / Mathf.Abs(Vector3.Distance(controller.m_Bounds.max, controller.m_Bounds.center));
                    var currentProgress = controller.m_CurrentProgress - boundsOffset;

                    if ((status.m_LastCarProgress - currentProgress) * controller.m_Distance < 50f)
                    {
                        m_Speed = 3f;
                    }
                    else if ((controller.m_CurrentProgress - currentProgress) * controller.m_Distance < 100f) //適当な差 
                    {
                        m_Speed = controller.IsRoad ? 15f : 10f; //適当なスピード
                    }
                    else
                    {
                        m_Speed = controller.IsRoad ? 30f : 20f; //適当なスピード
                    }
                }
                else
                {
                    m_Speed = controller.IsRoad ? 35f : 30f; //適当なスピード
                }

                //intersection
                //T字路 (行くまで待機）
                if (controller.m_Intersection?.GetAllConnectedRoads(getter).Count == 3)
                {
                    //Debug.LogWarning($"T字路 {info.m_NumVehiclesCrossing}");
                    //if (info.m_NumVehiclesOncominglane > 0 || info.m_NumVehiclesCrossing > 0)
                    if (controller.GetTrack().TurnType != RnTurnType.Straight && status.m_NumVehiclesCrossing > 0)
                    {
                        //result.m_Speed = 0f;
                    }
                }
            }
            else
            {
                m_Speed = controller.IsRoad ? 40f : 30f; //適当なスピード
            }
        }
    }
}
