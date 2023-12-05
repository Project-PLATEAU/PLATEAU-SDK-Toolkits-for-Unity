using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;
using PLATEAU.CityInfo;

namespace PlateauToolkit.Rendering.Editor
{
    public enum PlateauMeshLodLevel
    {
        NonTarget,
        Lod1,
        Lod2,
        Lod3
    }

    public enum PlateauMeshStructure
    {
        /// <summary>対象外</summary>
        NonTarget,
        /// <summary>地物単位</summary>
        UnitBuilding,
        /// <summary>地域単位</summary>
        CombinedArea,
        /// <summary>最小地物単位</summary>
        SeparatedParts
    }
    static class PlateauRenderingBuildingUtilities
    {
        public static PlateauMeshStructure GetMeshStructure(GameObject obj)
        {
            if (!IsPlateauBuilding(obj.transform))
            {
                return PlateauMeshStructure.NonTarget;
            }

            if (obj.transform.childCount > 0)
            {
                return PlateauMeshStructure.SeparatedParts;
            }

            if (obj.name.StartsWith("group"))
            {
                return PlateauMeshStructure.CombinedArea;
            }

            return PlateauMeshStructure.UnitBuilding;
        }

        public static PlateauMeshLodLevel GetMeshLodLevel(GameObject obj)
        {
            if (!IsPlateauBuilding(obj.transform))
            {
                return PlateauMeshLodLevel.NonTarget;
            }

            if (obj.name.Contains("LOD1") || obj.transform.parent.name.Contains("LOD1"))
            {
                return PlateauMeshLodLevel.Lod1;
            }
            if (obj.name.Contains("LOD2") || obj.transform.parent.name.Contains("LOD2"))
            {
                return PlateauMeshLodLevel.Lod2;
            }
            if (obj.name.Contains("LOD3") || obj.transform.parent.name.Contains("LOD3"))
            {
                return PlateauMeshLodLevel.Lod3;
            }
            return PlateauMeshLodLevel.NonTarget;
        }

        /// <summary>
        /// Checks if the input transform is from a PLATEAU building mesh.
        /// </summary>
        /// <remarks>We check if the root object of this game object has a PLATEAU Model component.
        /// </remarks>
        /// <param name="transformOfSelectedMesh"></param>
        /// <returns></returns>
        public static bool IsPlateauBuilding(Transform transformOfSelectedMesh)
        {
            return transformOfSelectedMesh.HasParentWithComponent<PLATEAUInstancedCityModel>() && (transformOfSelectedMesh.name.Contains("BLD") || transformOfSelectedMesh.name.Contains("bldg"));
        }

        static bool HasParentWithComponent<T>(this Transform child) where T : Component
        {
            Transform currentParent = child.parent;

            while (currentParent != null)
            {
                if (currentParent.TryGetComponent<T>(out _))
                {
                    return true;
                }

                currentParent = currentParent.parent;
            }

            return false;
        }


        /// <summary>
        /// This method finds a sibling city model object with a different LOD level.
        /// We assume here the hierarchy has been modified such that different LOD levels of the same object are already grouped together.
        /// </summary>
        /// <param name="targetObject"></param>
        /// <param name="lodLevelToFind"></param>
        /// <returns></returns>
        public static GameObject FindSiblingLodObject(GameObject targetObject, string lodLevelToFind)
        {
            Transform parent = targetObject.transform.parent;
            foreach (Transform child in parent)
            {
                if(child != targetObject.transform && child.name.Contains(lodLevelToFind))
                {
                    return child.gameObject;
                }
            }

            return null;
        }

        public static GameObject CombineSeparatedLOD2(GameObject go)
        {
            PlateauMeshLodLevel type = GetMeshLodLevel(go);
            PlateauMeshStructure structure = GetMeshStructure(go);

            if (type == PlateauMeshLodLevel.Lod2 && structure == PlateauMeshStructure.SeparatedParts)
            {
                GameObject combinedObj = PlateauRenderingMeshUtilities.CombineChildren(go);

                // Get the MeshFilter and MeshRenderer from the combined object.
                MeshFilter meshFilter = combinedObj.GetComponent<MeshFilter>();
                MeshRenderer meshRenderer = combinedObj.GetComponent<MeshRenderer>();

                // Call WeldVertices and use the result to set the new mesh.
                if (meshFilter != null && meshRenderer != null)
                {
                    Mesh oldMesh = meshFilter.sharedMesh;
                    Mesh newMesh = PlateauRenderingMeshUtilities.WeldVertices(oldMesh, meshRenderer);
                    meshFilter.sharedMesh = newMesh;
                }

                return combinedObj;
            }
            else
            {
                Debug.Log("最小地物単位のLOD2ではありません。メッシュマージに失敗しました。");
                return null;
            }
        }

