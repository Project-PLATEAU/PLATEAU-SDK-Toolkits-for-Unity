using PLATEAU.CityInfo;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PlateauToolkit.Rendering.Editor
{
    class Grouping
    {
        internal event Action OnProcessingFinished;
        internal void TrySeparateMeshes()
        {
            PLATEAUInstancedCityModel[] plateauModelGroups = GameObject.FindObjectsOfType<PLATEAUInstancedCityModel>();
            List<Transform> copyOfBuildingsList = new List<Transform>();

            foreach (PLATEAUInstancedCityModel modelGroupRoot in plateauModelGroups)
            {
                foreach (Transform modelGroup in modelGroupRoot.transform)
                {
                    foreach (Transform lodLevel in modelGroup.transform)
                    {
                        copyOfBuildingsList.Clear();

                        foreach (Transform building in lodLevel.transform)
                        {
                            copyOfBuildingsList.Add(building);
                        }
                        foreach (Transform building in copyOfBuildingsList)
                        {
                            if (PlateauRenderingBuildingUtilities.GetMeshStructure(building.gameObject) == PlateauMeshStructure.CombinedArea)
                            {
                                if (building.transform.parent.name.Contains("LOD2"))
                                {
                                    PlateauRenderingMeshUtilities.SeparateSubmesh(building.gameObject);
                                }
                                else
                                {
                                    PlateauRenderingMeshUtilities.SeparateMesh(building.gameObject);
                                }
                            }
                        }
                    }
                }
            }
            OnProcessingFinished?.Invoke();
        }

        /// <summary>
        /// Groups game objects in the scene by building.
        /// </summary>
        /// <remarks>
        ///It makes several assumptions that match the current Plateau City Model structure.
        /// 1. It prepares a Parent game object
        /// 2. It iterates through all the gameobjects in the scene
        /// 3. The root object of the city group will always have LOD0, Lod2, Lod3... named objects as children
        /// 4. We take corresponding children from each LOD level and group them together, and reparent to the Parent game object.
        /// </remarks>
        internal void GroupObjects()
        {
            Dictionary<string, GameObject> rootObjects = new Dictionary<string, GameObject>();
            Dictionary<string, Dictionary<string, GameObject>> lodObjects = new Dictionary<string, Dictionary<string, GameObject>>();

            PLATEAUInstancedCityModel[] plateauModelRootObject = GameObject.FindObjectsByType<PLATEAUInstancedCityModel>(FindObjectsSortMode.None);
            List<Transform> copyOfGroupedBuildingsList = new List<Transform>();
            List<GameObject> objectsToDelete = new List<GameObject>();

            for (int i = 0; i < plateauModelRootObject.Length; i++)
            {
                if (plateauModelRootObject[i] == null)
                {
                    continue;
                }

                for (int j = 0; j < plateauModelRootObject[i].transform.childCount; j++)
                {
                    Transform gmlRoot = plateauModelRootObject[i].transform.GetChild(j);
                    float stepProgress = i / (float)plateauModelRootObject.Length;
                    if (EditorUtility.DisplayCancelableProgressBar("地物のグルーピング", "グルーピング中", stepProgress))
                    {
                        UnityEngine.Debug.Log("LOD生成用のグルーピングがキャンセルされました。");
                        break;
                    }

                    objectsToDelete.Clear();
                    for (int k = 0; k < gmlRoot.childCount; k++)
                    {
                        Transform lodGroupedBuildings = gmlRoot.transform.GetChild(k);
                        if (lodGroupedBuildings.name.Contains("LOD0") || lodGroupedBuildings.name.Contains("LOD1") || lodGroupedBuildings.name.Contains("LOD2"))
                        {
                            copyOfGroupedBuildingsList.Clear();
                            for (int l = 0; l < lodGroupedBuildings.transform.childCount; l++)
                            {
                                Transform building = lodGroupedBuildings.transform.GetChild(l);
                                copyOfGroupedBuildingsList.Add(building);
                            }

                            for (int m = 0; m < copyOfGroupedBuildingsList.Count; m++)
                            {
                                Transform buildingCopy = copyOfGroupedBuildingsList[m];
                                GroupByLodName(gmlRoot, rootObjects, lodObjects, buildingCopy.gameObject, lodGroupedBuildings.name);
                            }
                        }

                        if (lodGroupedBuildings.childCount == 0 && !PrefabUtility.IsPartOfAnyPrefab(lodGroupedBuildings.gameObject))
                        {
                            objectsToDelete.Add(lodGroupedBuildings.gameObject);
                        }
                    }

                    foreach (GameObject obj in objectsToDelete)
                    {
                        try
                        {
                            GameObject.DestroyImmediate(obj);
                        }
                        catch (Exception e)
                        {
                            Debug.LogException(e);
                            Debug.Log("object name: " + obj.name);
                        }
                    }
                }
            }
            EditorUtility.ClearProgressBar();
            OnProcessingFinished?.Invoke();
        }

        void GroupByLodName(Transform gmlRoot, Dictionary<string, GameObject> rootObjectDictionary, Dictionary<string, Dictionary<string, GameObject>> lodDictionary, GameObject obj, string lodLevel)
        {
            // Use the object name as the key for grouping but ignore if we find _plateau_auto_textured because user may have textured the Lod1 first before creating LOD group
            string uniqueId = obj.name.Replace(PlateauRenderingConstants.k_PostfixAutoTextured, "");

            // try to merge the meshes first
            if (PlateauRenderingBuildingUtilities.GetMeshStructure(obj) == PlateauMeshStructure.SeparatedParts && lodLevel.Contains("LOD2"))
            {
                Undo.IncrementCurrentGroup();
                Undo.SetCurrentGroupName("Grouping Separated");
                Undo.RegisterCompleteObjectUndo(obj, "Grouping Separated");
                obj = PlateauRenderingBuildingUtilities.CombineSeparatedLOD2(obj);
                Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
                Undo.ClearUndo(obj);
            }

            // no entry yet
            if (!rootObjectDictionary.ContainsKey(uniqueId))
            {
                GameObject rootObject = new GameObject(uniqueId + "groupParent");
                rootObjectDictionary[uniqueId] = rootObject;

                lodDictionary[uniqueId] = new Dictionary<string, GameObject>();
                lodDictionary[uniqueId][lodLevel] = obj;
                obj.name = uniqueId + "_" + lodLevel + "_" + PlateauRenderingConstants.k_Grouped;
                obj.transform.SetParent(rootObject.transform, false);
            }
            else
            {
                obj.name += "_" + lodLevel + "_" + PlateauRenderingConstants.k_Grouped;
                lodDictionary[uniqueId][lodLevel] = obj;
                obj.transform.SetParent(rootObjectDictionary[uniqueId].transform, false);
            }
            rootObjectDictionary[uniqueId].transform.SetParent(gmlRoot, false);
        }
    }
}