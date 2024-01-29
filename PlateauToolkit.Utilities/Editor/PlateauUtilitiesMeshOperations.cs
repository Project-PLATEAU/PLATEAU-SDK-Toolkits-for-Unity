using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace PlateauToolkit.Utilities.Editor
{
    static class PlateauUtilitiesMeshOperations
    {
        public static void SelectMeshRenderersByPrefix(string prefix = "")
        {
            GameObject[] selectedObjects = Selection.gameObjects;
            if (selectedObjects == null || selectedObjects.Length == 0)
            {
                EditorUtility.DisplayDialog("操作ガイド", "最低でも一つ以上のゲームオブジェクトをヒエラルキーから選択する必要があります。", "閉じる");
                return;
            }

            var visibleMeshRenderers = new List<GameObject>();
            foreach (GameObject obj in selectedObjects)
            {
                if (obj.activeInHierarchy) // GameObjectがアクティブな場合のみ処理
                {
                    CollectVisibleMeshRenderers(obj.transform, visibleMeshRenderers, prefix);
                }
            }

            Selection.objects = visibleMeshRenderers.ToArray();
        }

        static void CollectVisibleMeshRenderers(Transform parent, List<GameObject> visibleMeshRenderers, string prefix)
        {
            foreach (Transform child in parent)
            {
                if (child.gameObject.activeInHierarchy) // 子GameObjectがアクティブな場合のみ処理
                {
                    MeshRenderer meshRenderer = child.GetComponent<MeshRenderer>();
                    bool isNameMatch = string.IsNullOrEmpty(prefix) || child.name.StartsWith(prefix);
                    if (meshRenderer != null && meshRenderer.enabled && isNameMatch)
                    {
                        visibleMeshRenderers.Add(child.gameObject);
                    }

                    CollectVisibleMeshRenderers(child, visibleMeshRenderers, prefix);
                }
            }
        }

        public static void AlignMeshBottomsToBaseHeight(float baseHeight)
        {
            GameObject[] selectedObjects = Selection.gameObjects;
            if (selectedObjects == null || selectedObjects.Length == 0)
            {
                EditorUtility.DisplayDialog("操作ガイド", "このツールを実行するためには一つ以上のメッシュを選択して実行する必要があります。", "閉じる");
                return;
            }

            // 有効な高さの値が与えられているかどうかを確認
            if (float.IsNaN(baseHeight))
            {
                EditorUtility.DisplayDialog("操作ガイド", "有効な高さの値を入力してください。", "閉じる");
                return;
            }

            Undo.SetCurrentGroupName("Align Mesh Bottoms to Base Height");
            int undoGroup = Undo.GetCurrentGroup();

            foreach (GameObject obj in selectedObjects)
            {
                MeshRenderer meshRenderer = obj.GetComponent<MeshRenderer>();
                if (!meshRenderer)
                {
                    continue;
                }

                AlignBottomToBaseHeight(obj, baseHeight);
            }

            Undo.CollapseUndoOperations(undoGroup);
        }

        public static void AlignBottomToBaseHeight(GameObject obj, float baseHeight)
        {
            MeshRenderer meshRenderer = obj.GetComponent<MeshRenderer>();
            if (meshRenderer)
            {
                Bounds bounds = meshRenderer.bounds;
                float yOffset = baseHeight - bounds.min.y;

                Undo.RecordObject(obj.transform, "Align Bottom to Base Height");

                Vector3 position = obj.transform.position;
                position.y += yOffset;
                obj.transform.position = position;
            }
        }

        public static void FlattenSelectedMeshVertices(float targetHeight)
        {
            // 選択されたオブジェクトがあるかどうかを確認
            if (Selection.gameObjects.Length == 0)
            {
                EditorUtility.DisplayDialog("操作ガイド", "このツールを実行するためには一つ以上のメッシュを選択して実行する必要があります。", "閉じる");
                return;
            }

            // 有効な高さの値が与えられているかどうかを確認
            if (float.IsNaN(targetHeight))
            {
                EditorUtility.DisplayDialog("操作ガイド", "有効な高さの値を入力してください。", "閉じる");
                return;
            }

            foreach (GameObject selectedObject in Selection.gameObjects)
            {
                MeshFilter meshFilter = selectedObject.GetComponent<MeshFilter>();
                if (meshFilter)
                {
                    Mesh newMesh = Object.Instantiate(meshFilter.sharedMesh);
                    Vector3[] vertices = newMesh.vertices;

                    for (int i = 0; i < vertices.Length; i++)
                    {
                        Vector3 flattenedPosition = selectedObject.transform.TransformPoint(vertices[i]);
                        flattenedPosition.y = targetHeight;
                        vertices[i] = selectedObject.transform.InverseTransformPoint(flattenedPosition);
                    }

                    newMesh.vertices = vertices;
                    newMesh.RecalculateBounds();
                    newMesh.RecalculateNormals();

                    Undo.RecordObject(meshFilter, "Flatten Mesh Vertices");
                    meshFilter.sharedMesh = newMesh;

                    MeshCollider meshCollider = selectedObject.GetComponent<MeshCollider>();
                    if (meshCollider)
                    {
                        Undo.RecordObject(meshCollider, "Update Mesh Collider");
                        meshCollider.sharedMesh = newMesh;
                    }
                }
            }
        }
    }
}

