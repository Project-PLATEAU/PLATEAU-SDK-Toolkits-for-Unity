using UnityEngine;
using UnityEngine.Events;

namespace PlateauToolkit.Sandbox.Runtime.ElectricPost
{
    public class PlateauSandboxElectricPostContext
    {
        static PlateauSandboxElectricPostContext s_Current = new();

        private PlateauSandboxElectricPost m_TargetPost;
        public PlateauSandboxElectricPost TargetPost => m_TargetPost;

        private bool m_IsFrontNodeSelecting;
        public bool IsFrontNodeSelecting => m_IsFrontNodeSelecting;

        private bool m_IsBackNodeSelecting;
        public bool IsBackNodeSelecting => m_IsBackNodeSelecting;

        public UnityEvent OnSelected { get; } = new();
        public UnityEvent OnCancel { get; } = new();

        public static PlateauSandboxElectricPostContext GetCurrent()
        {
            return s_Current;
        }

        public void SetTarget(PlateauSandboxElectricPost target)
        {
            m_TargetPost = target;
        }

        public void SetConnect(bool isFront, PlateauSandboxElectricPost target)
        {
            if (m_TargetPost == null)
            {
                return;
            }

            m_TargetPost.SetConnectToPost(target, isFront);
        }

        public void SetSelect(bool isFront, bool isSelecting)
        {
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