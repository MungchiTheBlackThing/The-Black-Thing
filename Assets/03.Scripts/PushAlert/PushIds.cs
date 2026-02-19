public enum PushIdType
{
    Watching = 10,
    A = 20,
    A6 = 21,
    B = 30,
    B6 = 31,
    Night = 40,

    // 장기 미접속 같은 챕터 비의존 push는 별도 처리
    Missing24h = 101,
    Missing2d = 102,
    Missing4d = 103,
    Missing7d = 104,
    Missing14d = 105,
}

public static class PushIds
{
    // 챕터 기반 알림: 200000 + chapter*100 + typeOffset
    public static int Make(PushIdType type, int chapter)
    {
        int offset = (int)type;

        // Missing 계열은 chapter와 섞지 않음
        if (offset >= 100) return 210000 + offset;

        return 200000 + chapter * 100 + offset;
    }
}