        public static void SetLODVisibility(GameObject building, PlateauMeshLodLevel lodLevelToShow)
        {
            // Start from the top of the hierarchy
            Transform currentTransform = building.transform;

            // Traverse down through the hierarchy
            while (currentTransform != null)
            {
                foreach (Transform descendant in currentTransform.GetComponentsInChildren<Transform>(true))
                {
                    // Check only objects with a MeshFilter component
                    MeshFilter meshFilter = descendant.gameObject.GetComponent<MeshFilter>();
                    if (meshFilter != null)
                    {
                        PlateauMeshLodLevel level = GetMeshLodLevel(descendant.gameObject);

                        // If the LOD level matches the level to show, enable the game object, otherwise disable it
                        descendant.gameObject.SetActive(level == lodLevelToShow);

                        // check that the object is part of Lod2 - Subparts
                        if ((descendant.parent.name.Contains("bldg") || descendant.parent.name.Contains("BLD")) && GetMeshLodLevel(descendant.parent.gameObject) == lodLevelToShow)
                        {
                            descendant.gameObject.SetActive(true);
                        }
                    }
                }
                // Move to the next level in the hierarchy
                currentTransform = currentTransform.parent;
            }
        }

        public static List<Vector3> GetMinimumBoundingBoxOfRoof(GameObject selectedBuilding)
        {
            // Check if a GameObject is passed and it has a MeshFilter component
            if (selectedBuilding == null || selectedBuilding.GetComponent<MeshFilter>() == null)
            {
                Debug.LogError("No building selected or selected object has no MeshFilter component!");
                return null;
            }

            // Get all vertices and triangles from the mesh
            MeshFilter meshFilter = selectedBuilding.GetComponent<MeshFilter>();
            Vector3[] vertices = meshFilter.sharedMesh.vertices;
            int[] triangles = meshFilter.sharedMesh.triangles;

            var ceilingVerticesIndices = new HashSet<int>();

            // For each triangle, check if its normal is facing upwards
            for (int i = 0; i < triangles.Length; i += 3)
            {
                Vector3 v1 = selectedBuilding.transform.TransformPoint(vertices[triangles[i]]);
                Vector3 v2 = selectedBuilding.transform.TransformPoint(vertices[triangles[i + 1]]);
                Vector3 v3 = selectedBuilding.transform.TransformPoint(vertices[triangles[i + 2]]);

                var faceNormal = Vector3.Cross(v2 - v1, v3 - v1);

                // If the face is facing upwards, add its vertices to the ceiling vertices
                if (faceNormal.y > 0)
                {
                    ceilingVerticesIndices.Add(triangles[i]);
                    ceilingVerticesIndices.Add(triangles[i + 1]);
                    ceilingVerticesIndices.Add(triangles[i + 2]);
                }
            }

            // Transform vertices indices to actual world space vertices
            var ceilingVertices = ceilingVerticesIndices.Select(i => selectedBuilding.transform.TransformPoint(vertices[i])).ToList();

            // Calculate the convex hull from the ceiling vertices
            List<Vector3> hullVertices = PlateauRenderingGeomUtilities.GetConvexHull(ceilingVertices);

            // Determine the highest y value among all vertices of the convex hull
            float maxY = hullVertices.Max(vertex => vertex.y);

            // Adjust all y coordinates to match the highest y value
            for (int i = 0; i < hullVertices.Count; i++)
            {
                hullVertices[i] = new Vector3(hullVertices[i].x, maxY, hullVertices[i].z);
            }

            // Initialize boundingBox array
            var boundingBox = new Vector3[4];

            hullVertices = PlateauRenderingGeomUtilities.SortVerticesInClockwiseOrder(hullVertices);

            // Get the minimum bounding box for the convex hull
            Vector3[] mmb = PlateauRenderingGeomUtilities.OrientedMinimumBoundingBox2D(hullVertices, boundingBox);

            // Return the vertices of minimum bounding box as a list
            return new List<Vector3>(mmb);
        }

