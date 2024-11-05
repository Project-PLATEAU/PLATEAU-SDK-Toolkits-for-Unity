using AWSIM.TrafficSimulation;
using PLATEAU.RoadNetwork.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using PLATEAU.RoadNetwork.Data;
using PLATEAU.RoadNetwork.Structure;

namespace PlateauToolkit.Sandbox.RoadNetwork
{
    public class TrafficRouteCreator
    {

        RoadNetworkTrafficController m_Controller;

        public TrafficRouteCreator(RoadNetworkTrafficController controller)
        {
            m_Controller = controller;
        }



        TrafficLane.TurnDirectionType ConvertTurnType(RnTurnType turnType)
        {
            if (turnType == RnTurnType.LeftTurn || turnType == RnTurnType.LeftFront || turnType == RnTurnType.LeftBack)
            {
                return TrafficLane.TurnDirectionType.LEFT;
            }

            if (turnType == RnTurnType.RightTurn || turnType == RnTurnType.RightFront || turnType == RnTurnType.RightBack)
            {
                return TrafficLane.TurnDirectionType.RIGHT;
            }

            return TrafficLane.TurnDirectionType.STRAIGHT;
        }

        public List<TrafficLane> Create()
        {
            List<TrafficLane> route = new();

            var getter = m_Controller.RnGetter;

            var currentController = m_Controller;

            int laneIndex = 0;

            var parent = new GameObject("TrafficLanes");

            while (currentController != null)
            {
                if (currentController.IsRoad)
                {
                    var points = currentController.GetLineString().GetChildPointsVector(getter);
                    if (points.Count > 0)
                    {
                        TrafficLane lane = TrafficLane.Create($"TrafficLane_Road_{laneIndex}", parent.transform, points.ToArray(), TrafficLane.TurnDirectionType.STRAIGHT);
                        route.Add(lane);
                    }
                }
                else if (currentController.IsIntersection)
                {
                    var track = currentController.GetTrack();
                    var knotsPosistions = track.Spline.Knots.Select(x => (Vector3)x.Position).ToList();
                    if (knotsPosistions.Count > 0)
                    {
                        TrafficLane.TurnDirectionType turnDirType = ConvertTurnType(track.TurnType);
                        TrafficLane lane = TrafficLane.Create($"TrafficLane_Intersection_{laneIndex}", parent.transform, knotsPosistions.ToArray(), TrafficLane.TurnDirectionType.STRAIGHT);
                        route.Add(lane);
                    }
                }

                laneIndex++;
                currentController = currentController.GetNextRoad();
            }


            return route;
        }

    }
}
