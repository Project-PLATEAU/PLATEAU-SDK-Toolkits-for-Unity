using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PlateauToolkit.Rendering.Editor
{
    static class PlateauRenderingMeshUtilities
    {
        // Create a new instance of a mesh
        public static Mesh CreateMeshInstance(Mesh inputMesh, bool recalculateNormals)
        {
            var newInstance = new Mesh
            {
                vertices = inputMesh.vertices,
                triangles = inputMesh.triangles,
                uv = inputMesh.uv,
                normals = inputMesh.normals,
                tangents = inputMesh.tangents,
                colors = inputMesh.colors
            };

            // Make sure the new mesh is properly set up
            newInstance.RecalculateBounds();
            if (recalculateNormals)
            {
                newInstance.RecalculateNormals();
            }

            newInstance.Optimize();

            return newInstance;
        }

        // Select faces facing up within a tolerance angle
        public static List<int> SelectFacesFacingUp(Mesh mesh, float toleranceAngle)
        {
            Vector3[] normals = mesh.normals;
            int[] triangles = mesh.triangles;
            var selectedFaces = new List<int>();

            for (int i = 0; i < triangles.Length; i += 3)
            {
                Vector3 faceNormal = (normals[triangles[i]] + normals[triangles[i + 1]] + normals[triangles[i + 2]]) /
                                     3f;

                float angle = Vector3.Angle(Vector3.up, faceNormal);
                if (angle <= toleranceAngle)
                {
                    selectedFaces.Add(triangles[i]);
                    selectedFaces.Add(triangles[i + 1]);
                    selectedFaces.Add(triangles[i + 2]);
                }
            }

            return selectedFaces;
        }

        // Select faces facing down within a tolerance angle
        public static List<int> SelectFacesFacingDown(Mesh mesh, float toleranceAngle)
        {
            Vector3[] normals = mesh.normals;
            int[] triangles = mesh.triangles;
            var selectedFaces = new List<int>();

            for (int i = 0; i < triangles.Length; i += 3)
            {
                Vector3 faceNormal = (normals[triangles[i]] + normals[triangles[i + 1]] + normals[triangles[i + 2]]) /
                                     3f;

                float angle = Vector3.Angle(Vector3.down, faceNormal);
                if (angle <= toleranceAngle)
                {
                    selectedFaces.Add(triangles[i]);
                    selectedFaces.Add(triangles[i + 1]);
                    selectedFaces.Add(triangles[i + 2]);
                }
            }

            return selectedFaces;
        }

        // Invert the selection of triangles
        public static List<int> InvertSelectedTriangles(Mesh mesh, List<int> selectedTriangles)
        {
            int[] triangles = mesh.triangles;
            var invertedSelection = new List<int>();

            for (int i = 0; i < triangles.Length; i += 3)
            {
                if (!selectedTriangles.Contains(triangles[i]) ||
                    !selectedTriangles.Contains(triangles[i + 1]) ||
                    !selectedTriangles.Contains(triangles[i + 2]))
                {
                    invertedSelection.Add(triangles[i]);
                    invertedSelection.Add(triangles[i + 1]);
                    invertedSelection.Add(triangles[i + 2]);
                }
            }

            return invertedSelection;
        }

        // Change the vertex color of the specified triangles, retaining the original vertex colors of other triangles
        public static void SetTriangleVertexColors(Mesh mesh, List<int> selectedTriangles, Color color)
        {
            int[] triangles = mesh.triangles;
            Color[] originalColors = mesh.colors;

            if (originalColors.Length == 0)
            {
                originalColors = new Color[mesh.vertexCount];
                for (int i = 0; i < originalColors.Length; i++)
                {
                    originalColors[i] = Color.white;
                }
            }

            var newColors = (Color[])originalColors.Clone();

            foreach (int vertexIndex in selectedTriangles)
            {
                newColors[vertexIndex] = color;
            }

            mesh.colors = newColors;
        }

        // Set the vertex color of the specified edges, retaining the original vertex colors of other vertices
        public static void SetEdgeColors(Mesh mesh, List<Tuple<int, int>> selectedEdges, Color color)
        {
            Color[] originalColors = mesh.colors;

            if (selectedEdges == null)
            {
                return;
            }

            if (originalColors.Length == 0)
            {
                originalColors = new Color[mesh.vertexCount];
                for (int i = 0; i < originalColors.Length; i++)
                {
                    originalColors[i] = Color.white;
                }
            }

            var newColors = (Color[])originalColors.Clone();

            foreach (Tuple<int, int> edge in selectedEdges)
            {
                newColors[edge.Item1] = color;
                newColors[edge.Item2] = color;
            }

            mesh.colors = newColors;
        }

        public static void SetRandomAlpha(Mesh mesh)
        {
            // Generate a single random alpha value for the entire mesh
            float randomAlpha = Random.Range(0f, 1f);

            // Retrieve the original colors
            Color[] originalColors = mesh.colors;

            // If no colors are set yet, default them to white
            if (originalColors.Length == 0)
            {
                originalColors = new Color[mesh.vertexCount];
                for (int i = 0; i < originalColors.Length; i++)
                {
                    originalColors[i] = Color.white;
                }
            }

            // Clone the colors array to keep the original colors intact
            var newColors = (Color[])originalColors.Clone();

            // Apply the same random alpha value to all vertices
            for (int i = 0; i < newColors.Length; i++)
            {
                Color color = newColors[i];
                newColors[i] = new Color(color.r, color.g, color.b, randomAlpha);
            }

            // Apply the new colors to the mesh
            mesh.colors = newColors;
        }

        public static void UnwrapUVs(Mesh mesh, float tileSize)
        {
            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;
            var uvs = new Vector2[vertices.Length];

            for (int i = 0; i < triangles.Length; i += 3)
            {
                Vector3 v0 = vertices[triangles[i]];
                Vector3 v1 = vertices[triangles[i + 1]];
                Vector3 v2 = vertices[triangles[i + 2]];

                Vector3 edge1 = v1 - v0;
                Vector3 edge2 = v2 - v0;

                Vector3 normal = Vector3.Cross(edge1, edge2).normalized;

                // Check if the face is a roof (Y-axis)
                if (Mathf.Abs(normal.y) > 0.9f)
                {
                    uvs[triangles[i]] = new Vector2(v0.x / tileSize, v0.z / tileSize);
                    uvs[triangles[i + 1]] = new Vector2(v1.x / tileSize, v1.z / tileSize);
                    uvs[triangles[i + 2]] = new Vector2(v2.x / tileSize, v2.z / tileSize);
                }
                else
                {
                    Vector3 uAxis, vAxis;

                    if (Mathf.Abs(normal.x) > 0.9f)
                    {
                        uAxis = new Vector3(0, 0, 1);
                        vAxis = new Vector3(0, 1, 0);
                    }
                    else
                    {
                        uAxis = new Vector3(1, 0, 0);
                        vAxis = new Vector3(0, 1, 0);
                    }

                    float uCoord0 = Vector3.Dot(v0, uAxis);
                    float uCoord1 = Vector3.Dot(v1, uAxis);
                    float uCoord2 = Vector3.Dot(v2, uAxis);

                    float vCoord0 = Vector3.Dot(v0, vAxis);
                    float vCoord1 = Vector3.Dot(v1, vAxis);
                    float vCoord2 = Vector3.Dot(v2, vAxis);

                    uvs[triangles[i]] = new Vector2(uCoord0 / tileSize, vCoord0 / tileSize);
                    uvs[triangles[i + 1]] = new Vector2(uCoord1 / tileSize, vCoord1 / tileSize);
                    uvs[triangles[i + 2]] = new Vector2(uCoord2 / tileSize, vCoord2 / tileSize);
                }
            }

            mesh.uv = uvs;
        }

        // Create triplanar UVs for selected triangles of a mesh and leave the non-selected UVs as they were before
        public static void CreateTriplanarUVsForSelectedTriangles(Mesh mesh, List<int> selectedTriangles,
            float tileSize)
        {
            Vector3[] vertices = mesh.vertices;
            Vector3[] normals = mesh.normals;
            Vector2[] uvs = mesh.uv;

            var selectedVertices = new HashSet<int>(selectedTriangles);

            for (int i = 0; i < selectedTriangles.Count; i += 3)
            {
                for (int j = 0; j < 3; j++)
                {
                    int vertexIndex = selectedTriangles[i + j];
                    if (!selectedVertices.Contains(vertexIndex))
                    {
                        continue;
                    }

                    Vector3 v = vertices[vertexIndex];
                    Vector3 normal = normals[vertexIndex];
                    Vector2 newUV;

                    float x = Mathf.Abs(normal.x);
                    float y = Mathf.Abs(normal.y);
                    float z = Mathf.Abs(normal.z);

                    if (x >= y && x >= z)
                    {
                        newUV = new Vector2(v.x / tileSize, v.y / tileSize);
                    }
                    else if (y >= x && y >= z)
                    {
                        newUV = new Vector2(v.x / tileSize, v.z / tileSize);
                    }
                    else
                    {
                        newUV = new Vector2(-v.z / tileSize, v.y / tileSize);
                    }

                    uvs[vertexIndex] = newUV;
                    selectedVertices.Remove(vertexIndex);
                }
            }

            mesh.uv = uvs;
        }

        // Extrude the selected edges in a given direction and distance, directly editing the input mesh
        public static List<Tuple<int, int>> ExtrudeEdges(Mesh mesh, List<Tuple<int, int>> selectedEdges,
            Vector3 direction, float distance)
        {
            Vector3 extrudeDirection = direction.normalized * distance;
            Vector3[] vertices = mesh.vertices;
            var newTriangles = new List<int>(mesh.triangles);
            var vertexMap = new Dictionary<int, int>();
            var newEdges = new List<Tuple<int, int>>();

            foreach (Tuple<int, int> edge in selectedEdges)
            {
                int v1 = edge.Item1;
                int v2 = edge.Item2;

                if (!vertexMap.ContainsKey(v1))
                {
                    vertexMap[v1] = vertices.Length;
                    Array.Resize(ref vertices, vertices.Length + 1);
                    vertices[vertexMap[v1]] = vertices[v1] + extrudeDirection;
                }

                if (!vertexMap.ContainsKey(v2))
                {
                    vertexMap[v2] = vertices.Length;
                    Array.Resize(ref vertices, vertices.Length + 1);
                    vertices[vertexMap[v2]] = vertices[v2] + extrudeDirection;
                }

                newTriangles.Add(v1);
                newTriangles.Add(v2);
                newTriangles.Add(vertexMap[v2]);

                newTriangles.Add(v1);
                newTriangles.Add(vertexMap[v2]);
                newTriangles.Add(vertexMap[v1]);

                newEdges.Add(new Tuple<int, int>(vertexMap[v1], vertexMap[v2]));
            }

            mesh.vertices = vertices;
            mesh.triangles = newTriangles.ToArray();

            return newEdges;
        }

        //For UV unwrapping of building roofs
        public static void FlattenUVsOnBoundingBox(GameObject building, Mesh mesh, List<int> selectedTriangles)
        {
            if (mesh == null)
            {
                return;
            }

            // Get the minimum bounding box of the roof
            List<Vector3> boundingBox = PlateauRenderingBuildingUtilities.GetMinimumBoundingBoxOfRoof(building);

            // Calculate basis vectors for the bounding box
            Vector3 right = (boundingBox[1] - boundingBox[0]).normalized;
            Vector3 up = (boundingBox[3] - boundingBox[0]).normalized;

            // Calculate dimensions of the bounding box
            float width = (boundingBox[1] - boundingBox[0]).magnitude;
            float height = (boundingBox[3] - boundingBox[0]).magnitude;

            Vector2[] uvs = mesh.uv;

            for (int i = 0; i < selectedTriangles.Count; i++)
            {
                Vector3 v = mesh.vertices[selectedTriangles[i]];

                // Project vertex onto the bounding box
                Vector3 projected = boundingBox[0] + Vector3.Project(v - boundingBox[0], right) + Vector3.Project(v - boundingBox[0], up);

                // Calculate UVs based on the projected point, scaled by the dimensions of the bounding box
                var uv = new Vector2(Vector3.Dot(projected - boundingBox[0], right) / width, Vector3.Dot(projected - boundingBox[0], up) / height);
                uvs[selectedTriangles[i]] = uv;
            }

            mesh.uv = uvs;
        }

        //For UV unwrapping of emmisive floor
        public static void FlattenUVsWithBoundingBox(GameObject building, Mesh mesh, List<Vector3> boundingBox)
        {
            // Ensure we have a valid mesh
            if (mesh == null)
            {
                return;
            }

            // Calculate basis vectors for the bounding box
            Vector3 right = (boundingBox[1] - boundingBox[0]).normalized;
            Vector3 up = (boundingBox[3] - boundingBox[0]).normalized;

            // Calculate dimensions of the bounding box
            float width = (boundingBox[1] - boundingBox[0]).magnitude;
            float height = (boundingBox[3] - boundingBox[0]).magnitude;

            var uvs = new Vector2[mesh.vertexCount];

            for (int i = 0; i < mesh.vertexCount; i++)
            {
                // Convert the local space vertex position to world space
                Vector3 v = building.transform.TransformPoint(mesh.vertices[i]);

                // Project vertex onto the bounding box
                Vector3 projected = boundingBox[0] + Vector3.Project(v - boundingBox[0], right) + Vector3.Project(v - boundingBox[0], up);

                // Calculate UVs based on the projected point, scaled by the dimensions of the bounding box
                var uv = new Vector2(Vector3.Dot(projected - boundingBox[0], right) / width, Vector3.Dot(projected - boundingBox[0], up) / height);
                uvs[i] = uv;
            }

            mesh.uv = uvs;
        }

        public static List<List<Tuple<int, int>>> SortEdgesByConnection(List<Tuple<int, int>> selectedEdges, Mesh mesh)
        {
            List<Vector3> vertices = new List<Vector3>(mesh.vertices);
            var remainingEdges = new List<Tuple<int, int>>(selectedEdges);
            var sortedEdgeGroups = new List<List<Tuple<int, int>>>();
            int currentPointIndex = 0;

            List<Vector3> connectionPoints = GetConnectionPoints(selectedEdges.Select(edge => new Tuple<Vector3, Vector3>(vertices[edge.Item1], vertices[edge.Item2])).ToList());
            List<Vector3> sortedPoints = PlateauRenderingGeomUtilities.SortVerticesInClockwiseOrder(connectionPoints);
            var sortedIndices = sortedPoints.Select(point => vertices.IndexOf(point)).ToList();

            int previousRemainingEdgesCount = -1;
            while (remainingEdges.Count > 0)
            {
                if (previousRemainingEdgesCount == remainingEdges.Count)
                {
                    remainingEdges.Clear();
                    break;
                }

                var sortedEdgeList = new List<Tuple<int, int>>();
                int currentEdgeEndPoint = -1;

                while (currentPointIndex < sortedIndices.Count)
                {
                    bool edgeFound = false;

                    for (int nextPointIndex = 0; nextPointIndex < sortedIndices.Count; nextPointIndex++)
                    {
                        if (nextPointIndex == currentPointIndex)
                        {
                            continue;
                        }


                        Tuple<int, int> foundEdge = null;

                        float tolerance = 0.0001f;

                        foreach (Tuple<int, int> edge in remainingEdges)
                        {

                            currentEdgeEndPoint = edge.Item2;
                            if (Vector3.Distance(vertices[sortedIndices[currentPointIndex]], vertices[edge.Item1]) < tolerance &&
                                Vector3.Distance(vertices[sortedIndices[nextPointIndex]], vertices[edge.Item2]) < tolerance)
                            {
                                foundEdge = edge;
                            }
                            else if (Vector3.Distance(vertices[sortedIndices[currentPointIndex]], vertices[edge.Item2]) < tolerance &&
                                     Vector3.Distance(vertices[sortedIndices[nextPointIndex]], vertices[edge.Item1]) < tolerance)
                            {
                                foundEdge = new Tuple<int, int>(edge.Item2, edge.Item1);
                            }

                            if (foundEdge != null)
                            {
                                remainingEdges.Remove(edge); // Remove the original edge, regardless of orientation.
                                sortedEdgeList.Add(foundEdge); // Add the possibly reversed edge to the sorted list.
                                edgeFound = true;
                                currentPointIndex = nextPointIndex;
                                nextPointIndex = 0;
                                break;
                            }
                        }

                        if (edgeFound)
                        {
                            break;
                        }
                    }

                    if (!edgeFound)
                    {
                        if (sortedEdgeList.Count == 0) // If currentEdge is isolated
                        {
                            remainingEdges.RemoveAll(edge => edge.Item1 == currentEdgeEndPoint || edge.Item2 == currentEdgeEndPoint);
                        }

                        currentPointIndex++;
                        break;
                    }
                }
                sortedEdgeGroups.RemoveAll(group => group.Count == 0);
                sortedEdgeGroups.Add(sortedEdgeList);

                previousRemainingEdgesCount = remainingEdges.Count;
            }

            return sortedEdgeGroups;
        }

        public static List<Vector3> GetConnectionPoints(List<Tuple<Vector3, Vector3>> edges)
        {
            var points = new List<Vector3>();
            foreach (Tuple<Vector3, Vector3> edge in edges)
            {
                points.Add(edge.Item1);
                points.Add(edge.Item2);
            }

            var connectionPoints = points.GroupBy(p => p)
                                         .Where(group => group.Count() > 1)
                                         .Select(group => group.Key)
                                         .ToList();

            return connectionPoints;
        }

        public struct Triangle
        {
            public Vector3 m_V0, m_V1, m_V2;

            public Triangle(Vector3 v0, Vector3 v1, Vector3 v2)
            {
                m_V0 = v0;
                m_V1 = v1;
                m_V2 = v2;
            }
        }

        public static bool RayIntersectsTriangle(Vector3 rayOrigin, Vector3 rayDirection, Triangle triangle)
        {
            const float epsilon = 0.000001f;
            Vector3 edge1, edge2, h, s, q;
            float a, f, u, v;

            edge1 = triangle.m_V1 - triangle.m_V0;
            edge2 = triangle.m_V2 - triangle.m_V0;

            h = Vector3.Cross(rayDirection, edge2);
            a = Vector3.Dot(edge1, h);

            if (a > -epsilon && a < epsilon)
            {
                return false; // Ray is parallel to the triangle
            }

            f = 1.0f / a;
            s = rayOrigin - triangle.m_V0;
            u = f * Vector3.Dot(s, h);

            if (u < 0.0f || u > 1.0f)
            {
                return false;
            }

            q = Vector3.Cross(s, edge1);
            v = f * Vector3.Dot(rayDirection, q);

            if (v < 0.0f || u + v > 1.0f)
            {
                return false;
            }

            // At this stage, we can compute the intersection point
            float t = f * Vector3.Dot(edge2, q);

            if (t > epsilon) // Ray intersection
            {
                return true;
            }
            else // This means that there is a line intersection, but not a ray intersection
            {
                return false;
            }
        }

        static Vector3 CalculateIntersection(Vector3 line1Start, Vector3 line1End, Vector3 line2Start, Vector3 line2End)
        {
            float denominator = ((line1End.x - line1Start.x) * (line2End.z - line2Start.z)) - ((line1End.z - line1Start.z) * (line2End.x - line2Start.x));

            if (Mathf.Abs(denominator) < 1e-6f) // ���������s�ł���ꍇ�A�����_�͑��݂��܂���B
            {
                return Vector3.zero;
            }

            float t = (((line1Start.z - line2Start.z) * (line2End.x - line2Start.x)) - ((line1Start.x - line2Start.x) * (line2End.z - line2Start.z))) / denominator;
            float u = -(((line1Start.x - line1End.x) * (line1Start.z - line2Start.z)) - ((line1Start.z - line1End.z) * (line1Start.x - line2Start.x))) / denominator;

            if (t >= 0 && t <= 1 && u >= 0 && u <= 1)
            {
                float x = line1Start.x + t * (line1End.x - line1Start.x);
                float z = line1Start.z + t * (line1End.z - line1Start.z);
                return new Vector3(x, line1Start.y, z);
            }

            return Vector3.zero;
        }

        public struct OffsetRoofEdgesResult
        {
            public List<Tuple<int, int>> NewEdges { get; set; }
            public List<Vector3> OuterEdgeVertices { get; set; }
            public List<Vector3> InnerEdgeVertices { get; set; }
        }

        public static OffsetRoofEdgesResult OffsetRoofEdges(Mesh mesh, List<Tuple<int, int>> selectedEdges, float offset, float height)
        {
            var outerEdgeVertices = new List<Vector3>();
            var innerEdgeVertices = new List<Vector3>();

            var vertices = new List<Vector3>(mesh.vertices);
            var triangles = new List<int>(mesh.triangles);
            var newEdges = new List<Tuple<int, int>>();
            var uvs = new List<Vector2>(mesh.uv);
            var normals = new List<Vector3>(mesh.normals);

            var sortedEdges = selectedEdges;
            int edgeCount = sortedEdges.Count;

            // Find the shortest edge length.
            float shortestEdgeLength = sortedEdges.Min(edge =>
            {
                Vector3 p0 = vertices[edge.Item1];
                Vector3 p1 = vertices[edge.Item2];
                return Vector3.Distance(p0, p1);
            });

            // Adjust the offset based on the shortest edge length.
            if (shortestEdgeLength < offset * 2.0f)
            {
                offset = shortestEdgeLength / 2;
            }

            int originalVertexCount = vertices.Count;

            var triangleList = new List<Triangle>();
            for (int i = 0; i < triangles.Count; i += 3)
            {
                var tri = new Triangle(vertices[triangles[i]], vertices[triangles[i + 1]], vertices[triangles[i + 2]]);
                triangleList.Add(tri);
            }

            var offsetList = new List<Tuple<Vector3, Vector3>>(edgeCount);
            var intersectionsList = new List<List<Vector3>>(edgeCount);

            for (int i = 0; i < sortedEdges.Count; i++)
            {
                Tuple<int, int> edge = sortedEdges[i];
                Vector3 p0 = vertices[edge.Item1] + Vector3.up * height;
                Vector3 p1 = vertices[edge.Item2] + Vector3.up * height;

                outerEdgeVertices.Add(p0);
                outerEdgeVertices.Add(p1);

                Vector3 v1 = p1 - p0;
                Vector3 rotatedVector = new Vector3(-v1.z, v1.y, v1.x).normalized * offset;

                Vector3 p0Offset = p0 + rotatedVector;
                Vector3 p1Offset = p1 + rotatedVector;

                Vector3 midpoint = (p0Offset + p1Offset) / 2;
                Vector3 rayOriginMidpoint = midpoint + new Vector3(0, 0.1f, 0);
                Vector3 rayDirection = Vector3.down;

                bool meshUnderMidpoint = false;

                foreach (Triangle triangle in triangleList)
                {
                    if (RayIntersectsTriangle(rayOriginMidpoint, rayDirection, triangle))
                    {
                        meshUnderMidpoint = true;
                        break;
                    }
                }

                if (!meshUnderMidpoint)
                {
                    p0Offset = p0 - rotatedVector;
                    p1Offset = p1 - rotatedVector;
                }

                Vector3 distance = (p1Offset - p0Offset).normalized;
                p0Offset = p0Offset - distance * 1.1f;
                p1Offset = p1Offset + distance * 1.1f;

                offsetList.Add(new Tuple<Vector3, Vector3>(p0Offset, p1Offset));

                intersectionsList.Add(new List<Vector3>());
            }

            // Calculate all intersections
            for (int i = 0; i < edgeCount; i++)
            {
                Vector3 p0Offset = offsetList[i].Item1;
                Vector3 p1Offset = offsetList[i].Item2;

                for (int j = 0; j < edgeCount; j++)
                {
                    if (j != i)
                    {
                        Vector3 otherP0 = offsetList[j].Item1;
                        Vector3 otherP1 = offsetList[j].Item2;

                        Vector3 intersection = CalculateIntersection(p0Offset, p1Offset, otherP0, otherP1);
                        if (intersection != Vector3.zero)
                        {
                            // Raycast from intersection point downwards
                            Vector3 rayOrigin = intersection + new Vector3(0, 0.1f, 0);
                            Vector3 rayDirection = Vector3.down;

                            bool meshUnderIntersection = false;

                            foreach (Triangle triangle in triangleList)
                            {
                                if (RayIntersectsTriangle(rayOrigin, rayDirection, triangle))
                                {
                                    meshUnderIntersection = true;
                                    break;
                                }
                            }

                            // If the raycast hits the mesh, add the intersection point to the list
                            if (meshUnderIntersection)
                            {
                                intersectionsList[i].Add(intersection);
                            }
                        }
                    }
                }
            }

            float edgeDistanceSum = 0;

            // Update mesh vertices and triangles using the nearest intersections
            for (int i = 0; i < edgeCount; i++)
            {
                Vector3 p0Offset = offsetList[i].Item1;
                Vector3 p1Offset = offsetList[i].Item2;

                List<Vector3> intersections = intersectionsList[i];

                // Find nearest intersection to p0Offset
                Vector3 nearestIntersection1 = intersections.OrderBy(intersection => Vector3.Distance(p0Offset, intersection)).FirstOrDefault();

                // Find nearest intersection to p1Offset
                Vector3 nearestIntersection2 = intersections.OrderBy(intersection => Vector3.Distance(p1Offset, intersection)).FirstOrDefault();

                if (nearestIntersection1 != Vector3.zero && nearestIntersection2 != Vector3.zero)
                {

                    innerEdgeVertices.Add(nearestIntersection1);
                    innerEdgeVertices.Add(nearestIntersection2);

                    // Add new vertices directly to the vertices list
                    vertices.Add(vertices[sortedEdges[i].Item1] + Vector3.up * height);
                    vertices.Add(vertices[sortedEdges[i].Item2] + Vector3.up * height);
                    vertices.Add(nearestIntersection1);
                    vertices.Add(nearestIntersection2);

                    // Get the indices of the newly added vertices
                    int newItem1Index = vertices.Count - 4;
                    int newItem2Index = vertices.Count - 3;
                    int intersectionIndex = vertices.Count - 2;
                    int intersection2Index = vertices.Count - 1;

                    // Calculate the current edge's distance
                    float currentEdgeDistance = Vector3.Distance(vertices[sortedEdges[i].Item1], vertices[sortedEdges[i].Item2]);
                    float currentEdgeDistance2 = Vector3.Distance(nearestIntersection1, nearestIntersection2);
                    float uvInnerOffset = currentEdgeDistance - currentEdgeDistance2;

                    // Add UVs for the new vertices
                    // U coordinate for offset vertices is 0, for original vertices is 1
                    // V coordinate is based on the actual edge distance
                    uvs.Add(new Vector2(1, edgeDistanceSum)); // for vertices[sortedEdges[i].Item1]
                    uvs.Add(new Vector2(1, edgeDistanceSum + currentEdgeDistance - uvInnerOffset)); // for vertices[sortedEdges[i].Item2]
                    uvs.Add(new Vector2(0, edgeDistanceSum)); // for nearestIntersection1
                    uvs.Add(new Vector2(0, edgeDistanceSum + currentEdgeDistance)); // for nearestIntersection2

                    // add corresponding normals for new vertices
                    normals.Add(mesh.normals[sortedEdges[i].Item1]);
                    normals.Add(mesh.normals[sortedEdges[i].Item2]);
                    normals.Add(Vector3.up);  // Assuming you want the normal to be up for new vertices
                    normals.Add(Vector3.up);  // Assuming you want the normal to be up for new vertices

                    // Update the sum of edge distances
                    edgeDistanceSum += currentEdgeDistance;

                    // First triangle (intersection points and new edge vertices)
                    triangles.Add(newItem1Index);
                    triangles.Add(intersectionIndex);
                    triangles.Add(newItem2Index);

                    // Second triangle (intersection points and new edge vertices)
                    triangles.Add(intersectionIndex);
                    triangles.Add(intersection2Index);
                    triangles.Add(newItem2Index);

                    // Check if normals are inverted
                    bool firstTriangleInverted = CheckIfNormalsAreInverted(vertices.ToArray(), triangles.Skip(triangles.Count - 6).Take(3).ToArray());
                    bool secondTriangleInverted = CheckIfNormalsAreInverted(vertices.ToArray(), triangles.Skip(triangles.Count - 3).Take(3).ToArray());

                    if (firstTriangleInverted)
                    {
                        triangles.Reverse(triangles.Count - 6, 3);
                    }
                    if (secondTriangleInverted)
                    {
                        triangles.Reverse(triangles.Count - 3, 3);
                    }

                    // Add the new edges to the newEdges list
                    newEdges.Add(new Tuple<int, int>(intersectionIndex, intersection2Index));
                    newEdges.Add(new Tuple<int, int>(newItem1Index, newItem2Index));
                }
                // Draw debug lines
                //Debug.DrawLine(nearestIntersection1 + Vector3.up * 0.5f, nearestIntersection1 + Vector3.up * 5.5f, Color.blue, 50f);
                //Debug.DrawLine(nearestIntersection2 + Vector3.up * 0.5f, nearestIntersection2 + Vector3.up * 5.5f, Color.yellow, 50f);
                //Debug.DrawLine(p0Offset + Vector3.up * 0.5f, p1Offset + Vector3.up * 0.5f, Color.red, 50f);
            }
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.uv = uvs.ToArray();
            mesh.normals = normals.ToArray();

            return new OffsetRoofEdgesResult
            {
                NewEdges = newEdges,
                OuterEdgeVertices = outerEdgeVertices,
                InnerEdgeVertices = innerEdgeVertices,
            };
        }
        public static void CreateRoofSideFaces(Mesh mesh, List<Vector3> outerEdgeVertices, float offsetHeight, bool flipNormals = false)
        {
            var newVertices = new List<Vector3>(mesh.vertices);
            var newTriangles = new List<int>(mesh.triangles);
            var newNormals = new List<Vector3>(mesh.normals);

            int originalVertexCount = mesh.vertexCount;

            EnsureClockwiseOrder(outerEdgeVertices);

            for (int i = 0; i < outerEdgeVertices.Count; i++)
            {
                var vertex = outerEdgeVertices[i];
                var newVertex = vertex + new Vector3(0, offsetHeight, 0);

                // Duplicate the vertex
                newVertices.Add(vertex);
                newVertices.Add(newVertex);

                // Copy the normals for the duplicated vertices
                newNormals.Add(mesh.normals[mesh.vertices.ToList().IndexOf(vertex)]);
                newNormals.Add(mesh.normals[mesh.vertices.ToList().IndexOf(vertex)]);
            }

            var triangleList = new List<PlateauRenderingMeshUtilities.Triangle>();
            for (int i = 0; i < newTriangles.Count; i += 3)
            {
                var tri = new PlateauRenderingMeshUtilities.Triangle(newVertices[newTriangles[i]], newVertices[newTriangles[i + 1]], newVertices[newTriangles[i + 2]]);
                triangleList.Add(tri);
            }


            for (int i = 0; i < outerEdgeVertices.Count; i++)
            {
                int originalIndex = originalVertexCount + 2 * i;
                int offsetIndex = originalIndex + 1;

                int nextOriginalIndex = originalVertexCount + 2 * ((i + 1) % outerEdgeVertices.Count);
                int nextOffsetIndex = nextOriginalIndex + 1;

                Vector3 v0 = newVertices[originalIndex];
                Vector3 v1 = newVertices[offsetIndex];
                Vector3 v2 = newVertices[nextOffsetIndex];
                Vector3 v3 = newVertices[nextOriginalIndex];

                // Assume that the up direction is along the Y-axis
                Vector3 upDirection = new Vector3(0, 1, 0);

                // Calculate the vectors for the edges
                Vector3 edgeNormal = v3 - v0;

                // Calculate the normal by taking the cross product of the up direction and the edge vectors
                edgeNormal = Vector3.Cross(edgeNormal, upDirection).normalized;


                if (flipNormals)
                {
                    newTriangles.Add(nextOffsetIndex);
                    newTriangles.Add(offsetIndex);
                    newTriangles.Add(originalIndex);

                    newTriangles.Add(nextOriginalIndex);
                    newTriangles.Add(nextOffsetIndex);
                    newTriangles.Add(originalIndex);
                }
                else
                {
                    newTriangles.Add(originalIndex);
                    newTriangles.Add(offsetIndex);
                    newTriangles.Add(nextOffsetIndex);

                    newTriangles.Add(originalIndex);
                    newTriangles.Add(nextOffsetIndex);
                    newTriangles.Add(nextOriginalIndex);
                }


                if (flipNormals)
                {
                    edgeNormal = -edgeNormal;
                }

                // Assign the same normal to the vertices of two triangles
                newNormals[originalIndex] = edgeNormal;
                newNormals[offsetIndex] = edgeNormal;
                newNormals[nextOffsetIndex] = edgeNormal;
                newNormals[nextOriginalIndex] = edgeNormal;

                mesh.vertices = newVertices.ToArray();
                mesh.triangles = newTriangles.ToArray();
                mesh.normals = newNormals.ToArray();
            }
        }
        public static List<Vector3> EnsureClockwiseOrder(List<Vector3> points)
        {
            // Compute the centroid.
            Vector3 centroid = Vector3.zero;
            foreach (Vector3 point in points)
            {
                centroid += point;
            }
            centroid /= points.Count;

            // Compute the signed area of the polygon.
            float area = 0;
            for (int i = 0; i < points.Count; i++)
            {
                Vector3 point1 = points[i];
                Vector3 point2 = points[(i + 1) % points.Count]; // Next point, with wraparound.
                area += (point2.x - point1.x) * (point2.z + point1.z); // For 2D points, use y instead of z.
            }

            // If the area is positive, the points are in clockwise order.
            if (area >= 0)
            {
                return points;
            }
            else // If the area is negative, the points are in counterclockwise order.
            {
                points.Reverse();
                return points;
            }
        }

        static bool CheckIfNormalsAreInverted(Vector3[] vertices, int[] triangleIndices)
        {
            Vector3 p1 = vertices[triangleIndices[0]];
            Vector3 p2 = vertices[triangleIndices[1]];
            Vector3 p3 = vertices[triangleIndices[2]];

            Vector3 normal = Vector3.Cross(p2 - p1, p3 - p1).normalized;

            if (Vector3.Dot(normal, Vector3.up) < 0)
            {
                return true;
            }

            return false;
        }

        static void SetNormalsToUp(Mesh mesh, int startIndex)
        {
            Vector3[] normals = mesh.normals;

            for (int i = startIndex; i < normals.Length; i++)
            {
                normals[i] = Vector3.up;
            }

            mesh.normals = normals;
        }

        public static List<Tuple<int, int>> GetBoundaryEdgesOfUpwardFacingFaces(Mesh mesh, float toleranceAngle)
        {
            Vector3[] vertices = mesh.vertices;
            Vector3[] normals = mesh.normals;
            int[] triangles = mesh.triangles;
            var selectedTriangles = new List<int>();
            var edgeCount = new Dictionary<Tuple<int, int>, int>();

            // Select faces facing up
            for (int i = 0; i < triangles.Length; i += 3)
            {
                var faceNormal = Vector3.Normalize(normals[triangles[i]] + normals[triangles[i + 1]] + normals[triangles[i + 2]]);

                float angle = Vector3.Angle(Vector3.up, faceNormal);
                if (angle <= toleranceAngle)
                {
                    selectedTriangles.Add(triangles[i]);
                    selectedTriangles.Add(triangles[i + 1]);
                    selectedTriangles.Add(triangles[i + 2]);
                }
            }

            // Calculate boundary edges of selected faces
            for (int i = 0; i < selectedTriangles.Count; i += 3)
            {
                for (int j = 0; j < 3; j++)
                {
                    int v1 = selectedTriangles[i + j];
                    int v2 = selectedTriangles[i + ((j + 1) % 3)];

                    Tuple<int, int> edge = v1 < v2 ? new Tuple<int, int>(v1, v2) : new Tuple<int, int>(v2, v1);

                    if (edgeCount.ContainsKey(edge))
                    {
                        edgeCount[edge]++;
                    }
                    else
                    {
                        edgeCount[edge] = 1;
                    }
                }
            }

            var boundaryEdges = new List<Tuple<int, int>>();
            foreach (KeyValuePair<Tuple<int, int>, int> entry in edgeCount)
            {
                if (entry.Value == 1)
                {
                    boundaryEdges.Add(entry.Key);
                }
            }

            return boundaryEdges;
        }

        // Get the boundary edges for a list of triangles that are part of the same face
        public static List<Tuple<int, int>> GetBoundaryEdges(List<int> selectedTriangles)
        {
            var edgeCount = new Dictionary<Tuple<int, int>, int>();

            for (int i = 0; i < selectedTriangles.Count; i += 3)
            {
                for (int j = 0; j < 3; j++)
                {
                    int v1 = selectedTriangles[i + j];
                    int v2 = selectedTriangles[i + ((j + 1) % 3)];

                    Tuple<int, int> edge = v1 < v2 ? new Tuple<int, int>(v1, v2) : new Tuple<int, int>(v2, v1);
                    if (edgeCount.ContainsKey(edge))
                    {
                        edgeCount[edge]++;
                    }
                    else
                    {
                        edgeCount[edge] = 1;
                    }
                }
            }

            var boundaryEdges = new List<Tuple<int, int>>();
            foreach (KeyValuePair<Tuple<int, int>, int> entry in edgeCount)
            {
                if (entry.Value == 1)
                {
                    boundaryEdges.Add(entry.Key);
                }
            }

            return boundaryEdges;
        }

        // Get all triangles of a selected color
        public static List<int> GetTrianglesOfSelectedColor(Mesh mesh, Color targetColor)
        {
            Color[] colors = mesh.colors;
            int[] triangles = mesh.triangles;
            var selectedTriangles = new List<int>();

            for (int i = 0; i < triangles.Length; i += 3)
            {
                bool allVerticesMatch = true;

                for (int j = 0; j < 3; j++)
                {
                    int vertexIndex = triangles[i + j];
                    Color vertexColor = colors[vertexIndex];

                    if (vertexColor != targetColor)
                    {
                        allVerticesMatch = false;
                        break;
                    }
                }

                if (allVerticesMatch)
                {
                    selectedTriangles.Add(triangles[i]);
                    selectedTriangles.Add(triangles[i + 1]);
                    selectedTriangles.Add(triangles[i + 2]);
                }
            }

            return selectedTriangles;
        }

        // Get a list of all the triangles in the mesh
        public static List<int> GetAllTriangles(Mesh mesh)
        {
            int[] triangles = mesh.triangles;
            var allTriangles = new List<int>(triangles);

            return allTriangles;
        }

        // Delete selected triangles from a mesh and return a list of boundary edges left behind
        public static List<Tuple<int, int>> DeleteSelectedTriangles(Mesh mesh, List<int> selectedTriangles)
        {
            int[] triangles = mesh.triangles;
            var newTriangles = new List<int>(triangles.Length - selectedTriangles.Count);
            var edgeCount = new Dictionary<Tuple<int, int>, int>();

            for (int i = 0; i < triangles.Length; i += 3)
            {
                var currentEdges = new List<Tuple<int, int>>
                {
                    new Tuple<int, int>(triangles[i], triangles[i + 1]),
                    new Tuple<int, int>(triangles[i + 1], triangles[i + 2]),
                    new Tuple<int, int>(triangles[i + 2], triangles[i])
                };

                if (selectedTriangles.Contains(triangles[i]) ||
                    selectedTriangles.Contains(triangles[i + 1]) ||
                    selectedTriangles.Contains(triangles[i + 2]))
                {
                    foreach (Tuple<int, int> edge in currentEdges)
                    {
                        if (edgeCount.ContainsKey(edge))
                        {
                            edgeCount[edge]++;
                        }
                        else
                        {
                            var reverseEdge = new Tuple<int, int>(edge.Item2, edge.Item1);
                            if (edgeCount.ContainsKey(reverseEdge))
                            {
                                edgeCount[reverseEdge]++;
                            }
                            else
                            {
                                edgeCount[edge] = 1;
                            }
                        }
                    }
                }
                else
                {
                    newTriangles.AddRange(currentEdges.Select(edge => edge.Item1));
                }
            }

            mesh.triangles = newTriangles.ToArray();
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            var boundaryEdges = edgeCount
                .Where(pair => pair.Value == 1)
                .Select(pair => pair.Key)
                .ToList();

            return boundaryEdges;
        }

        // Get Height of Mesh bounds
        public static float GetMeshHeight(Mesh mesh)
        {
            Bounds meshBounds = mesh.bounds;
            return meshBounds.size.y;
        }

        // return true if majority of mesh surface is pointing up (for example a road or ground)
        public static bool IsMajorityOfMeshPointingUp(Mesh mesh)
        {
            if (mesh == null)
            {
                return false;
            }

            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;

            float totalArea = 0f;
            float upwardArea = 0f;

            for (int i = 0; i < triangles.Length; i += 3)
            {
                Vector3 v0 = vertices[triangles[i]];
                Vector3 v1 = vertices[triangles[i + 1]];
                Vector3 v2 = vertices[triangles[i + 2]];

                Vector3 edge1 = v1 - v0;
                Vector3 edge2 = v2 - v0;

                var normal = Vector3.Cross(edge1, edge2);
                float area = 0.5f * normal.magnitude;

                totalArea += area;

                // Check if the face normal is pointing up (Y-axis)
                if (normal.y > 0)
                {
                    upwardArea += area;
                }
            }

            // Calculate the proportion of upward-facing area
            float upwardAreaProportion = upwardArea / totalArea;

            // Check if the majority of the surface area is pointing up
            return upwardAreaProportion > 0.5f;
        }

        public static void GetSelectedGameObjects(List<GameObject> results)
        {
            results.Clear();
            GameObject[] selected = Selection.gameObjects;
            foreach (GameObject obj in selected)
            {
                results.Add(obj);
            }
        }

        public static Material GetSubmaterialByLargestFaceArea(Renderer renderer, Mesh mesh)
        {
            // Check if the Renderer component exists
            if (renderer == null)
            {
                return null;
            }

            // Check if the sharedMaterials array is not empty
            if (renderer.sharedMaterials.Length == 0)
            {
                return null;
            }

            if (mesh.subMeshCount == 1)
            {
                // If there is only one submesh, return the last material.
                return renderer.sharedMaterials[renderer.sharedMaterials.Length - 1];
            }
            else
            {
                // Count the number of vertices for each submesh.
                int maxVertices = -1;
                int maxSubmeshIndex = -1;
                for (int i = 0; i < mesh.subMeshCount; i++)
                {
                    int[] subMeshTriangles = mesh.GetTriangles(i);
                    if (subMeshTriangles.Length > maxVertices)
                    {
                        maxVertices = subMeshTriangles.Length;
                        maxSubmeshIndex = i;
                    }
                }

                // Make sure the index is within the range of sharedMaterials array
                if (maxSubmeshIndex >= renderer.sharedMaterials.Length)
                {
                    return renderer.sharedMaterials[0];
                }

                // Only preserve the material of the submesh with the most vertices.
                Material maxMaterial = renderer.sharedMaterials[maxSubmeshIndex];
                return maxMaterial;
            }
        }

        static void AddChildrenToList(Transform parent, List<GameObject> list)
        {
            foreach (Transform child in parent)
            {
                list.Add(child.gameObject);
                AddChildrenToList(child, list);
            }
        }
        public static Mesh WeldVertices(Mesh mesh, MeshRenderer renderer,
            float mergeThresholdVertex = 0.1f,
            float mergeThresholdUV = 0.0001f,
            float mergeThresholdNormal = 10f,
            bool preserveUVs = true,
            bool preserveNormals = true)
        {

            var newMesh = new Mesh();
            Vector3[] vertices = mesh.vertices;
            Vector2[] uvs = mesh.uv;
            Vector3[] normals = mesh.normals;
            int[] triangles = mesh.triangles;

            // Only preserve the material of the submesh with the most vertices.
            Material preservedMaterial = GetSubmaterialByLargestFaceArea(renderer, mesh);

            var newVertices = new List<VertexData>();
            int[] remapIndices = new int[vertices.Length];

            for (int i = 0; i < vertices.Length; i++)
            {
                bool isDuplicate = false;
                for (int j = 0; j < newVertices.Count; j++)
                {
                    if (Vector3.Distance(vertices[i], newVertices[j].m_Position) < mergeThresholdVertex &&
                        (!preserveUVs || Vector2.Distance(uvs[i], newVertices[j].m_Uv) < mergeThresholdUV) &&
                        (!preserveNormals || Vector3.Angle(normals[i], newVertices[j].m_Normal) < mergeThresholdNormal))
                    {
                        isDuplicate = true;
                        remapIndices[i] = j;
                        break;
                    }
                }

                if (!isDuplicate)
                {
                    remapIndices[i] = newVertices.Count;
                    newVertices.Add(new VertexData(vertices[i], uvs[i], normals[i]));
                }
            }

            for (int i = 0; i < triangles.Length; i++)
            {
                triangles[i] = remapIndices[triangles[i]];
            }

            var finalVertices = new Vector3[newVertices.Count];
            var finalUVs = new Vector2[newVertices.Count];
            var finalNormals = new Vector3[newVertices.Count];

            for (int i = 0; i < newVertices.Count; i++)
            {
                finalVertices[i] = newVertices[i].m_Position;
                finalUVs[i] = newVertices[i].m_Uv;
                finalNormals[i] = newVertices[i].m_Normal;
            }

            renderer.materials = new Material[] { preservedMaterial };

            newMesh.Clear();
            newMesh.vertices = finalVertices;
            newMesh.uv = finalUVs;
            newMesh.normals = finalNormals;
            newMesh.triangles = triangles;
            newMesh.triangles = RemoveDuplicateTriangles(newMesh.triangles);

            newMesh.RecalculateBounds();
            newMesh.RecalculateNormals();

            return newMesh;
        }

        static int[] RemoveDuplicateTriangles(int[] triangles)
        {
            var triangleSet = new HashSet<Vector3Int>();
            for (int i = 0; i < triangles.Length; i += 3)
            {
                var triangle = new Vector3Int(triangles[i], triangles[i + 1], triangles[i + 2]);
                triangleSet.Add(triangle);
            }

            int[] newTriangles = new int[triangleSet.Count * 3];
            int index = 0;
            foreach (Vector3Int triangle in triangleSet)
            {
                newTriangles[index] = triangle.x;
                newTriangles[index + 1] = triangle.y;
                newTriangles[index + 2] = triangle.z;
                index += 3;
            }

            return newTriangles;
        }

        struct VertexData
        {
            public Vector3 m_Position;
            public Vector2 m_Uv;
            public Vector3 m_Normal;

            public VertexData(Vector3 pos, Vector2 uv, Vector3 normal)
            {
                m_Position = pos;
                m_Uv = uv;
                m_Normal = normal;
            }
        }

        public static GameObject CombineChildren(GameObject parent, bool shouldSeparateGrandChildren = false)
        {
            // Get only the direct children's MeshFilters and MeshRenderers.
            var meshFilters = new List<MeshFilter>();
            var meshRenderers = new List<MeshRenderer>();
            var grandChildren = new List<GameObject>();

            foreach (Transform child in parent.transform)
            {
                MeshFilter meshFilter = child.GetComponent<MeshFilter>();
                MeshRenderer meshRenderer = child.GetComponent<MeshRenderer>();
                if (meshFilter != null && meshRenderer != null)
                {
                    meshFilters.Add(meshFilter);
                    meshRenderers.Add(meshRenderer);
                }

                if (shouldSeparateGrandChildren)
                {
                    foreach (Transform grandChild in child.transform)
                    {
                        grandChildren.Add(grandChild.gameObject);
                    }
                }
            }

            var materialToMeshFilters = new Dictionary<string, List<MeshFilter>>();
            for (int i = 0; i < meshFilters.Count; i++)
            {
                string materialName = meshRenderers[i].sharedMaterial.name;
                if (!materialToMeshFilters.ContainsKey(materialName))
                {
                    materialToMeshFilters[materialName] = new List<MeshFilter>();
                }
                materialToMeshFilters[materialName].Add(meshFilters[i]);
            }

            MeshFilter parentMeshFilter = parent.GetComponent<MeshFilter>();
            if (parentMeshFilter == null)
            {
                parentMeshFilter = Undo.AddComponent<MeshFilter>(parent);  // Add the MeshFilter component with Undo.
            }

            MeshRenderer parentMeshRenderer = parent.GetComponent<MeshRenderer>();
            if (parentMeshRenderer == null)
            {
                parentMeshRenderer = Undo.AddComponent<MeshRenderer>(parent);  // Add the MeshRenderer component with Undo.
            }

            var newMesh = new Mesh();
            var combineInstances = new List<CombineInstance>();
            var materials = new List<Material>();

            foreach (KeyValuePair<string, List<MeshFilter>> pair in materialToMeshFilters)
            {
                CombineInstance[] combineInstancesForMaterial = pair.Value.Select(meshFilter => new CombineInstance
                {
                    mesh = meshFilter.sharedMesh,
                    transform = meshFilter.transform.localToWorldMatrix
                }).ToArray();

                var combinedMeshForMaterial = new Mesh();
                combinedMeshForMaterial.CombineMeshes(combineInstancesForMaterial, true);

                var combineInstance = new CombineInstance
                {
                    mesh = combinedMeshForMaterial,
                    transform = parent.transform.worldToLocalMatrix
                };
                combineInstances.Add(combineInstance);

                materials.Add(meshRenderers.First(mr => mr.sharedMaterial.name == pair.Key).sharedMaterial);
            }

            newMesh.CombineMeshes(combineInstances.ToArray(), false);

            Undo.RecordObject(parentMeshFilter, "Combine Meshes");  // Record the mesh combining for Undo.
            parentMeshFilter.sharedMesh = newMesh;

            Undo.RecordObject(parentMeshRenderer, "Change Material");  // Record the material change for Undo.
            parentMeshRenderer.sharedMaterials = materials.ToArray();

            if (shouldSeparateGrandChildren)
            {
                // Create a new empty GameObject to hold all the grand children.
                var newParentForGrandChildren = new GameObject("BuildingComponents");
                Undo.RegisterCreatedObjectUndo(newParentForGrandChildren, "Create New Parent For GrandChildren");
                newParentForGrandChildren.transform.SetParent(parent.transform, false);

                // Loop through all the grandChildren and set their parent to the newParentForGrandChildren
                foreach (GameObject grandChild in grandChildren)
                {
                    // This will set the parent of the grandChild to newParentForGrandChildren, with undo support
                    Undo.SetTransformParent(grandChild.transform, newParentForGrandChildren.transform, "Move GrandChild");
                }
            }

            // Remove the original children after their meshes are combined in parent
            foreach (MeshFilter meshFilter in meshFilters)
            {
                Undo.DestroyObjectImmediate(meshFilter.gameObject);
            }

            return parent;
        }

        public static List<GameObject> SeparateMesh(GameObject parentGameObject)
        {
            // Prepare the original mesh and its data
            MeshRenderer meshRenderer = parentGameObject.GetComponent<MeshRenderer>();
            MeshFilter meshFilter = parentGameObject.GetComponent<MeshFilter>();
            Mesh originalMesh = meshFilter.sharedMesh;

            // Record the parent game object for undo
            Undo.RecordObject(parentGameObject, "Separate Mesh");
            Undo.RecordObject(meshRenderer, "Separate Mesh");
            Undo.RecordObject(meshFilter, "Separate Mesh");

            Transform parentOfParent = parentGameObject.transform.parent.parent;

            parentGameObject.GetComponent<MeshFilter>().sharedMesh = null;

            // Convert triangles and vertices to lists for easier manipulation
            var triangles = originalMesh.triangles.ToList();
            var vertices = originalMesh.vertices.ToList();

            // Prepare a list to hold the new meshes
            var newMeshes = new List<Mesh>();

            // As long as there are triangles left in the original mesh
            while (triangles.Count > 0)
            {
                // Create a new list to hold the triangles of the current connected surface
                var newTriangles = new List<int>
                {

                    // Add the first triangle to the list
                    triangles[0],
                    triangles[1],
                    triangles[2]
                };
                triangles.RemoveRange(0, 3);

                int triangleCount = 0;
                // As long as we are still adding new
                // triangles to the connected surface
                while (triangleCount < newTriangles.Count)
                {
                    // For each triangle in the original mesh
                    for (int i = 0; i < triangles.Count; i += 3)
                    {
                        // If the current triangle in the original mesh is connected to the current triangle in the new mesh
                        if (IsConnected(new int[] { triangles[i], triangles[i + 1], triangles[i + 2] }, new int[] { newTriangles[triangleCount], newTriangles[triangleCount + 1], newTriangles[triangleCount + 2] }, vertices))
                        {
                            // Add the current triangle in the original mesh to the new mesh
                            newTriangles.Add(triangles[i]);
                            newTriangles.Add(triangles[i + 1]);
                            newTriangles.Add(triangles[i + 2]);

                            // Remove the current triangle in the original mesh from the original mesh
                            triangles.RemoveAt(i);
                            triangles.RemoveAt(i);
                            triangles.RemoveAt(i);

                            // Restart the loop
                            i -= 3;
                        }
                    }

                    // Move on to the next triangle in the new mesh
                    triangleCount += 3;
                }

                // Create a new list for the new vertices
                var newVertices = new List<Vector3>();
                // Create a dictionary to map old vertex indices to new ones
                var vertexMap = new Dictionary<int, int>();

                // Go through the new triangles
                for (int i = 0; i < newTriangles.Count; i++)
                {
                    int oldVertexIndex = newTriangles[i];

                    // If this vertex has not been added to the new vertices list yet
                    if (!vertexMap.ContainsKey(oldVertexIndex))
                    {
                        // Add the vertex to the new vertices list
                        newVertices.Add(vertices[oldVertexIndex]);
                        // Map the old vertex index to the new one
                        vertexMap[oldVertexIndex] = newVertices.Count - 1;
                    }

                    // Update the index of the vertex in the triangle
                    newTriangles[i] = vertexMap[oldVertexIndex];
                }

                // Create a new mesh from the triangles of the connected surface
                var newMesh = new Mesh
                {
                    vertices = newVertices.ToArray(),
                    triangles = newTriangles.ToArray()
                };
                newMesh.RecalculateNormals();
                newMesh.RecalculateBounds();

                // Add the new mesh to the list of new meshes
                newMeshes.Add(newMesh);
            }

            var newGameObjects = new List<GameObject>();

            // For each new mesh
            for (int i = 0; i < newMeshes.Count; i++)
            {
                // Create a new game object
                var newGameObject = new GameObject($"unit_from_{parentOfParent.name}_{parentGameObject.name}_{i + 1}");

                // Add a mesh filter and mesh renderer to the new game object
                newGameObject.AddComponent<MeshFilter>().sharedMesh = newMeshes[i];
                newGameObject.AddComponent<MeshRenderer>().sharedMaterial = meshRenderer.sharedMaterial;

                // Make the new game object a child of the parent game object
                newGameObject.transform.parent = parentGameObject.transform.parent;

                // Reset the position, rotation, and scale of the new game object
                newGameObject.transform.localPosition = Vector3.zero;
                newGameObject.transform.localRotation = Quaternion.identity;
                newGameObject.transform.localScale = Vector3.one;

                newGameObjects.Add(newGameObject);

                Undo.RegisterCreatedObjectUndo(newGameObject, "Create Separate Mesh");
            }
            UnityEngine.Object.DestroyImmediate(parentGameObject);
            return newGameObjects;
        }

        static bool IsConnected(int[] faceA, int[] faceB, List<Vector3> vertices)
        {
            // For each vertex in the first face
            for (int i = 0; i < faceA.Length; i++)
            {
                // If the vertex is also in the second face
                for (int j = 0; j < faceB.Length; j++)
                {
                    if (vertices[faceA[i]] == vertices[faceB[j]])
                    {
                        // The faces are connected
                        return true;
                    }
                }
            }

            // If no shared vertices were found, the faces are not connected
            return false;
        }
        public static List<GameObject> SeparateSubmesh(GameObject parentGameObject)
        {
            MeshRenderer meshRenderer = parentGameObject.GetComponent<MeshRenderer>();
            MeshFilter meshFilter = parentGameObject.GetComponent<MeshFilter>();
            Mesh originalMesh = meshFilter.sharedMesh;

            // Get count of each material
            var materialCounts = meshRenderer.sharedMaterials
                .GroupBy(material => material.name)
                .ToDictionary(group => group.Key, group => group.Count());

            Transform parentOfParent = parentGameObject.transform.parent.transform.parent;

            var newGameObjects = new List<GameObject>();

            int processedIndex = 0;
            if (originalMesh.subMeshCount > 1)
            {
                int maxVertexCountIndex = 0;
                if (originalMesh.subMeshCount == 2)
                {
                    maxVertexCountIndex = originalMesh.GetTriangles(0).Length > originalMesh.GetTriangles(1).Length ? 0 : 1;
                }
                for (int subMeshIndex = 0; subMeshIndex < originalMesh.subMeshCount; subMeshIndex++)
                {
                    Material material = meshRenderer.sharedMaterials[subMeshIndex];

                    // Only process the submesh if its material is unique
                    if (materialCounts[material.name] == 1)
                    {
                        if (originalMesh.subMeshCount == 2 && subMeshIndex != maxVertexCountIndex)
                        {
                            continue;
                        }
                        int[] oldTriangles = originalMesh.GetTriangles(subMeshIndex);
                        var newVertices = new List<Vector3>();
                        var newNormals = new List<Vector3>();
                        var newUVs = new List<Vector2>();
                        var newTriangles = new List<int>();

                        // Map of old vertex indices to new vertex indices
                        var indexMap = new Dictionary<int, int>();

                        for (int i = 0; i < oldTriangles.Length; i++)
                        {
                            int oldIndex = oldTriangles[i];

                            if (!indexMap.ContainsKey(oldIndex))
                            {
                                // Add vertex to new vertex list and get the new index
                                newVertices.Add(originalMesh.vertices[oldIndex]);
                                newNormals.Add(originalMesh.normals[oldIndex]);
                                newUVs.Add(originalMesh.uv[oldIndex]);
                                int newIndex = newVertices.Count - 1;

                                // Add mapping from old vertex index to new vertex index
                                indexMap.Add(oldIndex, newIndex);
                            }

                            // Add new triangle index
                            newTriangles.Add(indexMap[oldIndex]);
                        }

                        var newMesh = new Mesh
                        {
                            vertices = newVertices.ToArray(),
                            normals = newNormals.ToArray(),
                            uv = newUVs.ToArray(),
                            triangles = newTriangles.ToArray()
                        };

                        // Recalculate the bounding box for the new mesh
                        newMesh.RecalculateBounds();
                        var newGameObject = new GameObject($"unit_from_{parentOfParent.name}_{parentGameObject.name}_{processedIndex + 1}");

                        MeshFilter newMeshFilter = newGameObject.AddComponent<MeshFilter>();
                        newMeshFilter.sharedMesh = newMesh;
                        Undo.RegisterCreatedObjectUndo(newMeshFilter, "Create Mesh Filter");

                        MeshRenderer newMeshRenderer = newGameObject.AddComponent<MeshRenderer>();
                        newMeshRenderer.sharedMaterial = material;
                        Undo.RegisterCreatedObjectUndo(newMeshRenderer, "Create Mesh Renderer");

                        newGameObject.transform.SetParent(parentGameObject.transform.parent, false);
                        Undo.RegisterCreatedObjectUndo(newGameObject, "Create GameObject");
                        processedIndex++;
                        newGameObjects.Add(newGameObject);
                    }
                }

                Undo.RecordObject(meshFilter, "Clear Mesh");
                meshFilter.sharedMesh = null;
            }
            UnityEngine.Object.DestroyImmediate(parentGameObject);

            return newGameObjects;
        }
    }
}