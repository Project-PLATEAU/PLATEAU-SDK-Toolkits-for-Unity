using UnityEngine;
using UnityEngine.Events;

namespace PlateauToolkit.Sandbox.Runtime.ElectricPost
{
    public class PlateauSandboxElectricPostContext
    {
        static PlateauSandboxElectricPostContext s_Current = new();

        private (PlateauSandboxElectricPost target, bool isTargetFront) m_FrontConnectedPost = (null, false);
        public (PlateauSandboxElectricPost target, bool isTargetFront) FrontConnectedPost => m_FrontConnectedPost;

        private (PlateauSandboxElectricPost target, bool isTargetFront) m_BackConnectedPost = (null, false);
        public (PlateauSandboxElectricPost target, bool isTargetFront) BackConnectedPost => m_BackConnectedPost;

        private bool m_IsFrontNodeSelecting;
        public bool IsFrontNodeSelecting => m_IsFrontNodeSelecting;

        private bool m_IsBackNodeSelecting;
        public bool IsBackNodeSelecting => m_IsBackNodeSelecting;

        // 選択した電柱、Front / Back
        public UnityEvent<PlateauSandboxElectricPost, bool> OnSelected { get; } = new();

        // マウスの位置、Front / Back
        public UnityEvent<Vector3, bool> OnMoseMove { get; } = new();

        // Front / Back の選択をキャンセル
        public UnityEvent<bool> OnCancel { get; } = new();

        public static PlateauSandboxElectricPostContext GetCurrent()
        {
            return s_Current;
        }

        public void SetFrontConnect(PlateauSandboxElectricPost target, bool isTargetFront)
        {
            m_FrontConnectedPost = (target, isTargetFront);
        }

        public void SetBackConnect(PlateauSandboxElectricPost target, bool isTargetFront)
        {
            m_BackConnectedPost = (target, isTargetFront);
        }

        public void SetSelecting(bool isFront, bool isSelecting)
        {
            m_IsFrontNodeSelecting = false;
            m_IsBackNodeSelecting = false;

            if (isFront)
            {
                m_IsFrontNodeSelecting = isSelecting;
            }
            else
            {
                m_IsBackNodeSelecting = isSelecting;
            }
        }
    }
}