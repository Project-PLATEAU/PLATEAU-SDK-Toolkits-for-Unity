using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

namespace PlateauToolkit.Editor
{
    class PlateauToolbarSaveSceneButton : Button
    {

        public const string k_Id = "PlateauToolbar/SaveSceneButton";

        public PlateauToolbarSaveSceneButton()
        {
            text = "シーンを保存";
            tooltip = "現在のシーンを保存します。";
            clicked += OnClick;
        }

        void OnClick()
        {
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }
    }

    class PlateauToolbarRestoreSceneButton : Button
    {

        public const string k_Id = "PlateauToolbar/RestoreSceneButton";

        public PlateauToolbarRestoreSceneButton()
        {
            text = "シーンを復元";
            tooltip = "現在のシーンを最後の保存状態に戻します。";
            style.backgroundColor = new Color(125 / 255f, 42 / 255f, 9 / 255f);
            clicked += OnClick;
        }

        void OnClick()
        {
            if (EditorUtility.DisplayDialog(
                "シーンを復元",
                "本当にシーンを復元しますか？全ての変更は破棄されます。",
                "復元する", "キャンセル"))
            {
                EditorSceneManager.OpenScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().path);
            }
        }
    }

    [Overlay(typeof(SceneView), "plateau-toolkit-foundation-overlay", "PLATEAU Toolkit", true)]
    public class PlateauToolkitOverlay : Overlay, ITransientOverlay
    {
        public bool visible => true;

        public override VisualElement CreatePanelContent()
        {
            var root = new VisualElement() { name = "PLATEAU Toolkit toolbar" };
            root.style.flexDirection = FlexDirection.Row;
            root.style.alignContent = Align.FlexStart;
            root.style.justifyContent = Justify.Center;

            root.Add(new PlateauToolbarSaveSceneButton());
            root.Add(new PlateauToolbarRestoreSceneButton());
            return root;
        }
    }
}