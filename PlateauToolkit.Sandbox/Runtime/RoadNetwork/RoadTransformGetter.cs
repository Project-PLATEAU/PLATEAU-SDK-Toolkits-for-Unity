
using PLATEAU.RoadAdjust.RoadNetworkToMesh;
using UnityEngine;

namespace PlateauToolkit.Sandbox.RoadNetwork
{
    public class RoadTransformGetter
    {
        const string k_ROAD_PREFIX = "Road-"; //PLATEAUReproducedRoad名のPrefix

        Transform m_ReproducedRoadRoot;
        public Transform ReproducedRoad => m_ReproducedRoadRoot;

        public RoadTransformGetter()
        {
            // 最初に見つかったPLATEAUReproducedRoadの親を取得する(ReproducedRoadのParentはシーンに1つと想定）
            PLATEAUReproducedRoad found = GameObject.FindObjectOfType<PLATEAUReproducedRoad>();
            if (found != null)
            {
                m_ReproducedRoadRoot = found.transform.parent;
            }
        }

        public Transform GetRoadTransform(string gmlId)
        {
            if (m_ReproducedRoadRoot == null)
            {
                return null;
            }

            string roadName = k_ROAD_PREFIX + gmlId;
            Transform roadTransform = m_ReproducedRoadRoot.Find(roadName);

            if (roadTransform == null)
            {
                //取得できないケースもあるが無視
                return null;
            }

            return roadTransform;
        }
    }
}
