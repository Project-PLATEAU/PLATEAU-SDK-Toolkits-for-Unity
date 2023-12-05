using UnityEditor;

namespace PlateauToolkit.Sandbox.Editor
{
    class PlateauSandboxWindowVehicleView : IPlateauSandboxWindowView
    {
        readonly PlateauSandboxAssetListState<PlateauSandboxVehicle> m_AssetListState = new();

        public string Name => "車両";

        public void OnGUI(PlateauSandboxContext context, EditorWindow window)
        {
            EditorGUILayout.LabelField("ツール", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                PlateauSandboxGUI.PlacementToolButton(context);
            }

            PlateauSandboxAssetListGUI.OnGUI(window.position.width, context, m_AssetListState);
        }
    }
}