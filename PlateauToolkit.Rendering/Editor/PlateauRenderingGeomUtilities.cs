using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PlateauToolkit.Rendering.Editor
{
    public static class PlateauRenderingGeomUtilities
    {
        // Used in approximate equality checks
        const float k_TwoPi = Mathf.PI * 2f;

        // constants/cached constructions for Vector/UV operations
        static readonly Vector3 k_Up = Vector3.up;
        static readonly Vector3 k_Forward = Vector3.forward;
        static readonly Vector3 k_Zero = Vector3.zero;
        static readonly Quaternion k_VerticalCorrection = Quaternion.AngleAxis(180.0f, k_Up);
        const float k_MostlyVertical = 0.95f;

        // Local method use only -- created here to reduce garbage collection. Collections must be cleared before use
        static readonly List<Vector3> k_HullEdgeDirections = new List<Vector3>();
        static readonly HashSet<int> k_HullIndices = new HashSet<int>();

        public static Mesh CreateMeshFromBoundingBox(List<Vector3> boundingBox)
        {
            Mesh mesh = new Mesh();

            // Calculate the center of the bounding box
            Vector3 center = Vector3.zero;
            foreach (Vector3 point in boundingBox)
            {
                center += point;
            }
            center /= boundingBox.Count;

            // Create vertices relative to the center of the bounding box
            Vector3[] vertices = new Vector3[boundingBox.Count];
            for (int i = 0; i < boundingBox.Count; i++)
            {
                vertices[i] = boundingBox[i] - center;
            }

            mesh.vertices = vertices;

            // Change the order of the vertices to make the normals face upwards
            int[] triangles = new int[]
            {
                0, 2, 1,
                0, 3, 2
            };

            mesh.triangles = triangles;

            // Set the UVs to fit a 0-1 range square
            Vector2[] uvs = new Vector2[4];
            uvs[0] = new Vector2(0, 0); // Bottom left corner of the texture
            uvs[1] = new Vector2(1, 0); // Bottom right corner of the texture
            uvs[2] = new Vector2(1, 1); // Top right corner of the texture
            uvs[3] = new Vector2(0, 1); // Top left corner of the texture

            mesh.uv = uvs;

            return mesh;
        }

        public static bool ClosestTimesOnTwoLines(Vector3 positionA, Vector3 velocityA, Vector3 positionB, Vector3 velocityB,
            out float s, out float t, double parallelTest = double.Epsilon)
        {
            // Cast dot products to doubles because parallel test can fail on some hardware (iOS)
            double a = Vector3.Dot(velocityA, velocityA);
            double b = Vector3.Dot(velocityA, velocityB);
            double e = Vector3.Dot(velocityB, velocityB);

            double d = a * e - b * b;

            //lines are parallel
            if (Math.Abs(d) < parallelTest)
            {
                s = 0;
                t = 0;
                return false;
            }

            Vector3 r = positionA - positionB;
            float c = Vector3.Dot(velocityA, r);
            float f = Vector3.Dot(velocityB, r);

            s = (float)((b * f - c * e) / d);
            t = (float)((a * f - c * b) / d);

            return true;
        }

        public static bool ClosestTimesOnTwoLinesXZ(Vector3 positionA, Vector3 velocityA, Vector3 positionB, Vector3 velocityB,
            out float s, out float t, double parallelTest = double.Epsilon)
        {
            // Cast dot products to doubles because parallel test can fail on some hardware (iOS)
            double a = velocityA.x * velocityA.x + velocityA.z * velocityA.z;
            double b = velocityA.x * velocityB.x + velocityA.z * velocityB.z;
            double e = velocityB.x * velocityB.x + velocityB.z * velocityB.z;

            double d = a * e - b * b;

            //lines are parallel
            if (Math.Abs(d) < parallelTest)
            {
                s = 0;
                t = 0;
                return false;
            }

            Vector3 r = positionA - positionB;
            float c = velocityA.x * r.x + velocityA.z * r.z;
            float f = velocityB.x * r.x + velocityB.z * r.z;

            s = (float)((b * f - c * e) / d);
            t = (float)((a * f - c * b) / d);

            return true;
        }

        public static bool ClosestPointsOnTwoLineSegments(Vector3 a, Vector3 aLineVector, Vector3 b, Vector3 bLineVector,
            out Vector3 resultA, out Vector3 resultB, double parallelTest = double.Epsilon)
        {
            bool parallel = !ClosestTimesOnTwoLines(a, aLineVector, b, bLineVector,
                    out float s, out float t, parallelTest);

            if (s > 0 && s <= 1 && t > 0 && t <= 1)
            {
                resultA = a + aLineVector * s;
                resultB = b + bLineVector * t;
            }
            else
            {
                // Edge cases (literally--we are checking each of the four endpoints against the opposite segment)
                Vector3 bNeighbor = b + bLineVector;
                Vector3 aNeighbor = a + aLineVector;
                Vector3 aOnB = ClosestPointOnLineSegment(a, b, bNeighbor);
                Vector3 aNeighborOnB = ClosestPointOnLineSegment(aNeighbor, b, bNeighbor);
                float minDist = Vector3.Distance(a, aOnB);
                resultA = a;
                resultB = aOnB;

                float nextDist = Vector3.Distance(aNeighbor, aNeighborOnB);
                if (nextDist < minDist)
                {
                    resultA = aNeighbor;
                    resultB = aNeighborOnB;
                    minDist = nextDist;
                }

                Vector3 bOnA = ClosestPointOnLineSegment(b, a, aNeighbor);
                nextDist = Vector3.Distance(b, bOnA);
                if (nextDist < minDist)
                {
                    resultA = bOnA;
                    resultB = b;
                    minDist = nextDist;
                }

                Vector3 bNeighborOnA = ClosestPointOnLineSegment(bNeighbor, a, aNeighbor);
                nextDist = Vector3.Distance(bNeighbor, bNeighborOnA);
                if (nextDist < minDist)
                {
                    resultA = bNeighborOnA;
                    resultB = bNeighbor;
                }

                if (parallel)
                {
                    if (Vector3.Dot(aLineVector, bLineVector) > 0)
                    {
                        t = Vector3.Dot(bNeighbor - a, aLineVector.normalized) * 0.5f;
                        Vector3 midA = a + aLineVector.normalized * t;
                        Vector3 midB = bNeighbor + bLineVector.normalized * -t;
                        if (t > 0 && t < aLineVector.magnitude)
                        {
                            resultA = midA;
                            resultB = midB;
                        }
                    }
                    else
                    {
                        t = Vector3.Dot(aNeighbor - bNeighbor, aLineVector.normalized) * 0.5f;
                        Vector3 midA = aNeighbor + aLineVector.normalized * -t;
                        Vector3 midB = bNeighbor + bLineVector.normalized * -t;
                        if (t > 0 && t < aLineVector.magnitude)
                        {
                            resultA = midA;
                            resultB = midB;
                        }
                    }
                }
            }

            return parallel;
        }

        public static Vector3 ClosestPointOnLineSegment(Vector3 point, Vector3 a, Vector3 b)
        {
            Vector3 segment = b - a;
            Vector3 direction = segment.normalized;
            float projection = Vector3.Dot(point - a, direction);
            if (projection < 0)
            {
                return a;
            }

            if (projection * projection > segment.sqrMagnitude)
            {
                return b;
            }

            return a + projection * direction;
        }

        public static void ClosestPolygonApproach(List<Vector3> verticesA, List<Vector3> verticesB,
            out Vector3 pointA, out Vector3 pointB, float parallelTest = 0f)
        {
            pointA = default;
            pointB = default;
            float closest = float.MaxValue;
            int aCount = verticesA.Count;
            int bCount = verticesB.Count;
            int aCountMinusOne = aCount - 1;
            int bCountMinusOne = bCount - 1;
            Vector3 firstVertexA = verticesA[0];
            Vector3 firstVertexB = verticesB[0];
            for (int i = 0; i < aCount; i++)
            {
                Vector3 vertexA = verticesA[i];
                Vector3 aNeighbor = i == aCountMinusOne ? firstVertexA : verticesA[i + 1];
                Vector3 aLineVector = aNeighbor - vertexA;

                for (int j = 0; j < bCount; j++)
                {
                    Vector3 vertexB = verticesB[j];
                    Vector3 bNeighbor = j == bCountMinusOne ? firstVertexB : verticesB[j + 1];
                    Vector3 bLineVector = bNeighbor - vertexB;

                    bool parallel = ClosestPointsOnTwoLineSegments(vertexA, aLineVector, vertexB, bLineVector,
                            out Vector3 a, out Vector3 b, parallelTest);

                    float dist = Vector3.Distance(a, b);

                    if (parallel)
                    {
                        float delta = dist - closest;
                        if (delta < parallelTest)
                        {
                            closest = dist - parallelTest;
                            pointA = a;
                            pointB = b;
                        }
                    }
                    else if (dist < closest)
                    {
                        closest = dist;
                        pointA = a;
                        pointB = b;
                    }
                }
            }
        }

        public static bool PointInPolygon3D(Vector3 testPoint, List<Vector3> vertices)
        {
            // Not enough bounds vertices = nothing to be inside of
            if (vertices.Count < 3)
            {
                return false;
            }

            // Compute the sum of the angles between the test point and each pair of edge points
            double angleSum = 0;
            for (int vertIndex = 0; vertIndex < vertices.Count; vertIndex++)
            {
                Vector3 toA = vertices[vertIndex] - testPoint;
                Vector3 toB = vertices[(vertIndex + 1) % vertices.Count] - testPoint;
                float sqrDistances = toA.sqrMagnitude * toB.sqrMagnitude; // Use sqrMagnitude, take sqrt of result later
                if (sqrDistances <= PlateauRenderingMathUtilities.k_EpsilonScaled) // On a vertex
                {
                    return true;
                }

                double cosTheta = Vector3.Dot(toA, toB) / Mathf.Sqrt(sqrDistances);
                double angle = Math.Acos(cosTheta);
                angleSum += angle;
            }
            // The sum will only be 2*PI if the point is on the plane of the polygon and on the interior
            const float radiansCompareThreshold = 0.01f;
            return Mathf.Abs((float)angleSum - k_TwoPi) < radiansCompareThreshold;
        }

        public static Vector3[] OrientedMinimumBoundingBox2D(List<Vector3> convexHull, Vector3[] boundingBox)
        {
            var caliperLeft = new Vector3(0f, 0f, 1f);
            var caliperRight = new Vector3(0f, 0f, -1f);
            var caliperTop = new Vector3(1f, 0f, 0f);
            var caliperBottom = new Vector3(-1f, 0f, 0f);

            float xMin = float.MaxValue, yMin = float.MaxValue;
            float xMax = float.MinValue, yMax = float.MinValue;
            int leftIndex = 0, rightIndex = 0, topIndex = 0, bottomIndex = 0;

            // find the indices of the 'extreme points' in the hull to use as starting edge indices
            int vertexCount = convexHull.Count;
            for (int i = 0; i < vertexCount; i++)
            {
                Vector3 vertex = convexHull[i];
                float x = vertex.x;
                if (x < xMin)
                {
                    xMin = x;
                    leftIndex = i;
                }

                if (x > xMax)
                {
                    xMax = x;
                    rightIndex = i;
                }

                float z = vertex.z;
                if (z < yMin)
                {
                    yMin = z;
                    bottomIndex = i;
                }

                if (z > yMax)
                {
                    yMax = z;
                    topIndex = i;
                }
            }

            // compute & store the direction of every edge in the hull
            k_HullEdgeDirections.Clear();
            int lastVertexIndex = vertexCount - 1;
            for (int i = 0; i < lastVertexIndex; i++)
            {
                Vector3 edgeDirection = convexHull[i + 1] - convexHull[i];
                edgeDirection.Normalize();
                k_HullEdgeDirections.Add(edgeDirection);
            }

            // by doing the last vertex on its own, we can skip checking indices while iterating above
            Vector3 lastEdgeDirection = convexHull[0] - convexHull[lastVertexIndex];
            lastEdgeDirection.Normalize();
            k_HullEdgeDirections.Add(lastEdgeDirection);

            double bestOrientedBoundingBoxArea = double.MaxValue;
            // for every vertex in the hull, try aligning a caliper edge with an edge the vertex lies on
            for (int i = 0; i < vertexCount; i++)
            {
                Vector3 leftEdge = k_HullEdgeDirections[leftIndex];
                Vector3 rightEdge = k_HullEdgeDirections[rightIndex];
                Vector3 topEdge = k_HullEdgeDirections[topIndex];
                Vector3 bottomEdge = k_HullEdgeDirections[bottomIndex];

                // find the angles between our caliper lines and the polygon edges, by doing
                // ` arccosine(caliperEdge · hullEdge) ` for each pair of caliper edge & polygon edge
                double leftAngle = Math.Acos(caliperLeft.x * leftEdge.x + caliperLeft.z * leftEdge.z);
                double rightAngle = Math.Acos(caliperRight.x * rightEdge.x + caliperRight.z * rightEdge.z);
                double topAngle = Math.Acos(caliperTop.x * topEdge.x + caliperTop.z * topEdge.z);
                double bottomAngle = Math.Acos(caliperBottom.x * bottomEdge.x + caliperBottom.z * bottomEdge.z);

                // find smallest angle among the lines
                int smallestAngleIndex = 0;
                double smallestAngle = leftAngle;
                if (rightAngle < smallestAngle)
                {
                    smallestAngle = rightAngle;
                    smallestAngleIndex = 1;
                }

                if (topAngle < smallestAngle)
                {
                    smallestAngle = topAngle;
                    smallestAngleIndex = 2;
                }

                if (bottomAngle < smallestAngle)
                {
                    smallestAngleIndex = 3;
                }

                // based on which caliper edge had the smallest angle between it & the polygon, rotate our calipers
                // and recalculate corners
                Vector3 upperLeft, upperRight, bottomLeft, bottomRight;
                switch (smallestAngleIndex)
                {
                    // left
                    case 0:
                        RotateCalipers(leftEdge, convexHull, ref leftIndex, out topIndex, out rightIndex, out bottomIndex,
                            out caliperLeft, out caliperTop, out caliperRight, out caliperBottom,
                            out upperLeft, out upperRight, out bottomRight, out bottomLeft);
                        break;
                    // right
                    case 1:
                        RotateCalipers(rightEdge, convexHull, ref rightIndex, out bottomIndex, out leftIndex, out topIndex,
                            out caliperRight, out caliperBottom, out caliperLeft, out caliperTop,
                            out bottomRight, out bottomLeft, out upperLeft, out upperRight);
                        break;
                    // top
                    case 2:
                        RotateCalipers(topEdge, convexHull, ref topIndex, out rightIndex, out bottomIndex, out leftIndex,
                            out caliperTop, out caliperRight, out caliperBottom, out caliperLeft,
                            out upperRight, out bottomRight, out bottomLeft, out upperLeft);
                        break;
                    // bottom
                    default:
                        RotateCalipers(bottomEdge, convexHull, ref bottomIndex, out leftIndex, out topIndex, out rightIndex,
                            out caliperBottom, out caliperLeft, out caliperTop, out caliperRight,
                            out bottomLeft, out upperLeft, out upperRight, out bottomRight);
                        break;
                }

                // usually with rotating calipers, this comparison is talked about in terms of distance,
                // but since we just want to know which is bigger it works to use square magnitudes
                float sqrDistanceX = (upperLeft - upperRight).sqrMagnitude;
                float sqrDistanceZ = (upperLeft - bottomLeft).sqrMagnitude;
                float sqrDistanceProduct = sqrDistanceX * sqrDistanceZ;

                // if this is a smaller box than any we've found before, it's our new candidate
                if (sqrDistanceProduct < bestOrientedBoundingBoxArea)
                {
                    bestOrientedBoundingBoxArea = sqrDistanceProduct;
                    boundingBox[0] = bottomLeft;
                    boundingBox[1] = bottomRight;
                    boundingBox[2] = upperRight;
                    boundingBox[3] = upperLeft;
                }
            }

            // compute the size of the 2d bounds
            Vector3 topLeft = boundingBox[0];
            float leftRightDistance = Vector3.Distance(topLeft, boundingBox[3]);
            float topBottomDistance = Vector3.Distance(topLeft, boundingBox[1]);

            return boundingBox;
        }

        static void RotateCalipers(Vector3 alignEdge, List<Vector3> vertices,
            ref int indexA, out int indexB, out int indexC, out int indexD,
            out Vector3 caliperA, out Vector3 caliperB, out Vector3 caliperC, out Vector3 caliperD,
            out Vector3 caliperAEndCorner, out Vector3 caliperBEndCorner, out Vector3 caliperCEndCorner, out Vector3 caliperDEndCorner)
        {
            int vertexCount = vertices.Count;
            caliperA = alignEdge;
            caliperB = new Vector3(caliperA.z, 0f, -caliperA.x); // orthogonal
            caliperC = -caliperA; // opposite
            caliperD = -caliperB; // opposite orthogonal
            indexA = (indexA + 1) % vertexCount;

            // For each caliper, determine the polygon edge for the next caliper by testing intersection between the current caliper
            // and the opposite orthogonal from subsequent polygon vertices until we've found the maximum intersection point.
            Vector3 startA = vertices[indexA];
            indexB = indexA;
            float maxS = 0f;
            while (true)
            {
                int nextIndex = (indexB + 1) % vertexCount;
                ClosestTimesOnTwoLinesXZ(startA, caliperA, vertices[nextIndex], caliperD, out float s, out _);
                if (s <= maxS)
                {
                    break;
                }

                maxS = s;
                indexB = nextIndex;
            }

            caliperAEndCorner = startA + caliperA * maxS;
            Vector3 startB = vertices[indexB];
            indexC = indexB;
            maxS = 0f;
            while (true)
            {
                int nextIndex = (indexC + 1) % vertexCount;
                ClosestTimesOnTwoLinesXZ(startB, caliperB, vertices[nextIndex], caliperA, out float s, out _);
                if (s <= maxS)
                {
                    break;
                }

                maxS = s;
                indexC = nextIndex;
            }

            caliperBEndCorner = startB + caliperB * maxS;
            Vector3 startC = vertices[indexC];
            indexD = indexC;
            maxS = 0f;
            while (true)
            {
                int nextIndex = (indexD + 1) % vertexCount;
                ClosestTimesOnTwoLinesXZ(startC, caliperC, vertices[nextIndex], caliperB, out float s, out _);
                if (s <= maxS)
                {
                    break;
                }

                maxS = s;
                indexD = nextIndex;
            }

            caliperCEndCorner = startC + caliperC * maxS;

            // No need for any intersection tests for the last corner since we have all the other corners
            caliperDEndCorner = caliperCEndCorner + caliperAEndCorner - caliperBEndCorner;
        }

        public static List<Vector3> GetConvexHull(List<Vector3> vertices, bool isLooping = false)
        {
            var hullVertices = new List<Vector3>();

            Vector3 initialVertex = FindMinX(vertices);

            Vector3 currentVertex = initialVertex;
            Vector3 nextVertex;

            do
            {
                hullVertices.Add(currentVertex);

                nextVertex = vertices[0];

                for (int i = 1; i < vertices.Count; i++)
                {
                    if ((nextVertex == currentVertex) || (DirectionOfTurn(currentVertex, nextVertex, vertices[i]) < 0))
                    {
                        nextVertex = vertices[i];
                    }
                }

                currentVertex = nextVertex;
            }
            while (currentVertex != hullVertices[0]);

            if (isLooping)
            {
                hullVertices.Add(hullVertices[0]);
            }

            return hullVertices;
        }

        static Vector3 FindMinX(List<Vector3> list)
        {
            return list.FirstOrDefault(vertex => vertex.x == list.Min(vector => vector.x));
        }

        static void ExchangeElements(ref Vector3[] array, int index1, int index2)
        {
            Vector3 temporary = array[index1];
            array[index1] = array[index2];
            array[index2] = temporary;
        }

        public static float DirectionOfTurn(Vector3 point1, Vector3 point2, Vector3 point3)
        {
            return Mathf.Sign((point2.x - point1.x) * (point3.z - point1.z) - (point3.x - point1.x) * (point2.z - point1.z));
        }

        // Helper function to create a triangulation of the convex hull
        public static int[] Triangulate(List<Vector3> vertices)
        {
            var triangles = new List<int>();
            for (int i = 1; i < vertices.Count - 1; i++)
            {
                triangles.Add(0);
                triangles.Add(i);
                triangles.Add(i + 1);
            }
            return triangles.ToArray();
        }

        public static List<Vector3> SortVerticesInClockwiseOrder(List<Vector3> vertices)
        {
            // Compute the center of the list of vertices.
            Vector3 center = Vector3.zero;
            foreach (Vector3 vertex in vertices)
            {
                center += vertex;
            }
            center /= vertices.Count;

            // Sort the vertices based on the angle from the center.
            vertices.Sort((a, b) =>
            {
                float a1 = Mathf.Atan2(a.z - center.z, a.x - center.x);
                float a2 = Mathf.Atan2(b.z - center.z, b.x - center.x);

                return a2.CompareTo(a1); // Note that we are using a2.CompareTo(a1) to sort in clockwise order.
            });

            return vertices;
        }
    }
}