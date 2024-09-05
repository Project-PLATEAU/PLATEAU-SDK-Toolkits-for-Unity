namespace PlateauToolkit.Sandbox.Editor
{
    static class PlateauSandboxPaths
    {
        const string k_SpritesFolder = "Packages/com.synesthesias.plateau-unity-toolkit/PlateauToolkit.Sandbox/Editor/Sprites";
        const string k_UIFolder = "Packages/com.synesthesias.plateau-unity-toolkit/PlateauToolkit.Sandbox/Editor/UI";
        const string k_AssetIconsFolder = "Packages/com.synesthesias.plateau-unity-toolkit/PlateauToolkit.Sandbox/Editor/Sprites/AssetIcons";

        public static string TracksIcon { get; } = $"{k_SpritesFolder}/PlateauToolkitSandbox_TrackIcon.png";
        public static string PlaceIcon { get; } = $"{k_SpritesFolder}/PlateauToolkitSandbox_PlaceIcon.png";
        public static string BulkPlaceIcon { get; } = $"{k_SpritesFolder}/PlateauToolkitSandbox_BulkPlaceIcon.png";

        public static string AdvertisementIcon { get; } = $"{k_AssetIconsFolder}/PlateauToolkitSandbox_AdvertisementIcon.png";
        public static string HumanIcon { get; } = $"{k_AssetIconsFolder}/PlateauToolkitSandbox_HumanIcon.png";
        public static string BuildingIcon { get; } = $"{k_AssetIconsFolder}/PlateauToolkitSandbox_BuildingIcon.png";
        public static string MiscellaneousIcon { get; } = $"{k_AssetIconsFolder}/PlateauToolkitSandbox_MiscellaneousIcon.png";
        public static string PlantIcon { get; } = $"{k_AssetIconsFolder}/PlateauToolkitSandbox_PlantIcon.png";
        public static string SignIcon { get; } = $"{k_AssetIconsFolder}/PlateauToolkitSandbox_SignIcon.png";
        public static string StreetFurnitureIcon { get; } = $"{k_AssetIconsFolder}/PlateauToolkitSandbox_StreetFurnitureIcon.png";
        public static string VehicleIcon { get; } = $"{k_AssetIconsFolder}/PlateauToolkitSandbox_VehicleIcon.png";

        public static string PlacementToolOverlayUxml { get; } = $"{k_UIFolder}/PlacementToolOverlay.uxml";
    }
}