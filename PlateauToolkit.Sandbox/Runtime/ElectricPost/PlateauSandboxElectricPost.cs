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
        private PlateauSandboxElectricPostWireHandler m_FrontElectricPostWireHandler;
        private PlateauSandboxElectricPostWireHandler m_BackElectricPostWireHandler;

        // 自動で接続される範囲
        private const float m_SearchDistance = 50.0f;

        private PlateauSandboxElectricPostContext m_Context;

        // メッシュ操作
        private PlateauSandboxElectricPostMesh m_Mesh;

        private void Start()
        {
            m_Context = PlateauSandboxElectricPostContext.GetCurrent();
            m_Context.OnSelected.AddListener(SetConnectToPost);
            m_Context.OnMoseMove.AddListener(UpdateToMousePoint);
            m_Context.OnCancel.AddListener(Cancel);

            m_FrontElectricPostWireHandler = new PlateauSandboxElectricPostWireHandler(gameObject, true);
            m_BackElectricPostWireHandler = new PlateauSandboxElectricPostWireHandler(gameObject, false);

            m_Mesh = new PlateauSandboxElectricPostMesh(gameObject);

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
            if (m_Context == null)
            {
                return;
            }

            if (m_Context.FrontConnectedPost.target != null)
            {
                UpdatePostWire(true);
            }

            if (m_Context.BackConnectedPost.target != null)
            {
                UpdatePostWire(false);
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

                // 向いている方向を判定
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

        private bool CanConnect(bool isFront)
        {
            return (isFront && m_Context.FrontConnectedPost.target == null) ||
                   (!isFront && m_Context.BackConnectedPost.target == null);
        }

        private bool TryIsObstacleBetween(GameObject target)
        {
            // 障害物チェック
            // var startPoint =
            return false;
        }

        private bool IsTargetFacingForward(Vector3 position)
        {
            Vector3 toTarget = position - gameObject.transform.position;
            float angle = Vector3.Angle(gameObject.transform.forward, toTarget);

            // 90度未満の場合は正面
            return angle < 90f;
        }

        public void SetConnectToPost(PlateauSandboxElectricPost targetPost, bool isOwnFront)
        {
            // すでに設定されていればnullに
            if (isOwnFront && m_Context.BackConnectedPost.target == targetPost)
            {
                m_Context.SetBackConnect(null, false);
            }
            else if (!isOwnFront && m_Context.FrontConnectedPost.target == targetPost)
            {
                m_Context.SetFrontConnect(null, false);
            }

            // セット
            bool isTargetFront = targetPost.IsTargetFacingForward(transform.position);
            if (isOwnFront)
            {
                m_Context.SetFrontConnect(targetPost, isTargetFront);
            }
            else
            {
                m_Context.SetBackConnect(targetPost, isTargetFront);
            }
        }

        private void UpdatePostWire(bool isFront)
        {
            var targetPost = isFront ? m_Context.FrontConnectedPost.target : m_Context.BackConnectedPost.target;
            if (targetPost == null)
            {
                return;
            }

            bool isTargetFront = isFront ? m_Context.FrontConnectedPost.isTargetFront : m_Context.BackConnectedPost.isTargetFront;
            var targetCenterPoint = targetPost.GetTargetCenterPoint(isTargetFront);
            if (isFront)
            {
                m_FrontElectricPostWireHandler.ShowToPoint(targetCenterPoint);
            }
            else
            {
                m_BackElectricPostWireHandler.ShowToPoint(targetCenterPoint);
            }
        }

        public void UpdateToMousePoint(Vector3 mousePoint, bool isFrontSelect)
        {
            // マウスの位置まで表示
            if (isFrontSelect)
            {
                m_FrontElectricPostWireHandler.ShowToPoint(mousePoint);
            }
            else
            {
                m_BackElectricPostWireHandler.ShowToPoint(mousePoint);
            }
        }

        private Vector3 GetTargetCenterPoint(bool isFront)
        {
            if (isFront)
            {
                return m_FrontElectricPostWireHandler.GetCenterWirePoint();
            }
            else
            {
                return m_BackElectricPostWireHandler.GetCenterWirePoint();
            }
        }

        public void Cancel(bool isFront)
        {
            if (isFront)
            {
                if (m_Context.FrontConnectedPost.target == null)
                {
                    m_FrontElectricPostWireHandler.Cancel();
                }
            }
            else
            {
                if (m_Context.BackConnectedPost.target == null)
                {
                    m_BackElectricPostWireHandler.Cancel();
                }
            }
        }

        public void SetSelecting(bool isSelect)
        {
            m_Mesh.SetHighLight(isSelect);
        }

        // Vector3 setPoint = Vector3.zero;
        // private void OnDrawGizmos()
        // {
        //     Gizmos.color = Color.yellow;
        //     Gizmos.DrawSphere(setPoint, 0.2f);
        // }
    }
}