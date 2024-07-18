using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Editor
{
    public class PlateauSandboxBulkPlaceData
    {
        public struct PrefabContext
        {
            public string m_AssetName;
            public int m_ConstantId;
        }

        public const string k_CsvExtension = ".csv";
        public const string k_ShapeFileExtension = ".shp";
        public const string k_DbfFileExtension = ".dbf";

        public const string k_LongitudeTitle = "緯度";
        public const string k_LatitudeTitle = "経度";
        public const string k_HeightTitle = "高さ";
        public const string k_AssetType = "アセット種別";

        readonly string[] m_AssetTypeSeparators = new string[] { ",", "，", "、", "・" };

        public int Id { get; protected set; }
        public float Longitude { get; protected set; }
        public float Latitude { get; protected set; }
        public float Height { get; protected set; }

        public List<PrefabContext> PrefabContexts { get; private set; } = new List<PrefabContext>();

        public void Set(int index, float longitude, float latitude, float height, string[] assetTypes)
        {
            Id = index;
            Longitude = longitude;
            Latitude = latitude;
            Height = height;
            SetPrefabContexts(assetTypes);
        }

        protected void ParseAssetType(string assetType)
        {
            if (string.IsNullOrEmpty(assetType))
            {
                return;
            }
            // Remove WhiteSpace.
            assetType = Regex.Replace(assetType, @"\s+", "");

            // Split by separators.
            string pattern = string.Join("|", m_AssetTypeSeparators.Select(Regex.Escape));
            string[] assetTypes = Regex.Split(assetType, pattern);

            SetPrefabContexts(assetTypes);
        }

        void SetPrefabContexts(string[] assetTypes)
        {
            for (int i = 0; i < assetTypes.Length; i++)
            {
                var prefabContext = new PrefabContext()
                {
                    m_AssetName = assetTypes[i],
                    m_ConstantId = -1,
                };
                PrefabContexts.Add(prefabContext);
            }
        }
    }

    public class PlateauSandboxBulkPlaceCsvData : PlateauSandboxBulkPlaceData
    {
        public PlateauSandboxBulkPlaceCsvData(int index, string[] csvData)
        {
            Id = index;
            Longitude = float.Parse(csvData[0]);
            Latitude = float.Parse(csvData[1]);
            Height = float.Parse(csvData[2]);

            ParseAssetType(csvData[3]);
        }
    }

    public class PlateauSandboxBulkPlaceShapeData : PlateauSandboxBulkPlaceData
    {
        private const string k_AssetTypeName = "JUSHUMEI";

        struct DbfField
        {
            public string m_FieldName;
            public string m_FieldValue;
        }

        private List<DbfField> m_Fields = new List<DbfField>();

        public PlateauSandboxBulkPlaceShapeData(int index, IShape shape, string[] fieldNames, string[] fieldValues)
        {
            Id = index;
            Longitude = shape.Points[0].x;
            Latitude = shape.Points[0].z;
            Height = shape.Points[0].y;

            for (int i = 0; i < fieldNames.Length; i++)
            {
                m_Fields.Add(new DbfField
                {
                    m_FieldName = fieldNames[i],
                    m_FieldValue =  fieldValues[i],
                });
            }
            DbfField assetType = m_Fields.FirstOrDefault(field => field.m_FieldName == k_AssetTypeName);
            ParseAssetType(assetType.m_FieldValue);
        }
    }
}