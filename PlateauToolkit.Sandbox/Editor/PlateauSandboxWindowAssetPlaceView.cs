using PlateauToolkit.Editor;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEditor;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Editor
{
    class PlateauSandboxWindowAssetPlaceView : IPlateauSandboxWindowView
    {
        public string Name => "アセット配置";

        PlateauSandboxAssetContainerView m_AssetContainerView = new PlateauSandboxAssetContainerView();

        public virtual void OnBegin(PlateauSandboxContext context, EditorWindow editorWindow)
        {
            m_AssetContainerView.OnBegin(context, editorWindow);
        }

        public virtual void OnGUI(PlateauSandboxContext context, EditorWindow window)
        {
            PlateauToolkitEditorGUILayout.Header("ツール");
            PlateauSandboxGUI.PlacementToolButton(context);

            m_AssetContainerView.OnGUI(context, window);
        }

        public virtual void OnUpdate(EditorWindow editorWindow)
        {
            m_AssetContainerView.OnUpdate(editorWindow);
        }

        public virtual void OnEnd(PlateauSandboxContext context)
        {
            m_AssetContainerView.OnEnd(context);
        }
    }
}