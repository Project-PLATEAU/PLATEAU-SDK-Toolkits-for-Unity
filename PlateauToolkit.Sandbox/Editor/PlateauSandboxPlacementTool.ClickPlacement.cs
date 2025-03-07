﻿using System.Collections.Generic;
using System;
using UnityEditor;
using UnityEngine.Splines;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Editor
{
    partial class PlateauSandboxPlacementTool
    {
        class ClickPlacement : IPlacement
        {
            /// <summary>Mouse screen position where the mouse is down.</summary>
            Vector2? m_MouseDownPosition;

            /// <summary>Point to place objects.</summary>
            PlacePoint m_PlacePoint;

            /// <summary>Direction of the placed object</summary>
            Vector3 m_HandleDirectionVector = k_DefaultDirectionVector;

            PlateauSandboxInstantiation m_PreviewInstantiation;
            PlateauSandboxPlacementCollision m_PreviewCollision;
            IPlateauSandboxPlaceableObject m_PreviewPlaceable;

            readonly List<BezierCurve> m_CurveList = new();

            readonly PlateauSandboxContext m_Context;

            bool m_IsHandleRotationChanged;

            public ClickPlacement(PlateauSandboxContext context)
            {
                m_Context = context;
            }

            public bool IsPlaceable { get; private set; }

            public void Dispose()
            {
                if (m_PreviewInstantiation != null)
                {
                    if (m_PreviewInstantiation.SceneObject != null)
                    {
                        DestroyImmediate(m_PreviewInstantiation.SceneObject);
                    }

                    m_PreviewInstantiation = null;
                }

                m_IsHandleRotationChanged = false;
            }

            Quaternion GetPlaceRotation()
            {
                switch (m_Context.PlacementSettings.UpVector)
                {
                    case PlacementUpVector.Normal:
                    {
                        var toDirection = m_PlacePoint.Normal;
                        if (!m_PreviewPlaceable.IsGroundPlacementVertical())
                        {
                            Vector3 placePointNormalDirection = GetPlacePointNormalDirection();
                            if (placePointNormalDirection == Vector3.up)
                            {
                                toDirection = Vector3.back;
                            }
                            else if (placePointNormalDirection == Vector3.down)
                            {
                                toDirection = Vector3.forward;
                            }
                            else
                            {
                                toDirection = Vector3.up;
                            }
                        }
                        Quaternion toNormal = Quaternion.FromToRotation(Vector3.up, toDirection);
                        Vector3 rotatedDirection = toNormal * m_HandleDirectionVector;
                        return Quaternion.LookRotation(rotatedDirection, toDirection);
                    }
                    case PlacementUpVector.World:
                    {
                        return Quaternion.LookRotation(m_HandleDirectionVector, Vector3.up);
                    }
                    default:
                        throw new ArgumentOutOfRangeException(nameof(PlacementUpVector));
                }
            }

            public void Repaint(EditorWindow window)
            {
                if (m_PlacePoint == null)
                {
                    return;
                }
                if (m_Context.IsSelectedObject(null))
                {
                    return;
                }

                if (m_PreviewInstantiation != null && !m_Context.IsSelectedObject(m_PreviewInstantiation.Prefab))
                {
                    DestroyImmediate(m_PreviewInstantiation.SceneObject);
                    m_PreviewInstantiation = null;
                    m_PreviewCollision = null;
                }

                if (m_PreviewInstantiation == null)
                {
                    m_PreviewInstantiation = m_Context.InstantiateSelectedObject(
                        Vector3.zero, Quaternion.identity, HideFlags.DontSave | HideFlags.HideInHierarchy);

                    List<Collider> colliders = new();
                    colliders.AddRange(m_PreviewInstantiation.SceneObject.GetComponents<Collider>());
                    colliders.AddRange(m_PreviewInstantiation.SceneObject.GetComponentsInChildren<Collider>());

                    // Set up collider of the preview object
                    if (colliders.Count > 0)
                    {
                        Rigidbody rigidbody = m_PreviewInstantiation.SceneObject.AddComponent<Rigidbody>();
                        rigidbody.constraints = RigidbodyConstraints.FreezeAll;
                        m_PreviewCollision = m_PreviewInstantiation.SceneObject.AddComponent<PlateauSandboxPlacementCollision>();
                        m_PreviewInstantiation.SceneObject.layer = LayerMask.NameToLayer("Ignore Raycast");

                        foreach (Collider collider in colliders)
                        {
                            collider.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
                        }
                    }

                    // Set up placeable reference for the preview object.
                    // IPlateauSandboxPlaceableObject.SetPosition behaves different from setting transfrom.position.
                    m_PreviewInstantiation.SceneObject.TryGetComponent(out m_PreviewPlaceable);
                    Debug.Assert(m_PreviewPlaceable != null);
                }

                Debug.Assert(m_PreviewInstantiation != null);
                Debug.Assert(m_PreviewInstantiation.SceneObject != null);

                if (!Application.isPlaying)
                {
                    // For the case where the collided object is destroyed and OnCollisionExit isn't called,
                    // Always reset the flag before simulating physics.
                    if (m_PreviewCollision != null)
                    {
                        m_PreviewCollision.ResetCollided();
                    }

                    // Simulate physics to perform collision.
                    SimulationMode prevSimulationMode = Physics.simulationMode;
                    Physics.simulationMode = SimulationMode.Script;
                    Physics.Simulate(Time.fixedDeltaTime);
                    Physics.simulationMode = prevSimulationMode;
                }

                SetGroundHandleDirection(m_PreviewPlaceable.IsGroundPlacementVertical());
                m_PreviewPlaceable.SetPosition(m_PlacePoint.Position);
                m_PreviewInstantiation.SceneObject.transform.rotation = GetPlaceRotation();

                float scale;
                {
                    Camera sceneViewCamera = SceneView.lastActiveSceneView.camera;
                    if (sceneViewCamera.orthographic)
                    {
                        scale = sceneViewCamera.orthographicSize * 0.1f;
                    }
                    else
                    {
                        float distance = (m_PlacePoint.Position - SceneView.lastActiveSceneView.camera.transform.position).magnitude;
                        scale = distance * 0.05f;
                    }
                }

                Vector3 upVector;
                switch (m_Context.PlacementSettings.UpVector)
                {
                    case PlacementUpVector.Normal:
                        upVector = m_PlacePoint.Normal;
                        break;
                    case PlacementUpVector.World:
                        upVector = Vector3.up;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(PlacementUpVector));
                }

                Color handleColor;
                if (m_PreviewCollision != null && m_PreviewCollision.IsCollided ||
                    m_PlacePoint.HitOnSandboxObject)
                {
                    handleColor = Color.red;
                }
                else
                {
                    handleColor = Color.green;
                }

                DrawHandle(m_PlacePoint.Position, m_HandleDirectionVector, upVector, scale, handleColor);
            }

            public void MouseDown(EditorWindow window)
            {
                if (Event.current.button != 0 || Tools.viewToolActive)
                {
                    return;
                }

                m_MouseDownPosition = Event.current.mousePosition;
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

                if (m_Context.PlacementSettings.Location == PlacementLocation.PlaceAlongTrack)
                {
                    // On placement along tracks, TrackMovement will be attached
                    // and the configured rotation will be ignored.
                    return;
                }

                Debug.Assert(m_MouseDownPosition != null, "MouseDrag is called after MouseDown");

                if (Vector2.Distance(m_MouseDownPosition.Value, Event.current.mousePosition) < k_SelectDirectionThreshold)
                {
                    return;
                }

                Vector3 linePoint = SceneView.lastActiveSceneView.camera.transform.position;
                Vector3 lineDirection = -HandleUtility.GUIPointToWorldRay(Event.current.mousePosition).direction;
                lineDirection.Normalize();
                Vector3 planePoint = m_PlacePoint.Position;
                Vector3 planeNormal = m_PlacePoint.Normal;

                // Calculate the denominator and see if it's not small.
                float denominator = Vector3.Dot(lineDirection, planeNormal);

                if (Mathf.Abs(denominator) < Mathf.Epsilon)
                {
                    Debug.Log("The line is parallel to the plane or the denominator is too close to zero.");
                }
                else if (denominator < 0)
                {
                    Debug.Log("The line is looking in the opposite direction of the plane.");
                }
                else
                {
                    // The distance to the plane
                    float t = Vector3.Dot(planePoint - linePoint, planeNormal) / denominator;

                    // Where the ray intersects with a plane
                    Vector3 intersectionPoint = linePoint + lineDirection * t;

                    Quaternion toUpRotation = Quaternion.FromToRotation(m_PlacePoint.Normal, Vector3.up);
                    Vector3 direction = toUpRotation * (intersectionPoint - m_PlacePoint.Position);
                    direction.Normalize();
                    m_HandleDirectionVector = direction;

                    m_IsHandleRotationChanged = true;
                }

                window.Repaint();
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

                        bool hitOnSandboxObject = PlateauSandboxObjectFinder
                            .TryGetSandboxObject(raycastHit.collider, out _);

                        m_PlacePoint = new PlacePoint(raycastHit.point, raycastHit.normal, null, hitOnSandboxObject);
                        IsPlaceable = true;
                        break;
                    }

                    case PlacementLocation.PlaceAlongTrack:
                    {
                        (Vector3 Position, Vector3 Forward, PlateauSandboxTrack Track)? nearestPosition = null;
                        float nearestDistance = float.MaxValue;

                        foreach (PlateauSandboxTrack track in m_Context.Tracks)
                        {
                            // Obtain the all curves in the track.
                            track.GetCurves(m_CurveList);

                            // Search the nearest point.
                            foreach (BezierCurve curve in m_CurveList)
                            {
                                BezierCurveUtility.GetHandleNearestPointOnCurve(curve, out Vector3 position, out float t, out float distance);

                                if (nearestDistance > distance)
                                {
                                    nearestDistance = distance;
                                    nearestPosition = (position, CurveUtility.EvaluateTangent(curve, t), track);
                                }
                            }

                            m_CurveList.Clear();
                        }

                        if (nearestPosition == null)
                        {
                            IsPlaceable = false;
                            m_PlacePoint = null;
                        }
                        else
                        {
                            IsPlaceable = true;
                            (Vector3 position, Vector3 forward, PlateauSandboxTrack track) = nearestPosition.Value;

                            Vector3 up;
                            switch (m_Context.PlacementSettings.UpVector)
                            {
                                case PlacementUpVector.Normal:
                                    track.GetNearestPoint(position, out _, out int splineIndex, out float splineT);
                                    up = track.SplineContainer.Splines[splineIndex].EvaluateUpVector(splineT);
                                    break;
                                case PlacementUpVector.World:
                                    up = Vector3.up;
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }

                            m_PlacePoint = new PlacePoint(position, up, track, false);
                            forward = Vector3.ProjectOnPlane(forward, Vector3.up);
                            forward.Normalize();

                            m_HandleDirectionVector = forward;
                        }

                        break;
                    }
                }

                window.Repaint();
            }

            public void MouseUp(EditorWindow window)
            {
                if (Event.current.button == 0 && !Tools.viewToolActive &&
                    m_PlacePoint != null && !m_Context.IsSelectedObject(null))
                {
                    if (m_PreviewCollision != null && m_PreviewCollision.IsCollided)
                    {
                        // Object is overlapping with other some game object.
                        return;
                    }

                    if (m_PlacePoint.HitOnSandboxObject)
                    {
                        // Object is going to be placed on another Sandbox object.
                        return;
                    }

                    // Instantiate an object
                    PlateauSandboxInstantiation instantiation =
                        m_Context.InstantiateSelectedObject(m_PlacePoint.Position, GetPlaceRotation());

                    if (m_Context.PlacementSettings.Location == PlacementLocation.PlaceAlongTrack &&
                        m_PlacePoint.Track != null &&
                        m_Context.IsSelectedObjectMoveable())
                    {
                        // If placement mode is along tracks and the placed object is moveable, attach TrackMovement.
                        PlateauSandboxTrackMovement trackMovement = instantiation.SceneObject.AddComponent<PlateauSandboxTrackMovement>();

                        // The target track is the track where the object was placed.
                        trackMovement.Track = m_PlacePoint.Track;

                        // The position of objects with TrackMovement is calculated by the interpolation value,
                        // Then, calculate the normalized position along the track.
                        m_PlacePoint.Track.GetNearestPoint(m_PlacePoint.Position, out _, out int splineIndex, out float t);
                        trackMovement.TrySetSplineContainerT(splineIndex + t);
                    }

                    Undo.RegisterCreatedObjectUndo(instantiation.SceneObject, "Undo click placement");
                }

                m_MouseDownPosition = null;
                m_PlacePoint = null;
            }

            void SetGroundHandleDirection(in bool isGroundVertical)
            {
                if (isGroundVertical || // 地面に垂直
                    m_IsHandleRotationChanged || // ハンドルの向きが変更された
                    m_Context.PlacementSettings.UpVector == PlacementUpVector.World) // ワールド座標での設定の場合
                {
                    return;
                }

                Vector3 placePointNormalDirection = GetPlacePointNormalDirection();
                if (placePointNormalDirection == Vector3.up || placePointNormalDirection == Vector3.down)
                {
                    m_HandleDirectionVector = k_DefaultDirectionVector;
                }
                else
                {
                    m_HandleDirectionVector = placePointNormalDirection;
                }
            }

            Vector3 GetPlacePointNormalDirection()
            {
                if (Vector3.Dot(m_PlacePoint.Normal, Vector3.up) > 0.9f)
                {
                    return Vector3.up;
                }
                else if (Vector3.Dot(m_PlacePoint.Normal, Vector3.down) > 0.9f)
                {
                    return Vector3.down;
                }
                else if (Vector3.Dot(m_PlacePoint.Normal, Vector3.right) > 0.9f)
                {
                    return Vector3.right;
                }
                else if (Vector3.Dot(m_PlacePoint.Normal, Vector3.left) > 0.9f)
                {
                    return Vector3.left;
                }
                else if (Vector3.Dot(m_PlacePoint.Normal, Vector3.forward) > 0.9f)
                {
                    return Vector3.forward;
                }
                else if (Vector3.Dot(m_PlacePoint.Normal, Vector3.back) > 0.9f)
                {
                    return Vector3.back;
                }
                return m_PlacePoint.Normal;
            }
        }
    }
}