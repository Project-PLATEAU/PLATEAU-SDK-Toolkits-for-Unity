using PlateauToolkit.Sandbox.Runtime;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace PlateauToolkit.Sandbox.Runtime
{
    public static class PlateauSandboxCsvTemplate
    {
        public static void Create(string path)
        {
            // Generate CSV Template
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            var templateData = new List<PlateauSandboxBulkPlaceDataBase>();
            var data = new PlateauSandboxBulkPlaceCsvData(0, new List<string>()
            {
                "34.9873", "135.7596", "14.23", "イチョウ"
            }, new List<string>()
            {
                "緯度", "経度", "高さ", "アセット種別"
            });
            templateData.Add(data);

            data = new PlateauSandboxBulkPlaceCsvData(1, new List<string>()
            {
                "34.98742", "135.7596", "16.3", "ユリノキ"
            }, new List<string>()
            {
                "緯度", "経度", "高さ", "アセット種別"
            });
            templateData.Add(data);

            bool saveSuccess = new PlateauSandboxFileCsvParser().Save(path, templateData);
            if (!saveSuccess)
            {
                return;
            }
            string directoryName = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directoryName))
            {
                System.Diagnostics.Process.Start(directoryName);
            }

            Debug.Log($"CSVテンプレートを保存しました。{path}");
        }
    }
}