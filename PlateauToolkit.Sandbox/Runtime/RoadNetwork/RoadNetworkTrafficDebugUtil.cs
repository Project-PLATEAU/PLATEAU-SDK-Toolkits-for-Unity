
using PLATEAU.RoadNetwork.Data;
using PLATEAU.RoadNetwork.Structure;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

namespace PlateauToolkit.Sandbox.RoadNetwork
{
    [ExecuteAlways]
    public class RoadNetworkTrafficDebugUtil : MonoBehaviour
    {



        [HideInInspector][SerializeField] RoadNetworkDataGetter m_RoadNetworkGetter;

        //[SerializeField] RoadNetworkTrafficController m_RoadParam;

        [SerializeField] List<RoadNetworkTrafficController> m_RoadParams;

        RoadNetworkDataGetter RnGetter
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

        void Awake()
        {
            var roads = RnGetter.GetRoadBases().OfType<RnDataRoad>().ToList();

            //初回
            RoadInfo info = new RoadInfo();
            info.m_LaneIndex = 0;
            info.m_RoadId = roads.First().GetId(RnGetter); //最初のRnDataRoad
            //info.m_RoadBase = RoadNetworkTrafficControllers.First();

            //info.m_RoadBase = RoadNetworkTrafficControllers[2];
            //random
            //info.m_RoadBase = RoadNetworkTrafficControllers[ UnityEngine.Random.Range(0, RoadNetworkTrafficControllers.Count)];

            m_RoadParams = new();

            var m_RoadParam = new RoadNetworkTrafficController(info);
            m_RoadParams.Add(m_RoadParam);

            var next = m_RoadParam.GetNextRoad();
            m_RoadParams.Add(next);

            //SplineContainer splineCont = gameObject.AddComponent<SplineContainer>();
            //splineCont.AddSpline(next.m_Track.Spline);

            //var next2 = next.GetNextRoad();
            //m_RoadParams.Add(next2);

            //var next3 = next2.GetNextRoad();
            //m_RoadParams.Add(next3);

            //var next4 = next3.GetNextRoad();
            //m_RoadParams.Add(next4);

            //var next5 = next4.GetNextRoad();
            //m_RoadParams.Add(next5);

            for (int i = 0; i < 5; i++)
            {
                next = next.GetNextRoad();
                m_RoadParams.Add(next);
            }

        }


        //Debug Gizmo
        void OnDrawGizmos()
        {
            if (RnGetter == null)
                return;

            for(int z= 0; z< m_RoadParams.Count; z++)
            {
                var m_RoadParam = m_RoadParams[z];
                if (m_RoadParam == null)
                    return;

                if (m_RoadParam.IsRoad)
                {
                    var points = m_RoadParam.GetLineString().GetChildPointsVector(RnGetter);

                    Gizmos.color = Color.blue;
                    for (int j = 0; j < points.Count - 1; j++)
                    {
                        Gizmos.DrawLine(points[j], points[j + 1]);
                    }

                    Gizmos.color = Color.magenta;
                    Vector3 lastpos = Vector3.zero;
                    for (int i = 0; i < 100; i++)
                    {
                        var percent = i * 0.01f;
                        Vector3 pos = SplineTool.GetPointOnSpline(points, percent);
                        //Vector3 pos = SplineTool.GetPointOnLine(points, percent);
                        if (lastpos == Vector3.zero)
                            lastpos = pos;

                        Gizmos.DrawLine(pos, lastpos);
                        lastpos = pos;
                    }
                }

                else if (m_RoadParam.IsIntersection)
                {
                    //intersection
                    Gizmos.color = Color.yellow;
                    Vector3 lastpos = Vector3.zero;
                    for (int i = 0; i < 100; i++)
                    {
                        var percent = i * 0.01f;
                        //var track = m_RoadParam.m_Intersection.Tracks[m_RoadParam.m_TrackPosition];
                        var track = m_RoadParam.GetTrack();
                        Vector3 pos = SplineTool.GetPointOnSpline(track.Spline, percent);

                        //var points = track.GetToLineString(RoadNetworkGetter).GetChildPointsVector(RoadNetworkGetter);
                        //Vector3 pos = SplineTool.GetPointOnSpline(points, percent);

                        if (lastpos == Vector3.zero)
                            lastpos = pos;

                        Gizmos.DrawLine(pos, lastpos);
                        lastpos = pos;
                    }
                }
            }
        }


    }
}