        public static void PlaceObstacleLightsOnBuildingCorners(GameObject go)
        {
            // Define the light prefab path based on the current render pipeline
            string lightPrefabPath = PlateauToolkitRenderingPaths.k_ObstacleLightPrefabPathUrp;
#if UNITY_HDRP
    lightPrefabPath = PlateauToolkitRenderingPaths.k_ObstacleLightPrefabPathHdrp;
#endif

            // Load the light prefab
            GameObject lightPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(lightPrefabPath);

            // Check if the light prefab exists
            if (lightPrefab == null)
            {
                Debug.LogWarning("Obstacle light prefab not found at path: " + lightPrefabPath);
                return;
            }

            // Get the MeshFilter component from the GameObject
            MeshFilter meshFilter = go.GetComponent<MeshFilter>();
            if (meshFilter == null)
            {
                return;
            }

            // Calculate the bounding box of the building
            Bounds bounds = meshFilter.sharedMesh.bounds;
            float buildingHeightWorld = bounds.size.y * go.transform.lossyScale.y;

            // Check if the building is taller than 60m
            if (buildingHeightWorld > 60)
            {
                // Get roof triangles facing upwards
                List<int> listofRoofTriangles = PlateauRenderingMeshUtilities.SelectFacesFacingUp(meshFilter.sharedMesh, 15);

                // Get the boundary edges of the roof
                List<Tuple<int, int>> roofBoundaryEdges = PlateauRenderingMeshUtilities.GetBoundaryEdges(listofRoofTriangles);

                // Extract vertices that form the outline of the roof
                List<Vector3> roofVerticesLocal = GetRoofOutlineVertices(meshFilter.sharedMesh, roofBoundaryEdges);
                List<Vector3> roofVertices = roofVerticesLocal.Select(v => go.transform.TransformPoint(v)).ToList();

                // Determine if we should use all mesh vertices instead
                float avgHeight = roofVertices.Average(v => v.y);
                if (avgHeight < buildingHeightWorld * 0.7)
                {
                    roofVertices = meshFilter.sharedMesh.vertices.Select(v => go.transform.TransformPoint(v)).ToList();
                }

                // Calculate the minimum bounding box of the roof
                List<Vector3> corners = GetMinimumBoundingBoxOfRoof(go);
                float buildingMaxYWorld = go.transform.position.y + (bounds.max.y * go.transform.lossyScale.y);

                // Adjust corner vertices to the top of the building
                for (int i = 0; i < corners.Count; i++)
                {
                    Vector3 corner = corners[i];
                    corners[i] = new Vector3(corner.x, buildingMaxYWorld, corner.z);
                }

                // For each corner, identify the closest vertex on the roof
                var closestVertices = new Vector3[4];
                for (int i = 0; i < 4; i++)
                {
                    float minDistance = float.MaxValue;
                    Vector3 closestVertex = Vector3.zero;
                    foreach (Vector3 vertex in roofVertices)
                    {
                        float distance = Vector3.Distance(corners[i], vertex);
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            closestVertex = vertex;
                        }
                    }

                    Vector3 cornerWithSameY = new Vector3(corners[i].x, corners[i].y, corners[i].z);
                    Vector3 direction = (cornerWithSameY - closestVertex).normalized;
                    closestVertices[i] = closestVertex + direction * -0.5f;
                }

                // Calculate the median height among the identified vertices
                List<float> heights = closestVertices.Select(v => v.y).OrderBy(v => v).ToList();
                float median;
                if (heights.Count % 2 == 0)
                {
                    median = (heights[heights.Count / 2 - 1] + heights[heights.Count / 2]) / 2;
                }
                else
                {
                    median = heights[heights.Count / 2];
                }

                // Exclude vertices that deviate significantly from the median height
                float threshold = Math.Abs(median * 0.5f);
                closestVertices = closestVertices.Where(v => Math.Abs(v.y - median) <= threshold).ToArray();

                // Create a new GameObject to store instantiated light prefabs
                Transform parent = meshFilter.transform;
                GameObject obstacleLightsNode = new GameObject("ObstacleLights");
                obstacleLightsNode.transform.SetParent(parent, false);

                // Instantiate and position the light prefabs
                foreach (Vector3 vertex in closestVertices)
                {
                    var light = PrefabUtility.InstantiatePrefab(lightPrefab) as GameObject;
                    light.transform.position = vertex;
                    light.transform.rotation = Quaternion.identity;
                    light.transform.SetParent(obstacleLightsNode.transform, true);
                    Undo.RegisterCreatedObjectUndo(light, "Place Lights On Building Corners");
                }
            }
        }

        static List<Vector3> GetRoofOutlineVertices(Mesh mesh, List<Tuple<int, int>> selectedEdges)
        {
            Vector3[] vertices = mesh.vertices;
            var roofVertices = new HashSet<Vector3>();

            foreach (Tuple<int, int> edge in selectedEdges)
            {
                Vector3 p0 = vertices[edge.Item1];
                Vector3 p1 = vertices[edge.Item2];

                roofVertices.Add(p0);
                roofVertices.Add(p1);
            }

            return roofVertices.ToList();
        }

