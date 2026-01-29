using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SubTutorial : MonoBehaviour
{
    public List<GameObject> Guideline = new List<GameObject>();

    [SerializeField] MenuController MenuController;
    [SerializeField] GameObject menuBut;
    [SerializeField] GameObject progressBut;
    [SerializeField] ProgressUIController progressUIController;
    [SerializeField] GameObject icon;
    [SerializeField] GameObject Ch1;
    [SerializeField] GameObject DayProgress;
    [SerializeField] GameObject Subicon;
    [SerializeField] GameManager gameManager;

    [SerializeField] private GameObject guide0BubbleRoot;
    [SerializeField] private Button guide0Button;
    [SerializeField] private TextMeshProUGUI guide0Text;
    CanvasGroup tutorialMaskGroup;
    CanvasGroup Spider;
    CanvasGroup Progress;
    GameObject preparent;
    Vector3 originalPosition;


    int presibling;
    private Coroutine guide0UnlockRoutine;
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
        ScreenShield.On();
        subtutoinit();
        index = 0;

        Guideline.RemoveAll(g => g == null);

        for (int i = 0; i < Guideline.Count; i++)
        {
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
            G2 = true;  // �� �� ����� �Ŀ��� ����
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
        this.GetComponent<Image>().enabled = false;
        Guide0();
    }
    public IEnumerator guide()
    {
        yield return new WaitForSeconds(1f);
        Guide0();
    }
    public void Guide0() // 앙증맞은 폴라로이드네
    {
        index = 0;
        InputGuard.WorldInputLocked = true; 
        
        GameObject step0 = Guideline[0];
        step0.SetActive(true);

        // CanvasGroup 확보
        CanvasGroup cg = step0.GetComponent<CanvasGroup>();
        if (cg == null) cg = step0.AddComponent<CanvasGroup>();

        // Button 확보
        Button b = step0.GetComponent<Button>();
        b.onClick.RemoveAllListeners();
        b.onClick.AddListener(Guide1);

        // 연출 - 딜레이 후 클릭 가능
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;
        b.interactable = false;

        if (guide0UnlockRoutine != null)
            StopCoroutine(guide0UnlockRoutine);

        StartCoroutine(FadeInGuide0(cg, b, 0.6f));
    }
    private IEnumerator FadeInGuide0(CanvasGroup cg, Button b, float duration)
    {
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Clamp01(t / duration);
            yield return null;
        }

        // 완전히 보인 뒤에만 클릭 가능
        cg.alpha = 1f;
        cg.interactable = true;
        cg.blocksRaycasts = true;
        b.interactable = true;
    }


    public void Guide1() // 다시 이쪽으로 이쪽으로
    {
        var btn = menuBut.GetComponent<Button>();
        if (btn != null)
            btn.interactable = true;
        // Guide0(폴라로이드) 끄고 다음 단계로
        Guideline[index].SetActive(false);
        index++;
        Guideline[index].SetActive(true);

        this.GetComponent<Image>().enabled = true;
        this.GetComponent<Button>().interactable = true;

        preparent = menuBut.transform.parent.gameObject;
        presibling = 1;
        Debug.Log("Guide1");
        menuBut.transform.SetParent(this.transform);
        menuBut.transform.SetAsLastSibling();
    }


    public void Guide2() // 여기다 여기
    {
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

    public IEnumerator Guide3() // 클릭 클릭 클릭
    { 
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
                ScreenShield.Off();
                SubDialogue.isSubmoldtutoend = true;
                InputGuard.WorldInputLocked = false; 
                this.gameObject.SetActive(false);
                var menu = gameManager?.Menu ?? FindObjectOfType<MenuController>(true);
                menu?.allon();
                return;
            }
            Debug.Log(index);
        }
    }
}
