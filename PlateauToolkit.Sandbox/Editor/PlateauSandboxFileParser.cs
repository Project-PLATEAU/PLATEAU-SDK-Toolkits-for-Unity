
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Editor
{
    public enum PlateauSandboxFileParserValidationType
    {
        k_Valid = 0,
        k_NotExistsFile = 1,
        k_AccessControl = 2,
        k_FileOpened = 3,

        k_NotExistsDbfFile = 10,
    }

    /// <summary>
    /// Operate File Parse for Sandbox Toolkit.
    /// </summary>
    public abstract class PlateauSandboxFileParserBase
    {
        public virtual PlateauSandboxFileParserValidationType IsValidate(string filePath)
        {
            if (File.Exists(filePath))
            {
                // for AccessControl.
                try
                {
                    File.GetAccessControl(filePath);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error Not Access Control file: {e.Message}");
                    return PlateauSandboxFileParserValidationType.k_AccessControl;
                }

                // fot File Opened.
                try
                {
                    using FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                    stream.Close();
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error File is Opened: {e.Message}");
                    return PlateauSandboxFileParserValidationType.k_FileOpened;
                }
                return PlateauSandboxFileParserValidationType.k_Valid;
            }
            return PlateauSandboxFileParserValidationType.k_NotExistsFile;
        }

        public abstract List<PlateauSandboxBulkPlaceDataBase> Load(string filePath);
        public abstract bool Save(string filePath, List<PlateauSandboxBulkPlaceDataBase> data);
    }

    public class PlateauSandboxFileCsvParser : PlateauSandboxFileParserBase
    {
        public override List<PlateauSandboxBulkPlaceDataBase> Load(string filePath)
        {
            int lineNumber = 0;
            var csvData = new List<PlateauSandboxBulkPlaceDataBase>();
            string[] fieldLabels = Array.Empty<string>();

            try
            {
                using (var reader = new StreamReader(filePath, Encoding.GetEncoding("Shift-JIS")))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        if (line == null)
                        {
                            continue;
                        }

                        lineNumber++;
                        if (lineNumber == 1)
                        {
                            fieldLabels = line.Split(',');
                            continue;
                        }

                        int index = lineNumber - 1;
                        string[] values = line.Split(',');
                        var data = new PlateauSandboxBulkPlaceCsvData(index, values, fieldLabels);
                        csvData.Add(data);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error loading CSV file: {ex.Message}");
            }
            return csvData;
        }

        public override bool Save(string filePath, List<PlateauSandboxBulkPlaceDataBase> templateDatas)
        {
            try
            {
                using (var writer = new StreamWriter(filePath, false, Encoding.GetEncoding("Shift-JIS")))
                {
                    string[] titleLine = new string[]
                    {
                        PlateauSandboxBulkPlaceCategory.k_Longitude.Label(),
                        PlateauSandboxBulkPlaceCategory.k_Latitude.Label(),
                        PlateauSandboxBulkPlaceCategory.k_Height.Label(),
                        PlateauSandboxBulkPlaceCategory.k_AssetType.Label(),
                    };
                    writer.WriteLine(string.Join(",", titleLine));
                    foreach (var bulkPlaceData in templateDatas)
                    {
                        string[] line = new string[]
                        {
                            bulkPlaceData.Longitude.ToString(),
                            bulkPlaceData.Latitude.ToString(),
                            bulkPlaceData.Height.ToString(),
                            bulkPlaceData.AssetType,
                        };
                        writer.WriteLine(string.Join(",", line));
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error Saving CSV file: {ex.Message}");
            }
            return false;
        }
    }

    public class PlateauSandboxFileShapeFileParser : PlateauSandboxFileParserBase
    {
        public static readonly string[] k_AssetTypePatterns = {"JUSHUMEI", "ASSET_TYPE"};

        private string GetDbfFilePath(string filePath) => filePath.Replace(PlateauSandboxBulkPlaceDataBase.k_ShapeFileExtension, PlateauSandboxBulkPlaceDataBase.k_DbfFileExtension);

        public override PlateauSandboxFileParserValidationType IsValidate(string filePath)
        {
            PlateauSandboxFileParserValidationType isValidate = base.IsValidate(filePath);
            if (isValidate != PlateauSandboxFileParserValidationType.k_Valid)
            {
                return isValidate;
            }

            if (File.Exists(filePath))
            {
                if (File.Exists(GetDbfFilePath(filePath)))
                {
                    return PlateauSandboxFileParserValidationType.k_Valid;
                }
            }
            return PlateauSandboxFileParserValidationType.k_NotExistsDbfFile;
        }

        public override List<PlateauSandboxBulkPlaceDataBase> Load(string filePath)
        {
            var shapeFileData = new List<PlateauSandboxBulkPlaceDataBase>();
            List<IShape> listOfShapes;
            using (var shapeFileReader = new PlateauSandboxShapeFileReader(filePath))
            {
                listOfShapes = shapeFileReader.ReadShapes();
            }

            using (var dbfReader = new PlateauSandboxDbfReader(GetDbfFilePath(filePath), SupportedEncoding.UTF8))
            {
                dbfReader.ReadHeader();

                for (int i = 0; i < listOfShapes.Count; i++)
                {
                    DbfRecord record = dbfReader.ReadNextRecord();

                    var data = new PlateauSandboxBulkPlaceShapeData(
                        i, listOfShapes[i], dbfReader.GetFieldNames().ToArray(), record.Fields);
                    shapeFileData.Add(data);

                    Debug.Log($"ShapeData: {data.Id}, {data.Latitude}, {data.Longitude}, {data.Height}, {data.AssetType}");
                }
            }
            return shapeFileData;
        }

        public override bool Save(string filePath, List<PlateauSandboxBulkPlaceDataBase> data)
        {
            return true;
        }
    }
}