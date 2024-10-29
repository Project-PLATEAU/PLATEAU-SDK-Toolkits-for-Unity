using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using PLATEAU.RoadNetwork.Data;
using System.Linq;

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

        public static void DebugRoadNetwork(RoadNetworkTrafficController cont, RoadNetworkDataGetter getter)
        {
            if (cont.IsRoad)
            {
                var points = cont.GetLineString().GetChildPointsVector(getter);
                GizmoUtil.DrawLine(points, Color.blue);
                GizmoUtil.DrawSpline(SplineTool.CreateSplineFromPoints(points), Color.magenta);
            }
            else if (cont.IsIntersection)
            {
                //intersection
                GizmoUtil.DrawSpline(cont.GetTrack().Spline, Color.yellow);

                //debug
                //var onComing = cont.m_Intersection.GetOncomingTracks(getter, cont.GetTrack());
                ////if(onComing.Count > 0)
                ////    Debug.LogWarning($"onComing {onComing.Count}");

                //foreach (var track in onComing)
                //{
                //    GizmoUtil.DrawSpline(track.Spline, Color.red);
                //}

                var crossingTracks = cont.m_Intersection.GetCrossingTracks(getter, cont.GetTrack());
                foreach (var track in crossingTracks)
                {
                    GizmoUtil.DrawSpline(track.Spline, Color.magenta);
                }

                //var edges = cont.m_Intersection.GetStraightLneEdgesFromBorder(getter, cont.m_FromBorder);
                //foreach (var e in edges)
                //{
                //    GizmoUtil.DrawLine(e.GetBorder(getter).GetChildLineString(getter).GetChildPointsVector(getter), Color.red);
                //}

                GizmoUtil.DrawLabel(cont.GetTrack().Spline.Knots.First().Position, $"TurnType:{cont.GetTrack().TurnType.ToString()}", Color.red);
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


        }
    }
}
