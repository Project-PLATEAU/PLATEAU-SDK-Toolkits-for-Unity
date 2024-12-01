using UnityEngine;
using UnityEngine.Events;

namespace PlateauToolkit.Sandbox.Runtime.ElectricPost
{
    public class PlateauSandboxElectricPostContext
    {
        static PlateauSandboxElectricPostContext s_Current = new();

        public UnityEvent OnSelected { get; } = new();
        public UnityEvent OnCancel { get; } = new();

        private (PlateauSandboxElectricPost target, bool isFront) m_SelectingPost;
        public (PlateauSandboxElectricPost target, bool isFront) SelectingPost => m_SelectingPost;

        public static PlateauSandboxElectricPostContext GetCurrent()
        {
            return s_Current;
        }

        public void SetSelectingPost(PlateauSandboxElectricPost post, bool isSelectFront)
        {
            m_SelectingPost = (post, isSelectFront);
        }
    }
}