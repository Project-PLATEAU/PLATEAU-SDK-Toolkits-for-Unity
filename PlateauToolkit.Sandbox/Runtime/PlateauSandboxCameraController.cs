using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace PlateauToolkit.Sandbox
{
    class PlateauSandboxCameraController
    {
        readonly PlateauSandboxCameraSettings m_Settings;

        readonly Transform m_CameraPivot;
        readonly IPlateauSandboxCameraTarget m_Target;
        readonly PlateauSandboxCameraMode m_Mode;

        float m_RotationX;
        float m_RotationY;// Default offset
        float m_Pitch; // Vertical angle
        float m_Yaw; // Horizontal angle

        Vector3 m_ViewOffset;

        public PlateauSandboxCameraController(
            PlateauSandboxCameraSettings settings,
            Transform cameraPivot,
            IPlateauSandboxCameraTarget target,
            PlateauSandboxCameraMode mode)
        {
            m_Settings = settings;
            m_CameraPivot = cameraPivot;
            m_Target = target;
            m_Mode = mode;

            switch (mode)
            {
                case PlateauSandboxCameraMode.FirstPersonView:
                    m_CameraPivot.localRotation = Quaternion.Euler(
                        m_Target.Rotation.eulerAngles.x,
                        m_Target.Rotation.eulerAngles.y,
                        0f);
                    break;
                case PlateauSandboxCameraMode.ThirdPersonOrbit:
                    m_ViewOffset = new Vector3(0, 0, target.CameraTargetSettings.ThirdPersonOrbitDefaultDistance);
                    m_Yaw = target.CameraTargetSettings.ThirdPersonOrbitInitialRotation.x;
                    m_Pitch = target.CameraTargetSettings.ThirdPersonOrbitInitialRotation.y;
                    break;
            }
        }

        public PlateauSandboxCameraMode CameraMode => m_Mode;
        public IPlateauSandboxCameraTarget Target => m_Target;

        bool m_IsDragStartedFromUI;

        public void HandleInput()
        {
            Vector2 delta = Vector2.zero;
            Vector2 scroll = Vector2.zero;
            bool isMiddleButtonPressed = false;
            bool isLeftButtonPressed = false;

            if (Mouse.current != null)
            {
                do
                {
                    if (Mouse.current.leftButton.wasPressedThisFrame &&
                        EventSystem.current != null &&
                        EventSystem.current.IsPointerOverGameObject())
                    {
                        m_IsDragStartedFromUI = true;
                        break;
                    }

                    if (m_IsDragStartedFromUI)
                    {
                        if (Mouse.current.leftButton.isPressed)
                        {
                            break;
                        }

                        m_IsDragStartedFromUI = false;
                    }

                    delta = Mouse.current.delta.ReadValue();
                    scroll = Mouse.current.scroll.ReadValue();
                    isMiddleButtonPressed = Mouse.current.middleButton.isPressed;
                    isLeftButtonPressed = Mouse.current.leftButton.isPressed;
                } while (false);
            }

            switch (m_Mode)
            {
                case PlateauSandboxCameraMode.FirstPersonView:
                {
                    m_CameraPivot.position = m_Target.CameraTargetSettings.FirstPersonViewPosition;

                    if (isLeftButtonPressed)
                    {
                        m_RotationX += delta.y;
                        m_RotationY -= delta.x;
                    }

                    // Clamp the x rotation to prevent flipping the camera over
                    m_RotationX = Mathf.Clamp(m_RotationX, -80f, 25f);

                    // Apply the rotation to the camera pivot
                    m_CameraPivot.localRotation = Quaternion.Euler(
                        m_Target.Rotation.eulerAngles.x + m_RotationX,
                        m_Target.Rotation.eulerAngles.y + m_RotationY,
                        0f);

                    break;
                }

                case PlateauSandboxCameraMode.ThirdPersonView:
                {
                    Vector3 forward = m_Target.Rotation * Vector3.forward;
                    Vector3 up = m_Target.Rotation * Vector3.up;
                    Vector3 right = m_Target.Rotation * Vector3.right;

                    if (isMiddleButtonPressed)
                    {
                        m_ViewOffset += new Vector3(
                            -delta.x * m_Settings.m_HorizontalDragSpeed,
                            -delta.y * m_Settings.m_VerticalDragSpeed,
                            0);
                    }

                    m_ViewOffset.z -= scroll.y * m_Settings.m_ZoomSpeed;

                    m_CameraPivot.position = m_Target.CameraTargetSettings.ThirdPersonViewDefaultCameraPosition
                        + forward * m_ViewOffset.z
                        + up * m_ViewOffset.y
                        + right * m_ViewOffset.x;

                    if (isLeftButtonPressed) // Holding the left mouse button
                    {
                        m_Yaw -= delta.x * m_Settings.m_HorizontalSpeed;
                        m_Pitch -= delta.y * m_Settings.m_VerticalSpeed;
                        m_Pitch = Mathf.Clamp(m_Pitch, -70f, 70f); // Clamp to prevent flipping
                    }

                    m_CameraPivot.forward = Quaternion.Euler(m_Pitch, m_Yaw, 0) * forward;

                    break;
                }

                case PlateauSandboxCameraMode.ThirdPersonOrbit:
                {
                    // Adjust offset based on scroll wheel
                    m_ViewOffset.z = Mathf.Clamp(
                        m_ViewOffset.z - scroll.y * m_Settings.m_ZoomSpeed,
                        m_Settings.m_MinOffset,
                        m_Settings.m_MaxOffset);

                    if (isLeftButtonPressed) // Holding the left mouse button
                    {
                        // Rotate camera pivot around the target horizontally
                        m_Yaw += delta.x * m_Settings.m_HorizontalSpeed;

                        // Rotate camera pivot around the target vertically
                        m_Pitch -= delta.y * m_Settings.m_VerticalSpeed;
                        m_Pitch = Mathf.Clamp(m_Pitch, -70f, 70f); // Clamp to prevent flipping
                    }

                    // Apply the rotation to the pivot based on mouse input and target's rotation
                    Quaternion rotation = m_Target.Rotation * Quaternion.Euler(m_Pitch, m_Yaw, 0);
                    m_CameraPivot.rotation = rotation;

                    // Update the camera pivot position to keep it offset from the target
                    m_CameraPivot.position = m_Target.CameraTargetSettings.ThirdPersonOrbitCenter + m_CameraPivot.rotation * -m_ViewOffset;

                    // Ensure the camera is always looking at the target
                    m_CameraPivot.LookAt(m_Target.CameraTargetSettings.ThirdPersonOrbitCenter);

                    break;
                }
            }
        }
    }
}