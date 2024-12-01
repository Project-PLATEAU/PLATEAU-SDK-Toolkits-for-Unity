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
            if (isFront)
            {
                m_ParentPost.SetFrontConnectPoint(otherPost, isOtherFront);
            }
            else
            {
                m_ParentPost.SetBackConnectPoint(otherPost, isOtherFront);
            }
        }

        public void OnMoveLeave(PlateauSandboxElectricPost targetPost)
        {
            if (targetPost == null)
            {
                return;
            }

            m_ParentPost.RemoveConnectedPost(targetPost);
            m_ParentPost.SetHighLight(false);
        }
    }
}