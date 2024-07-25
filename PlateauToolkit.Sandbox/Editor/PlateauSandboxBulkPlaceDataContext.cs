using System.Collections.Generic;
using System.Linq;

namespace PlateauToolkit.Sandbox.Editor
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

        BulkPlaceFile m_LoadedFile = new BulkPlaceFile();
        public List<PlateauSandboxBulkPlaceDataBase> Datas { get; private set; } = new List<PlateauSandboxBulkPlaceDataBase>();
        int m_SelectedFieldIndex = 0;

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

        public void SetFilePath(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }
            m_LoadedFile.SetFilePath(filePath);
        }

        public void SetAllData(List<PlateauSandboxBulkPlaceDataBase> datas)
        {
            Datas = datas;
        }

        public void Clear()
        {
            Datas.Clear();
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
            return Datas.FirstOrDefault()?.GetFieldLabels().ToArray() ?? null;
        }

        public void ReplaceField(int oldFieldIndex, int newFieldIndex)
        {
            m_SelectedFieldIndex = newFieldIndex;
            Datas.ForEach(data => data.ReplaceField(oldFieldIndex, newFieldIndex));
        }
    }
}