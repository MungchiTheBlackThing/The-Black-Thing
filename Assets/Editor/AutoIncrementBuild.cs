using UnityEngine;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor;

public class AutoIncrementBuild : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
#if UNITY_IOS
        int buildNumber = int.Parse(PlayerSettings.iOS.buildNumber);
        buildNumber++;
        PlayerSettings.iOS.buildNumber = buildNumber.ToString();
        Debug.Log("iOS Build Number: " + buildNumber);
#endif

    }
}
