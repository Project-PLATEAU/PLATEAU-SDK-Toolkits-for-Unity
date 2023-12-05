using UnityEditor;

namespace PlateauToolkit.Rendering.Editor
{
    static class PlateauRenderingWindowMenuItems
    {
        [MenuItem("PLATEAU/PLATEAU Toolkit/Rendering Toolkit", priority = 0)]
        static void ShowRenderingWindow()
        {
            EditorWindow.GetWindow(typeof(PlateauToolkitRenderingWindow), false, "PLATEAU Rendering Toolkit");
        }
    }
}