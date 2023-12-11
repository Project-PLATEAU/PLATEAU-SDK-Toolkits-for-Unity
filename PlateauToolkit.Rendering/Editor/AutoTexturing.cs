using PlateauToolkit.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PlateauToolkit.Rendering.Editor
{

    class AutoTexturing
    {
        static PlateauRenderingMaterialAssignment s_BuildingMaterialAssignmentTable;

        internal event Action OnProcessingFinished;

        internal void Initialize()
        {
#if UNITY_URP
            s_BuildingMaterialAssignmentTable = AssetDatabase.LoadAssetAtPath<PlateauRenderingMaterialAssignment>(PlateauToolkitRenderingPaths.k_BuildingTextureAssetUrp);
#elif UNITY_HDRP
            s_BuildingMaterialAssignmentTable = AssetDatabase.LoadAssetAtPath<PlateauRenderingMaterialAssignment>(PlateauToolkitRenderingPaths.k_BuildingTextureAssetHdrp);
#endif
        }

        internal void RunOptimizeProcess(List<GameObject> selectedObjects)
        {
            if (s_BuildingMaterialAssignmentTable == null)
            {
                Debug.LogWarning("マテリアルテーブルが見つかりませんでした。");
                OnProcessingFinished();
                return;
            }

            EditorApplication.delayCall += () => OptimizeProcessDelayCall(selectedObjects);
        }

        void OptimizeProcessDelayCall(List<GameObject> selectedObjects)
        {
            // Run the process on the main thread using EditorApplication.delayCall
            EditorApplication.delayCall += () =>
            {
                for (int i = 0; i < selectedObjects.Count; i++)
                {
                    GameObject selectedObject = selectedObjects[i];

                    if (selectedObject.name.Contains(PlateauRenderingConstants.k_PostfixAutoTextured))
                    {
                        Debug.LogWarning("自動テクスチャ処理済。");
                        continue;
                    }

                    Undo.RegisterCompleteObjectUndo(selectedObject, "Auto texturing");
                    selectedObject.name += PlateauRenderingConstants.k_PostfixAutoTextured;

                    int siblingCount = selectedObject.transform.parent.childCount;

                    MeshRenderer meshRenderer = selectedObject.GetComponent<MeshRenderer>();
                    MeshFilter meshFilter = selectedObject.GetComponent<MeshFilter>();

                    if (selectedObject.GetComponent<Processed>() != null)
                    {
                        Debug.LogWarning("自動テクスチャ処理済のオブジェクトです。");
                        continue;
                    }

                    if (meshRenderer != null && meshFilter != null)
                    {
                        PlateauMeshLodLevel lodLevelOfSelectedObject = PlateauRenderingBuildingUtilities.GetMeshLodLevel(selectedObject);
                        PlateauMeshStructure structureOfSelectedObject = PlateauRenderingBuildingUtilities.GetMeshStructure(selectedObject);

                        if (lodLevelOfSelectedObject == PlateauMeshLodLevel.Lod1 && structureOfSelectedObject != PlateauMeshStructure.CombinedArea)
                        {
                            GameObject lod2Object = PlateauRenderingBuildingUtilities.FindSiblingLodObject(selectedObject, "LOD2");

                            if (lod2Object == null)
                            {
                                ProcessLOD1(selectedObject, meshRenderer, meshFilter);
                            }
                            else
                            {
                                PlateauMeshStructure siblingStructure = PlateauRenderingBuildingUtilities.GetMeshStructure(lod2Object);
                                PlateauMeshLodLevel siblingLodLevel = PlateauRenderingBuildingUtilities.GetMeshLodLevel(lod2Object);
                                if (siblingLodLevel == PlateauMeshLodLevel.Lod2 && siblingStructure == PlateauMeshStructure.UnitBuilding)
                                {
                                    GameObject targetObject;
                                    GameObject referenceObject;
                                    (referenceObject, targetObject) = PlateauRenderingLOD2MeshSimplifier.GetLOD1AndLOD2(selectedObject);
                                    if (targetObject != null && referenceObject != null)
                                    {
                                        ProcessNewLOD1(selectedObject, meshRenderer, meshFilter, targetObject, referenceObject);
                                    }
                                }
                                else if (siblingLodLevel == PlateauMeshLodLevel.Lod2 && siblingStructure == PlateauMeshStructure.SeparatedParts)
                                {
                                    // Store the original active state
                                    bool wasActive = lod2Object.activeInHierarchy;

                                    GameObject combinedObj = PlateauRenderingBuildingUtilities.CombineSeparatedLOD2(lod2Object);
                                    ProcessNewLOD1(selectedObject, meshRenderer, meshFilter, combinedObj, selectedObject);

                                    // Revert to the original active state
                                    if (!wasActive)
                                    {
                                        combinedObj.SetActive(false);
                                    }
                                }
                            }
                        }

                        else if (lodLevelOfSelectedObject == PlateauMeshLodLevel.Lod1 && structureOfSelectedObject == PlateauMeshStructure.CombinedArea)
                        {
                            // Add the Processed component to the GameObject
                            Undo.AddComponent<Processed>(selectedObject);
                            GameObject lod2Object = PlateauRenderingBuildingUtilities.FindSiblingLodObject(selectedObject, "LOD2");

                            if (lod2Object)
                            {
                                MeshFilter lod2MeshFilter = lod2Object.GetComponent<MeshFilter>();
                                MeshRenderer lod2MeshRenderer = lod2Object.GetComponent<MeshRenderer>();

                                // Replace the original mesh with lod2Mesh
                                MeshFilter goMeshFilter = selectedObject.GetComponent<MeshFilter>();
                                MeshRenderer goMeshRenderer = selectedObject.GetComponent<MeshRenderer>();
                                if (goMeshFilter != null)
                                {
                                    goMeshFilter.sharedMesh = lod2MeshFilter.sharedMesh;
                                    goMeshRenderer.sharedMaterials = lod2MeshRenderer.sharedMaterials;
                                }

                                List<GameObject> newGameObjects = PlateauRenderingMeshUtilities.SeparateSubmesh(selectedObject);
                                foreach (GameObject newGameObject in newGameObjects)
                                {
                                    MeshRenderer newMeshRenderer = newGameObject.GetComponent<MeshRenderer>();
                                    MeshFilter newMeshFilter = newGameObject.GetComponent<MeshFilter>();
                                    ProcessNewLOD1(newGameObject, newMeshRenderer, newMeshFilter, newGameObject, newGameObject);
                                }
                            }
                            else
                            {
                                List<GameObject> newGameObjects = PlateauRenderingMeshUtilities.SeparateMesh(selectedObject);

                                foreach (GameObject newGameObject in newGameObjects)
                                {
                                    MeshRenderer newMeshRenderer = newGameObject.GetComponent<MeshRenderer>();
                                    MeshFilter newMeshFilter = newGameObject.GetComponent<MeshFilter>();

                                    ProcessLOD1(newGameObject, newMeshRenderer, newMeshFilter);
                                }
                            }
                        }

                        else if (lodLevelOfSelectedObject == PlateauMeshLodLevel.Lod2 && structureOfSelectedObject == PlateauMeshStructure.UnitBuilding)
                        {
                            ProcessLod2(selectedObject, meshRenderer, meshFilter);
                        }

                        else if (lodLevelOfSelectedObject == PlateauMeshLodLevel.Lod2 && structureOfSelectedObject == PlateauMeshStructure.CombinedArea)
                        {
                            List<GameObject> newGameObjects = PlateauRenderingMeshUtilities.SeparateSubmesh(selectedObject);

                            foreach (GameObject newGameObject in newGameObjects)
                            {
                                MeshRenderer newMeshRenderer = newGameObject.GetComponent<MeshRenderer>();
                                MeshFilter newMeshFilter = newGameObject.GetComponent<MeshFilter>();

                                ProcessLod2(newGameObject, newMeshRenderer, newMeshFilter);
                            }
                        }

                        else if (lodLevelOfSelectedObject == PlateauMeshLodLevel.Lod2 && structureOfSelectedObject == PlateauMeshStructure.SeparatedParts)
                        {
                            GameObject combinedObj = PlateauRenderingBuildingUtilities.CombineSeparatedLOD2(selectedObject);
                            MeshRenderer newMeshRenderer = combinedObj.GetComponent<MeshRenderer>();
                            MeshFilter newMeshFilter = combinedObj.GetComponent<MeshFilter>();
                            ProcessLod2(combinedObj, newMeshRenderer, newMeshFilter);
                        }
                    }
                    else
                    {
                        Debug.LogWarning("自動テクスチャリングを行う前に主要地物単位になるようにメッシュを処理するのでこちらのケースはこの仕様が変わらない限り発生しない");
                    }

                    // after auto texturing the building object may have extra children objects such as Floor lights or Roof lights for tall buildings
                    // if the object already has an LOD group then add the new children objects to the group as well
                    // the null check is for combined mesh which will become null because auto texturing breaks the original selected object into submeshes
                    if (selectedObject != null && selectedObject.transform.childCount > 0 && selectedObject.transform.parent.name.Contains("LOD"))
                    {
                        LODGroup lodGroup = selectedObject.transform.parent.GetComponent<LODGroup>();
                        LOD[] lods = lodGroup.GetLODs();
                        Renderer[] renderersInChildren = selectedObject.GetComponentsInChildren<Renderer>();

                        lods[siblingCount - selectedObject.transform.GetSiblingIndex() - 1].renderers = renderersInChildren;

                        lodGroup.SetLODs(lods.ToArray());
                        lodGroup.RecalculateBounds();
                    }

                    float stepProgress = (float)(i + 1) / (selectedObjects.Count);
                    if (EditorUtility.DisplayCancelableProgressBar("Auto Texturing", "Applying textures...", stepProgress))
                    {
                        Debug.Log("テクスチャ生成がキャンセルされました。");
                        break;
                    }
                }
                EditorUtility.ClearProgressBar();
                OnProcessingFinished?.Invoke();
            };
        }

        void SetMaterialFromHeight(MeshRenderer meshRenderer, float height)
        {
            if (height <= 10 && s_BuildingMaterialAssignmentTable.m_Max10mMaterials.Length > 0)
            {
                int random = UnityEngine.Random.Range(0, s_BuildingMaterialAssignmentTable.m_Max10mMaterials.Length - 1);
                meshRenderer.sharedMaterial = s_BuildingMaterialAssignmentTable.m_Max10mMaterials[random];
            }
            else
                if (height <= 40 && s_BuildingMaterialAssignmentTable.m_Max40mMaterials.Length > 0)
            {
                int random = UnityEngine.Random.Range(0, s_BuildingMaterialAssignmentTable.m_Max40mMaterials.Length - 1);
                meshRenderer.sharedMaterial = s_BuildingMaterialAssignmentTable.m_Max40mMaterials[random];
            }
            else
                if (height <= 80 && s_BuildingMaterialAssignmentTable.m_Max80mMaterials.Length > 0)
            {
                int random = UnityEngine.Random.Range(0, s_BuildingMaterialAssignmentTable.m_Max80mMaterials.Length - 1);
                meshRenderer.sharedMaterial = s_BuildingMaterialAssignmentTable.m_Max80mMaterials[random];
            }
            else
                if (height <= 10 && s_BuildingMaterialAssignmentTable.m_Max150mMaterials.Length > 0)
            {
                int random = UnityEngine.Random.Range(0, s_BuildingMaterialAssignmentTable.m_Max150mMaterials.Length - 1);
                meshRenderer.sharedMaterial = s_BuildingMaterialAssignmentTable.m_Max150mMaterials[random];
            }
            else
                if (s_BuildingMaterialAssignmentTable.m_MaxHeightMaterials.Length > 0)
            {
                int random = UnityEngine.Random.Range(0, s_BuildingMaterialAssignmentTable.m_MaxHeightMaterials.Length - 1);
                meshRenderer.sharedMaterial = s_BuildingMaterialAssignmentTable.m_MaxHeightMaterials[random];
            }
        }

        void ProcessNewLOD1(GameObject go, MeshRenderer meshRenderer, MeshFilter meshFilter, GameObject targetObject, GameObject referenceObject)
        {
            Undo.RecordObject(meshFilter, "Optimize Mesh");
            Undo.RecordObject(meshRenderer, "Optimize Mesh");

            // Add the Processed component to the GameObject
            Undo.AddComponent<Processed>(go);

            float bottomRatio = 0.3f;
            float topRatio = 0.8f;

            Mesh newLOD1Mesh = PlateauRenderingLOD2MeshSimplifier.LOD2Simplify(targetObject, referenceObject, bottomRatio, topRatio);

            Bounds boundingBox = newLOD1Mesh.bounds;
            PlateauRenderingBuildingUtilities.SetBuildingVertexColorForWindow(newLOD1Mesh, boundingBox, go);

            PlateauRenderingBuildingUtilities.ChangeLOD2BuildingShader(go);
            PlateauRenderingBuildingUtilities.PlaceObstacleLightsOnBuildingCorners(go);
            PlateauRenderingBuildingUtilities.CreatePlaneUnderBuilding(go);
        }

        void ProcessLod2(GameObject go, MeshRenderer meshRenderer, MeshFilter meshFilter)
        {
            Undo.RecordObject(meshFilter, "Optimize Mesh");
            Undo.RecordObject(meshRenderer, "Optimize Mesh");

            // Add the Processed component to the GameObject
            Undo.AddComponent<Processed>(go);

            Bounds boundingBox = meshRenderer.bounds;
            PlateauRenderingBuildingUtilities.SetBuildingVertexColorForWindow(meshFilter.sharedMesh, boundingBox, go);

            PlateauRenderingBuildingUtilities.ChangeLOD2BuildingShader(go);
            PlateauRenderingBuildingUtilities.PlaceObstacleLightsOnBuildingCorners(go);
            PlateauRenderingBuildingUtilities.CreatePlaneUnderBuilding(go);
        }
        void ProcessLOD1(GameObject go, MeshRenderer meshRenderer, MeshFilter meshFilter)
        {
            Undo.RecordObject(meshRenderer, "Optimize Mesh");
            Undo.RecordObject(meshFilter, "Optimize Mesh");

            // Add the Processed component to the GameObject
            Undo.AddComponent<Processed>(go);

            // make new instance of mesh
            meshFilter.sharedMesh = PlateauRenderingMeshUtilities.CreateMeshInstance(meshFilter.sharedMesh, false);

            // unwrap uvs as best fit around edge of building
            PlateauRenderingMeshUtilities.UnwrapUVs(meshFilter.sharedMesh, 10);

            // select roof triangles
            List<int> listofRoofTriangles = PlateauRenderingMeshUtilities.SelectFacesFacingUp(meshFilter.sharedMesh, 15);

            // get roof boundary edges
            List<System.Tuple<int, int>> roofBoundaryEdges = PlateauRenderingMeshUtilities.GetBoundaryEdges(listofRoofTriangles);

            // extrude roof by 1m
            List<List<Tuple<int, int>>> sortedEdgeGroups = PlateauRenderingMeshUtilities.SortEdgesByConnection(roofBoundaryEdges, meshFilter.sharedMesh);
            List<Tuple<int, int>> newRoofEdges = null;
            if (sortedEdgeGroups.Count > 0)
            {
                PlateauRenderingMeshUtilities.OffsetRoofEdgesResult result = PlateauRenderingMeshUtilities.OffsetRoofEdges(meshFilter.sharedMesh, sortedEdgeGroups[0], -0.8f, 0.01f);
                newRoofEdges = result.NewEdges;
            }

            // select bottom triangles
            List<int> listofBottomTriangles = PlateauRenderingMeshUtilities.SelectFacesFacingDown(meshFilter.sharedMesh, 15);

            // delete bottom triables
            List<System.Tuple<int, int>> remainingEdges = PlateauRenderingMeshUtilities.DeleteSelectedTriangles(meshFilter.sharedMesh, PlateauRenderingMeshUtilities.SelectFacesFacingDown(meshFilter.sharedMesh, 15));

            // set base color to green for Windows
            PlateauRenderingMeshUtilities.SetTriangleVertexColors(meshFilter.sharedMesh, PlateauRenderingMeshUtilities.GetAllTriangles(meshFilter.sharedMesh), Color.green);

            // get all roof triangles
            listofRoofTriangles = PlateauRenderingMeshUtilities.SelectFacesFacingUp(meshFilter.sharedMesh, 15);

            // set new edge color to green for for first UV bake
            PlateauRenderingMeshUtilities.SetEdgeColors(meshFilter.sharedMesh, roofBoundaryEdges, Color.green);

            Bounds boundingBox = meshRenderer.bounds;
            PlateauRenderingBuildingUtilities.SetBuildingVertexColorForWindow(meshFilter.sharedMesh, boundingBox, go);

            PlateauRenderingMeshUtilities.SetEdgeColors(meshFilter.sharedMesh, newRoofEdges, Color.blue);

            // set new edge color to red for for roof UV unwrap
            PlateauRenderingMeshUtilities.SetEdgeColors(meshFilter.sharedMesh, roofBoundaryEdges, Color.red);

            List<int> roofTopFaces = PlateauRenderingMeshUtilities.GetTrianglesOfSelectedColor(meshFilter.sharedMesh, Color.red);
            List<int> roofEdgeFaces = PlateauRenderingMeshUtilities.GetTrianglesOfSelectedColor(meshFilter.sharedMesh, Color.blue);

            List<int> roofFaces = new List<int>();
            roofFaces.AddRange(roofTopFaces);
            roofFaces.AddRange(roofEdgeFaces);

            // unwrap uvs for roof with planar projection
            PlateauRenderingMeshUtilities.FlattenUVsOnBoundingBox(go, meshFilter.sharedMesh, roofTopFaces);

            //meshFilter.sharedMesh.RecalculateNormals();
            meshFilter.sharedMesh.Optimize();

            // get height of building
            float height = PlateauRenderingMeshUtilities.GetMeshHeight(meshFilter.sharedMesh);

            // set materials based on height
            SetMaterialFromHeight(meshRenderer, height);

            PlateauRenderingMeshUtilities.SetRandomAlpha(meshFilter.sharedMesh);

            PlateauRenderingBuildingUtilities.PlaceObstacleLightsOnBuildingCorners(go);
            PlateauRenderingBuildingUtilities.CreatePlaneUnderBuilding(go);
        }
    }
}