using System;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Runtime.ElectricPost
{
    /// <summary>
    ///. 電柱の接続コライダー
    /// </summary>
    [ExecuteAlways]
    public class PlateauSandboxElectricPostConnectCollider : MonoBehaviour
    {
        [SerializeField]
        private bool isFront = true;

        private PlateauSandboxElectricPost m_ParentPost;

        private void Awake()
        {
            m_ParentPost = transform.parent.GetComponent<PlateauSandboxElectricPost>();
        }

        public void OnMouseHover(PlateauSandboxElectricPost otherPost, bool isOtherFront)
        {
            if (otherPost == null)
            {
                return;
            }

            if (otherPost == m_ParentPost)
            {
                return;
            }

            m_ParentPost.SetHighLight(true);

            // 他の電柱から電線を表示してもらう
            otherPost.OnHoverConnectionPoint(isOtherFront, m_ParentPost, isFront);
        }

        public void OnMoveLeave(PlateauSandboxElectricPost otherPost, bool isOtherFront)
        {
            if (otherPost == null)
            {
                return;
            }

            otherPost.OnLeaveConnectionPoint(isOtherFront);
            m_ParentPost.SetHighLight(false);
        }

        public void OnSelect(PlateauSandboxElectricPost otherPost, bool isOtherFront)
        {
            // 接続完了
            if (isFront)
            {
                m_ParentPost.SetFrontConnectPoint(otherPost, isOtherFront);
            }
            else
            {
                m_ParentPost.SetBackConnectPoint(otherPost, isOtherFront);
            }
            m_ParentPost.SetHighLight(false);
        }
    }
}