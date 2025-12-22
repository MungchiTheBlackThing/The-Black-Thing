using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SubTutorial : MonoBehaviour
{
    public List<GameObject> Guideline = new List<GameObject>();
    public List<GameObject> zero = new List<GameObject>();
    [SerializeField] string dial = "앙증맞은 폴라로이드. 뭉치와 대화를 나누고\n나니 방이 가득 차 보이네. 기분 탓일까?";
    [SerializeField] MenuController MenuController;
    [SerializeField] GameObject menuBut;
    [SerializeField] GameObject progressBut;
    [SerializeField] ProgressUIController progressUIController;
    [SerializeField] GameObject icon;
    [SerializeField] GameObject Ch1;
    [SerializeField] GameObject DayProgress;
    [SerializeField] GameObject Subicon;
    [SerializeField] GameManager gameManager;
    [SerializeField] GameObject Screentouch;
    CanvasGroup tutorialMaskGroup;
    CanvasGroup Spider;
    CanvasGroup Progress;
    GameObject preparent;
    Vector3 originalPosition;

    int presibling;
    private bool G2 = false;
    private bool G3 = false;
    private bool G4 = false;
    private bool G5 = false;
    private bool G6 = false;
    int index = 0;
    // Start is called before the first frame update
    void Start()
    {
        MenuController.isprogress = false;
        progressUIController.guide1 = false;
        progressUIController.guide2 = false;
        Screentouch.SetActive(true);
        subtutoinit();
        index = 0;

        GameObject go;
        go = Instantiate(zero[gameManager.GetSITime], this.transform);
        go.transform.SetAsFirstSibling();
        go.name = "0";
        go.GetComponent<RectTransform>().anchoredPosition = new Vector3(1138, -453, 0);

        for (int i = 0; i < transform.childCount; i++)
        {
            Guideline.Add(transform.GetChild(i).gameObject);
            Guideline[i].SetActive(false);
        }
        tutorialMaskGroup = this.GetComponent<CanvasGroup>();
        Spider = menuBut.GetComponent<CanvasGroup>();
        Progress = progressBut.GetComponent<CanvasGroup>();
    }
    private void Update()
    {
        if (!G2 && MenuController.isOpening)
        {
            Guide2();
            G2 = true;  // 한 번 실행된 후에는 멈춤
        }

        if (!G3 && MenuController.isprogress)
        {
            StartCoroutine(Guide3());
            G3 = true;
            MenuController.isprogress = false;
        }

        if (!G5 && progressUIController.guide2)
        {
            Guide5();
            G5 = true;
            progressUIController.guide2 = false;
        }
    }
    public void subtutoinit()
    {
        G2 = false;
        G3 = false;
        G4 = false;
        G5 = false;
        G6 = false;
    }

    public void guidestart()
    {
        StartCoroutine(guide());
    }
    public IEnumerator guide()
    {
        yield return new WaitForSeconds(0.5f);
        Guide0();
    }
    public void Guide0()
    {
        Guideline[0].SetActive(true);

        this.GetComponent<Image>().enabled = false;
        this.GetComponent<Button>().interactable = false;

        Guideline[0].transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = dial;
        Guideline[0].GetComponent<Button>().onClick.AddListener(() => Guide1());
    }

    public void Guide1()
    {
        this.GetComponent<Image>().enabled = true;
        this.GetComponent<Button>().interactable = true;

        index = 0;
        Guideline[index].SetActive(false);
        index++;
        Guideline[index].SetActive(true);

        preparent = menuBut.transform.parent.gameObject;
        presibling = 1;
        Debug.Log("Guide1");
        menuBut.transform.SetParent(this.transform);
        menuBut.transform.SetAsLastSibling();
    }


    public void Guide2()
    {
        index = 1;
        Guideline[index].SetActive(false);
        index++;
        Guideline[index].SetActive(true);
        menuBut.transform.SetParent(preparent.transform);
        menuBut.transform.SetSiblingIndex(presibling);
        preparent = progressBut.transform.parent.gameObject;
        progressBut.transform.SetParent(this.transform);
        progressBut.transform.SetAsLastSibling();
        G2 = true;
        Debug.Log("Guide2");
    }

    public IEnumerator Guide3()
    {
        index = 2;
        Guideline[index].SetActive(false);
        index++;
        Guideline[index].SetActive(true);
        progressBut.transform.SetParent(preparent.transform);
        progressBut.transform.SetSiblingIndex(presibling);
        yield return new WaitForSeconds(0.1f);
        originalPosition = icon.transform.GetChild(0).position;
        Ch1 = Instantiate(icon.transform.GetChild(0).gameObject);
        Ch1.transform.SetParent(this.transform);
        Ch1.transform.SetAsLastSibling();
        Ch1.transform.position = originalPosition;
        Ch1.GetComponent<Button>().onClick.AddListener(progressUIController.onClickdragIcon);
        Debug.Log("Guide3");
    }
    public void Guide5()
    {
        index = 3;
        Guideline[index].SetActive(false);
        index++;
        Guideline[index].SetActive(true);
        Ch1.SetActive(false);
        Destroy(Ch1);
        preparent = Subicon.transform.parent.gameObject;
        Subicon.transform.SetParent(this.transform);
        Subicon.transform.SetAsLastSibling();
        Debug.Log("Guide5");
    }

    public void Guide6()
    {
        if (index >= 4)  
        {
            if (index < Guideline.Count)
            {
                Guideline[index].SetActive(false);  
            }

            index++; 

            if (index < Guideline.Count) 
            {
                Guideline[index].SetActive(true);
            }
            if (index >= Guideline.Count)
            {
                Subicon.transform.SetParent(preparent.transform);
                Subicon.transform.SetSiblingIndex(3);
                Screentouch.SetActive(false);
                this.gameObject.SetActive(false);
                return;
            }
            Debug.Log(index);
        }
    }
}
