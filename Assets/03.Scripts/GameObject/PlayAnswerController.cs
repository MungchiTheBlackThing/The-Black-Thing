using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayAnswerController : MonoBehaviour
{
    [SerializeField]
    GameObject playPlayer;
    [SerializeField]
    GameObject playDot;


    [SerializeField]
    bool answer;

    [SerializeField]
    GameObject poem;

    DotController dot;

    private void Start()
    {
        dot = playDot.transform.parent.GetComponent<DotController>();
    }
    private void OnMouseUp()
    {
        //YES�� �ǹ�
        if (answer)
        {
            //�� Canvas�� Ų��.
            if(poem)
            {
                Instantiate(poem, GameObject.Find("Canvas").transform);
            }
        }
        else
        {
            //No�� �ǹ��Ѵ�. No�� ��� ��ġ �ڴ� �ִϸ��̼� ����
            dot.GoSleep();
        }

        playPlayer.SetActive(false);
        playDot.SetActive(false);
    }
}
