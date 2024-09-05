using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PoemController : MonoBehaviour
{

    [SerializeField]
    Image nextPage;

    [SerializeField]
    Image prevPage;

    [SerializeField]
    Image background;

    [SerializeField]
    TMP_Text text;
    const string path = "Background/PoemBackground/";

    //���� Ÿ���� ������, �������� GameManager�� ����������
    GameManager gameManager;

    int currentPage;
    int totalPage;
    int chapter;


    private void OnEnable()
    {
        gameManager = GameObject.FindWithTag("GameController").GetComponent<GameManager>();

        if (gameManager)
        {
            nextPage.sprite = Resources.Load<Sprite>(path + "NextPage/" + gameManager.Time);
            prevPage.sprite = Resources.Load<Sprite>(path + "PrevPage/" + gameManager.Time);
            background.sprite = Resources.Load<Sprite>(path + gameManager.Time);
        }
        prevPage.gameObject.SetActive(false);
        currentPage = 0; //�� �� ���� ù��° ��������
        LoadPoem();
    }


    void LoadPoem()
    {
        if (gameManager)
        {
            //�� ������ ������Ʈ
            chapter = gameManager.Chapter;

            if (currentPage < DataManager.Instance.PoemData.poems[chapter].text.Count)
                text.text = DataManager.Instance.PoemData.poems[chapter].text[currentPage].textKor;

            totalPage = DataManager.Instance.PoemData.poems[chapter].text.Count; //���� ������ ��ġ�ε�
            
            if(totalPage == currentPage)
            {
                nextPage.gameObject.SetActive(false);
            }
        }
    }


    public void NextPage()
    {

        currentPage++;

        if (currentPage >= totalPage)
        {
            currentPage = totalPage - 1;
            return;
        }
        //���� �������� ���� ��쿡�� ��ư�� ���ֹ���
        if (currentPage + 1 >= totalPage)
        {
            nextPage.gameObject.SetActive(false);
        }

        prevPage.gameObject.SetActive(true);
        text.text = DataManager.Instance.PoemData.poems[chapter].text[currentPage].textKor;
    }

    public void PrevPage() 
    {

        currentPage--;

        if (currentPage < 0)
        {
            currentPage = 0;
            return;
        }
        if (currentPage - 1 < 0)
        {
            prevPage.gameObject.SetActive(false);
        }

        nextPage.gameObject.SetActive(true);
        text.text = DataManager.Instance.PoemData.poems[chapter].text[currentPage].textKor;
    }


    public void Exit()
    {
        //manager���� sleep ��û
        gameManager.GoSleep();
        Destroy(gameObject.transform.parent.gameObject);
    }

}
