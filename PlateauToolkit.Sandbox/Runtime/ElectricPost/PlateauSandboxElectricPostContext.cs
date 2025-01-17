using UnityEngine;
using UnityEngine.Events;

namespace PlateauToolkit.Sandbox.Runtime.ElectricPost
{
    public struct PlateauSandboxElectricPostSelectingInfo
    {
        public PlateauSandboxElectricPost post;
        public bool isFront;
        public int index;
    }

    public class PlateauSandboxElectricPostContext
    {
        static PlateauSandboxElectricPostContext s_Current = new();

        public UnityEvent OnCancel { get; } = new();
        public UnityEvent OnSelected { get; } = new();

        private PlateauSandboxElectricPostSelectingInfo m_SelectingPost;
        public PlateauSandboxElectricPostSelectingInfo SelectingPost => m_SelectingPost;

        public static PlateauSandboxElectricPostContext GetCurrent()
        {
            return s_Current;
        }

        public void SetSelectingPost(PlateauSandboxElectricPost post, bool isSelectFront, int index)
        {
            m_SelectingPost = new PlateauSandboxElectricPostSelectingInfo
            {
                post = post,
                isFront = isSelectFront,
                index = index
            };
        }

        public void ResetSelect()
        {
            m_SelectingPost = new PlateauSandboxElectricPostSelectingInfo();
        }

        public bool IsSelectingPost(PlateauSandboxElectricPost post, bool isFront)
        {
            return m_SelectingPost.post == post
                   && m_SelectingPost.isFront == isFront;
        }
    }
}