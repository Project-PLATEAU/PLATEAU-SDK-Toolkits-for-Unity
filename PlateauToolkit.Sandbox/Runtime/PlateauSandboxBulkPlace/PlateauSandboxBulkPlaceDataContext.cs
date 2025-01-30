using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Runtime
{
    public class PlateauSandboxBulkPlaceDataContext
    {
        struct BulkPlaceFile
        {
            public string FileName { get; private set; }
            public PlateauSandboxBulkPlaceFileType FileType { get; private set; }

            public void SetFilePath(string filePath)
            {
                FileName = System.IO.Path.GetFileName(filePath);
                string extension = System.IO.Path.GetExtension(filePath);
                if (extension == PlateauSandboxBulkPlaceDataBase.k_CsvExtension)
                {
                    FileType = PlateauSandboxBulkPlaceFileType.k_Csv;
                }
                else if (extension == PlateauSandboxBulkPlaceDataBase.k_ShapeFileExtension)
                {
                    FileType = PlateauSandboxBulkPlaceFileType.k_ShapeFile;
                }
            }

            public void Clear()
            {
                FileName = string.Empty;
                FileType = PlateauSandboxBulkPlaceFileType.k_InValid;
            }
        }

        BulkPlaceFile m_LoadedFile;
        public List<PlateauSandboxBulkPlaceDataBase> Data { get; private set; } = new List<PlateauSandboxBulkPlaceDataBase>();
        int m_SelectedFieldIndex;
        public PlateauSandboxFileParserValidationType FileValidationType { get; private set; }

        public bool HasLoadedFile()
        {
            return !string.IsNullOrEmpty(m_LoadedFile.FileName);
        }

        public string GetFileName()
        {
            return m_LoadedFile.FileName;
        }

        public PlateauSandboxBulkPlaceFileType GetFileType()
        {
            return m_LoadedFile.FileType;
        }

        public bool TryFileParse(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return false;
            }
            m_LoadedFile.SetFilePath(filePath);

            PlateauSandboxFileParserBase parser = null;
            switch (GetFileType())
            {
                case PlateauSandboxBulkPlaceFileType.k_Csv:
                    parser = new PlateauSandboxFileCsvParser();
                    break;
                case PlateauSandboxBulkPlaceFileType.k_ShapeFile:
                    parser = new PlateauSandboxFileShapeFileParser();
                    break;
                case PlateauSandboxBulkPlaceFileType.k_InValid:
                default:
                    Debug.LogWarning("Invalid file type.");
                    return false;
            }

            FileValidationType = parser.IsValidate(filePath);
            if (FileValidationType == PlateauSandboxFileParserValidationType.k_Valid)
            {
                var parsedData = parser.Load(filePath);
                if (parsedData.Count > 0)
                {
                    Data = parsedData;
                    return true;
                }
            }

            Clear();
            return false;
        }

        public void Clear()
        {
            Data.Clear();
            m_LoadedFile.Clear();
            m_SelectedFieldIndex = 0;
        }

        public int GetFieldIndex(PlateauSandboxBulkPlaceCategory category)
        {
            if (GetFileType() == PlateauSandboxBulkPlaceFileType.k_Csv)
            {
                return (int)category;
            }
            else
            {
                return m_SelectedFieldIndex;
            }
        }

        public string[] GetFieldLabels()
        {
            return Data.FirstOrDefault()?.GetFieldLabels().ToArray();
        }

        public void ReplaceField(int oldFieldIndex, int newFieldIndex)
        {
            m_SelectedFieldIndex = newFieldIndex;
            Data.ForEach(data => data.ReplaceField(oldFieldIndex, newFieldIndex));
        }
    }
}