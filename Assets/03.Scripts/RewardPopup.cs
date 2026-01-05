using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RewardPopup : MonoBehaviour
{
    [SerializeField]
    SubTutorial subtuto;
    [SerializeField] GameObject menuBut;
    // Start is called before the first frame update
    void Start()
    {
        if (subtuto.isActiveAndEnabled)
        {
            var btn = menuBut.GetComponent<Button>();
            if (btn != null)
                btn.interactable = false;
        }
        
    }

    private void OnDisable()
    {
        if (subtuto.isActiveAndEnabled)
        {
            subtuto.guidestart();
        }
    }

}
