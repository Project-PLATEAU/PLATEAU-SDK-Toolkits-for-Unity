namespace PlateauToolkit.Sandbox.Editor
{
    public abstract class PlateauSandboxFileDataBase
    {
        public int Id { get; set; }
    }

    public class PlateauSandboxBulkPlaceData : PlateauSandboxFileDataBase
    {
        public static readonly string k_LongitudeTitle = "緯度";
        public static readonly string k_LatitudeTitle = "経度";
        public static readonly string k_HeightTitle = "高さ";
        public static readonly string k_AssetType = "アセット種別";

        public float Longitude { get; set; }
        public float Latitude { get; set; }
        public float Height { get; set; }
        public string AssetType { get; set; }

        public PlateauSandboxBulkPlaceData(
            int id,
            string longitude,
            string latitude,
            string height,
            string assetType)
        {
            Id = id;
            Longitude = float.Parse(longitude);
            Latitude = float.Parse(latitude);
            Height = float.Parse(height);
            AssetType = assetType;
        }
    }
}