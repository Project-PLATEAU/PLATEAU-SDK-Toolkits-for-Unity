using UnityEditor;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Editor
{
    [CustomEditor(typeof(PlateauSandboxTrackMovement))]
    [CanEditMultipleObjects]
    public class PlateauSandboxTrackMovementInspector : UnityEditor.Editor
    {
        static readonly string k_EditingMultipleMovementsMessage =
            $"複数の{nameof(PlateauSandboxTrackMovement)}コンポーネントの位置を同時に変更することはできません。";

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (serializedObject.isEditingMultipleObjects)
            {
                EditorGUILayout.HelpBox(k_EditingMultipleMovementsMessage, MessageType.Warning);
                return;
            }

            var trackMovement = (PlateauSandboxTrackMovement)target;
            if (trackMovement.HasTrack)
            {
                EditorGUILayout.PrefixLabel("トラック上の位置");

                using (new EditorGUI.DisabledScope(trackMovement.IsMoving))
                {
                    EditorGUI.BeginChangeCheck();
                    SerializedProperty splineTProperty = serializedObject.FindProperty("m_SplineContainerT");
                    splineTProperty.floatValue = EditorGUILayout.Slider(splineTProperty.floatValue, 0, trackMovement.MaxSplineContainerT);

                    serializedObject.ApplyModifiedProperties();

                    if (EditorGUI.EndChangeCheck())
                    {
                        trackMovement.ApplyPosition();
                    }
                }
            }

            if (!Application.isPlaying)
            {
                return;
            }

            EditorGUILayout.LabelField("現在の速度", $"{trackMovement.CurrentVelocity:0.00} m/s");

            if (trackMovement.IsMoving)
            {
                if (GUILayout.Button("移動を終了"))
                {
                    trackMovement.Stop();
                }

                if (trackMovement.IsPaused)
                {
                    if (GUILayout.Button("移動を再開"))
                    {
                        trackMovement.IsPaused = false;
                    }
                }
                else
                {
                    if (GUILayout.Button("移動を一時停止"))
                    {
                        trackMovement.IsPaused = true;
                    }
                }
            }
            else
            {
                if (GUILayout.Button("移動を開始"))
                {
                    trackMovement.StartRandomWalk();
                }
            }
        }
    }
}
