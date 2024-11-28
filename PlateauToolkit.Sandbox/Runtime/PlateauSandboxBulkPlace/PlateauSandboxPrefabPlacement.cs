using PLATEAU.CityGML;
using PLATEAU.CityInfo;
using PLATEAU.Native;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PlateauToolkit.Sandbox.Runtime
{
    public class PlateauSandboxPrefabPlacement
    {
        enum PlaceResult
        {
            AllPlaceSuccess,
            PlaceFailed,
            AllPlaceFailed,
        }

        public struct PlacementContext
        {
            public double m_Longitude;
            public double m_Latitude;
            public float m_Height;
            public GameObject m_Prefab;
            public bool m_IsIgnoreHeight;
            public bool m_IsPlaced;
            public string m_ObjectName;
        }

        PLATEAUInstancedCityModel m_CityModel;
        List<PlacementContext> m_PlacementContexts = new List<PlacementContext>();
        public List<PlacementContext> PlacementContexts => m_PlacementContexts;

        public int PlacingCount { get; private set; }

        // Check the length to the collider.
        const float k_GroundCheckLength = 10000.0f;

        private string m_PlaceParentObjectName = "アセット一括配置";

        public PlateauSandboxPrefabPlacement()
        {
            m_CityModel = UnityEngine.Object.FindObjectOfType<PLATEAUInstancedCityModel>();
            if (m_CityModel == null)
            {
                Debug.LogError("CityModel is not found.");
            }
        }

        public void SetParentObjectName(string parentObjectName)
        {
            m_PlaceParentObjectName = parentObjectName;
        }

        public void AddContext(PlacementContext context)
        {
            m_PlacementContexts.Add(context);
            PlacingCount++;
        }

        public async Task PlaceAllAsync(CancellationToken cancellationToken)
        {
            // For the batch processing
            while (PlacingCount > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (m_PlacementContexts.Count == 0 || m_PlacementContexts.Count < PlacingCount)
                {
                    break;
                }
                PlacementContext context = m_PlacementContexts[PlacingCount - 1];
                bool isPlaced = TryPlace(context);

                // 配置できたかどうかのフラグを更新
                context.m_IsPlaced = isPlaced;
                m_PlacementContexts[PlacingCount - 1] = context;

                await Task.Yield();
                PlacingCount--;
            }

#if UNITY_EDITOR
            ShowResultDialog();
#endif
            var prefabCreator = GameObject.Find("PrefabCreator");
            if (prefabCreator != null)
            {
#if UNITY_EDITOR
                GameObject.DestroyImmediate(prefabCreator);
#else
                GameObject.Destroy(prefabCreator);
#endif
            }

            Debug.Log("アセットの一括配置が終了しました");
        }

        bool TryPlace(PlacementContext context)
        {
            // Set the position of the asset
            var geoCoordinate = new GeoCoordinate(context.m_Latitude, context.m_Longitude, context.m_Height);
            PlateauVector3d plateauPosition = m_CityModel.GeoReference.Project(geoCoordinate);
            var unityPosition = new Vector3((float)plateauPosition.X, (float)plateauPosition.Y, (float)plateauPosition.Z);

            if (context.m_IsIgnoreHeight)
            {
                // If the height is not set, then RayCast to get the height.
                bool isColliderFound = TryGetColliderHeight(unityPosition, out float colliderHeight);
                if (!isColliderFound)
                {
                    Debug.LogWarning($"{context.m_ObjectName} : オブジェクトを配置できるコライダーが見つかりませんでした。{unityPosition.ToString()}");
                    return false;
                }

                unityPosition.y = colliderHeight;
            }

            // Check if a parent GameObject already exists
            var parentObject = GameObject.Find(m_PlaceParentObjectName);
            if (parentObject == null)
            {
                // Create a new Parent GameObject if it doesn't exist
                parentObject = new GameObject(m_PlaceParentObjectName);
            }

            var prefabCreator = GameObject.Find("PrefabCreator");
            if (prefabCreator == null)
            {
                // コンポーネント追加
                prefabCreator = new GameObject("PrefabCreator");
                prefabCreator.AddComponent<PlateauSandboxPrefabCreator>();
            }

            // プレハブを配置
            prefabCreator
                .GetComponent<PlateauSandboxPrefabCreator>()
                .CreatePrefab(context.m_ObjectName, context.m_Prefab, unityPosition, parentObject);

            Debug.Log($"アセットを配置。{context.m_ObjectName} at {unityPosition.ToString()}");

            return true;
        }

        public void StopPlace()
        {
            PlacingCount = 0;
            m_PlacementContexts.Clear();
        }

        bool TryGetColliderHeight(Vector3 position, out float colliderHeight)
        {
            var rayStartPosition = new Vector3(position.x, k_GroundCheckLength, position.z);
            float rayDistance = k_GroundCheckLength * 2;

            // Send a ray downward to get the height of the collider.
            var ray = new Ray(rayStartPosition, Vector3.down);

            var hitPointHeights = new List<float>();
            RaycastHit[] results = Physics.RaycastAll(ray, rayDistance);
            foreach (RaycastHit rayCastHit in results)
            {
                if (rayCastHit.transform.TryGetComponent(out PLATEAUCityObjectGroup cityObjectGroup))
                {
                    if (cityObjectGroup.CityObjects.rootCityObjects.Any(o => o.CityObjectType == CityObjectType.COT_Building))
                    {
                        // 建物であればスキップ
                        continue;
                    }

                    // その他のオブジェクトは配置可能
                    hitPointHeights.Add(rayCastHit.point.y);
                }
            }

            if (hitPointHeights.Count > 0)
            {
                // 一番上にヒットしたコライダーの高さを取得
                colliderHeight = hitPointHeights.Max();
                return true;
            }

            // Not found.
            colliderHeight = 0.0f;
            return false;
        }

        public bool IsValidCityModel()
        {
            return m_CityModel != null;
        }

#if UNITY_EDITOR
        void ShowResultDialog()
        {
            bool isAllPlaceSuccess = m_PlacementContexts.All(context => context.m_IsPlaced);
            bool isAllPlaceFailed = m_PlacementContexts.All(context => !context.m_IsPlaced);

            if (isAllPlaceSuccess)
            {
                EditorUtility.DisplayDialog("アセット一括配置", "全てのアセットの配置に成功しました。", "OK");
            }
            else if (isAllPlaceFailed)
            {
                EditorUtility.DisplayDialog("アセット一括配置", "全てのアセットの配置に失敗しました。\n詳細はコンソールログのワーニングを確認してください。", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("アセット一括配置", "一部のアセットの配置に失敗しました。\n詳細はコンソールログのワーニングを確認してください。", "OK");
            }
        }
#endif
    }
}