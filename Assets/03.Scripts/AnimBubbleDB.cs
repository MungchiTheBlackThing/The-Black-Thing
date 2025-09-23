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
        // Resources/CSV/AnimBubble.csv  (Ȯ���� ���� ��� ����)
        var csv = Resources.Load<TextAsset>("CSV/AnimBubble");
        if (csv == null)
        {
            Debug.LogError("AnimBubble.csv�� ã�� ���߽��ϴ�. (Resources/CSV/AnimBubble.csv)");
            return;
        }

        var lines = csv.text.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;
            if (i == 0 && line.StartsWith("DotAnim")) continue; // ��� ��ŵ

            // ù ��° ��ǥ ��ġ�� �������� ��/�� �ʵ� �и� (�� ��° �ʵ�� "79, 10" ���� ��ǥ ����)
            int firstComma = line.IndexOf(',');
            if (firstComma <= 0) continue;

            string anim = line.Substring(0, firstComma).Trim();
            string posRaw = line.Substring(firstComma + 1).Trim();

            // ����ǥ ����
            if (posRaw.StartsWith("\"") && posRaw.EndsWith("\"") && posRaw.Length >= 2)
                posRaw = posRaw.Substring(1, posRaw.Length - 2);

            // "79, 10" �� ["79"," 10"]
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
