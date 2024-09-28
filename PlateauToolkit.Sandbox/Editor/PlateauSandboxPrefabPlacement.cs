using PLATEAU.CityGML;
using PLATEAU.CityInfo;
using PLATEAU.Native;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Editor
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
            public string m_AssetType;
            public string m_ObjectId;
            public bool m_IsIgnoreHeight;
            public bool m_IsPlaced;
        }

        PLATEAUInstancedCityModel m_CityModel;
        List<PlacementContext> m_PlacementContexts = new List<PlacementContext>();

        public int PlacingCount { get; private set; }

        // Check the length to the collider.
        const float k_GroundCheckLength = 10000.0f;

        public PlateauSandboxPrefabPlacement()
        {
            m_CityModel = UnityEngine.Object.FindObjectOfType<PLATEAUInstancedCityModel>();
            if (m_CityModel == null)
            {
                Debug.LogError("CityModel is not found.");
            }
        }

        public void AddContext(PlacementContext context)
        {
            m_PlacementContexts.Add(context);
            PlacingCount++;
        }

        public async void PlaceAllAsync(CancellationToken cancellationToken)
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

            ShowResultDialog();

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
                    Debug.LogWarning($"{context.m_ObjectId} : オブジェクトを配置できるコライダーが見つかりませんでした。{unityPosition.ToString()}");
                    return false;
                }

                unityPosition.y = colliderHeight;
            }

            // Name for the GameObject
            string gameObjectName = GameObjectUtility.GetUniqueNameForSibling(null,
                $"{context.m_ObjectId}_{context.m_AssetType}_{context.m_Prefab.name}");

            // Check if a parent GameObject already exists
            string parentName = $"アセット一括配置";
            var parentObject = GameObject.Find(parentName);
            if (parentObject == null)
            {
                // Create a new Parent GameObject if it doesn't exist
                parentObject = new GameObject(parentName);
            }

            // Create a new Asset GameObject
            var asset = (GameObject)PrefabUtility.InstantiatePrefab(context.m_Prefab);
            asset.name = gameObjectName;
            asset.transform.SetParent(parentObject.transform);
            asset.transform.position = unityPosition;

            Debug.Log($"アセットを配置。{gameObjectName} at {asset.transform.position.ToString()}");

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
    }
}