using UnityEditor;

namespace PlateauToolkit.Utilities.Editor
{
    static class PlateauRenderingWindowMenuItems
    {
        [MenuItem("PLATEAU/PLATEAU Toolkit/Utilities", priority = 4)]
        static void ShowRenderingWindow()
        {
            EditorWindow.GetWindow(typeof(PlateauUtilitiesWindow), false, "PLATEAU Utilities");
        }
    }
}