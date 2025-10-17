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
    GameManager gameManager;
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
        //YES를 의미
        if (answer)
        {
            //시 Canvas를 킨다.
            if(poem)
            {
                Instantiate(poem, GameObject.Find("Canvas").transform);
            }
        }
        else
        {
            //No를 의미한다. No일 경우 뭉치 자는 애니메이션 수행
            gameManager.GoSleep();
        }

        playPlayer.SetActive(false);
        playDot.SetActive(false);
    }
}
