using System.Collections.Generic;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Runtime.PlateauSandboxBuildingsLib
{
    public static class NormalSolver
    {
        /// <summary>
        ///     Recalculate the normals of a mesh based on an angle threshold. This takes
        ///     into account distinct vertices that have the same position.
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="angle">
        ///     The smoothing angle. Note that triangles that already share
        ///     the same vertex will be smooth regardless of the angle!
        /// </param>
        public static void RecalculateNormals(this Mesh mesh, float angle)
        {
            float cosineThreshold = Mathf.Cos(angle * Mathf.Deg2Rad);

            Vector3[] vertices = mesh.vertices;
            var normals = new Vector3[vertices.Length];

            // Holds the normal of each triangle in each sub mesh.
            var triNormals = new Vector3[mesh.subMeshCount][];

            var dictionary = new Dictionary<VertexKey, List<VertexEntry>>(vertices.Length);

            for (int subMeshIndex = 0; subMeshIndex < mesh.subMeshCount; ++subMeshIndex)
            {
                int[] triangles = mesh.GetTriangles(subMeshIndex);

                triNormals[subMeshIndex] = new Vector3[triangles.Length / 3];

                for (int i = 0; i < triangles.Length; i += 3)
                {
                    int i1 = triangles[i];
                    int i2 = triangles[i + 1];
                    int i3 = triangles[i + 2];

                    // Calculate the normal of the triangle
                    Vector3 p1 = vertices[i2] - vertices[i1];
                    Vector3 p2 = vertices[i3] - vertices[i1];
                    Vector3 normal = Vector3.Cross(p1, p2).normalized;
                    int triIndex = i / 3;
                    triNormals[subMeshIndex][triIndex] = normal;

                    VertexKey key;

                    if (!dictionary.TryGetValue(key = new VertexKey(vertices[i1]), out List<VertexEntry> entry))
                    {
                        entry = new List<VertexEntry>(4);
                        dictionary.Add(key, entry);
                    }

                    entry.Add(new VertexEntry(subMeshIndex, triIndex, i1));

                    if (!dictionary.TryGetValue(key = new VertexKey(vertices[i2]), out entry))
                    {
                        entry = new List<VertexEntry>();
                        dictionary.Add(key, entry);
                    }

                    entry.Add(new VertexEntry(subMeshIndex, triIndex, i2));

                    if (!dictionary.TryGetValue(key = new VertexKey(vertices[i3]), out entry))
                    {
                        entry = new List<VertexEntry>();
                        dictionary.Add(key, entry);
                    }

                    entry.Add(new VertexEntry(subMeshIndex, triIndex, i3));
                }
            }

            // Each entry in the dictionary represents a unique vertex position.

            foreach (List<VertexEntry> vertList in dictionary.Values)
            {
                foreach (VertexEntry t in vertList)
                {
                    var sum = new Vector3();

                    foreach (VertexEntry rhsEntry in vertList)
                    {
                        if (t.m_VertexIndex == rhsEntry.m_VertexIndex)
                        {
                            sum += triNormals[rhsEntry.m_MeshIndex][rhsEntry.m_TriangleIndex];
                        }
                        else
                        {
                            // The dot product is the cosine of the angle between the two triangles.
                            // A larger cosine means a smaller angle.
                            float dot = Vector3.Dot(
                                triNormals[t.m_MeshIndex][t.m_TriangleIndex],
                                triNormals[rhsEntry.m_MeshIndex][rhsEntry.m_TriangleIndex]);
                            if (dot >= cosineThreshold)
                            {
                                sum += triNormals[rhsEntry.m_MeshIndex][rhsEntry.m_TriangleIndex];
                            }
                        }
                    }

                    normals[t.m_VertexIndex] = sum.normalized;
                }
            }

            mesh.normals = normals;
        }

        private readonly struct VertexKey
        {
            private readonly long m_X;
            private readonly long m_Y;
            private readonly long m_Z;

            // Change this if you require a different precision.
            private const int k_Tolerance = 100000;

            // Magic FNV values. Do not change these.
            private const long k_Fnv32Init = 0x811c9dc5;
            private const long k_Fnv32Prime = 0x01000193;

            public VertexKey(Vector3 position)
            {
                m_X = (long)(Mathf.Round(position.x * k_Tolerance));
                m_Y = (long)(Mathf.Round(position.y * k_Tolerance));
                m_Z = (long)(Mathf.Round(position.z * k_Tolerance));
            }

            public override bool Equals(object obj)
            {
                if (obj == null)
                {
                    return false;
                }

                var key = (VertexKey)obj;
                return m_X == key.m_X && m_Y == key.m_Y && m_Z == key.m_Z;
            }

            public override int GetHashCode()
            {
                long rv = k_Fnv32Init;
                rv ^= m_X;
                rv *= k_Fnv32Prime;
                rv ^= m_Y;
                rv *= k_Fnv32Prime;
                rv ^= m_Z;
                rv *= k_Fnv32Prime;

                return rv.GetHashCode();
            }
        }

        private struct VertexEntry
        {
            public readonly int m_MeshIndex;
            public readonly int m_TriangleIndex;
            public readonly int m_VertexIndex;

            public VertexEntry(int meshIndex, int triIndex, int vertIndex)
            {
                m_MeshIndex = meshIndex;
                m_TriangleIndex = triIndex;
                m_VertexIndex = vertIndex;
            }
        }
    }
}