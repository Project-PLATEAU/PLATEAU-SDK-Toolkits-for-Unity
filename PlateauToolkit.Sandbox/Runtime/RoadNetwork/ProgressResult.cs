using PLATEAU.RoadNetwork.Data;
using PLATEAU.RoadNetwork.Structure;
using UnityEngine;
using System.Linq;

namespace PlateauToolkit.Sandbox.RoadNetwork
{
    public struct ProgressResult
    {
        public float m_Speed;

        public float m_Distance_from_Other;

        public ProgressResult(RoadNetworkTrafficController controller, TrafficManager.LaneStatus status, RoadNetworkDataGetter getter)
        {
            m_Distance_from_Other = -1f;

            if (status.m_IsValid)
            {
                //if(status.m_NumVehiclesOnTheLane > 0)
                if (status.m_NumVehiclesForward > 0)
                {
                    //var boundsOffset = controller.m_Distance / Mathf.Abs(Vector3.Distance(controller.m_RoadInfo.m_Bounds.max, controller.m_RoadInfo.m_Bounds.center));
                    //var currentProgress = controller.m_RoadInfo.m_CurrentProgress - boundsOffset;
                    //var currentProgress = controller.m_RoadInfo.m_CurrentProgress;
                    //m_Distance_from_Other = (status.m_LastCarProgress - currentProgress) * controller.m_Distance; // * controller.m_Distance;
                    //var distance_from_start = controller.m_RoadInfo.m_CurrentProgress * controller.m_Distance;

                    if (status.m_DistanceBetweenLastCar < 10f)
                    {
                        m_Speed = 0f; //適当なスピード
                    }
                    else if (status.m_DistanceBetweenLastCar < 20f) //適当な差 
                    {
                        m_Speed = 10f;
                    }
                    else if (status.m_DistanceFromFirstPoint < 1f) //侵入したて
                    {
                        m_Speed = 5f;
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
                //T字路
                if (controller.m_Intersection?.GetAllConnectedRoads(getter).Count == 3)
                {
                    //直線トラックがない場合は待機
                    var straightTrack = controller.m_Intersection.GetTraksOfSameOriginByType(getter, controller.GetTrack(), RnTurnType.Straight)?.FirstOrDefault();
                    if (straightTrack == null && status.m_NumVehiclesCrossing > 0)
                    {
                        //m_Speed = 2f;
                    }
                }
                else if (controller.m_Intersection?.GetAllConnectedRoads(getter).Count == 4)
                {
                    var straightTrack = controller.m_Intersection.GetTraksOfSameOriginByType(getter, controller.GetTrack(), RnTurnType.Straight)?.FirstOrDefault();
                    if (straightTrack != null && status.m_NumVehiclesCrossing > 0 && !status.m_IsPriorityTrack)
                    {
                        //m_Speed = 5f;
                    }
                    else if (controller.GetTrack().TurnType == RnTurnType.RightTurn && straightTrack != null && status.m_NumVehiclesOncominglane > 0)
                    {
                        //m_Speed = 3f;
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