        public static void SetWindowFlag(GameObject obj, bool isWindowOn)
        {
            int flagValue = isWindowOn ? 1 : 0;
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
            {
                Undo.RecordObject(renderer, "Set Window Flag");

                foreach (Material material in renderer.sharedMaterials)
                {
                    if (material.HasProperty("_IsWindow"))
                    {
                        Undo.RecordObject(material, "Set Window Flag");
                        material.SetInt("_IsWindow", flagValue);
                    }
                }
            }
            else
            {
                Debug.LogWarning("Renderer not found on object: " + obj.name);
            }
        }

        public static bool GetWindowFlag(GameObject obj)
        {
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
            {
                foreach (Material material in renderer.sharedMaterials)
                {
                    if (material.HasProperty("_IsWindow") && material.GetInt("_IsWindow") == 1)
                    {
                        return true;
                    }
                }
            }
            else
            {
                Debug.LogWarning("Renderer not found on object: " + obj.name);
            }
            return false;
        }

        public static void ChangeLOD2BuildingShader(GameObject obj)
        {
            string buildingShaderPath = "Shader Graphs/URP Building Shader";
#if UNITY_HDRP
            buildingShaderPath = "Shader Graphs/HDRP Building Shader";
#endif

            var newShader = Shader.Find(buildingShaderPath);
            if (newShader == null)
            {
                return;
            }

            Renderer renderer = obj.GetComponent<Renderer>();

            if (renderer != null)
            {
                Undo.RecordObject(renderer, "Change Lod2 Building Shader");

                foreach (Material material in renderer.sharedMaterials)
                {
                    // Check if the material has a main texture and if it's not null
                    if (material.mainTexture != null)
                    {
                        Undo.RecordObject(material, "Change Lod2 Building Shader");

                        // Store the previous shader for undo purposes
                        Shader previousShader = material.shader;

                        // Set the new shader
                        material.shader = newShader;

#if UNITY_URP
                        if (material.HasProperty("_AlphaClip"))
                        {
                            material.SetFloat("_AlphaClip", 1.0f);
                        }
#endif

#if UNITY_HDRP
                        if (material.HasProperty("_AlphaCutoffEnable"))
                        {
                            material.SetFloat("_AlphaCutoffEnable", 1.0f);
                        }
#endif

                        // Set shader-specific parameters if they exist
                        if (material.HasProperty("_FrameTileX"))
                        {
                            material.SetFloat("_FrameTileX", 0.4f);
                        }

                        if (material.HasProperty("_FrameTileY"))
                        {
                            material.SetFloat("_FrameTileY", 0.1f);
                        }

                        if (material.HasProperty("_FrameSizeX"))
                        {
                            material.SetFloat("_FrameSizeX", 0.7f);
                        }

                        if (material.HasProperty("_FrameSizeY"))
                        {
                            material.SetFloat("_FrameSizeY", 0.3f);
                        }

                        if (material.HasProperty("_NightEmission"))
                        {
                            material.SetFloat("_NightEmission", 0.35f);
                        }

                        if (material.HasProperty("_BaseMapOpacity"))
                        {
                            material.SetFloat("_BaseMapOpacity", 0.95f);
                        }

#if UNITY_URP
                        BaseShaderGUI.SetMaterialKeywords(material);
#endif

                    }
                }
            }
            else
            {
                Debug.LogWarning("Renderer not found on object: " + obj.name);
            }
        }
        public static void SetBuildingVertexColorForWindow(Mesh mesh, Bounds boundingBox, GameObject go, float maskPercentage = 0.2f, int? seed = null)
        {
            var colors = new Color[mesh.vertexCount];
            float largeFaceThreshold = 0.3f * boundingBox.size.y;

            // Find the minimum and maximum vertex heights in the mesh.
            float minY = float.PositiveInfinity;
            float maxY = float.NegativeInfinity;

            // Create a set to hold the world coordinates of upward facing vertices.
            HashSet<Vector3> upwardVertices = new HashSet<Vector3>();

            for (int i = 0; i < mesh.vertices.Length; i++)
            {
                Vector3 worldV = go.transform.TransformPoint(mesh.vertices[i]);
                minY = Mathf.Min(minY, worldV.y);
                maxY = Mathf.Max(maxY, worldV.y);

                // If the vertex is facing upwards and is at least 80% of the height of the object, add its world position to the set.
                if (mesh.normals[i].y > 0.9f && worldV.y >= minY + (1.0f - maskPercentage) * (maxY - minY))
                {
                    upwardVertices.Add(worldV);
                }
            }

            // Set random seed if provided
            if (seed.HasValue)
            {
                Random.InitState(seed.Value);
            }

            // Generate a single random alpha value for all vertices
            float randomAlpha = Random.Range(0f, 1f);

            for (int i = 0; i < mesh.vertices.Length; i++)
            {
                Vector3 worldVertex = go.transform.TransformPoint(mesh.vertices[i]);

                // Calculate normalized height based on the min and max heights of the mesh.
                float normalizedHeight = (worldVertex.y - minY) / (maxY - minY);

                // The gradient for green color, full green at the bottom fading to black at the top.
                Color vertexColor = (normalizedHeight < (1.0f - maskPercentage)) ? Color.Lerp(Color.green, Color.black, normalizedHeight) : Color.black;

                // Adjust the alpha for all colors to be the same random value.
                vertexColor.a = randomAlpha;

                colors[i] = vertexColor;
            }

            // Assign the colors back to the mesh.
            mesh.colors = colors;
        }

