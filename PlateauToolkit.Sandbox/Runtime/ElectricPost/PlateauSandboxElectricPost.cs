using PlateauToolkit.Sandbox.Editor;
using PlateauToolkit.Sandbox.Runtime;
using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Runtime.ElectricPost
{
    [ExecuteAlways]
    public class PlateauSandboxElectricPost : PlateauSandboxStreetFurniture
    {
        // ワイヤー
        private PlateauSandboxElectricPostWireHandler m_ElectricPostWireHandler;
        public bool IsShowingFrontWire => m_ElectricPostWireHandler.IsFrontShowing;
        public bool IsShowingBackWire => m_ElectricPostWireHandler.IsBackShowing;

        // 接続部分
        private PlateauSandboxElectricPostConnectPoints m_ElectricPostConnectPoints;

        // 自動で接続される範囲
        private const float m_SearchDistance = 50.0f;

        private PlateauSandboxElectricPostContext m_Context;

        // メッシュ操作
        private PlateauSandboxElectricPostMesh m_Mesh;

        private PlateauSandboxElectricPostInfo m_Info;
        public (PlateauSandboxElectricPost target, bool isFront) FrontConnectedPost => m_Info?.FrontConnectedPost ?? (null, false);
        public (PlateauSandboxElectricPost target, bool isFront) BackConnectedPost => m_Info?.BackConnectedPost ?? (null, false);

        private void Start()
        {
            m_Context = PlateauSandboxElectricPostContext.GetCurrent();
            m_Context.OnCancel.AddListener(() => SetHighLight(false));

            m_ElectricPostWireHandler = new PlateauSandboxElectricPostWireHandler(gameObject);
            m_ElectricPostConnectPoints = new PlateauSandboxElectricPostConnectPoints(gameObject);
            m_Mesh = new PlateauSandboxElectricPostMesh(gameObject);
            m_Info = new PlateauSandboxElectricPostInfo(this);

            if (hideFlags == HideFlags.None)
            {
                // 配置完了したら実行
                SearchPost();
            }
        }

        private void OnDestroy()
        {
            if (m_Info == null)
            {
                return;
            }

            if (m_Info.FrontConnectedPost.target != null)
            {
                m_Info.FrontConnectedPost.target.RemoveConnectedPost(this);
            }
            if (m_Info.BackConnectedPost.target != null)
            {
                m_Info.BackConnectedPost.target.RemoveConnectedPost(this);
            }
        }

        private void SearchPost()
        {
            // 他の配置されている一番近い電柱を取得
            var nearestPost = GetNearestPost();
            if (nearestPost != null)
            {
                bool isOwnFront = IsTargetFacingForward(nearestPost.transform.position);

                // 接続できる方を取得
                bool isOtherFront = nearestPost.CanConnect(true);
                if (!isOwnFront)
                {
                    isOtherFront = !isOtherFront;
                }

                if (isOwnFront)
                {
                    RemoveConnectedPost(false);
                    SetFrontConnectPoint(nearestPost, isOtherFront);
                }
                else
                {
                    RemoveConnectedPost(true);
                    SetBackConnectPoint(nearestPost, isOtherFront);
                }
            }
        }

        private void Update()
        {
            if (m_Info == null)
            {
                return;
            }

            TryShowFrontWire();
            TryShowBackWire();
        }

        private void TryShowFrontWire()
        {
            if (m_Info.CanShowFrontWire())
            {
                m_ElectricPostWireHandler.ShowToTarget(
                    true,
                    m_Info.FrontConnectedPost.target,
                    m_Info.FrontConnectedPost.isFront);
            }
            else
            {
                m_ElectricPostWireHandler.Hide(true);
            }
        }

        private void TryShowBackWire()
        {
            if (m_Info.CanShowBackWire())
            {
                m_ElectricPostWireHandler.ShowToTarget(
                    false,
                    m_Info.BackConnectedPost.target,
                    m_Info.BackConnectedPost.isFront);
            }
            else
            {
                m_ElectricPostWireHandler.Hide(false);
            }
        }

        private PlateauSandboxElectricPost GetNearestPost()
        {
            var electricPosts = FindObjectsOfType<PlateauSandboxElectricPost>();
            PlateauSandboxElectricPost nearestPost = null;
            float nearestDistance = float.MaxValue;
            foreach (var electricPost in electricPosts)
            {
                if (electricPost == this)
                {
                    continue;
                }

                // すでに別の電柱と接続されている場合はスキップ
                if (!electricPost.CanConnect(true) && !electricPost.CanConnect(false))
                {
                    continue;
                }

                if (TryIsObstacleBetween(electricPost.gameObject))
                {
                    // 障害物チェック
                    continue;
                }

                // 範囲チェック
                float distance = Vector3.Distance(electricPost.transform.position, transform.position);
                if (distance > m_SearchDistance)
                {
                    continue;
                }

                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestPost = electricPost;
                }
            }

            return nearestPost;
        }

        public bool IsTargetFacingForward(Vector3 position)
        {
            Vector3 toTarget = position - gameObject.transform.position;
            float angle = Vector3.Angle(gameObject.transform.forward, toTarget);

            // 90度未満の場合は正面
            return angle < 90f;
        }

        public bool CanConnect(bool isFront)
        {
            return (isFront && m_Info.FrontConnectedPost.target == null) ||
                   (!isFront && m_Info.BackConnectedPost.target == null);
        }

        private bool TryIsObstacleBetween(GameObject target)
        {
            // 障害物チェック
            // var startPoint =
            return false;
        }

        public void SetFrontConnectPointToFacing(PlateauSandboxElectricPost other)
        {
            // 向いている方で設定
            bool isOtherFront = other.IsTargetFacingForward(transform.position);
            SetFrontConnectPoint(other, isOtherFront);
        }

        public void SetFrontConnectPoint(PlateauSandboxElectricPost other, bool isOtherFront)
        {
            if (!m_Info.CanConnect(true, other))
            {
                return;
            }

            m_Info.SetFrontConnect(other, isOtherFront);

            // 他の電柱にも接続を通知
            if (isOtherFront)
            {
                other.SetFrontConnectPoint(this, true);
            }
            else
            {
                other.SetBackConnectPoint(this, true);
            }
        }

        public void SetBackConnectPointToFacing(PlateauSandboxElectricPost other)
        {
            // 向いている方で設定
            bool isOtherFront = other.IsTargetFacingForward(transform.position);
            SetBackConnectPoint(other, isOtherFront);
        }

        public void SetBackConnectPoint(PlateauSandboxElectricPost other, bool isOtherFront)
        {
            if (!m_Info.CanConnect(false, other))
            {
                return;
            }

            m_Info.SetBackConnect(other, isOtherFront);

            // 他の電柱にも接続を通知
            if (isOtherFront)
            {
                other.SetFrontConnectPoint(this, false);
            }
            else
            {
                other.SetBackConnectPoint(this, false);
            }
        }

        private void RemoveConnectedPost(bool isFront)
        {
            if (isFront)
            {
                m_Info.SetFrontConnect(null, false);
            }
            else
            {
                m_Info.SetBackConnect(null, false);
            }
        }

        public void RemoveConnectedPost(PlateauSandboxElectricPost targetPost)
        {
            if (m_Info.FrontConnectedPost.target == targetPost)
            {
                m_Info.SetFrontConnect(null, true);
                targetPost?.RemoveConnectedPost(this);
            }
            else if (m_Info.BackConnectedPost.target == targetPost)
            {
                m_Info.SetBackConnect(null, false);
                targetPost?.RemoveConnectedPost(this);
            }
        }

        public Vector3 GetConnectPoint(PlateauSandboxElectricPostWireType wireType, bool isFront)
        {
            switch (wireType)
            {
                case PlateauSandboxElectricPostWireType.k_TopA:
                    return m_ElectricPostConnectPoints.GetConnectPoint(PlateauSandboxElectricPostWireType.k_TopA, isFront);
                case PlateauSandboxElectricPostWireType.k_TopB:
                    return m_ElectricPostConnectPoints.GetConnectPoint(PlateauSandboxElectricPostWireType.k_TopB, isFront);
                case PlateauSandboxElectricPostWireType.k_TopC:
                    return m_ElectricPostConnectPoints.GetConnectPoint(PlateauSandboxElectricPostWireType.k_TopC, isFront);
                case PlateauSandboxElectricPostWireType.k_BottomA:
                    return m_ElectricPostConnectPoints.GetConnectPoint(PlateauSandboxElectricPostWireType.k_BottomA, isFront);
                case PlateauSandboxElectricPostWireType.k_BottomB:
                    return m_ElectricPostConnectPoints.GetConnectPoint(PlateauSandboxElectricPostWireType.k_BottomB, isFront);
            }
            return Vector3.zero;
        }

        public void SetHighLight(bool isSelecting)
        {
            m_Mesh.SetHighLight(isSelecting);
        }

        //
        // Vector3 _point = Vector3.zero;
        // void OnDrawGizmos()
        // {
        //     foreach (PlateauSandboxElectricPostWire postWire in m_ElectricPostWireHandler.PostWires)
        //     {
        //         var point = Info.FrontConnectedPost.GetConnectPoint(postWire.WireType);
        //         Gizmos.color = Color.yellow;
        //         Gizmos.DrawSphere(point, 0.2f);
        //         Gizmos.color = Color.red;
        //         Gizmos.DrawSphere(postWire.WirePosition, 0.2f);
        //     }
        // }
    }
}