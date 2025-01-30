using PlateauToolkit.Editor;
using PlateauToolkit.Sandbox.Runtime.ElectricPost;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using EditorGUILayout = UnityEditor.EditorGUILayout;

namespace PlateauToolkit.Sandbox.Editor
{
    /// <summary>
    /// 電柱コンポーネントのGUI表示
    /// </summary>
    public class PlateauSandboxElectricPostConnectionGUI
    {
        public PlateauSandboxElectricPostConnectionGUI(PlateauSandboxElectricPost own, bool isFront)
        {
            m_Context = PlateauSandboxElectricPostContext.GetCurrent();
            m_Own = own;
            m_IsFront = isFront;
        }
        private PlateauSandboxElectricPostContext m_Context;
        private PlateauSandboxElectricPost m_Own;
        private bool m_IsFront;
        private int m_SelectingIndex = -1;

        public UnityEvent<PlateauSandboxElectricPost> OnDirectSelect = new ();
        public UnityEvent<bool> OnClickSelect = new ();

        public void DrawLayout(List<PlateauSandboxElectricConnectInfo> connectedPosts)
        {
            GUILayout.Space(5);
            PlateauToolkitEditorGUILayout.BorderLine();
            GUILayout.Space(5);

            // タイトル
            DrawTitle();

            // 接続先の電柱
            if (connectedPosts != null)
            {
                foreach (var connectedPost in connectedPosts.ToList())
                {
                    PlateauSandboxElectricPost selectingPost = null;

                    GUILayout.Space(5);
                    selectingPost = DrawConnectedPost(connectedPost.m_OwnIndex, connectedPost.m_Target);
                    GUILayout.Space(5);

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (selectingPost != null)
                        {
                            DrawIsConnectedFront(connectedPost.m_OwnIndex, selectingPost);
                        }
                    }

                    GUILayout.Space(5);
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.FlexibleSpace();
                        DrawSelectButton(connectedPost.m_OwnIndex, selectingPost);
                        GUILayout.Space(5);
                        DrawDeleteButton(connectedPost.m_OwnIndex, selectingPost);
                    }

                    GUILayout.Space(5);
                }
            }

            // 追加ボタン
            GUILayout.Space(15);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                DrawAddButton();
                GUILayout.FlexibleSpace();
            }
            GUILayout.Space(10);
        }

        public void DrawTitle()
        {
            // タイトル
            EditorGUILayout.LabelField(m_IsFront ? "前方接続部" : "後方接続部", EditorStyles.boldLabel);
        }

        public PlateauSandboxElectricPost DrawConnectedPost(int index, PlateauSandboxElectricPost target)
        {
            string focusName = "";
            if (target != null)
            {
                // フォーカス時の名前を設定
                focusName = $"{target.name}_{index}";
                GUI.SetNextControlName(focusName);
            }

            // 接続先の電柱
            var selectedPost = EditorGUILayout.ObjectField(
                                   "",
                                   target,
                                   typeof(PlateauSandboxElectricPost), true) as PlateauSandboxElectricPost;

            if (selectedPost != null &&
                selectedPost != target &&
                selectedPost != m_Own)
            {
                // ドロップで直接セット
                bool isOtherFront = selectedPost.IsTargetFacingForward(selectedPost.gameObject.transform.position);
                int otherIndex = selectedPost.AddConnectionAndWires(isOtherFront);

                string wireID = m_Own.GetWireID();
                m_Own.SetConnectPoint(selectedPost, m_IsFront, isOtherFront, index, wireID, otherIndex);
                selectedPost.SetConnectPoint(m_Own, isOtherFront, m_IsFront, otherIndex, wireID, index);
            }

            // Deleteイベント
            if (Event.current.keyCode == KeyCode.Delete ||
                Event.current.keyCode == KeyCode.Backspace)
            {
                if (GUI.GetNameOfFocusedControl() == focusName)
                {
                    // 選択をnullに設定
                    var post = m_Own.GetConnectedPost(m_IsFront, index);
                    selectedPost?.ResetConnection(post.m_IsTargetFront, post.m_TargetIndex);
                    m_Own.ResetConnection(m_IsFront, index);
                }
            }

            return selectedPost;
        }

        private void DrawIsConnectedFront(int count, PlateauSandboxElectricPost target)
        {
            var post = m_Own.GetConnectedPost(m_IsFront, count);

            GUI.enabled = false;
            string text = post.m_IsTargetFront ? "接続部：前方" : "接続部：後方";
            EditorGUILayout.LabelField(text);
            GUI.enabled = true;
        }

        private void DrawSelectButton(int index, PlateauSandboxElectricPost other)
        {
            bool isSelect = index == m_SelectingIndex;
            if (new PlateauToolkitImageButtonGUI(
                    100,
                    20,
                    isSelect ? PlateauToolkitGUIStyles.k_ButtonCancelColor : PlateauToolkitGUIStyles.k_ButtonNormalColor,
                    false)
                .Button("選択する"))
            {
                // 選択時
                if (!isSelect)
                {
                    // ワイヤーを外す
                    var post = m_Own.GetConnectedPost(m_IsFront, index);
                    other?.ResetConnection(post.m_IsTargetFront, post.m_TargetIndex);
                    m_Own.ResetConnection(m_IsFront, index);

                    // ワイヤーを非表示に
                    m_Own.HideWire(m_IsFront, index);
                    other?.HideWire(post.m_IsTargetFront, post.m_TargetIndex);

                    m_SelectingIndex = index;
                    m_Context.SetSelectingPost(m_Own, m_IsFront, index);
                }
                else
                {
                    m_SelectingIndex = -1;
                    m_Context.ResetSelect();
                }
                OnClickSelect.Invoke(m_SelectingIndex >= 0);
            }
        }

        private void DrawDeleteButton(int index, PlateauSandboxElectricPost other)
        {
            if (new PlateauToolkitImageButtonGUI(
                    100,
                    20,
                    PlateauToolkitGUIStyles.k_ButtonPrimaryColor,
                    false)
                .Button("削除する"))
            {

                var post = m_Own.GetConnectedPost(m_IsFront, index);
                other?.RemoveConnection(post.m_IsTargetFront, post.m_TargetIndex);
                m_Own.RemoveConnection(m_IsFront, index);

                Reset();
            }
        }

        private void DrawAddButton()
        {
            if (new PlateauToolkitImageButtonGUI(
                    150,
                    20,
                    PlateauToolkitGUIStyles.k_ButtonPrimaryColor,
                    false)
                .Button("追加する"))
            {
                // ワイヤーを追加
                m_Own.AddConnectionAndWires(m_IsFront);
            }
        }

        public void Reset()
        {
            m_Context.ResetSelect();
            m_SelectingIndex = -1;
        }
    }

}