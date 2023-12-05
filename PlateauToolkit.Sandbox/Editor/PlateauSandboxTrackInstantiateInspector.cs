using PlateauToolkit.Editor;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;
using Object = UnityEngine.Object;

namespace PlateauToolkit.Sandbox.Editor
{
    [CustomEditor(typeof(PlateauSandboxTrackInstantiate))]
    public class PlateauSandboxTrackInstantiateInspector : UnityEditor.Editor
    {
        static readonly GUILayoutOption k_ItemListThumbnailWidth = GUILayout.Width(55);
        static readonly GUILayoutOption k_ItemListThumbnailHeight = GUILayout.Height(55);
        static readonly GUILayoutOption k_ItemListDeleteButtonWidth = GUILayout.Width(20);
        static readonly GUILayoutOption k_ItemListDeleteButtonHeight = GUILayout.Height(20);
        static readonly Color k_DragAndDropHoverColor = new(0.2f, 0.7f, 1f, 0.4f);

        PlateauSandboxTrackInstantiate m_TrackInstantiate;
        SplineInstantiate m_SplineInstantiate;
        SerializedObject m_SplineInstantiateSerializedObject;
        SerializedProperty m_ItemsProperty;
        SerializedProperty m_AutoRefreshProperty;
        SerializedProperty m_SeedProperty;
        UnityEditor.Editor m_SplineInstantiateEditor;

        bool m_SplineInstantiateFolded = true;
        bool m_IsDragging;

        void OnEnable()
        {
            m_TrackInstantiate = (PlateauSandboxTrackInstantiate)target;
            m_SplineInstantiate = m_TrackInstantiate.SplineInstantiate;
            m_SplineInstantiateEditor = CreateEditor(m_SplineInstantiate);

            m_SplineInstantiateSerializedObject = new SerializedObject(m_SplineInstantiate);
            m_ItemsProperty = m_SplineInstantiateSerializedObject.FindProperty("m_ItemsToInstantiate");
            m_AutoRefreshProperty = m_SplineInstantiateSerializedObject.FindProperty("m_AutoRefresh");
            m_SeedProperty = m_SplineInstantiateSerializedObject.FindProperty("m_Seed");

        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            m_SplineInstantiateSerializedObject.Update();

            PlateauSandboxTrack track;
            if (m_SplineInstantiate.Container == null)
            {
                track = null;
            }
            else if (!m_SplineInstantiate.Container.TryGetComponent(out track))
            {
                EditorGUILayout.HelpBox("設定されているスプラインはトラックではありません", MessageType.Warning);
            }
            track = (PlateauSandboxTrack)EditorGUILayout.ObjectField("トラック", track, typeof(PlateauSandboxTrack), true);
            m_SplineInstantiate.Container = track.SplineContainer;

            EditorGUILayout.LabelField("生成アイテムリスト", EditorStyles.boldLabel);

            bool dirtyInstance = false;
            using (var itemsScope = new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                if (m_ItemsProperty.arraySize == 0)
                {
                    EditorGUILayout.HelpBox("ここにドラッグアンドドロップしてトラックに沿って生成するアイテムを設定", MessageType.Warning);
                }
                else
                {
                    EditorGUI.BeginChangeCheck();

                    int removedIndex = -1;
                    for (int i = 0; i < m_ItemsProperty.arraySize; i++)
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUILayout.Space(1);

                            SerializedProperty itemProperty = m_ItemsProperty.GetArrayElementAtIndex(i);

                            IEnumerator fieldEnumerator = itemProperty.GetEnumerator();
                            fieldEnumerator.MoveNext();
                            SerializedProperty prefabProperty = ((SerializedProperty)fieldEnumerator.Current)?.Copy();
                            Debug.Assert(prefabProperty != null);
                            fieldEnumerator.MoveNext();
                            SerializedProperty probabilityProperty = ((SerializedProperty)fieldEnumerator.Current)?.Copy();
                            Debug.Assert(probabilityProperty != null);

                            Rect previewRect = EditorGUILayout.GetControlRect(k_ItemListThumbnailWidth, k_ItemListThumbnailHeight);
                            Texture2D previewTexture = AssetPreview.GetAssetPreview(prefabProperty.objectReferenceValue);

                            if (previewTexture != null)
                            {
                                GUI.DrawTexture(previewRect, previewTexture, ScaleMode.ScaleToFit);
                            }

                            EditorGUILayout.Space(5);

                            using (new EditorGUILayout.VerticalScope())
                            {
                                using (new EditorGUILayout.HorizontalScope())
                                {

                                    // Match the height to the delete button
                                    EditorGUILayout.ObjectField(prefabProperty, GUIContent.none, k_ItemListDeleteButtonHeight);

                                    using (PlateauToolkitEditorGUILayout.BackgroundColorScope(Color.red))
                                    {
                                        if (GUILayout.Button("✖", k_ItemListDeleteButtonWidth, k_ItemListDeleteButtonHeight))
                                        {
                                            // DO NOT remove the element of the items here.
                                            // The property is used in the following section, so removing the element
                                            // causes the exception that property has been disposed.
                                            // Also, removing the element while iterating is dangerous.
                                            removedIndex = i;
                                        }
                                    }
                                }

                                GUILayout.FlexibleSpace();

                                using (new EditorGUILayout.HorizontalScope())
                                {
                                    EditorGUILayout.LabelField("生成割合", GUILayout.Width(60));
                                    EditorGUILayout.Slider(probabilityProperty, 0, 100, GUIContent.none);
                                    EditorGUILayout.LabelField("%", GUILayout.Width(20));
                                }

                                GUILayout.FlexibleSpace();
                            }
                        }

                        EditorGUILayout.Space(8);
                        EditorGUILayout.HelpBox("ここにドラッグアンドドロップしてアイテムを追加", MessageType.Info);
                        EditorGUILayout.Space(4);
                    }

                    // Apply the removed item.
                    if (removedIndex != -1)
                    {
                        m_ItemsProperty.DeleteArrayElementAtIndex(removedIndex);
                    }

                    dirtyInstance |= EditorGUI.EndChangeCheck();
                }

