using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PlateauToolkit.Rendering.Editor
{
    public static class PlateauRenderingLOD2MeshSimplifier
    {

        static void ProcessTriangles(Mesh mesh, System.Func<int[], Vector3[], bool> shouldKeepTriangle)
        {
            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;
            var newTrianglesList = new List<int>();

            for (int i = 0; i < triangles.Length; i += 3)
            {
                if (shouldKeepTriangle(triangles[i..(i + 3)], vertices))
                {
                    newTrianglesList.AddRange(triangles[i..(i + 3)]);
                }
            }

            mesh.triangles = newTrianglesList.ToArray();
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
        }

        public static void FlattenFaces(GameObject target, GameObject reference, float heightRatioTop, float heightRatioBottom)
        {
            MeshFilter targetMeshFilter = target.GetComponent<MeshFilter>();
            MeshFilter referenceMeshFilter = reference.GetComponent<MeshFilter>();

            // Create a copy of the target's mesh for modification
            Mesh mesh = UnityEngine.Object.Instantiate(targetMeshFilter.sharedMesh);
            mesh.name = targetMeshFilter.sharedMesh.name;

            Renderer targetRenderer = target.GetComponent<Renderer>();
            Renderer referenceRenderer = reference.GetComponent<Renderer>();
            Material preservedMaterial = PlateauRenderingMeshUtilities.GetSubmaterialByLargestFaceArea(targetRenderer, mesh);

            Vector3[] vertices = mesh.vertices;
            Bounds referenceBounds = reference.GetComponent<Renderer>().bounds;
            float minY = referenceBounds.min.y;
            float maxY = referenceBounds.max.y;
            float topThreshold = maxY * heightRatioTop;
            float bottomThreshold = minY + (maxY - minY) * heightRatioBottom;

            float offset = referenceBounds.max.y;

            for (int i = 0; i < vertices.Length; i++)
            {
                if (vertices[i].y > topThreshold)
                {
                    vertices[i].y = offset + 0.001f * vertices[i].y;
                }
                else if (vertices[i].y < bottomThreshold)
                {
                    vertices[i].y = minY;
                }
            }

            mesh.vertices = vertices;

            // Process triangles for both top and bottom faces
            ProcessTriangles(mesh, (triangles, vertices) =>
            {
                Vector3 v1 = target.transform.TransformPoint(vertices[triangles[0]]);
                Vector3 v2 = target.transform.TransformPoint(vertices[triangles[1]]);
                Vector3 v3 = target.transform.TransformPoint(vertices[triangles[2]]);

                Vector3 normal = Vector3.Cross(v2 - v1, v3 - v1);
                Vector3 faceUp = target.transform.TransformDirection(normal);

                float miny = Mathf.Min(v1.y, Mathf.Min(v2.y, v3.y));
                float maxy = Mathf.Max(v1.y, Mathf.Max(v2.y, v3.y));
                float centerY = (miny + maxy) / 2f;

                return !(faceUp.y < 0.9f && centerY > referenceBounds.max.y || faceUp.y < -0.9f && centerY < referenceBounds.min.y + 0.1f);
            });

            mesh.RecalculateBounds();
            mesh.RecalculateNormals();

            // Replace reference mesh with the new one
            Undo.RecordObject(referenceMeshFilter, "Flatten Faces");
            Undo.RecordObject(referenceRenderer, "Change Material");
            referenceMeshFilter.sharedMesh = mesh;
            referenceRenderer.materials = new Material[] { preservedMaterial };
        }

        public static Mesh LOD2Simplify(GameObject target, GameObject reference, float heightRatioBottom, float heightRatioTop)
        {

            // Flatten bottom and top faces
            FlattenFaces(target, reference, heightRatioTop, heightRatioBottom);

            // Get the mesh filter and the mesh
            MeshFilter meshFilter = reference.GetComponent<MeshFilter>();

            Mesh mesh = meshFilter.sharedMesh;

            // Get the mesh renderer
            MeshRenderer meshRenderer = reference.GetComponent<MeshRenderer>();
            if (meshRenderer == null)
            {
                return null;
            }

            // Record changes for the Undo operation
            Undo.RecordObject(mesh, "Weld Vertices");
            Undo.RecordObject(meshRenderer, "Change Material");

            // Weld the vertices
            Mesh newMesh = PlateauRenderingMeshUtilities.WeldVertices(mesh, meshRenderer, preserveNormals: false);

            meshFilter.sharedMesh = newMesh;

            // Return the new mesh
            return newMesh;
        }

        public static (GameObject, GameObject) GetLOD1AndLOD2(GameObject building)
        {
            if (building == null)
            {
                throw new System.ArgumentNullException(nameof(building));
            }

            GameObject lod1 = null;
            GameObject lod2 = null;
            if (building.name.Contains("LOD1"))
            {
                lod1 = building;
                lod2 = PlateauRenderingBuildingUtilities.FindSiblingLodObject(building, "LOD2");
            }
            else if (building.name.Contains("LOD2"))
            {
                lod1 = PlateauRenderingBuildingUtilities.FindSiblingLodObject(building, "LOD1");
                lod2 = building;
            }
            else
            {
                throw new ArgumentException($"対象地物には 'Lod1' または 'Lod2' が見つかりませんでした。: {building.name}");
            }

            if (lod1 == null)
            {
                throw new Exception("LOD1 object not found.");
            }

            if (lod2 == null)
            {
                throw new Exception("LOD2 object not found.");
            }

            return (lod1, lod2);
        }
    }
}