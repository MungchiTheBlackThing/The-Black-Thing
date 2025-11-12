using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardPopup : MonoBehaviour
{
    [SerializeField]
    SubTutorial subtuto;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnDisable()
    {
        if (subtuto.isActiveAndEnabled)
        {
            subtuto.guidestart();
        }
    }

}
