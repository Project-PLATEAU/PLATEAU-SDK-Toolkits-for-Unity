using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

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

    public abstract class PlateauSandboxBulkPlaceDataBase
    {
        public const string k_CsvExtension = ".csv";
        public const string k_ShapeFileExtension = ".shp";
        public const string k_DbfFileExtension = ".dbf";

        public int ID { get; protected set; }
        public string Longitude { get; protected set; }
        public string Latitude { get; protected set; }
        public string Height { get; protected set; }
        protected string m_AssetType;
        public string AssetType =>
            m_AssetType.Length > 20 ? m_AssetType.Substring(0, 20) + "..." : m_AssetType;
        public bool IsIgnoreHeight { get; protected set; }
        public abstract void ReplaceField(int oldFieldIndex, int newFieldIndex);
        public abstract List<string> GetFieldLabels();
    }

    public class PlateauSandboxBulkPlaceCsvData : PlateauSandboxBulkPlaceDataBase
    {
        List<string> m_FieldNames;

        public PlateauSandboxBulkPlaceCsvData(int index, List<string> csvData, List<string> fieldNames)
        {
            ID = index;

            int latitudeIndex = fieldNames.FindIndex(name => PlateauSandboxBulkPlaceCategory.k_Latitude.IsMatch(name));
            int longitudeIndex = fieldNames.FindIndex(name => PlateauSandboxBulkPlaceCategory.k_Longitude.IsMatch(name));
            int heightIndex = fieldNames.FindIndex(name => PlateauSandboxBulkPlaceCategory.k_Height.IsMatch(name));
            int assetTypeIndex = fieldNames.FindIndex(name => PlateauSandboxBulkPlaceCategory.k_AssetType.IsMatch(name));

            Latitude = csvData[latitudeIndex > 0 ? latitudeIndex : 0];
            Longitude = csvData[longitudeIndex > 0 ? longitudeIndex : 1];
            Height = csvData[heightIndex > 0 ? heightIndex : 2];

            string assetType = csvData[assetTypeIndex > 0 ? assetTypeIndex : 3];
            m_AssetType = string.IsNullOrEmpty(assetType) ? "指定なし" : assetType;

            m_FieldNames = fieldNames;
            IsIgnoreHeight = false;
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
                m_FieldNames[0] + $"（要素例：{Latitude})",
                m_FieldNames[1] + $"（要素例：{Longitude})",
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
            Longitude = shape.Points[0].x.ToString(CultureInfo.CurrentCulture);
            Latitude = shape.Points[0].z.ToString(CultureInfo.CurrentCulture);
            Height = shape.Points[0].y.ToString(CultureInfo.CurrentCulture);

            for (int i = 0; i < fieldNames.Length; i++)
            {
                string fieldValue = fieldValues[i].Replace(" ", "");
                m_Fields.Add(new DbfField
                {
                    m_FieldName = fieldNames[i],
                    m_FieldValue = string.IsNullOrEmpty(fieldValue) ? "指定なし" : fieldValue,
                });
            }

            // Find ObjectId field.
            foreach (DbfField dbfField in m_Fields)
            {
                if (PlateauSandboxFileShapeFileParser.k_ObjectIdPatterns
                    .Any(pattern => pattern == dbfField.m_FieldName))
                {
                    try
                    {
                        ID = int.Parse(dbfField.m_FieldValue);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"parse error: {dbfField.m_FieldValue}, {e.Message}");
                    }
                    break;
                }
            }
            if (ID <= 0)
            {
                ID = index;
            }

            // Find AssetType field.
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

            // For ShapeFile Set the flag to ignore the height.
            IsIgnoreHeight = true;
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