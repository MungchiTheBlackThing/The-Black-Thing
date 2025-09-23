using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public static class AnimBubbleDB
{
    private static Dictionary<string, Vector2> _offsets;

    public static void EnsureLoaded()
    {
        if (_offsets != null) return;

        _offsets = new Dictionary<string, Vector2>();
        // Resources/CSV/AnimBubble.csv  (확장자 빼고 경로 적기)
        var csv = Resources.Load<TextAsset>("CSV/AnimBubble");
        if (csv == null)
        {
            Debug.LogError("AnimBubble.csv를 찾지 못했습니다. (Resources/CSV/AnimBubble.csv)");
            return;
        }

        var lines = csv.text.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;
            if (i == 0 && line.StartsWith("DotAnim")) continue; // 헤더 스킵

            // 첫 번째 쉼표 위치를 기준으로 좌/우 필드 분리 (두 번째 필드는 "79, 10" 같이 쉼표 포함)
            int firstComma = line.IndexOf(',');
            if (firstComma <= 0) continue;

            string anim = line.Substring(0, firstComma).Trim();
            string posRaw = line.Substring(firstComma + 1).Trim();

            // 따옴표 제거
            if (posRaw.StartsWith("\"") && posRaw.EndsWith("\"") && posRaw.Length >= 2)
                posRaw = posRaw.Substring(1, posRaw.Length - 2);

            // "79, 10" → ["79"," 10"]
            var xy = posRaw.Split(',');
            if (xy.Length < 2) continue;

            if (float.TryParse(xy[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float x) &&
                float.TryParse(xy[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float y))
            {
                _offsets[anim] = new Vector2(x, y);
            }
        }

        Debug.Log($"AnimBubble offsets loaded: count={_offsets.Count}");
    }

    public static Vector2 GetOffset(string animKey)
    {
        EnsureLoaded();
        if (animKey != null && _offsets != null && _offsets.TryGetValue(animKey, out var v))
            return v;
        return Vector2.zero;
    }
}