        public static void CreatePlaneUnderBuilding(GameObject building)
        {
            // Ensure the selectedBuilding and materialPath is valid
            if (building == null)
            {
                return;
            }

            string materialPath = PlateauToolkitRenderingPaths.k_FloorEmissionMaterialPathUrp;

#if UNITY_HDRP
            materialPath = PlateauToolkitRenderingPaths.k_FloorEmissionMaterialPathHdrp;
#endif

            Material floorMaterial = AssetDatabase.LoadAssetAtPath<Material>(materialPath);

            if (floorMaterial == null)
            {
                Debug.LogWarning("Floor Emission prefab not found at path: " + materialPath);
                return;
            }
            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            {
                Debug.LogWarning("Floor Emission Material not found at given path!");
                return;
            }

            // Get the minimum bounding box of the building's roof
            List<Vector3> boundingBox = GetMinimumBoundingBoxOfRoof(building);
            if (boundingBox == null || boundingBox.Count != 4)
            {
                return;
            }

            // Calculate the area of the bounding box
            float boundingBoxArea = Vector3.Cross(boundingBox[1] - boundingBox[0], boundingBox[2] - boundingBox[0]).magnitude * 0.5f;
            boundingBoxArea += Vector3.Cross(boundingBox[2] - boundingBox[0], boundingBox[3] - boundingBox[0]).magnitude * 0.5f;

            // If the area is smaller than the minimum, do not create the plane
            if (boundingBoxArea < 1.0f)
            {
                return;
            }

            // Create a plane under the selected building based on the bounding box
            Vector3 position = building.transform.position;
            position.y -= building.GetComponent<MeshFilter>().sharedMesh.bounds.size.y / 2;

            // Create a new GameObject which will be the parent of the plane
            var parentObject = new GameObject("FloorEmissionParent");

            // Set the position of the parent object to be the bottom of the building's bounding box in world space
            MeshFilter buildingMeshFilter = building.GetComponent<MeshFilter>();
            Bounds bounds = buildingMeshFilter.sharedMesh.bounds;
            Vector3 boundingBoxCenterLocal = bounds.center;
            Vector3 boundingBoxBottomLocal = boundingBoxCenterLocal - new Vector3(0, bounds.extents.y, 0); // Calculate the bottom of the bounding box in local space
            Vector3 boundingBoxBottomWorld = building.transform.TransformPoint(boundingBoxBottomLocal); // Convert from local to world space

            parentObject.transform.position = boundingBoxBottomWorld;

            // Create a new plane GameObject and configure it
            var plane = new GameObject("FloorEmission");
            plane.transform.SetParent(parentObject.transform); // Make the plane a child of the parent object
            plane.transform.localPosition = new Vector3(0.0f, 0.5f, 0.0f); // Set the position of the plane to be the same as its parent

            MeshFilter meshFilter = plane.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = PlateauRenderingGeomUtilities.CreateMeshFromBoundingBox(boundingBox);

            var planeFaces = new List<int>(meshFilter.sharedMesh.GetTriangles(0));

            // Assign the material
            MeshRenderer meshRenderer = plane.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = material;

            // Disable the casting of shadows
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            // Scale the parent object
            parentObject.transform.localScale = new Vector3(1.8f, 1.0f, 1.8f);

            // Make the parent object a child of the selectedBuilding
            parentObject.transform.parent = building.transform;

            // Record the plane creation for the Undo system
            Undo.RegisterCreatedObjectUndo(parentObject, "Create Floor Emission Plane");

            return;
        }
    }
}
