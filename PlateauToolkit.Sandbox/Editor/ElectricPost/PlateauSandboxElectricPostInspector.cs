using PlateauToolkit.Editor;
using PlateauToolkit.Sandbox.Runtime;
using PlateauToolkit.Sandbox.Runtime.ElectricPost;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Editor
{
    [CustomEditor(typeof(PlateauSandboxElectricPost))]
    public class PlateauSandboxElectricPostInspector : UnityEditor.Editor
    {
        private PlateauSandboxElectricPostContext m_Context;

        private const string k_FrontTargetNodeName = "Front Connected Target Node";
        private const string k_BackTargetNodeName = "Back Connected Target Node";

        // 選択中キャンセル用にキャッシュした電柱
        private PlateauSandboxElectricPost m_CachedConnectedPost;

        private void OnEnable()
        {
            m_Context = PlateauSandboxElectricPostContext.GetCurrent();
            m_Context.OnSelected.AddListener(ResetSelect);
        }

        public override void OnInspectorGUI()
        {
            if (m_Context == null || m_Context.TargetPost == null)
            {
                return;
            }

            serializedObject.Update();
            base.OnInspectorGUI();

            GUILayout.Space(5);

            var frontTarget = EditorGUILayout.ObjectField(
                 k_FrontTargetNodeName,
                 m_Context.TargetPost.FrontConnectedPost,
                 typeof(PlateauSandboxElectricPost), true) as PlateauSandboxElectricPost;

            if (frontTarget != null && m_Context.TargetPost.FrontConnectedPost == null)
            {
                m_Context.SetConnect(true, frontTarget);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (new PlateauToolkitImageButtonGUI(
                        100,
                        20,
                        m_Context.IsFrontNodeSelecting ? PlateauToolkitGUIStyles.k_ButtonCancelColor : PlateauToolkitGUIStyles.k_ButtonNormalColor,
                        false)
                    .Button("選択する"))
                {
                    if (!m_Context.IsFrontNodeSelecting)
                    {
                        if (m_Context.TargetPost.FrontConnectedPost != null)
                        {
                            // 選択時にキャッシュする
                            m_CachedConnectedPost = m_Context.TargetPost.FrontConnectedPost;
                        }
                        m_Context.SetConnect(true, null);
                        m_Context.SetSelect(true, !m_Context.IsFrontNodeSelecting);
                        SetActiveTool();
                    }
                    else
                    {
                        SetConnectFromCache();
                        ResetSelect();
                    }
                }
            }

            GUILayout.Space(10);

            var backTarget = EditorGUILayout.ObjectField(
                k_BackTargetNodeName,
                  m_Context.TargetPost.BackConnectedPost,
                  typeof(PlateauSandboxElectricPost), true) as PlateauSandboxElectricPost;

            if (backTarget != null && m_Context.TargetPost.BackConnectedPost == null)
            {
                m_Context.SetConnect(false, backTarget);
            }

            GUILayout.Space(5);

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (new PlateauToolkitImageButtonGUI(
                        100,
                        20,
                        m_Context.IsBackNodeSelecting ? PlateauToolkitGUIStyles.k_ButtonCancelColor : PlateauToolkitGUIStyles.k_ButtonNormalColor,
                        false)
                    .Button("選択する"))
                {
                    if (!m_Context.IsBackNodeSelecting)
                    {
                        if (m_Context.TargetPost.BackConnectedPost != null)
                        {
                            // 選択時にキャッシュする
                            m_CachedConnectedPost = m_Context.TargetPost.BackConnectedPost;
                        }
                        m_Context.SetConnect( false, null);
                        m_Context.SetSelect(false, !m_Context.IsBackNodeSelecting);

                        SetActiveTool();
                    }
                    else
                    {
                        SetConnectFromCache();
                        ResetSelect();
                    }
                }
            }

            GUILayout.Space(10);

            if (Event.current.keyCode == KeyCode.Escape)
            {
                ResetSelect();
            }
        }

        private void ResetSelect()
        {
            GUIUtility.hotControl = 0;
            Event.current.Use();

            ToolManager.RestorePreviousPersistentTool();

            m_Context.SetSelect(true, false);
            m_Context.SetSelect(false, false);
        }

        private void SetConnectFromCache()
        {
            if (m_CachedConnectedPost != null)
            {
                // 解除時にキャッシュされた電柱を再設定
                if (m_Context.IsFrontNodeSelecting)
                {
                    m_Context.SetConnect( true, m_CachedConnectedPost);
                }
                else if (m_Context.IsBackNodeSelecting)
                {
                    m_Context.SetConnect(false, m_CachedConnectedPost);
                }
            }
        }

        private void SetActiveTool()
        {
            if (ToolManager.activeToolType != typeof(PlateauSandboxElectricPostSelectTool))
            {
                ToolManager.SetActiveTool<PlateauSandboxElectricPostSelectTool>();
            }
            else
            {
                ToolManager.RestorePreviousPersistentTool();
            }
        }
    }
}