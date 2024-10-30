using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using PLATEAU.RoadNetwork.Data;
using System.Linq;
using PLATEAU.RoadNetwork.Structure;
using UnityEngine.InputSystem.XR;
using static Codice.Client.Common.Servers.RecentlyUsedServers;



#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PlateauToolkit.Sandbox.RoadNetwork
{
    public class GizmoUtil
    {
        public static void DrawSpline(Spline spline, Color color)
        {
            Gizmos.color = color;
            Vector3 lastpos = Vector3.zero;
            for (int i = 0; i < 100; i++)
            {
                float percent = i * 0.01f;
                Vector3 pos = SplineTool.GetPointOnSpline(spline, percent);
                if (lastpos == Vector3.zero)
                {
                    lastpos = pos;
                }

                Gizmos.DrawLine(pos, lastpos);
                lastpos = pos;
            }
        }

        public static void DrawLine(List<Vector3> points, Color color)
        {
            Gizmos.color = color;
            for (int j = 0; j < points.Count - 1; j++)
            {
                Gizmos.DrawLine(points[j], points[j + 1]);
            }
        }

        public static void DrawLabel(Vector3 position, string str, Color color)
        {
#if UNITY_EDITOR
            GUIStyle style = new GUIStyle();
            style.normal.textColor = color;
            Handles.Label(position, str, style);
#endif
        }

        public static void DebugVehicle(PlateauSandboxTrafficMovement vehicle)
        {

        }

        public static void DebugRoadNetwork(RoadNetworkTrafficController cont, RoadNetworkDataGetter getter)
        {
            if (cont.IsRoad)
            {
                //var points = cont.GetLineString().GetChildPointsVector(getter);
                //GizmoUtil.DrawLine(points, Color.blue);
                //GizmoUtil.DrawSpline(SplineTool.CreateSplineFromPoints(points), Color.magenta);
            }
            else if (cont.IsIntersection)
            {
                //intersection
                GizmoUtil.DrawSpline(cont.GetTrack().Spline, Color.yellow);

                //debug
                //var straightTrack = cont.m_Intersection.GetTraksOfSameOriginByType(getter, cont.GetTrack(), RnTurnType.Straight);

                var onComing = cont.m_Intersection.GetOncomingTracks(getter, cont.GetTrack());
                foreach (var track in onComing)
                {
                    GizmoUtil.DrawSpline(track.Spline, Color.green);
                }

                var crossingTracks = cont.m_Intersection.GetCrossingTracks(getter, cont.GetTrack());
                foreach (var track in crossingTracks)
                {
                    GizmoUtil.DrawSpline(track.Spline, Color.magenta);
                }

                //var edges = cont.m_Intersection.GetStraightLineEdgesFromBorder(getter, cont.m_FromBorder);
                //foreach (var e in edges)
                //{
                //    GizmoUtil.DrawLine(e.GetBorder(getter).GetChildLineString(getter).GetChildPointsVector(getter), Color.red);
                //}

                //GizmoUtil.DrawLabel(cont.GetTrack().Spline.Knots.First().Position, $"TurnType:{cont.GetTrack().TurnType.ToString()}", Color.red);

                var numRoads = cont.m_Intersection.GetAllConnectedRoads(getter).Count();
                GizmoUtil.DrawLabel(cont.GetTrack().Spline.Knots.First().Position, $"Roads:{numRoads}", Color.red);
                //var roadIDs = string.Join("," ,cont.m_Intersection.GetAllConnectedRoads(getter).Select(x => x.GetId(getter)).ToList());
                //GizmoUtil.DrawLabel(cont.GetTrack().Spline.Knots.First().Position, $"Roads:{numRoads} : {roadIDs}", Color.red);
            }

            //From / To Border
            if (cont.m_FromBorder != null)
            {
                List<Vector3> vec = cont.m_FromBorder.GetChildLineString(getter).GetChildPointsVector(getter);
                if (vec.Count > 0)
                {
                    Handles.Label(vec.First(), $"from :{cont.m_FromBorder.LineString.ID}");
                    GizmoUtil.DrawLine(vec, Color.blue);
                }
            }

            if (cont.m_ToBorder != null)
            {
                List<Vector3> vec = cont.m_ToBorder.GetChildLineString(getter).GetChildPointsVector(getter);
                if (vec.Count > 0)
                {
                    Handles.Label(vec.First(), $"to :{cont.m_ToBorder.LineString.ID} rev {cont.m_RoadInfo.m_IsReverse}/{cont.GetLane()?.IsReverse::false}");
                    GizmoUtil.DrawLine(vec, Color.cyan);
                }
            }

            //Debug
            //if (m_RoadParam.expectedBorders?.Count > 0)
            //{
            //    foreach (var border in m_RoadParam.expectedBorders)
            //    {
            //        var vec = border.GetChildLineString(getter).GetChildPointsVector(getter);
            //        Handles.Label(vec.First(), $"id:{border.LineString.ID}");
            //        GizmoUtil.DrawLine(vec, Color.green);
            //    }
            //}

            //if (m_RoadParam.actualBorders?.Count > 0)
            //{
            //    Handles.color = Color.red;
            //    foreach (var border in m_RoadParam.actualBorders)
            //    {
            //        var vec = border.GetChildLineString(getter).GetChildPointsVector(getter);
            //        Handles.Label(vec.Last(), $"id:{border.LineString.ID}");
            //        GizmoUtil.DrawLine(vec, Color.red);
            //    }
            //}


            //Vehicle
            //if (cont != null && cont.IsRoad)
            //{
            //    var boundsOffset = cont.m_Distance / Mathf.Abs(Vector3.Distance(cont.m_Bounds.max, cont.m_Bounds.center));


            //    Debug.Log($"bounds {cont.m_Bounds.max} boundsOffset {boundsOffset}");
            //    var currentProgress = cont.m_CurrentProgress - boundsOffset;

            //    var points = cont.GetLineString().GetChildPointsVector(getter);
            //    var spline = SplineTool.CreateSplineFromPoints(points);

            //    DrawLine(new List<Vector3>() { SplineTool.GetPointOnSpline(spline, cont.m_CurrentProgress), SplineTool.GetPointOnSpline(spline, currentProgress) }, Color.red);
            //}

        }
    }
}
