namespace PlateauToolkit.Sandbox.Runtime
{
    public enum PlateauSandboxBulkPlaceCategory
    {
        k_InValid = -1,
        k_Latitude = 0,
        k_Longitude,
        k_Height,
        k_AssetType,
    }

    static class PlateauSandboxBulkPlaceFileTypeExtensions
    {
        const string k_LongitudeTitle = "経度";
        const string k_LatitudeTitle = "緯度";
        const string k_HeightTitle = "高さ";
        const string k_AssetType = "アセット種別";

        public static bool IsMatch(this PlateauSandboxBulkPlaceCategory category, string target)
        {
            switch (category)
            {
                case PlateauSandboxBulkPlaceCategory.k_Longitude:
                    return target.Equals("Longitude") || target.Equals(k_LongitudeTitle);
                case PlateauSandboxBulkPlaceCategory.k_Latitude:
                    return target.Equals("Latitude") || target.Equals(k_LatitudeTitle);
                case PlateauSandboxBulkPlaceCategory.k_Height:
                    return target.Equals("Height") || target.Equals(k_HeightTitle);
                case PlateauSandboxBulkPlaceCategory.k_AssetType:
                    return target.Equals("AssetType") || target.Equals(k_AssetType);
                default:
                    return false;
            }
        }
        public static string Label(this PlateauSandboxBulkPlaceCategory category)
        {
            switch (category)
            {
                case PlateauSandboxBulkPlaceCategory.k_Longitude:
                    return k_LongitudeTitle;
                case PlateauSandboxBulkPlaceCategory.k_Latitude:
                    return k_LatitudeTitle;
                case PlateauSandboxBulkPlaceCategory.k_Height:
                    return k_HeightTitle;
                case PlateauSandboxBulkPlaceCategory.k_AssetType:
                    return k_AssetType;
                default:
                    return "";
            }
        }
    }
}