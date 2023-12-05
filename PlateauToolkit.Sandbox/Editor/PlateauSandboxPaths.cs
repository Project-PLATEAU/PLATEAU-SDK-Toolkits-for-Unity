namespace PlateauToolkit.Sandbox.Editor
{
    static class PlateauSandboxPaths
    {
        const string k_SpritesFolder = "Packages/com.unity.plateautoolkit/PlateauToolkit.Sandbox/Editor/Sprites";
        const string k_UIFolder = "Packages/com.unity.plateautoolkit/PlateauToolkit.Sandbox/Editor/UI";

        public static string VehicleIcon { get; } = $"{k_SpritesFolder}/PlateauToolkitSandbox_VehicleIcon.png";
        public static string TracksIcon { get; } = $"{k_SpritesFolder}/PlateauToolkitSandbox_TrackIcon.png";
        public static string HumanIcon { get; } = $"{k_SpritesFolder}/PlateauToolkitSandbox_HumanIcon.png";
        public static string PropsIcon { get; } = $"{k_SpritesFolder}/PlateauToolkitSandbox_PropsIcon.png";

        public static string PlacementToolOverlayUxml { get; } = $"{k_UIFolder}/PlacementToolOverlay.uxml";
    }
}