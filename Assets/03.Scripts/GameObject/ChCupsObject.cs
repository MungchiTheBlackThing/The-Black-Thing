using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChCupSwapObject : MonoBehaviour
{
    [SerializeField] private bool isCupDot = false; // cupdot이면 true, cup이면 false

    // 요구사항 상 상수처럼 고정돼도 되지만, 인스펙터로 두고 싶으면 SerializeField 유지해도 됨
    [SerializeField] private int showFromChapter = 9;   // 9일부터 후보 등장
    [SerializeField] private int lastChapter = 12;      // 12까지만 고려, 13부터는 전부 OFF

    public bool ShouldBeActive(int chapter, GamePatternState phase)
    {
        // 1~8: 둘 다 OFF
        if (chapter < showFromChapter) return false;

        // 13~엔딩: 둘 다 OFF
        if (chapter > lastChapter) return false;

        // 9~11: cup ON, cupdot OFF
        if (chapter >= 9 && chapter <= 11)
            return !isCupDot;

        // ---- 여기서부터 chapter == 12 ----

        // 12 watching: cup ON
        if (phase == GamePatternState.Watching)
            return !isCupDot;

        // 12 MainA: cupdot ON
        if (phase == GamePatternState.MainA)
            return isCupDot;

        // 12 thinking, mainb, writing, play, sleeping, nextchapter... 전부 OFF
        return false;
    }
}