#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using Google.Android.AppBundle.Editor;           
using Google.Android.AppBundle.Editor.AssetPacks; 

public static class PadVideoPackConfig
{
    private const string PackName = "videos_pack";
    private const string SourceFolderName = "PadRawVideos"; // 프로젝트 루트(Assets 옆)

    [MenuItem("Tools/PAD/Configure videos_pack (Install-time)")]
    public static void Configure()
    {
        var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        var folder = Path.Combine(projectRoot, SourceFolderName);

        if (!Directory.Exists(folder))
        {
            Debug.LogError($"[PAD] Folder not found: {folder}");
            return;
        }

        var config = new AssetPackConfig();
        config.AddAssetsFolder(PackName, folder, AssetPackDeliveryMode.InstallTime);
        AssetPackConfigSerializer.SaveConfig(config);

        Debug.Log($"[PAD] Saved AssetPackConfig: {PackName} <- {folder}");
    }
}
#endif