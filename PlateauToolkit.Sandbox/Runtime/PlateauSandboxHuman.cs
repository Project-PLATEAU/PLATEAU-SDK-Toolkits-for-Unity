using UnityEngine;

namespace PlateauToolkit.Sandbox
{
    [SelectionBase]
    public class PlateauSandboxHuman :
        PlateauSandboxPlaceableHandler,
        IPlateauSandboxMovingObject,
        IPlateauSandboxCameraTarget
    {
        const float k_LerpSpeed = 0.5f;

        [SerializeField] Animator m_Animator;
        [SerializeField] bool m_IsCameraViewAvailable;
        [SerializeField] PlateauSandboxCameraTargetSettings m_CameraTargetSettings;

        float m_Speed;
        static readonly int k_IsWalking = Animator.StringToHash("IsWalking");
        static readonly int k_MoveSpeed = Animator.StringToHash("MoveSpeed");

        public Vector3 TransformUp => transform.up;

        bool IPlateauSandboxCameraTarget.IsCameraViewAvailable => m_IsCameraViewAvailable;
        public PlateauSandboxCameraTargetSettings CameraTargetSettings => m_CameraTargetSettings;

        public Quaternion Rotation => transform.rotation;

        void Awake()
        {
            m_CameraTargetSettings.Transform = transform;
        }

        public void OnMoveBegin()
        {
            if (m_Animator == null)
            {
                return;
            }
            m_Animator.SetBool(k_IsWalking, true);
        }

        public void OnMove(in MovementInfo movementInfo, PlateauSandboxTrack track)
        {
            (Vector3 position, Vector3 forward) = track.GetPositionAndForwardBySplineContainerT(movementInfo.m_SplineContainerT);
            m_Speed = movementInfo.m_MoveDelta / Time.deltaTime;

            Quaternion toRotation = Quaternion.LookRotation(forward, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, Time.deltaTime);

            SetPosition(position);
        }

        public void OnMoveEnd()
        {
            m_Speed = 0;

            if (m_Animator == null)
            {
                return;
            }
            m_Animator.SetBool(k_IsWalking, false);
        }

        void LateUpdate()
        {
            if (m_Animator == null)
            {
                return;
            }
            float currentMoveSpeed = m_Animator.GetFloat(k_MoveSpeed);
            m_Animator.SetFloat(
                k_MoveSpeed,
                Mathf.Lerp(currentMoveSpeed, m_Speed, Time.deltaTime * k_LerpSpeed));
        }
    }

}