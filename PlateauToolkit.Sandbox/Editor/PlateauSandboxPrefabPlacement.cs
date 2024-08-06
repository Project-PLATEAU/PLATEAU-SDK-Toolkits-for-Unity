using PLATEAU.CityInfo;
using PLATEAU.Native;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Editor
{
    public class PlateauSandboxPrefabPlacement
    {
        public struct PlacementContext
        {
            public float m_Longitude;
            public float m_Latitude;
            public float m_Height;
            public GameObject m_Prefab;
            public string m_AssetType;
            public string m_ObjectId;
        }

        PLATEAUInstancedCityModel m_CityModel;
        List<PlacementContext> m_PlacementContexts = new List<PlacementContext>();

        public int PlacingCount { get; private set; }

        // For checking the height of the placed object.
        const float k_HeightCheckDistance = 10000.0f;
        const float k_PlacementCheckDistance = 1f;

        public PlateauSandboxPrefabPlacement()
        {
            m_CityModel = Object.FindObjectOfType<PLATEAUInstancedCityModel>();
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
                TryPlace(m_PlacementContexts[PlacingCount - 1]);

                await Task.Yield();
                PlacingCount--;
            }

            Debug.Log("アセットの一括配置が終了しました");
        }

        void TryPlace(PlacementContext context)
        {
            // Set the position of the asset
            var geoCoordinate = new GeoCoordinate(context.m_Latitude, context.m_Longitude, context.m_Height);
            PlateauVector3d plateauPosition = m_CityModel.GeoReference.Project(geoCoordinate);
            var unityPosition = new Vector3((float)plateauPosition.X, (float)plateauPosition.Y, (float)plateauPosition.Z);
            if (unityPosition.y <= 0)
            {
                unityPosition.y = TryGetHeightPosition(unityPosition);
            }

            // Name for the GameObject
            string gameObjectName = GameObjectUtility.GetUniqueNameForSibling(null,
                $"{context.m_ObjectId}_{context.m_AssetType}_{context.m_Prefab.name}");

            var ray = new Ray(unityPosition + Vector3.up * k_PlacementCheckDistance, -Vector3.up);
            if (!Physics.Raycast(ray, k_PlacementCheckDistance))
            {
                Debug.LogWarning($"{gameObjectName} : オブジェクトを配置できるコライダーが見つかりませんでした。{unityPosition.ToString()}");
                return;
            }

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
        }

        public void StopPlace()
        {
            PlacingCount = 0;
            m_PlacementContexts.Clear();
        }

        private float TryGetHeightPosition(Vector3 position)
        {
            // If the height is not set, then RayCast to get the height.
            var rayStartPosition = new Vector3(position.x, k_HeightCheckDistance, position.z);
            var ray = new Ray(rayStartPosition, Vector3.down);
            if (Physics.Raycast(ray, out RaycastHit hit, k_HeightCheckDistance))
            {
                return hit.point.y;
            }
            return 0;
        }

        public bool IsValidCityModel()
        {
            return m_CityModel != null;
        }
    }
}