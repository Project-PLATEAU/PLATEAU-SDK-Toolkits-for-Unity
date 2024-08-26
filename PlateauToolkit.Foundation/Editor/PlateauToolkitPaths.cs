namespace PlateauToolkit.Editor
{
    public static class PlateauToolkitPaths
    {
        const string k_SpritesFolder = "Packages/com.synesthesias.plateautoolkit/PlateauToolkit.Foundation/Editor/Sprites";
        const string k_UIFolder = "Packages/com.synesthesias.plateautoolkit/PlateauToolkit.Foundation/Editor/UI";
        public static string PlateauLogo { get; } = $"{k_SpritesFolder}/PlateauLogo.png";
        public static string PlateauTitleBackground { get; } = $"{k_SpritesFolder}/PlateauTitleBackground.png";
        public static string SavePointOverlayUxml { get; } = $"{k_UIFolder}/SavePointOverlay.uxml";

    }
}
