using System.Threading;
using UnityEditor;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Editor
{
    abstract class PlateauSandboxWindowAssetsViewBase<TAsset> : IPlateauSandboxWindowView
        where TAsset : Component
    {
        SandboxAssetListState<TAsset> m_AssetListState;
        CancellationTokenSource m_Cancellation;
        bool m_IsReadyApplied;

        public abstract string Name { get; }

        public virtual void OnBegin(PlateauSandboxContext context, EditorWindow editorWindow)
        {
            m_AssetListState = new SandboxAssetListState<TAsset>();
            m_Cancellation = new CancellationTokenSource();

            _ = m_AssetListState.PrepareAsync(m_Cancellation.Token);
        }

        public virtual void OnGUI(PlateauSandboxContext context, EditorWindow window)
        {
            EditorGUILayout.LabelField("ツール", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                PlateauSandboxGUI.PlacementToolButton(context);
            }

            if (m_AssetListState.IsReady)
            {
                PlateauSandboxAssetListGUI.OnGUI(window.position.width, context, m_AssetListState);
            }
            else
            {
                EditorGUILayout.HelpBox("アセットを読み込んでいます...", MessageType.Info);
            }
        }

        public virtual void OnUpdate(EditorWindow editorWindow)
        {
            if (m_AssetListState.IsReady && !m_IsReadyApplied)
            {
                editorWindow.Repaint();
                m_IsReadyApplied = true;
            }
        }

        public virtual void OnEnd(PlateauSandboxContext context)
        {
            m_IsReadyApplied = false;

            m_Cancellation.Cancel();
            m_Cancellation.Dispose();
            m_Cancellation = null;

            m_AssetListState.Dispose();
            m_AssetListState = null;
        }
    }
}