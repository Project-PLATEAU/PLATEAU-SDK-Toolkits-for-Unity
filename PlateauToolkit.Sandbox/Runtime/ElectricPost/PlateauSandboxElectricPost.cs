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

        // 接続部分
        private PlateauSandboxElectricPostConnectPoints m_ElectricPostConnectPoints;

        // 自動で接続される範囲
        private const float m_SearchDistance = 50.0f;

        private PlateauSandboxElectricPostContext m_Context;

        // メッシュ操作
        private PlateauSandboxElectricPostMesh m_Mesh;

        public PlateauSandboxElectricPostInfo Info { get; private set; }
        public PlateauSandboxElectricPost FrontConnectedPost => Info.FrontConnectedPost;
        public PlateauSandboxElectricPost BackConnectedPost => Info.BackConnectedPost;

        private void Start()
        {
            m_Context = PlateauSandboxElectricPostContext.GetCurrent();
            m_Context.SetTarget(this);
            Info = new PlateauSandboxElectricPostInfo(this);

            m_ElectricPostWireHandler = new PlateauSandboxElectricPostWireHandler(gameObject);
            m_ElectricPostConnectPoints = new PlateauSandboxElectricPostConnectPoints(gameObject);
            m_Mesh = new PlateauSandboxElectricPostMesh(gameObject);
        }

        public override void SetPosition(in Vector3 position)
        {
            base.SetPosition(in position);

            SearchPost();
        }

        private void SearchPost()
        {
            if (Info == null)
            {
                return;
            }

            // 他の配置されている一番近い電柱を取得
            var nearestPost = GetNearestPost();
            if (nearestPost != null)
            {
                bool isOwnFront = IsTargetFacingForward(nearestPost.transform.position);
                SetConnectToPost(nearestPost, isOwnFront);
            }
        }

        private void Update()
        {
            if (Info == null)
            {
                return;
            }

            // 線を表示させるのは前方のみ
            if (Info.FrontConnectedPost != null)
            {
                m_ElectricPostWireHandler.ShowToTarget(Info.FrontConnectedPost);
            }
            else
            {
                m_ElectricPostWireHandler.Hide();
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

                Debug.Log($"target : {electricPost.gameObject.hideFlags} : {electricPost.gameObject.name}");
                if (electricPost.gameObject.hideFlags == HideFlags.HideAndDontSave)
                {
                    continue;
                }

                // 相手が向いている方向を判定
                bool isTargetFront = electricPost.IsTargetFacingForward(gameObject.transform.position);
                if (!electricPost.CanConnect(isTargetFront))
                {
                    // すでに別の電柱と接続されている場合はスキップ
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

        private bool CanConnect(bool isFront)
        {
            return (isFront && Info.FrontConnectedPost == null) ||
                   (!isFront && Info.BackConnectedPost == null);
        }

        private bool TryIsObstacleBetween(GameObject target)
        {
            // 障害物チェック
            // var startPoint =
            return false;
        }

        public void SetConnectToPost(PlateauSandboxElectricPost targetPost, bool isOwnFront)
        {
            if (isOwnFront && Info.FrontConnectedPost == targetPost)
            {
                return;
            }
            else if (!isOwnFront && Info.BackConnectedPost == targetPost)
            {
                return;
            }

            // すでに別の方に設定されていればnullに
            if (isOwnFront && Info.BackConnectedPost == targetPost)
            {
                Info.SetConnect(null, false);
            }
            else if (!isOwnFront && Info.FrontConnectedPost == targetPost)
            {
                Info.SetConnect(null,  true);
            }

            // セット
            Info.SetConnect(targetPost, isOwnFront);
        }

        public Vector3 GetConnectPoint(PlateauSandboxElectricPostWireType wireType)
        {
            // 受け側は位置を反転させる
            switch (wireType)
            {
                case PlateauSandboxElectricPostWireType.k_TopA:
                    return m_ElectricPostConnectPoints.GetConnectPoint(PlateauSandboxElectricPostWireType.k_TopC);
                case PlateauSandboxElectricPostWireType.k_TopB:
                    return m_ElectricPostConnectPoints.GetConnectPoint(PlateauSandboxElectricPostWireType.k_TopB);
                case PlateauSandboxElectricPostWireType.k_TopC:
                    return m_ElectricPostConnectPoints.GetConnectPoint(PlateauSandboxElectricPostWireType.k_TopA);
                case PlateauSandboxElectricPostWireType.k_BottomA:
                    return m_ElectricPostConnectPoints.GetConnectPoint(PlateauSandboxElectricPostWireType.k_BottomB);
                case PlateauSandboxElectricPostWireType.k_BottomB:
                    return m_ElectricPostConnectPoints.GetConnectPoint(PlateauSandboxElectricPostWireType.k_BottomA);
            }
            return Vector3.zero;
        }

        public void SetHighLight(bool isSelecting)
        {
            m_Mesh.SetHighLight(isSelecting);
        }
    }
}