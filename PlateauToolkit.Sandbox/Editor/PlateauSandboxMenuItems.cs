using UnityEditor;

namespace PlateauToolkit.Sandbox.Editor
{
    static class PlateauSandboxMenuItems
    {
        [MenuItem("PLATEAU/PLATEAU Toolkit/Sandbox Toolkit", priority = 0)]
        static void ShowSandboxWindow()
        {
            EditorWindow.GetWindow(typeof(PlateauSandboxWindow), false, "PLATEAU Sandbox Toolkit");
        }

        [MenuItem("PLATEAU/PLATEAU Toolkit/新しいSandboxアセットを作成...", priority = 20)]
        static void ShowPrefabCreationWizard()
        {
            PlateauSandboxPrefabCreationWizard.DisplayWizard();
        }
    }
}