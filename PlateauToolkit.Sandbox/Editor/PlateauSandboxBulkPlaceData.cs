using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace PlateauToolkit.Sandbox.Editor
{
    public enum PlateauSandboxBulkPlaceFileType
    {
        k_InValid = -1,
        k_Csv = 0,
        k_ShapeFile,
    }

    public enum PlateauSandboxBulkPlaceCategory
    {
        k_InValid = -1,
        k_Longitude = 0,
        k_Latitude,
        k_Height,
        k_AssetType,
    }
    static class PlateauSandboxBulkPlaceFileTypeExtensions
    {
        const string k_LongitudeTitle = "緯度";
        const string k_LatitudeTitle = "経度";
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

    public abstract class PlateauSandboxBulkPlaceDataBase
    {
        public const string k_CsvExtension = ".csv";
        public const string k_ShapeFileExtension = ".shp";
        public const string k_DbfFileExtension = ".dbf";

        public int Id { get; protected set; }
        public string Longitude { get; protected set; }
        public string Latitude { get; protected set; }
        public string Height { get; protected set; }
        protected string m_AssetType;
        public string AssetType
        {
            get => m_AssetType.Length > 20 ? m_AssetType.Substring(0, 20) + "..." : m_AssetType;
        }
        public abstract void ReplaceField(int oldFieldIndex, int newFieldIndex);
        public abstract List<string> GetFieldLabels();
    }

    public class PlateauSandboxBulkPlaceCsvData : PlateauSandboxBulkPlaceDataBase
    {
        List<string> m_FieldNames;

        public PlateauSandboxBulkPlaceCsvData(int index, string[] csvData, string[] fieldNames)
        {
            Id = index;
            Longitude = csvData[0];
            Latitude = csvData[1];
            Height = csvData[2];
            m_AssetType = csvData[3];

            m_FieldNames = fieldNames.ToList();
        }

        string GetFieldValue(int fieldIndex)
        {
            switch (fieldIndex)
            {
                case (int)PlateauSandboxBulkPlaceCategory.k_Longitude:
                    return Longitude;
                case (int)PlateauSandboxBulkPlaceCategory.k_Latitude:
                    return Latitude;
                case (int)PlateauSandboxBulkPlaceCategory.k_Height:
                    return Height;
                case (int)PlateauSandboxBulkPlaceCategory.k_AssetType:
                    return AssetType;
                default:
                    return string.Empty;
            }
        }

        public override void ReplaceField(int oldFieldIndex, int newFieldIndex)
        {
            string oldValue = GetFieldValue(oldFieldIndex);

            switch (oldFieldIndex)
            {
                case (int)PlateauSandboxBulkPlaceCategory.k_Longitude:
                    Longitude = GetFieldValue(newFieldIndex);
                    break;
                case (int)PlateauSandboxBulkPlaceCategory.k_Latitude:
                    Latitude = GetFieldValue(newFieldIndex);
                    break;
                case (int)PlateauSandboxBulkPlaceCategory.k_Height:
                    Height = GetFieldValue(newFieldIndex);
                    break;
                case (int)PlateauSandboxBulkPlaceCategory.k_AssetType:
                    m_AssetType = GetFieldValue(newFieldIndex);
                    break;
            }

            switch (newFieldIndex)
            {
                case (int)PlateauSandboxBulkPlaceCategory.k_Longitude:
                    Longitude = oldValue;
                    break;
                case (int)PlateauSandboxBulkPlaceCategory.k_Latitude:
                    Latitude = oldValue;
                    break;
                case (int)PlateauSandboxBulkPlaceCategory.k_Height:
                    Height = oldValue;
                    break;
                case (int)PlateauSandboxBulkPlaceCategory.k_AssetType:
                    m_AssetType = oldValue;
                    break;
            }

            // Replace FieldName.
            (m_FieldNames[oldFieldIndex], m_FieldNames[newFieldIndex]) = (m_FieldNames[newFieldIndex], m_FieldNames[oldFieldIndex]);
        }

        public override List<string> GetFieldLabels()
        {
            return new List<string>() {
                m_FieldNames[0] + $"（要素例：{Longitude})",
                m_FieldNames[1] + $"（要素例：{Latitude})",
                m_FieldNames[2] + $"（要素例：{Height})",
                m_FieldNames[3] + $"（要素例：{AssetType})",
            };
        }
    }

    public class PlateauSandboxBulkPlaceShapeData : PlateauSandboxBulkPlaceDataBase
    {
        struct DbfField
        {
            public string m_FieldName;
            public string m_FieldValue;
        }

        List<DbfField> m_Fields = new List<DbfField>();

        public PlateauSandboxBulkPlaceShapeData(int index, IShape shape, string[] fieldNames, string[] fieldValues)
        {
            Id = index;
            Longitude = shape.Points[0].x.ToString("G20");  // G20 is To maintain precision.
            Latitude = shape.Points[0].z.ToString("G20");
            Height = shape.Points[0].y.ToString("G20");

            for (int i = 0; i < fieldNames.Length; i++)
            {
                string fieldValue = fieldValues[i].Replace(" ", "");
                m_Fields.Add(new DbfField
                {
                    m_FieldName = fieldNames[i],
                    m_FieldValue = string.IsNullOrEmpty(fieldValue) ? "指定なし" : fieldValue,
                });
            }
            DbfField assetField = m_Fields
                .FirstOrDefault(field =>
                {
                    return PlateauSandboxFileShapeFileParser.k_AssetTypePatterns
                        .Any(pattern => field.m_FieldName == pattern);
                });
            m_AssetType = assetField.m_FieldValue;

            // Move AssetType to the top.
            m_Fields.Remove(assetField);
            m_Fields.Insert(0, assetField);
        }

        public override List<string> GetFieldLabels()
        {
            return m_Fields
                .Select(field => $"{field.m_FieldName}（要素例：{field.m_FieldValue})")
                .ToList();
        }

        public override void ReplaceField(int oldFieldIndex, int newFieldIndex)
        {
            if (m_Fields.Count <= newFieldIndex)
            {
                return;
            }
            m_AssetType = m_Fields[newFieldIndex].m_FieldValue;
        }
    }
}