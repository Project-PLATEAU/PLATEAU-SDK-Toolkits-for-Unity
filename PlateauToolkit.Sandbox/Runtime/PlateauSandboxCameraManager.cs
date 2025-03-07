using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PlateauToolkit.Sandbox
{
    public enum PlateauSandboxCameraMode
    {
        None,
        FirstPersonView,
        ThirdPersonView,
        ThirdPersonOrbit,
    }

    [Serializable]
    public class PlateauSandboxCameraTargetSettings
    {
        [SerializeField] Vector3 m_FirstPersonViewPosition;
        [SerializeField] Vector3 m_ThirdPersonViewDefaultCameraPosition;
        [SerializeField] Vector2 m_ThirdPersonOrbitInitialRotation;
        [SerializeField] Vector3 m_ThirdPersonOrbitOffset;
        [SerializeField] float m_ThirdPersonOrbitDefaultDistance;

        public Transform Transform { get; set; }
        public Vector3 FirstPersonViewPosition =>
            Transform.TransformPoint(m_FirstPersonViewPosition);
        public Vector3 ThirdPersonViewDefaultCameraPosition =>
            Transform.TransformPoint(m_ThirdPersonViewDefaultCameraPosition);
        public Vector3 ThirdPersonOrbitCenter =>
            Transform.position + m_ThirdPersonOrbitOffset;
        public Vector2 ThirdPersonOrbitInitialRotation =>
            m_ThirdPersonOrbitInitialRotation;
        public float ThirdPersonOrbitDefaultDistance =>
            m_ThirdPersonOrbitDefaultDistance;
    }

    interface IPlateauSandboxCameraTarget
    {
        bool IsCameraViewAvailable { get; }
        Quaternion Rotation { get; }
        PlateauSandboxCameraTargetSettings CameraTargetSettings { get; }
    }

    [Serializable]
    class PlateauSandboxCameraSettings
    {
        public float m_HorizontalSpeed = 2.0f;
        public float m_VerticalSpeed = 2.0f;
        public float m_HorizontalDragSpeed = 0.1f;
        public float m_VerticalDragSpeed = 0.1f;
        public float m_ZoomSpeed = 0.001f;
        public float m_MinOffset = 1f;
        public float m_MaxOffset = 50f;
    }

    public class PlateauSandboxCameraManager : MonoBehaviour
    {
        [SerializeField] Camera m_SubCamera;
        [SerializeField] Transform m_CameraPivot;
        [SerializeField] PlateauSandboxCameraSettings m_CameraSettings;

        [SerializeField] bool m_EnableKeyboardCameraSwitch = true;

        Camera m_LastMainCamera;
        PlateauSandboxCameraController m_CurrentCameraController;

        public bool EnableKeyboardCameraSwitch
        {
            get => m_EnableKeyboardCameraSwitch;
            set => m_EnableKeyboardCameraSwitch = value;
        }

        public PlateauSandboxCameraMode CurrentCameraMode =>
            m_CurrentCameraController?.CameraMode ?? PlateauSandboxCameraMode.None;

        public void UseLastMainCamera()
        {
            m_CurrentCameraController = null;
            m_SubCamera.enabled = false;
            m_LastMainCamera.enabled = true;
            m_LastMainCamera = null;
        }

        public void SwitchCamera(PlateauSandboxCameraMode cameraMode)
        {
            if (m_CurrentCameraController == null)
            {
                // The sandbox camera is now not in use.
                return;
            }

            if (m_CurrentCameraController.CameraMode == cameraMode)
            {
                // Already using the mode.
                return;
            }

            if (cameraMode == PlateauSandboxCameraMode.None)
            {
                UseLastMainCamera();
                return;
            }

            m_CurrentCameraController = new(
                m_CameraSettings, m_CameraPivot, m_CurrentCameraController.Target, cameraMode);
        }

        void Update()
        {
            if (m_CurrentCameraController == null)
            {
                Mouse mouse = Mouse.current;
                if (mouse != null && mouse.leftButton.wasPressedThisFrame)
                {
                    Vector2 mousePosition = mouse.position.ReadValue();
                    CheckCameraTarget(mousePosition);
                }
            }
            else
            {
                Camera mainCamera = Camera.main;
                if (mainCamera != null && m_SubCamera != mainCamera)
                {
                    // The main camera has been changed outside
                    m_SubCamera.enabled = false;
                    m_LastMainCamera = null;
                    m_CurrentCameraController = null;
                    return;
                }

                if (m_EnableKeyboardCameraSwitch)
                {
                    CheckKeyboardCameraSwitch();
                }
            }
        }

        void CheckKeyboardCameraSwitch()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null)
            {
                PlateauSandboxCameraMode cameraMode;
                if (keyboard.digit1Key.wasPressedThisFrame)
                {
                    cameraMode = PlateauSandboxCameraMode.FirstPersonView;
                }
                else if (keyboard.digit2Key.wasPressedThisFrame)
                {
                    cameraMode = PlateauSandboxCameraMode.ThirdPersonView;
                }
                else if (keyboard.digit3Key.wasPressedThisFrame)
                {
                    cameraMode = PlateauSandboxCameraMode.ThirdPersonOrbit;
                }
                else if (keyboard.digit0Key.wasPressedThisFrame)
                {
                    cameraMode = PlateauSandboxCameraMode.None;
                }
                else
                {
                    return;
                }

                SwitchCamera(cameraMode);
            }
        }

        void LateUpdate()
        {
            try
            {
                m_CurrentCameraController?.HandleInput();
            }
            catch (MissingReferenceException)
            {
                // 追跡車両が消えたらMissingReferenceExceptionが発生
                // SandboxCameraの制御を解除
                SwitchCamera(PlateauSandboxCameraMode.None);
            }
        }

        void CheckCameraTarget(Vector3 screenPosition)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                return;
            }

            Ray ray = mainCamera.ScreenPointToRay(screenPosition);
            if (!Physics.Raycast(ray, out RaycastHit hit))
            {
                return;
            }

            if (!PlateauSandboxObjectFinder.TryGetSandboxObject(hit.collider, out IPlateauSandboxPlaceableObject sandboxObject))
            {
                return;
            }

            if (sandboxObject is not IPlateauSandboxCameraTarget cameraTarget)
            {
                return;
            }

            if (!cameraTarget.IsCameraViewAvailable)
            {
                return;
            }

            m_LastMainCamera = mainCamera;
            m_LastMainCamera.enabled = false;
            m_SubCamera.enabled = true;
            m_CurrentCameraController = new(
                m_CameraSettings, m_CameraPivot, cameraTarget, PlateauSandboxCameraMode.FirstPersonView);
        }

        public void SetCameraTarget(Collider targetCollider)
        {
            if (targetCollider == null)
            {
                return;
            }

            if (!PlateauSandboxObjectFinder.TryGetSandboxObject(targetCollider, out IPlateauSandboxPlaceableObject sandboxObject))
            {
                return;
            }

            if (sandboxObject is not IPlateauSandboxCameraTarget cameraTarget)
            {
                return;
            }

            if (!cameraTarget.IsCameraViewAvailable)
            {
                return;
            }

            if (Camera.main == null)
            {
                return;
            }
            m_LastMainCamera = Camera.main;
            m_LastMainCamera.enabled = false;
            m_SubCamera.enabled = true;
            m_CurrentCameraController = new(
                m_CameraSettings, m_CameraPivot, cameraTarget, PlateauSandboxCameraMode.FirstPersonView);
        }
    }
}