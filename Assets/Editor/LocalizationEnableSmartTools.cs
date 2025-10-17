#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization.Tables;


public class LocalizationEnableSmartTools
{
    [MenuItem("Tools/Localization/Enable Smart on ALL String Tables")]
    public static void EnableSmartOnAll() => ToggleSmartOnAll(true);

    [MenuItem("Tools/Localization/Disable Smart on ALL String Tables")]
    public static void DisableSmartOnAll() => ToggleSmartOnAll(false);

    private static void ToggleSmartOnAll(bool enable)
    {
        var collections = LocalizationEditorSettings.GetStringTableCollections();
        if (collections == null || collections.Count == 0)
        {
            Debug.LogWarning("[Localization] No StringTableCollections found.");
            return;
        }

        int tableCount = 0;
        int entryCount = 0;
        int changed = 0;

        foreach (var coll in collections)
        {
            foreach (var table in coll.StringTables) 
            {
                tableCount++;
                foreach (var entry in table.Values)    
                {
                    entryCount++;
                    if (entry.IsSmart != enable)
                    {
                        entry.IsSmart = enable;
                        changed++;
                    }
                }
                EditorUtility.SetDirty(table);
            }
            EditorUtility.SetDirty(coll);
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[Localization] {(enable ? "Enabled" : "Disabled")} Smart on {changed}/{entryCount} entries across {tableCount} tables in {collections.Count} collections.");
    }
}
#endif