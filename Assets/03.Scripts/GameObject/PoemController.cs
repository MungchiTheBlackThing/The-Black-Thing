using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
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

    [SerializeField]
    GameObject DotText;
    const string path = "Background/PoemBackground/";

    //현재 타임이 밤인지, 낮인지는 GameManager가 가지고있음
    GameManager gameManager;
    public PlayAnswerController playAnswerController;

    private bool hasShownDotText = false;
    int currentPage;
    int totalPage;
    int chapter;

    private const string poemTableName = "PoemText";


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
        currentPage = 0; //뜰 때 마다 첫번째 페이지로
        StartCoroutine(LoadPoem());
    }


    private IEnumerator LoadPoem()
    {
        yield return LocalizationSettings.InitializationOperation;

        //시 내용을 업데이트
        chapter = gameManager.Chapter;

        totalPage = GetTotalPages(chapter);

        LoadPageLocalized(currentPage);
    }

    private int GetTotalPages(int chapter)
    {
        StringTable table = LocalizationSettings.StringDatabase.GetTable(poemTableName);
        if (table != null)
        {
            int page = 1;
            while (true)
            {
                string key = $"PT{chapter}_L{page:0000}";
                if (table.GetEntry(key) == null)
                    break;
                page++;
            }
            return page - 1;
        }
        return 0;
    }


    private void LoadPageLocalized(int pageIndex)
    {
        string key = $"PT{chapter}_L{pageIndex + 1:0000}";
        StringTable table = LocalizationSettings.StringDatabase.GetTable(poemTableName);

        if (table != null)
        {
            var entry = table.GetEntry(key);
            if (entry != null)
            {
                text.text = entry.GetLocalizedString();
            }
            else
            {
                text.text = "(Missing Translation)";
            }
        }

        bool isFirstPage = pageIndex == 0;
        bool isLastPage = (pageIndex + 1 >= totalPage);

        if (totalPage == 1)
        {
            nextPage.gameObject.SetActive(!hasShownDotText);
            prevPage.gameObject.SetActive(false);
        }
        else
        {
            nextPage.gameObject.SetActive(!isLastPage);
            prevPage.gameObject.SetActive(!isFirstPage);
        }
    }


    public void NextPage()
    {
        if (totalPage == 1)
        {
            if (!hasShownDotText)
            {
                DotTextOn();
                hasShownDotText = true;
            }

            nextPage.gameObject.SetActive(false);
            return;
        }
        currentPage++;

        if (currentPage >= totalPage)
        {
            currentPage = totalPage - 1;
            return;
        }

        if (currentPage + 1 >= totalPage)
        {
            if (!hasShownDotText)
            {
                DotTextOn();
                hasShownDotText = true;
            }

            nextPage.gameObject.SetActive(false);
        }

        prevPage.gameObject.SetActive(true);
        LoadPageLocalized(currentPage);
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
        LoadPageLocalized(currentPage);
    }

    private void DotTextOn()
    {
        DotText.SetActive(true);
        DotText.GetComponent<DotTextReview>().StartReview();
    }

    public void Exit()
    {
        playAnswerController = GameObject.FindWithTag("PlayAnswerController").GetComponent<PlayAnswerController>();
        playAnswerController.AfterReadingPoem();
        Destroy(gameObject.transform.parent.gameObject);
        hasShownDotText = false;
    }

}
