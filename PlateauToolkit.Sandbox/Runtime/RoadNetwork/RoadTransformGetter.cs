
using PLATEAU.RoadAdjust.RoadNetworkToMesh;
using PLATEAU.RoadNetwork;
using UnityEngine;

namespace PlateauToolkit.Sandbox.RoadNetwork
{
    /// <summary>
    /// 道路ネットワークのデータに紐づくUnityのTransform等の情報を取得するためのクラス
    /// </summary>
    public class RoadTransformGetter
    {
        const string k_ROAD_PREFIX = "Road-"; //PLATEAUReproducedRoad名のPrefix

        Transform m_ReproducedRoadRoot;
        public Transform ReproducedRoadRoot => TryGetReproducedRoadRoot();

        public RoadTransformGetter()
        {
            // 初期化
            TryGetReproducedRoadRoot();
        }

        public Transform GetRoadTransform(string gmlId)
        {
            if (TryGetReproducedRoadRoot() == null)
            {
                return null;
            }

            string roadName = k_ROAD_PREFIX + gmlId;
            // 取得できないケースもあるが無視
            return m_ReproducedRoadRoot.Find(roadName);
        }

        /// <summary>
        /// RnCityObjectGroupKeyからTransforomを取得
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public Transform GetRoadTransform(RnCityObjectGroupKey key)
        {
            if (!key.IsValid)
            {
                return null;
            }

            return GetRoadTransform(key.GmlId);
        }

        /// <summary>
        /// ReproducedRoadのTransformを取得しm_ReproducedRoadRootに設定
        /// </summary>
        /// <returns></returns>
        Transform TryGetReproducedRoadRoot()
        {
            if (m_ReproducedRoadRoot != null)
            {
                return m_ReproducedRoadRoot;
            }

            // 最初に見つかったPLATEAUReproducedRoadの親を取得する(ReproducedRoadのParentはシーンに1つと想定）
            PLATEAUReproducedRoad found = Object.FindFirstObjectByType<PLATEAUReproducedRoad>();
            m_ReproducedRoadRoot = found?.transform.parent;
            return m_ReproducedRoadRoot;
        }
    }
}
