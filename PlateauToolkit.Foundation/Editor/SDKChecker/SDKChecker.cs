using System.IO;
using UnityEditor;
using UnityEngine;

public class SDKChecker : EditorWindow
{
    [MenuItem("Window/SDK Checker")]
    public static void ShowWindow()
    {
        GetWindow<SDKChecker>("SDK Checker");
    }

    private void OnGUI()
    {
        GUILayout.Label("Check if SDK is installed", EditorStyles.boldLabel);

        if (GUILayout.Button("Check for Cesium SDK"))
        {
            CheckForSDK();
        }
    }

    private void CheckForSDK()
    {
        string targetFilePath = "Packages/com.cesium.unity/README.md";

        if (File.Exists(targetFilePath) || Directory.Exists(targetFilePath))
        {
            Debug.Log("SDK is installed.");
        }
        else
        {
            Debug.Log("SDK is not installed.");
        }
    }
}
