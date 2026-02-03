namespace Assets.Script.TimeEnum
{
    public enum SITime : int
    {
        Dawn = 0,
        Evening = 1,
        Morning = 2,
        Night = 3,
    };

    public enum STime : int
    { //���� �ð�
        T_MORNING = 8, //8~16�ñ���
        T_EVENING = 16, //16~20����
        T_NIGHT = 20, //20~5�ñ���
        T_DAWN = 4, //4~8�ñ���
    };


    public enum ChapterDay : int
    {
        C_1DAY = 1,
        C_2DAY,
        C_3DAY,
        C_4DAY,
        C_5DAY,
        C_6DAY,
        C_7DAY,
        C_8DAY,
        C_9DAY,
        C_10DAY,
        C_11DAY,
        C_12DAY,
        C_13DAY,
        C_14DAY,
        END
    };
}