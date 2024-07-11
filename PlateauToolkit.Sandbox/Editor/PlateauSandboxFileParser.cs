
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Editor
{
    /// <summary>
    /// Operate File Parse for Sandbox Toolkit.
    /// </summary>
    public abstract class PlateauSandboxFileParserBase<TData>
        where TData : PlateauSandboxFileDataBase
    {
        public void Parse(string filePath)
        {
            var data = Load(filePath);
        }

        public abstract bool IsValidate(string filePath);
        public abstract List<TData> Load(string filePath);
        public abstract bool Save(string filePath, List<TData> data);
    }

    public class PlateauSandboxFileCsvParser : PlateauSandboxFileParserBase<PlateauSandboxBulkPlaceData>
    {
        public override bool IsValidate(string filePath)
        {
            if (File.Exists(filePath))
            {
                try
                {
                    File.GetAccessControl(filePath);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error Not Access Control CSV file: {e.Message}");
                    return false;
                }
                return true;
            }
            return false;
        }

        public override List<PlateauSandboxBulkPlaceData> Load(string filePath)
        {
            int lineNumber = 0;
            var csvData = new List<PlateauSandboxBulkPlaceData>();

            try
            {
                using (var reader = new StreamReader(filePath, Encoding.GetEncoding("Shift-JIS")))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        lineNumber++;
                        if (lineNumber == 1) { continue; }


                        var values = line.Split(',');
                        csvData.Add(new PlateauSandboxBulkPlaceData(
                            lineNumber, values[0], values[1], values[2], values[3]));
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error loading CSV file: {ex.Message}");
            }
            return csvData;
        }

        public override bool Save(string filePath, List<PlateauSandboxBulkPlaceData> data)
        {
            try
            {
                using (var writer = new StreamWriter(filePath, false, Encoding.GetEncoding("Shift-JIS")))
                {
                    var titleLine = new string[]
                    {
                        PlateauSandboxBulkPlaceData.k_LongitudeTitle,
                        PlateauSandboxBulkPlaceData.k_LatitudeTitle,
                        PlateauSandboxBulkPlaceData.k_HeightTitle,
                        PlateauSandboxBulkPlaceData.k_AssetType,
                    };
                    writer.WriteLine(string.Join(",", titleLine));
                    foreach (var bulkPlaceData in data)
                    {
                        var line = new string[]
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
}