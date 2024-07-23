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

        public void Place(PlacementContext context)
        {
            var cityModel = UnityEngine.Object.FindObjectOfType<PLATEAUInstancedCityModel>();

            GameObject prefab = context.m_Prefab;
            if (prefab == null)
            {
                return;
            }

            string gameObjectName = GameObjectUtility.GetUniqueNameForSibling(null, prefab.name);
            var gameObject = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

            gameObject.name = gameObjectName;
            PlateauVector3d position = cityModel.GeoReference.Project(new GeoCoordinate(context.m_Latitude, context.m_Longitude, context.m_Height));
            gameObject.transform.position = new Vector3((float)position.X, (float)position.Y, (float)position.Z);

            Debug.Log($"Asset Place. {gameObjectName} at {position}");
        }
    }
}