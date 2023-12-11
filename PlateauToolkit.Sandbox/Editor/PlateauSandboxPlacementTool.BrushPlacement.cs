using System.Collections.Generic;
using System;
using UnityEditor;
using UnityEngine.Splines;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Editor
{
    partial class PlateauSandboxPlacementTool
    {
        class BrushPlacement : IPlacement
        {
            /// <summary>Distance of checking if the placement is acceptable or not</summary>
            const float k_PlacementCheckDistance = 1f;

            PlacePoint m_PlacePoint;

            /// <summary>The last position where objectes are placed in the current stroke</summary>
            Vector3? m_LastPlacePointInCurrentStorke;

            readonly List<BezierCurve> m_CurveList = new();
            readonly PlateauSandboxContext m_Context;

            public BrushPlacement(PlateauSandboxContext context)
            {
                m_Context = context;
            }

            public bool IsPlaceable { get; private set; }

            public void Dispose()
            {
            }

            public void Repaint(EditorWindow window)
            {
                var brushSettings = m_Context.PlacementSettings.Brush;

                if (m_PlacePoint == null)
                {
                    return;
                }

                foreach (Vector3 offset in brushSettings.GetRandomOffsets())
                {
                    DrawHandle(m_PlacePoint.Position + offset, brushSettings.Forward, m_PlacePoint.Normal, 1, Color.green);
                }
            }

            int? m_CurrentUndoGroupIndex;

            public void MouseDown(EditorWindow window)
            {
                if (Event.current.button != 0 || Tools.viewToolActive)
                {
                    return;
                }

                if (m_PlacePoint == null)
                {
                    return;
                }

                Undo.IncrementCurrentGroup();
                Undo.SetCurrentGroupName("Undo brush placement");
                m_CurrentUndoGroupIndex = Undo.GetCurrentGroup();

                Place();
            }

            public void MouseDrag(EditorWindow window)
            {
                if (Event.current.button != 0 || Tools.viewToolActive)
                {
                    return;
                }

                if (m_PlacePoint == null)
                {
                    return;
                }

                switch (m_Context.PlacementSettings.Location)
                {
                    case PlacementLocation.PlaceOnSurface:
                    {
                        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                        if (!Physics.Raycast(ray, out RaycastHit raycastHit))
                        {
                            break;
                        }

                        m_PlacePoint = new PlacePoint(raycastHit.point, raycastHit.normal, null, false);
                        break;
                    }

                    case PlacementLocation.PlaceAlongTrack:
                    {
                        Vector3? nearestPosition = null;
                        float nearestDistance = float.MaxValue;
                        foreach (PlateauSandboxTrack track in m_Context.Tracks)
                        {
                            track.GetCurves(m_CurveList);
                        }
                        foreach (BezierCurve curve in m_CurveList)
                        {
                            BezierCurveUtility.GetHandleNearestPointOnCurve(curve, out Vector3 position, out _, out float distance);

                            if (nearestDistance > distance)
                            {
                                nearestDistance = distance;
                                nearestPosition = position;
                            }
                        }
                        m_CurveList.Clear();

                        m_PlacePoint = nearestPosition != null
                            ? new PlacePoint(nearestPosition.Value, Vector3.up, null, false)
                            : null;
                        break;
                    }
                }

                // Check the distance from the last placement.
                if (m_LastPlacePointInCurrentStorke != null &&
                    Vector3.Magnitude(m_LastPlacePointInCurrentStorke.Value - m_PlacePoint.Position) < m_Context.PlacementSettings.Brush.Spacing)
                {
                    window.Repaint();
                    return;
                }

                Place();

                window.Repaint();
            }

            void Place()
            {
                if (m_Context.IsSelectedObject(null))
                {
                    return;
                }

                m_LastPlacePointInCurrentStorke = m_PlacePoint.Position;

                foreach (Vector3 offset in m_Context.PlacementSettings.Brush.GetRandomOffsets())
                {
                    Vector3 position = m_PlacePoint.Position + offset;

                    Vector3 up;
                    switch (m_Context.PlacementSettings.UpVector)
                    {
                        case PlacementUpVector.Normal:
                            up = m_PlacePoint.Normal;
                            break;
                        case PlacementUpVector.World:
                            up = Vector3.up;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(PlacementUpVector));
                    }

                    // Check the position has a plane to place an object.
                    var ray = new Ray(position + up * (k_PlacementCheckDistance / 2), -up);
                    if (!Physics.Raycast(ray, k_PlacementCheckDistance))
                    {
                        // No plane to place on the position, then skip it.
                        continue;
                    }

                    Quaternion rotation;
                    switch (m_Context.PlacementSettings.UpVector)
                    {
                        case PlacementUpVector.Normal:
                            // Calculate the quaternion to rotate from (0, 1, 0) to `normal`
                            Quaternion toNormal = Quaternion.FromToRotation(Vector3.up, m_PlacePoint.Normal);

                            // Rotate `direction` to make it relative to `normal`
                            Vector3 rotatedDirection = toNormal * m_Context.PlacementSettings.Brush.Forward;

                            // Then, create the rotation quaternion
                            rotation = Quaternion.LookRotation(rotatedDirection, m_PlacePoint.Normal);
                            break;
                        case PlacementUpVector.World:
                            rotation = Quaternion.identity;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(PlacementUpVector));
                    }

                    // Instantiate the selected object
                    PlateauSandboxInstantiation instantiation =
                        m_Context.InstantiateSelectedObject(position, rotation);

                    Debug.Assert(m_CurrentUndoGroupIndex != null);
                    Undo.RegisterCreatedObjectUndo(
                        instantiation.SceneObject, $"Undo create object {instantiation.SceneObject.name}");
                }

                // Update the seed to change the shape of brush if the "fixed" option isn't selected.
                if (!m_Context.PlacementSettings.Brush.IsShapeRandomSeedFixed)
                {
                    m_Context.PlacementSettings.Brush.RandomizeShapeSeed();
                }
            }

            public void MouseMove(EditorWindow window)
            {
                m_PlacePoint = null;

                switch (m_Context.PlacementSettings.Location)
                {
                    case PlacementLocation.PlaceOnSurface:
                    {
                        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                        if (!Physics.Raycast(ray, out RaycastHit raycastHit))
                        {
                            IsPlaceable = false;
                            break;
                        }

                        m_PlacePoint = new PlacePoint(raycastHit.point, raycastHit.normal, null, false);
                        IsPlaceable = true;
                        break;
                    }

                    case PlacementLocation.PlaceAlongTrack:
                    {
                        Vector3? nearestPosition = null;
                        float nearestDistance = float.MaxValue;
                        foreach (PlateauSandboxTrack track in m_Context.Tracks)
                        {
                            track.GetCurves(m_CurveList);
                        }
                        foreach (BezierCurve curve in m_CurveList)
                        {
                            BezierCurveUtility.GetHandleNearestPointOnCurve(curve, out Vector3 position, out _, out float distance);

                            if (nearestDistance > distance)
                            {
                                nearestDistance = distance;
                                nearestPosition = position;
                            }
                        }
                        m_CurveList.Clear();

                        if (nearestPosition != null)
                        {
                            IsPlaceable = true;
                            m_PlacePoint = new PlacePoint(nearestPosition.Value, Vector3.up, null, false);
                        }
                        else
                        {
                            IsPlaceable = false;
                            m_PlacePoint = null;
                        }

                        break;
                    }
                }
                window.Repaint();
            }

            public void MouseUp(EditorWindow window)
            {
                if (m_CurrentUndoGroupIndex != null)
                {
                    Undo.CollapseUndoOperations(m_CurrentUndoGroupIndex.Value);
                    m_CurrentUndoGroupIndex = null;
                }

                m_LastPlacePointInCurrentStorke = null;
            }
        }
    }
}