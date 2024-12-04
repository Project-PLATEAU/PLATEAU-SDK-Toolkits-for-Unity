using PlateauToolkit.Editor;
using PlateauToolkit.Sandbox.Runtime;
using PlateauToolkit.Sandbox.Runtime.ElectricPost;
using System.Threading.Tasks;
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

        // 接続先が正面かどうか
        private const string k_IsDestinationFrontName = "Is Destination Front";

        private PlateauSandboxElectricPost m_Target;

        private bool m_IsFrontNodeSelecting;
        private bool m_IsBackNodeSelecting;

        private void OnEnable()
        {
            m_Target = target as PlateauSandboxElectricPost;

            m_Context = PlateauSandboxElectricPostContext.GetCurrent();
            m_Context.OnSelected.AddListener(ResetSelect);
        }

        public override void OnInspectorGUI()
        {
            if (m_Context == null || m_Target == null)
            {
                return;
            }

            serializedObject.Update();
            base.OnInspectorGUI();

            GUILayout.Space(5);

            var frontTarget = EditorGUILayout.ObjectField(
                                  k_FrontTargetNodeName,
                                  m_Target.FrontConnectedPost.target,
                                  typeof(PlateauSandboxElectricPost), true) as PlateauSandboxElectricPost;

            if (frontTarget != null && frontTarget != m_Target)
            {
                m_Target.SetFrontConnectPointToFacing(frontTarget);
            }

            CreateDestinationCheckbox(true);
            GUILayout.Space(5);

            // ボタン作成
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                CreateSelectButton(true);
                GUILayout.Space(5);
                CreateReleaseButton(true);
            }
            GUILayout.Space(10);

            // 後ろの電線の設定
            var backTarget = EditorGUILayout.ObjectField(
                                 k_BackTargetNodeName,
                                 m_Target.BackConnectedPost.target,
                                 typeof(PlateauSandboxElectricPost), true) as PlateauSandboxElectricPost;

            if (backTarget != null && backTarget != m_Target)
            {
                m_Target.SetBackConnectPointToFacing(backTarget);
            }

            CreateDestinationCheckbox(false);
            GUILayout.Space(5);

            // ボタン作成
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                CreateSelectButton(false);
                GUILayout.Space(5);
                CreateReleaseButton(false);
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
            m_Context.SetSelectingPost(null, false);

            m_IsBackNodeSelecting = false;
            m_IsFrontNodeSelecting = false;

            m_Context.OnCancel.Invoke();
        }

        private void SelectedPost(PlateauSandboxElectricPost targetPost)
        {
            ResetSelect();
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

        private void CreateSelectButton(bool isFront)
        {
            bool isSelecting = isFront ? m_IsFrontNodeSelecting : m_IsBackNodeSelecting;

            if (new PlateauToolkitImageButtonGUI(
                    100,
                    20,
                    isSelecting ? PlateauToolkitGUIStyles.k_ButtonCancelColor : PlateauToolkitGUIStyles.k_ButtonNormalColor,
                    false)
                .Button("選択する"))
            {
                // 選択時
                if (!isSelecting)
                {
                    // ワイヤーを外す
                    TryReleaseWire(isFront);
                    SetActiveTool();

                    // 選択中の状態にする
                    m_Context.SetSelectingPost(m_Target, isFront);

                    if (isFront)
                    {
                        m_IsFrontNodeSelecting = true;
                    }
                    else
                    {
                        m_IsBackNodeSelecting = true;
                    }
                }
                else
                {
                    ResetSelect();
                }
            }
        }

        private void CreateReleaseButton(bool isFront)
        {
            bool isConnected = isFront ? m_Target.FrontConnectedPost.target != null : m_Target.BackConnectedPost.target != null;

            if (new PlateauToolkitImageButtonGUI(
                    100,
                    20,
                    isConnected ? PlateauToolkitGUIStyles.k_ButtonPrimaryColor : PlateauToolkitGUIStyles.k_ButtonDisableColor,
                    false)
                .Button("解除する"))
            {
                if (!isConnected)
                {
                    return;
                }
                TryReleaseWire(isFront);
                ResetSelect();
            }
        }

        private void TryReleaseWire(bool isFront)
        {
            if (isFront)
            {
                if (m_Target.FrontConnectedPost.target != null)
                {
                    m_Target.FrontConnectedPost.target.RemoveConnectedPost(m_Target);
                    m_Target.RemoveConnectedPost(m_Target.FrontConnectedPost.target);
                }
            }
            else
            {
                if (m_Target.BackConnectedPost.target != null)
                {
                    m_Target.BackConnectedPost.target.RemoveConnectedPost(m_Target);
                    m_Target.RemoveConnectedPost(m_Target.BackConnectedPost.target);
                }
            }
        }

        private void CreateDestinationCheckbox(bool isFront)
        {
            bool isFrontActive = false;
            if (isFront)
            {
                if (m_Target.FrontConnectedPost.target == null)
                {
                    return;
                }
                isFrontActive = m_Target.FrontConnectedPost.isFront;
            }
            else
            {
                if (m_Target.BackConnectedPost.target == null)
                {
                    return;
                }
                isFrontActive = m_Target.BackConnectedPost.isFront;
            }

            GUI.enabled = false;
            EditorGUILayout.Toggle(k_IsDestinationFrontName, isFrontActive);
            GUI.enabled = true;
        }
    }
}