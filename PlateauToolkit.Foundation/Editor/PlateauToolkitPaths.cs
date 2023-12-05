namespace PlateauToolkit.Editor
{
    public static class PlateauToolkitPaths
    {
        const string k_SpritesFolder = "Packages/com.unity.plateautoolkit/PlateauToolkit.Foundation/Editor/Sprites";
        const string k_UIFolder = "Packages/com.unity.plateautoolkit/PlateauToolkit.Foundation/Editor/UI";
        public static string PlateauLogo { get; } = $"{k_SpritesFolder}/PlateauLogo.png";
        public static string SavePointOverlayUxml { get; } = $"{k_UIFolder}/SavePointOverlay.uxml";

    }
}
