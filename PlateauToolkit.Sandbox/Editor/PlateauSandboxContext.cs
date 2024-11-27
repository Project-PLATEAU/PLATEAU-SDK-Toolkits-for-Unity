using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace PlateauToolkit.Sandbox.Editor
{
    class PlateauSandboxContext
    {
        static PlateauSandboxContext s_Current;

        readonly List<PlateauSandboxTrack> m_Tracks = new();
        readonly PlacementSettings m_PlacementSettings = new();
        GameObject m_SelectedObject;

        //複数選択
        List<GameObject> m_SelectedObjects;
        public List<GameObject> SelectedObjectsMultiple { get =>  m_SelectedObjects; }

        public UnityEvent<GameObject> OnSelectedObjectChanged { get; } = new UnityEvent<GameObject>();

        public IReadOnlyList<PlateauSandboxTrack> Tracks
        {
            get
            {
                // TODO: Managing lifecycles of objects is the best way.
                foreach (PlateauSandboxTrack track in m_Tracks)
                {
                    if (track == null)
                    {
                        RefreshArrays(m_Tracks);
                        break;
                    }
                }

                return m_Tracks;
            }
        }

        public PlacementSettings PlacementSettings => m_PlacementSettings;

        static void RefreshArrays<T>(List<T> list) where T : Component
        {
            list.Clear();

            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene is { isLoaded: true })
            {
                GameObject[] rootGameObjects = activeScene.GetRootGameObjects();
                foreach (GameObject rootGameObject in rootGameObjects)
                {
                    Component[] components = rootGameObject.GetComponentsInChildren(typeof(T), true);
                    foreach (Component component in components)
                    {
                        list.Add(component as T);
                    }
                }
            }
        }

        public static PlateauSandboxContext GetCurrent()
        {
            return s_Current ??= new PlateauSandboxContext();
        }

        public void SelectPlaceableObject(GameObject placeableObject)
        {
            m_SelectedObject = placeableObject;
            OnSelectedObjectChanged.Invoke(placeableObject);
        }

        public bool IsSelectedObject(GameObject other)
        {
            return m_SelectedObject == other;
        }

        //複数選択用処理
        public void SelectPlaceableObjectMultiple(GameObject placeableObject)
        {
            if (m_SelectedObjects == null)
            {
                m_SelectedObjects = new();
            }
            if (IsSelectedObjectMultiple(placeableObject)) //選択中の場合は非選択に
            {
                m_SelectedObjects.Remove(placeableObject);
            }
            else
            {
                m_SelectedObjects.Add(placeableObject);
            }
            OnSelectedObjectChanged.Invoke(placeableObject);
        }

        public bool IsSelectedObjectMultiple(GameObject other)
        {
            if (m_SelectedObjects == null)
            {
                return false;
            }
            return m_SelectedObjects.Contains(other);
        }

        public void RemoveSelectedObjectMultiple(GameObject placeableObject)
        {
            if (m_SelectedObjects == null)
            {
                return;
            }
            m_SelectedObjects.Remove(placeableObject);
            OnSelectedObjectChanged.Invoke(placeableObject);
        }

        public bool IsSelectedObjectMoveable()
        {
            return m_SelectedObject.TryGetComponent<IPlateauSandboxMovingObject>(out _);
        }

        public PlateauSandboxInstantiation InstantiateSelectedObject(Vector3 position, Quaternion rotation, HideFlags? hideFlags = null)
        {
            if (m_SelectedObject == null)
            {
                throw new NullReferenceException(nameof(m_SelectedObject));
            }

            string gameObjectName = GameObjectUtility.GetUniqueNameForSibling(null, m_SelectedObject.name);

            var gameObject = (GameObject)PrefabUtility.InstantiatePrefab(m_SelectedObject);
            gameObject.name = gameObjectName;
            gameObject.transform.rotation = rotation;

            if (gameObject.TryGetComponent(out IPlateauSandboxPlaceableObject placeable))
            {
                placeable.SetPosition(position);
            }
            else
            {
                gameObject.transform.position = position;
            }

            if (hideFlags != null)
            {
                gameObject.hideFlags = hideFlags.Value;
            }

            return new PlateauSandboxInstantiation(gameObject, m_SelectedObject);
        }

        public void Refresh()
        {
            RefreshTracks();
        }

        public void RefreshTracks()
        {
            RefreshArrays(m_Tracks);
        }
    }

    class PlateauSandboxInstantiation
    {
        public PlateauSandboxInstantiation(GameObject sceneObject, GameObject prefab)
        {
            SceneObject = sceneObject;
            Prefab = prefab;
        }

        public GameObject SceneObject { get; }
        public GameObject Prefab { get; }
    }
}