using PLATEAU.CityInfo;
using PLATEAU.Geometries;
using PLATEAU.Native;
using UnityEditor;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Editor
{
    public class PlateauSandboxPrefabPlacement
    {
        public struct PlacementContext
        {
            public double m_Longitude;
            public double m_Latitude;
            public double m_Height;
            public GameObject m_Prefab;
            public string m_AssetType;
        }

        PLATEAUInstancedCityModel m_CityModel;
        const float k_RayCastMaxDistance = 10000.0f;

        public PlateauSandboxPrefabPlacement()
        {
            m_CityModel = UnityEngine.Object.FindObjectOfType<PLATEAUInstancedCityModel>();
            if (m_CityModel == null)
            {
                Debug.LogError("CityModel is not found.");
            }
        }

        public void Place(PlacementContext context)
        {
            GameObject prefab = context.m_Prefab;
            if (prefab == null)
            {
                return;
            }

            string gameObjectName = GameObjectUtility.GetUniqueNameForSibling(null, context.m_AssetType + "_" + prefab.name);
            var gameObject = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            gameObject.name = gameObjectName;

            var geoCoordinate = new GeoCoordinate(context.m_Latitude, context.m_Longitude, context.m_Height);
            var plateauPosition = m_CityModel.GeoReference.Project(geoCoordinate);
            var unityPosition = new Vector3((float)plateauPosition.X, (float)plateauPosition.Y, (float)plateauPosition.Z);
            unityPosition.y = GetHeightPosition(unityPosition);

            // Set Position.
            gameObject.transform.position = unityPosition;

            Debug.Log($"Asset Place. {gameObjectName} at {gameObject.transform.position.ToString()}");
        }

        private float GetHeightPosition(Vector3 position)
        {
            if (position.y > 0)
            {
                return (float)position.y;
            }

            var rayStartPosition = new Vector3(position.x, k_RayCastMaxDistance, position.z);
            var ray = new Ray(rayStartPosition, Vector3.down);
            if (Physics.Raycast(ray, out RaycastHit hit, k_RayCastMaxDistance))
            {
                return hit.point.y;
            }
            return 0;
        }

        public bool IsValid()
        {
            return m_CityModel != null;
        }
    }
}