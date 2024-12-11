using System.Collections.Generic;
using System.Linq;
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

        // info
        private PlateauSandboxElectricPostInfo m_Info;
        public List<PlateauSandboxElectricConnectInfo> FrontConnectedPosts => m_Info?.FrontConnectedPosts;
        public List<PlateauSandboxElectricConnectInfo> BackConnectedPosts => m_Info?.BackConnectedPosts;

        private void Start()
        {
            m_Context = PlateauSandboxElectricPostContext.GetCurrent();
            m_Context.OnCancel.AddListener(() => SetHighLight(false));

            m_ElectricPostWireHandler = new PlateauSandboxElectricPostWireHandler(gameObject);
            m_ElectricPostConnectPoints = new PlateauSandboxElectricPostConnectPoints(gameObject);
            m_Mesh = new PlateauSandboxElectricPostMesh(gameObject);
            m_Info = new PlateauSandboxElectricPostInfo();

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

            for (int i = 0; i < m_Info.FrontConnectedPosts.Count; i++)
            {
                var target = m_Info.FrontConnectedPosts[i].m_Target;
                if (target != null)
                {
                    target.RemoveConnection(m_Info.FrontConnectedPosts[i].m_IsTargetFront, i);
                }
            }
            for (int i = 0; i < m_Info.BackConnectedPosts.Count; i++)
            {
                var target = m_Info.BackConnectedPosts[i].m_Target;
                if (target != null)
                {
                    target.RemoveConnection(m_Info.BackConnectedPosts[i].m_IsTargetFront, i);
                }
            }
        }

        private void SearchPost()
        {
            // 他の配置されている一番近い電柱を取得
            var nearestPost = GetNearestPost();
            if (nearestPost != null)
            {
                // 向きで接続部を決定
                bool isOwnFront = IsTargetFacingForward(nearestPost.transform.position);
                bool isOtherFront = nearestPost.IsTargetFacingForward(transform.position);

                // Wire生成
                int ownIndex = AddConnectionAndWires(isOwnFront);
                int otherIndex = nearestPost.AddConnectionAndWires(isOtherFront);

                // 接続
                string wireID = m_Info.GetNextWireID();
                SetConnectPoint(nearestPost, isOwnFront, isOtherFront, ownIndex, wireID, otherIndex);
                nearestPost.SetConnectPoint(this, isOtherFront, isOwnFront, otherIndex, wireID, ownIndex);
            }
        }

        public int AddConnectionAndWires(bool isFront)
        {
            int index = AddConnectionSpace(isFront);
            if (isFront)
            {
                m_ElectricPostWireHandler.CreateWires(true, index);
            }
            else
            {
                m_ElectricPostWireHandler.CreateWires(false, index);
            }
            return index;
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
            if (m_Context.IsSelectingPost(this, true))
            {
                // 選択中であれば処理しない
                return;
            }

            foreach (var plateauSandboxElectricConnectInfo in m_Info.FrontConnectedPosts)
            {
                m_ElectricPostWireHandler.TryShowWires(true, plateauSandboxElectricConnectInfo);
            }
        }

        private void TryShowBackWire()
        {
            if (m_Context.IsSelectingPost(this, false))
            {
                // 選択中であれば処理しない
                return;
            }

            foreach (var plateauSandboxElectricConnectInfo in m_Info.BackConnectedPosts)
            {
                m_ElectricPostWireHandler.TryShowWires(false, plateauSandboxElectricConnectInfo);
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

                // 障害物チェック
                if (TryIsObstacleBetween(electricPost))
                {
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

        private bool TryIsObstacleBetween(PlateauSandboxElectricPost target)
        {
            var startPoint = GetTopCenterPoint();
            Vector3 direction = target.GetTopCenterPoint() - startPoint;
            float distance = direction.magnitude;
            direction.Normalize();

            var ray = new Ray(startPoint, direction);
            var results = Physics.RaycastAll(new Ray(startPoint, direction), distance);
            foreach (RaycastHit rayCastHit in results)
            {
                bool isOwn = rayCastHit.collider.gameObject == gameObject ||
                             rayCastHit.collider.transform.parent?.gameObject == gameObject;
                bool isTarget = rayCastHit.collider.gameObject == target.gameObject ||
                                rayCastHit.collider.transform.parent?.gameObject == target.gameObject;

                if (isOwn || isTarget)
                {
                    // ヒットが自分か、相手の電柱の場合判定しない
                    continue;
                }

                // 障害物と判定
                return true;
            }

            return false;
        }

        public int AddConnectionSpace(bool isFront)
        {
            return m_Info.AddConnectionSpace(isFront);
        }

        public string RemoveConnection(bool isFront, int index)
        {
            m_Info.RemoveConnection(isFront, index);
            return m_ElectricPostWireHandler.RemoveWires(isFront, index);
        }

        public void SetConnectPoint(PlateauSandboxElectricPost other, bool isFront, bool isOtherFront, int index, string wireID, int otherIndex)
        {
            m_ElectricPostWireHandler.SetTarget(isFront, index, new PlateauSandboxElectricConnectInfo()
            {
                m_Target = other,
                m_IsTargetFront = isOtherFront,
                m_OwnIndex = otherIndex
            });
            m_ElectricPostWireHandler.SetWireID(isFront, index, wireID);
            if (isFront)
            {
                m_Info.SetFrontConnect(other, isOtherFront, index, otherIndex);
            }
            else
            {
                m_Info.SetBackConnect(other, isOtherFront, index, otherIndex);
            }
        }

        public void TryShowWire(bool isFront, int index, PlateauSandboxElectricPost target, bool isTargetFront)
        {
            var info = new PlateauSandboxElectricConnectInfo()
            {
                m_Target = target,
                m_IsTargetFront = isTargetFront,
                m_OwnIndex = index
            };
            m_ElectricPostWireHandler.TryShowWires(isFront, info, true);
        }

        public void HideWire(bool isFront, int index)
        {
            m_ElectricPostWireHandler.HideWires(isFront, index);
        }

        public void SetWire(bool isFront, int index)
        {
            string wireID = m_Info.GetNextWireID();
            m_ElectricPostWireHandler.SetWireID(isFront, index, wireID);
        }

        public void RemoveWireID(bool isFront, int index)
        {
            // ホバー終了したら非表示
            m_ElectricPostWireHandler.RemoveWireID(isFront, index);
        }

        public string ResetConnection(bool isFront, int index)
        {
            string wireID = m_ElectricPostWireHandler.RemoveWireID(isFront, index);
            m_Info.ResetConnection(isFront, index);
            return wireID;
        }

        public string GetWireID()
        {
            return m_Info.GetNextWireID();
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

        public Vector3 GetTopCenterPoint()
        {
            return m_ElectricPostConnectPoints.GetConnectPoint(PlateauSandboxElectricPostWireType.k_TopB, true);
        }

        public PlateauSandboxElectricConnectInfo GetConnectedPost(bool isFront, int index)
        {
            return m_Info.GetConnectedPost(isFront, index);
        }

        public bool IsShowingWire(string wireID)
        {
            return m_ElectricPostWireHandler.IsShowingWire(wireID);
        }
    }
}