using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITutorial : MonoBehaviour
{
    public List<GameObject> Guideline = new List<GameObject>();
    [SerializeField] MenuController MenuController;
    [SerializeField] GameObject menuBut;
    [SerializeField] GameObject progressBut;
    CanvasGroup tutorialMaskGroup;
    CanvasGroup Spider;
    CanvasGroup Progress;
    GameObject preparent;
    int presibling;
    private bool G2 = false;
    private bool G3 = false;
    int index = 0;
    // Start is called before the first frame update
    void Start()
    {
        index = 0;
        for (int i = 0; i < transform.childCount; i++)
        {
            Guideline.Add(transform.GetChild(i).gameObject);
        }
        tutorialMaskGroup = this.GetComponent<CanvasGroup>();
        Spider = menuBut.GetComponent<CanvasGroup>();
        Progress = progressBut.GetComponent<CanvasGroup>();

        StartCoroutine(guide());
    }
    private void Update()
    {
        if (!G2 && MenuController.isOpening)
        {
            Guide2();
            G2 = true;  // ÇÑ ¹ø ½ÇÇàµÈ ÈÄ¿¡´Â ¸ØÃã
        }

        if (!G3 && MenuController.isprogress)
        {
            Guide3();
            G3 = true;
            MenuController.isprogress = false;
        }
    }
    IEnumerator guide()
    {
        yield return new WaitForSeconds(2f);
        Guideline[0].SetActive(true);
        Guide1();
    }
    public void Guide1()
    {
        preparent = menuBut.transform.parent.gameObject;
        presibling = 1;
        Debug.Log("Guide1");
        menuBut.transform.SetParent(this.transform);
        menuBut.transform.SetAsLastSibling();
    }

    public void Guide2()
    {
        Guideline[index].SetActive(false);
        index++;
        Guideline[index].SetActive(true);
        menuBut.transform.SetParent(preparent.transform);
        menuBut.transform.SetSiblingIndex(presibling);
        preparent = progressBut.transform.parent.gameObject;
        progressBut.transform.SetParent(this.transform);
        progressBut.transform.SetAsLastSibling();
        Debug.Log("Guide2");
    }

    public void Guide3()
    {
        Guideline[index].SetActive(false);
        index++;
        Guideline[index].SetActive(true);
        progressBut.transform.SetParent(preparent.transform);
        progressBut.transform.SetSiblingIndex(presibling);
        Debug.Log("Guide3");
    }
    public void Guide4()
    {

    }
    public void Guide5()
    {
        if (index >= 6 && index < transform.childCount -1)
        {
            Guideline[index].SetActive(false);
            index++;
            Guideline[index].SetActive(true);
            Debug.Log(index);
        }
        else if(index>=transform.childCount -1)
        {
            this.gameObject.SetActive(false);
        }
    }


}
