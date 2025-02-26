
using UnityEditor;
using UnityEditor.PackageManager;

namespace PlateauToolkit.Utilities.Editor
{
    /// <summary>
    /// Packagesフォルダに入っている PLATEAU SDK Toolkits for Unity を tarball 形式で出力します。
    /// デプロイで利用します。
    /// </summary>
    internal static class PackagePacker
    {
        [MenuItem("PLATEAU/Debug/Pack PLATEAU Toolkit Package to tarball")]
        public static void Pack()
        {
            string destDir = EditorUtility.SaveFolderPanel("出力先", "", "");
            Client.Pack(PlateauUtilitiesPaths.k_ToolkitBasePath, destDir);
        }
    }
}