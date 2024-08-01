using PLATEAU.CityInfo;
using PLATEAU.Native;
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
        }

        PLATEAUInstancedCityModel m_CityModel;

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

            string gameObjectName = GameObjectUtility.GetUniqueNameForSibling(null, prefab.name);
            var gameObject = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

            gameObject.name = gameObjectName;
            PlateauVector3d position = m_CityModel.GeoReference.Project(new GeoCoordinate(context.m_Latitude, context.m_Longitude, context.m_Height));
            gameObject.transform.position = new Vector3((float)position.X, (float)position.Y, (float)position.Z);

            Debug.Log($"Asset Place. {gameObjectName} at {gameObject.transform.position.ToString()}");
        }

        public bool IsValid()
        {
            return m_CityModel != null;
        }
    }
}