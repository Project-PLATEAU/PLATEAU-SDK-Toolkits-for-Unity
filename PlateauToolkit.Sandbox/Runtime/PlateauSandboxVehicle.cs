﻿using System.Collections.Generic;
using UnityEngine;

namespace PlateauToolkit.Sandbox
{
    /// <summary>
    /// The definition of a vehicle
    /// </summary>
    [SelectionBase]
    public class PlateauSandboxVehicle :
        MonoBehaviour,
        IPlateauSandboxMovingObject,
        IPlateauSandboxCameraTarget
    {
        [SerializeField] Transform[] m_BackWheels;
        [SerializeField] Transform[] m_FrontWheels;

        /// <summary>
        /// The radius of the wheels [m].
        /// </summary>
        [SerializeField] float m_WheelRadius = 0.3f;

        [SerializeField] Transform m_BackWheelAxisTransform;

        /// <summary>
        /// The length of wheelbase [m].
        /// </summary>
        [SerializeField] float m_Wheelbase = 2.5f;

        [SerializeField] bool m_IsCameraViewAvailable;
        [SerializeField] PlateauSandboxCameraTargetSettings m_CameraTargetSettings;

        Transform[] m_AllWheels;

        public Vector3 TransformUp => transform.up;

        public Vector3 FirstPersonViewPosition => transform.position + transform.forward + transform.up;
        public Quaternion Rotation => transform.rotation;
        public float SecondAxisDistance => m_Wheelbase;
        public bool IsCameraViewAvailable => m_IsCameraViewAvailable;

        public PlateauSandboxCameraTargetSettings CameraTargetSettings => m_CameraTargetSettings;

        void Awake()
        {
            m_CameraTargetSettings.Transform = transform;

            List<Transform> allWheels = new(m_BackWheels.Length + m_FrontWheels.Length);
            allWheels.AddRange(m_FrontWheels);
            allWheels.AddRange(m_BackWheels);
            m_AllWheels = allWheels.ToArray();
        }

        public void SetPosition(in Vector3 position)
        {
            transform.position = position - (m_BackWheelAxisTransform.position - transform.position);
        }

        float IPlateauSandboxMovingObject.GetVelocityRatio(float lookaheadAngle)
        {
            // Avoid having ratio = 0.
            float minVelocityRatio = 0.01f;

            // Calculate max velocity depending on the state of movement.
            float velocityRatio = Mathf.Max(1 - Mathf.Clamp01(lookaheadAngle / 45), minVelocityRatio);

            return velocityRatio;
        }

        public void OnMove(in MovementInfo movementInfo, PlateauSandboxTrack track)
        {
            // Align the object to the spline.
            Vector3 position = track.GetPositionBySplineContainerT(movementInfo.m_SplineContainerT);

            // Set the position first.
            SetPosition(position);

            Vector3 toForward = movementInfo.m_SecondAxisPoint - position;

            transform.up = Vector3.up;
            transform.forward = toForward;

            foreach (Transform wheelTransform in m_AllWheels)
            {
                wheelTransform.localRotation = Quaternion.identity;
            }

            if (movementInfo.m_SecondAxisForward != Vector3.zero)
            {
                foreach (Transform frontWheel in m_FrontWheels)
                {
                    frontWheel.forward = movementInfo.m_SecondAxisForward;
                }
            }

            if (m_WheelRadius > 0)
            {
                m_CurrentWheelRotation += 360 * movementInfo.m_MoveDelta / (2 * Mathf.PI * m_WheelRadius);
                m_CurrentWheelRotation %= 360;

                foreach (Transform wheelTransform in m_AllWheels)
                {
                    wheelTransform.Rotate(new Vector3(m_CurrentWheelRotation, 0, 0));
                }
            }
        }

        float m_CurrentWheelRotation = 0;

        static readonly float k_GizmoSize = 0.15f;

        void OnDrawGizmos()
        {
            if (m_BackWheelAxisTransform == null)
            {
                return;
            }

            Vector3 frontWheelAxis = m_BackWheelAxisTransform.position + transform.forward * m_Wheelbase;

            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(m_BackWheelAxisTransform.position, k_GizmoSize);
            Gizmos.DrawSphere(frontWheelAxis, k_GizmoSize);

            if (m_BackWheels != null)
            {
                foreach (Transform wheelTransform in m_BackWheels)
                {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawLine(wheelTransform.position,
                        wheelTransform.position + wheelTransform.up * m_WheelRadius);
                }
            }
        }
    }
}