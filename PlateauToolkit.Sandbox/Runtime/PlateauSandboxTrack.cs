using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace PlateauToolkit.Sandbox
{
    /// <summary>
    /// Represents a track where objects can move, controls <see cref="Spline" />
    /// and has additional data for specific features of Sandbox Toolkit.
    /// </summary>
    [RequireComponent(typeof(SplineContainer))]
    public class PlateauSandboxTrack : MonoBehaviour
    {
        static readonly List<SplineKnotIndex> k_CalcKnotsList = new(128);

        [SerializeField] float m_SpeedLimit;
        [SerializeField] bool m_HasSpeedLimit;

        /// <summary>
        /// The reference of attached <see cref="UnityEngine.Splines.SplineContainer" />
        /// </summary>
        /// <remarks>
        /// Do not use this field, use <see cref="SplineContainer"/>
        /// </remarks>
        SplineContainer m_SplineContainerReference;

        /// <summary>
        /// The <see cref="UnityEngine.Splines.SplineContainer" /> which contains a <see cref="Spline" />.
        /// </summary>
        internal SplineContainer SplineContainer
        {
            get
            {
                if (m_SplineContainerReference == null)
                {
                    m_SplineContainerReference = GetComponent<SplineContainer>();
                }
                return m_SplineContainerReference;
            }
        }

        /// <summary>
        /// The speed limit on the track.
        /// </summary>
        public float? SpeedLimit => m_HasSpeedLimit ? m_SpeedLimit : null;

        /// <summary>
        /// The max interpolation value of the <see cref="SplineContainer" />
        /// </summary>
        public float MaxSplineContainerT => SplineContainer.Splines.Count;

        public int GetTrackId()
        {
            return gameObject.GetHashCode();
        }

        /// <summary>
        /// Enumerate interpolation values of positions in the <see cref="SplineContainer" /> to random walk.
        /// </summary>
        /// <returns>an interpolation value in the spline container, the move delta and velocity</returns>
        public TrackPathIterator GetRandomWalk(float startSplineContainerT, int randomPathSeed)
        {
            if (SplineContainer.Splines.Count == 0)
            {
                return new TrackPathIterator();
            }
            if (SplineContainer.Splines[0].Count == 0)
            {
                return new TrackPathIterator();
            }

            // Calculate a start point from given startSplineContainerT and create a path enumerator.
            IEnumerator<TrackPath> pathEnumerator;
            float startCurveT;
            {
                SplineKnotIndex startKnotIndex;
                bool startKnotRandom;
                {
                    (int splineIndex, float splineT) = GetSplineIndex(startSplineContainerT);
                    startKnotRandom = splineT == 0f;

                    Spline spline = SplineContainer.Splines[splineIndex];
                    int knotIndex = spline.SplineToCurveT(splineT, out startCurveT);

                    startKnotIndex = new SplineKnotIndex(splineIndex, knotIndex);
                }

                pathEnumerator = GetRandomWalkPaths(startKnotIndex, startKnotRandom, randomPathSeed);
            }

            // Prepare TrackPathIterator.
            LinkedList<TrackPath> sharedPaths = new();
            return new TrackPathIterator(pathEnumerator, sharedPaths, startCurveT, true);
        }

        /// <summary>
        /// Enumerates randomly selected paths on the track.
        /// </summary>
        /// <remarks>
        /// This collection will be an infinite list if the spline has a loop.
        /// </remarks>
        /// <returns></returns>
        IEnumerator<TrackPath> GetRandomWalkPaths(
            SplineKnotIndex startKnotIndex, bool startKnotRandom, int seed)
        {
            var random = new System.Random(seed);

            if (SplineContainer.Splines.Count == 0)
            {
                yield break;
            }
            if (SplineContainer.Splines[0].Count == 0)
            {
                yield break;
            }

            if (startKnotRandom)
            {
                RandomSelectIfLinked(startKnotIndex, out startKnotIndex);
            }
            SplineKnotIndex currentKnotIndex = startKnotIndex;

            while (true)
            {
                Spline currentSpline = SplineContainer.Splines[currentKnotIndex.Spline];
                float length = CurveUtility.CalculateLength(
                    currentSpline.GetCurve(currentKnotIndex.Knot).Transform(transform.localToWorldMatrix));

                yield return new TrackPath
                {
                    Spline = new LengthCachedSpline(currentSpline),
                    SplineIndex = currentKnotIndex.Spline,
                    KnotIndex = currentKnotIndex.Knot,
                    CurveLength = length,
                };

                // The next knot in the same spline
                SplineKnotIndex incrementedKnotIndex = new(currentKnotIndex.Spline, currentKnotIndex.Knot + 1);

                if (incrementedKnotIndex.Knot < (currentSpline.Closed ? currentSpline.Count : currentSpline.Count - 1))
                {
                    RandomSelectIfLinked(incrementedKnotIndex, out currentKnotIndex);
                }
                else // The knot is the last one in its spline.
                {
                    if (currentSpline.Closed)
                    {
                        // If the spline is closed, the spline loops.
                        RandomSelectIfLinked(new(currentKnotIndex.Spline, 0), out currentKnotIndex);
                    }
                    else
                    {
                        // If the spline is opened but the last knot is linked, the spline loops.
                        if (!RandomSelectIfLinked(incrementedKnotIndex, out currentKnotIndex))
                        {
                            // The next knot doesn't have any linked knot, then it's terminal.
                            // Random walk paths can last anymore, finish the enumeration.
                            yield break;
                        }
                    }
                }
            }

            // Select a knot at random in the given knot and the other knots linked to it.
            // If the knot isn't linked, the knot will be returned.
            bool RandomSelectIfLinked(SplineKnotIndex splineKnotIndex, out SplineKnotIndex selectedKnotIndex)
            {
                IReadOnlyList<SplineKnotIndex> linkedKnots;
                if (!SplineContainer.KnotLinkCollection.TryGetKnotLinks(splineKnotIndex, out linkedKnots))
                {
                    // The knot isn't linked, the return the knot itself.
                    selectedKnotIndex = splineKnotIndex;
                    return false;
                }

                foreach (SplineKnotIndex linkedKnot in linkedKnots)
                {
                    Spline spline = SplineContainer.Splines[linkedKnot.Spline];
                    if (!spline.Closed && linkedKnot.Knot == spline.Count - 1)
                    {
                        // Filter the terminal knots if the spline is opened.
                        continue;
                    }

                    k_CalcKnotsList.Add(linkedKnot);
                }

                // Select a knot at random in the available knots.
                selectedKnotIndex = k_CalcKnotsList[random.Next(0, k_CalcKnotsList.Count)];
                k_CalcKnotsList.Clear();

                return true;
            }
        }

        /// <summary>
        /// Get the start point of the <see cref="SplineContainer" />
        /// </summary>
        /// <returns></returns>
        public Vector3 GetStartPosition()
        {
            foreach (Spline spline in GetComponent<SplineContainer>().Splines)
            {
                foreach (BezierKnot knot in spline.Knots)
                {
                    return knot.Position;
                }
            }

            return Vector3.zero;
        }

        /// <summary>
        /// Get the count of knots in the spline.
        /// </summary>
        /// <returns></returns>
        public int GetKnotsCount()
        {
            int count = 0;
            foreach (Spline spline in SplineContainer.Splines)
            {
                count += spline.Knots.Count();
            }

            return count;
        }

        /// <summary>
        /// Get all curves in the spline.
        /// </summary>
        /// <param name="curves">the result of curves</param>
        public void GetCurves(List<BezierCurve> curves)
        {
            foreach (Spline spline in SplineContainer.Splines)
            {
                int curveCount = spline.Closed ? spline.Count : spline.Count - 1;
                for (int curveIndex = 0; curveIndex < curveCount; curveIndex++)
                {
                    BezierCurve curve = spline.GetCurve(curveIndex).Transform(transform.localToWorldMatrix);
                    curves.Add(curve);
                }
            }
        }

        public float GetNearestPoint(Vector3 position, out Vector3 nearestPoint, out int nearestSplineIndex, out float nearestT)
        {
            nearestPoint = Vector3.zero;
            nearestSplineIndex = 0;
            nearestT = 0;

            float nearestDistance = float.PositiveInfinity;
            for (int splineIndex = 0; splineIndex < SplineContainer.Splines.Count; splineIndex++)
            {
                Spline spline = SplineContainer[splineIndex];
                float distance = SplineUtility.GetNearestPoint(
                    spline, position, out float3 p, out float t,
                    SplineUtility.PickResolutionMax, SplineUtility.PickResolutionMax);

                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestSplineIndex = splineIndex;
                    nearestPoint = p;
                    nearestT = t;
                }
            }

            return nearestDistance;
        }

        public Vector3 GetPositionBySplineContainerT(float splineContainerT)
        {
            (int splineIndex, float t) = GetSplineIndex(splineContainerT);

            Spline spline = SplineContainer[splineIndex];
            return transform.TransformPoint(spline.EvaluatePosition(t));
        }

        public (Vector3, Vector3) GetPositionAndForwardBySplineContainerT(float splineContainerT)
        {
            (int splineIndex, float t) = GetSplineIndex(splineContainerT);

            Spline spline = SplineContainer[splineIndex];
            return (
                transform.TransformPoint(spline.EvaluatePosition(t)),
                transform.TransformDirection(spline.EvaluateTangent(t)).normalized);
        }

        public Vector3 GetForwardBySplineContainerT(float splineContainerT)
        {
            (int splineIndex, float t) = GetSplineIndex(splineContainerT);

            Spline spline = SplineContainer[splineIndex];
            return transform.TransformPoint(spline.EvaluateTangent(t));
        }

        /// <summary>
        /// Get the index and the interpolation value from the interpolation value
        /// normalized in the <see cref="SplineContainer" />
        /// </summary>
        /// <remarks>
        /// The start of a knot and the end of the next knot are same.
        /// To handle the case where the knots in different splines are linked,
        /// regard it as the end of the next when splineT is an integer.
        /// </remarks>
        /// <param name="splineContainerT"></param>
        /// <returns></returns>
        (int splineIndex, float t) GetSplineIndex(float splineContainerT)
        {
            int splineIndex = (int)math.floor(splineContainerT);
            float t = math.frac(splineContainerT);
            if (splineIndex > 0 && t == 0f)
            {
                return (splineIndex - 1, 1f);
            }
            else
            {
                return (splineIndex, t);
            }
        }
    }

    /// <summary>
    /// Represents a path in the track.
    /// </summary>
    public struct TrackPath
    {
        public ISpline Spline { get; set; }
        public int SplineIndex { get; set; }
        public int KnotIndex { get; set; }
        public float CurveLength { get; set; }

        public float GetSplineT(float curveT)
        {
            return Spline.CurveToSplineT(KnotIndex + curveT);
        }

        public float GetSplineContainerT(float curveT)
        {
            return SplineIndex + Spline.CurveToSplineT(KnotIndex + curveT);
        }
    }

    /// <summary>
    /// Iterates a position to move along a track.
    /// </summary>
    public struct TrackPathIterator
    {
        /// <summary>Cached paths generated from <see cref="m_PathEnumerator" /></summary>
        readonly LinkedList<TrackPath> m_Paths;

        /// <summary>If removes the paths in <see cref="m_Paths"/> when they will be no longer used</summary>
        readonly bool m_RemoveHistory;

        /// <summary>The enumerator of paths</summary>
        readonly IEnumerator<TrackPath> m_PathEnumerator;

        LinkedListNode<TrackPath> m_CurrentPathNode;
        float m_CurveT;

        public TrackPathIterator(IEnumerator<TrackPath> pathEnumerator, LinkedList<TrackPath> paths, float startCurveT, bool removeHistory)
        {
            m_PathEnumerator = pathEnumerator;
            m_Paths = paths;
            m_RemoveHistory = removeHistory;
            m_CurrentPathNode = null;
            m_CurveT = startCurveT;
        }

        public TrackPath CurrentPath => m_CurrentPathNode.Value;

        LinkedListNode<TrackPath> MoveNextPathInternal()
        {
            // Enumerate the next path
            if (!m_PathEnumerator.MoveNext())
            {
                return null;
            }

            // Add the new path to the list
            return m_Paths.AddLast(m_PathEnumerator.Current);
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        bool MoveNextPath()
        {
            LinkedListNode<TrackPath> nextPathNode;
            if (m_CurrentPathNode == null)
            {
                if (m_Paths.Count == 0)
                {
                    nextPathNode = MoveNextPathInternal();
                }
                else
                {
                    nextPathNode = m_Paths.First;
                }
            }
            else if (m_CurrentPathNode.Next == null)
            {
                nextPathNode = MoveNextPathInternal();
            }
            else
            {
                nextPathNode = m_CurrentPathNode.Next;
            }

            if (nextPathNode == null)
            {
                return false;
            }

            if (m_RemoveHistory && m_CurrentPathNode != null)
            {
                m_Paths.Remove(m_CurrentPathNode);
            }

            m_CurrentPathNode = nextPathNode;
            return true;
        }

        /// <summary>
        /// Clone <see cref="TrackPathIterator" />
        /// </summary>
        /// <remarks>
        /// To find a specific position, <see cref="TrackPathIterator" /> updates its state.
        /// So, to use it for the purpose, it should be cloned not to update the original state.
        /// </remarks>
        public TrackPathIterator Clone()
        {
            TrackPathIterator clone = new(m_PathEnumerator, m_Paths, m_CurveT, false);
            clone.m_CurrentPathNode = m_CurrentPathNode;
            return clone;
        }

        /// <summary>
        /// Update the interpolation cursor to move along the path.
        /// </summary>
        /// <param name="delta">the distance to move</param>
        /// <param name="t">the interpolation value of the moved position</param>
        /// <returns></returns>
        public bool MovePoint(float delta, out float t)
        {
            if (m_CurrentPathNode == null)
            {
                if (!MoveNextPath())
                {
                    t = 0;
                    return false;
                }
            }
            return MovePointInternal(delta, ref m_CurveT, out t);
        }

        bool MovePointInternal(float delta, ref float curveT, out float t)
        {
            if (m_CurrentPathNode == null)
            {
                t = 0;
                return false;
            }

            // Find the position that is moved by the given delta from the starting point.
            // (NOTE) Depending on the value of the delta, need to iterate pathIterator.
            while (true)
            {
                float previousStartCurveT = curveT;
                curveT += delta / CurrentPath.CurveLength;
                if (curveT < 1f)
                {
                    break;
                }

                // In the case that the requested position exceeds the current path.
                delta -= (1 - previousStartCurveT) * CurrentPath.CurveLength;

                if (!MoveNextPath())
                {
                    t = CurrentPath.GetSplineContainerT(1);
                    return false;
                }
                curveT = 0;
            }

            t = CurrentPath.GetSplineContainerT(curveT);
            return true;
        }

        /// <summary>
        /// Find a point by a linear distance from the current interpolation position <see cref="m_CurveT"/>.
        /// </summary>
        /// <remarks>
        /// The calculation method is rough and the returned point wouldn't be accurate.
        /// </remarks>
        /// <param name="distance">a linear distance from the current interpolation position</param>
        /// <param name="t">the interpolation value for the returned point</param>
        /// <param name="point">the point with the given linear distance</param>
        /// <param name="resolution">The resolution how detailed to check the distance</param>
        /// <returns></returns>
        public bool MoveByLinearDistance(float distance, out float t, float resolution = 0.1f)
        {
            if (m_CurrentPathNode == null)
            {
                t = 0;
                return false;
            }

            float curveT = m_CurveT;
            float3 basePoint = CurrentPath.Spline.EvaluatePosition(CurrentPath.GetSplineT(curveT));

            if (!MovePointInternal(distance, ref curveT, out _))
            {
                t = 0;
                return false;
            }

            while (true)
            {
                float3 checkPoint = CurrentPath.Spline.EvaluatePosition(CurrentPath.GetSplineT(curveT));
                if (Vector3.Distance(basePoint, checkPoint) >= distance)
                {
                    t = CurrentPath.GetSplineContainerT(curveT);
                    return true;
                }

                if (!MovePointInternal(resolution, ref curveT, out _))
                {
                    t = 0;
                    return false;
                }
            }
        }
    }

    class LengthCachedSpline : ISpline
    {
        readonly Spline m_Spline;

        float m_Length;
        float[] m_CurveLength;

        public LengthCachedSpline(Spline spline)
        {
            m_Spline = spline;
            m_Length = -1;
            m_CurveLength = new float[m_Spline.Count];
            Array.Fill(m_CurveLength, -1);
        }

        public BezierKnot this[int index] => m_Spline[index];

        public bool Closed => m_Spline.Closed;

        public int Count => m_Spline.Count;

        public BezierCurve GetCurve(int index)
        {
            return m_Spline.GetCurve(index);
        }

        public float GetCurveInterpolation(int curveIndex, float curveDistance)
        {
            return m_Spline.GetCurveInterpolation(curveIndex, curveDistance);
        }

        public float GetCurveLength(int index)
        {
            float curveLength = m_CurveLength[index];
            if (curveLength < 0)
            {
                curveLength = m_Spline.GetCurveLength(index);
                m_CurveLength[index] = curveLength;
            }

            return curveLength;
        }

        public IEnumerator<BezierKnot> GetEnumerator()
        {
            return m_Spline.GetEnumerator();
        }

        public float GetLength()
        {
            if (m_Length < 0f)
            {
                m_Length = 0f;
                for (int i = 0, c = Closed ? Count : Count - 1; i < c; ++i)
                {
                    m_Length += GetCurveLength(i);
                }
            }

            return m_Length;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_Spline.GetEnumerator();
        }
    }
}