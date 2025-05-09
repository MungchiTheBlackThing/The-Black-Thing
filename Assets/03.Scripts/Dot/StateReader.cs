using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class AnimationEntry
{
    public List<float> positions;
}

[Serializable]
public class AnimationData
{
    public List<AnimationKeyValue> animations;
}

[Serializable]
public class AnimationKeyValue
{
    public string key;
    public AnimationEntry value;
}
//public class StateReader
//{
//    public void ReadJson(DotState state, TextAsset jsonFile)
//    {
//        if (jsonFile == null)
//        {
//            Debug.LogError("JSON file is not assigned.");
//            return;
//        }
//        AnimationData animationData = JsonUtility.FromJson<AnimationData>(jsonFile.text);

//        foreach (var anim in animationData.animations)
//        {
//            DotAnimState stateEnum;
//            if (Enum.TryParse(anim.key, true, out stateEnum))
//            {
//                List<float> val = anim.value.positions;

//                // DotAnim에 하나씩 삽입.
//                state.Init(stateEnum, val);
//            }
//            else
//            {
//                Debug.LogWarning($"Invalid animation key: {anim.key}");
//            }
//        }

//        state.Read();
//    }
//}
