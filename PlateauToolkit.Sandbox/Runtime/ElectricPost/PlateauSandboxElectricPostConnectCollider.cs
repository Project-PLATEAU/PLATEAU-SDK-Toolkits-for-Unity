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

        public void OnMouseHover(PlateauSandboxElectricPostSelectingInfo info)
        {
            if (info.post == null)
            {
                return;
            }

            if (info.post == m_ParentPost)
            {
                return;
            }

            m_ParentPost.SetHighLight(true);

            // 選択中の電柱から電線を表示してもらう
            info.post.SetWire(info.isFront, info.index);
            info.post.TryShowWire(info.isFront, info.index, m_ParentPost, isFront);
        }

        public void OnMoveLeave(PlateauSandboxElectricPostSelectingInfo info)
        {
            if (info.post == null)
            {
                return;
            }

            // 電線非表示
            info.post.RemoveWireID(info.isFront, info.index);
            info.post.TryShowWire(info.isFront, info.index, null, isFront);

            m_ParentPost.SetHighLight(false);
        }

        public void OnSelect(PlateauSandboxElectricPostSelectingInfo info)
        {
            if (info.post == null)
            {
                return;
            }

            // 自身の接続情報を設定
            int ownIndex = m_ParentPost.AddConnectionAndWires(isFront);

            // 接続
            string wireID = m_ParentPost.GetWireID();
            m_ParentPost.SetConnectPoint(info.post, isFront, info.isFront, ownIndex, wireID, info.index);
            info.post.SetConnectPoint(m_ParentPost, info.isFront, isFront, info.index, wireID, ownIndex);

            // ハイライト解除
            m_ParentPost.SetHighLight(false);
        }
    }
}