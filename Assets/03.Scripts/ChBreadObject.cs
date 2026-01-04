using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChBreadObject : MonoBehaviour
{
    // ObjectManager에서 호출
    public bool ShouldBeActive(bool isDay8SleepEventCompleted)
    {
        // 성공 전(false) → true 반환 (보임)
        // 성공 후(true)  → false 반환 (안 보임)
        return !isDay8SleepEventCompleted;
    }
}
