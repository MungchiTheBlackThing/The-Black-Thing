#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

public static class AssetBundleBuilder
{
    [MenuItem("Tools/Build AssetBundles/Current Platform")]
    public static void BuildForCurrentPlatform()
    {
        var target = EditorUserBuildSettings.activeBuildTarget;

        var outputPath = Path.Combine("Assets/StreamingAssets/AssetBundles", target.ToString());
        if (!Directory.Exists(outputPath)) Directory.CreateDirectory(outputPath);

        BuildPipeline.BuildAssetBundles(outputPath, BuildAssetBundleOptions.None, target);
        Debug.Log($"[AssetBundles] Built for {target} -> {outputPath}");
    }
}
#endif