                if (m_IsDragging)
                {
                    PlateauSandboxGUI.DrawColorTexture(itemsScope.rect, k_DragAndDropHoverColor, 2);
                }

                if (Event.current.type != EventType.Repaint && Event.current.type != EventType.Layout)
                {
                    m_IsDragging = false;
                }

                if (itemsScope.rect.Contains(Event.current.mousePosition))
                {
                    if (Event.current.type == EventType.DragUpdated)
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                        m_IsDragging = true;
                    }
                    else if (Event.current.type == EventType.DragPerform)
                    {
                        // Adding a undo history for the operation of adding items.
                        Undo.RegisterCompleteObjectUndo(m_TrackInstantiate.gameObject, "Add TrackInstantiate items");
                        try
                        {
                            foreach (Object dragObject in DragAndDrop.objectReferences)
                            {
                                if (dragObject is not GameObject dragGameObject)
                                {
                                    return;
                                }

                                int index = m_ItemsProperty.arraySize;
                                m_ItemsProperty.InsertArrayElementAtIndex(index);
                                SerializedProperty itemSerializedProperty = m_ItemsProperty.GetArrayElementAtIndex(index);
                                IEnumerator fieldEnumerator = itemSerializedProperty.GetEnumerator();
                                fieldEnumerator.MoveNext();
                                SerializedProperty prefabProperty = ((SerializedProperty)fieldEnumerator.Current)?.Copy();
                                Debug.Assert(prefabProperty != null);

                                prefabProperty.objectReferenceValue = dragGameObject;
                                dirtyInstance |= true;
                            }

                            DragAndDrop.activeControlID = 0;
                            DragAndDrop.AcceptDrag();
                            Event.current.Use();
                        }
                        catch
                        {
                            // If some error occured, remove the undo history.
                            Undo.RevertAllInCurrentGroup();
                        }

                        Repaint();
                    }
                }
            }

            EditorGUILayout.LabelField("生成ツール", EditorStyles.boldLabel);

            bool updateInstances = false;

            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(m_AutoRefreshProperty, new GUIContent("自動生成", "設定が変更されたときに自動で再生成を実行します"));
                EditorGUILayout.PropertyField(m_SeedProperty, new GUIContent("生成シード値", "オブジェクトを生成するランダムシード値"));
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            if (GUILayout.Button(new GUIContent("ランダム生成", "ランダムにシードを選択し、再生成を実行します"), GUILayout.MaxWidth(100f)))
            {
                Undo.RecordObjects(targets, "Changing SplineInstantiate seed");
                m_SplineInstantiate.Randomize();
                updateInstances |= true;
            }

            if (GUILayout.Button(new GUIContent("生成", "設定されているシード値でオブジェクトを生成します"), GUILayout.MaxWidth(100f)))
            {
                updateInstances |= true;
            }

            if (GUILayout.Button(new GUIContent("リセット", "生成されているオブジェクトをクリアします"), GUILayout.MaxWidth(100f)))
            {
                m_SplineInstantiate.Clear();
            }

            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(8);
            PlateauToolkitEditorGUILayout.BorderLine();
            EditorGUILayout.Space(8);

            m_SplineInstantiateFolded = EditorGUILayout.Foldout(m_SplineInstantiateFolded, "詳細設定");
            if (m_SplineInstantiateFolded)
            {
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    m_SplineInstantiateEditor.OnInspectorGUI();
                }
            }

            if (dirtyInstance)
            {
                m_SplineInstantiate.SetDirty();
                SceneView.RepaintAll();
            }

            if (updateInstances)
            {
                m_SplineInstantiate.UpdateInstances();
                SceneView.RepaintAll();
            }

            if (m_SplineInstantiateSerializedObject.ApplyModifiedProperties())
            {
                Repaint();
            }
        }
    }
}