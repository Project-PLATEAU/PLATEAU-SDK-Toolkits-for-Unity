using PlateauToolkit.Editor;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Editor
{
    [EditorTool("PLATEAU トラック速度制限ツール", null, typeof(GameObjectToolContext))]
    sealed class PlateauSandboxSpeedLimitTool : EditorTool
    {
        [SerializeField] Texture2D m_ToolIcon;

        public override GUIContent toolbarIcon => EditorGUIUtility.TrIconContent(m_ToolIcon, "PLATEAU トラック速度制限ツール");

        public override void OnActivated()
        {
            base.OnActivated();

            ToolManager.SetActiveContext<GameObjectToolContext>();
        }

        public override void OnToolGUI(EditorWindow window)
        {
            var context = PlateauSandboxContext.GetCurrent();

            if (context.Tracks.Count == 0)
            {
                ToolManager.RestorePreviousPersistentTool();
            }

            foreach (PlateauSandboxTrack track in context.Tracks)
            {
                Vector2 guiPoint = HandleUtility.WorldToGUIPoint(track.GetStartPosition());

                var trackSerializedObject = new SerializedObject(track);
                SerializedProperty speedLimitProperty = trackSerializedObject.FindProperty("m_SpeedLimit");
                SerializedProperty hasSpeedLimitProperty = trackSerializedObject.FindProperty("m_HasSpeedLimit");

                Handles.BeginGUI();
                var areaSize = new Vector2(70, 300);
                GUILayout.BeginArea(new Rect(guiPoint - new Vector2(areaSize.x / 2, 0), areaSize));
                {
                    // Display a colored label with a background
                    Rect rect = EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true), GUILayout.Height(20));
                    EditorGUI.DrawRect(rect, new Color(0.2f, 0.2f, 0.2f, 1));
                    GUI.Label(rect, "速度制限");

                    if (hasSpeedLimitProperty.boolValue)
                    {
                        EditorGUILayout.PropertyField(speedLimitProperty, GUIContent.none);
                        using (PlateauToolkitEditorGUILayout.BackgroundColorScope(Color.red))
                        {
                            if (GUILayout.Button("削除"))
                            {
                                hasSpeedLimitProperty.boolValue = false;
                            }
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("設定"))
                        {
                            hasSpeedLimitProperty.boolValue = true;
                        }
                    }

                    trackSerializedObject.ApplyModifiedProperties();
                }
                GUILayout.EndArea();
                Handles.EndGUI();
            }
        }
    }
}