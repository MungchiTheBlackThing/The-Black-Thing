using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MoonnoteUI : MonoBehaviour
{
    [Header("Page State")]
    [SerializeField] int currentPage = 0; // 현재 페이지
    [SerializeField] int totalPages = 5; // 전체 페이지

    [Header("Page Buttons")]
    [SerializeField] GameObject prevButton; //드래그해야 함...
    [SerializeField] GameObject nextButton;
    [SerializeField] GameObject Menu;

    private MenuController menuController;

    private DotController dotController;

    [SerializeField]
    GameManager gameManager;

    private Moonnote Moonnote;

    public bool ischeck = false;

    [SerializeField]
    GameObject exitbut;
    // Start is called before the first frame update
    void Start()
    {
        Menu = GameObject.FindWithTag("Menu");
        gameManager = GameObject.FindWithTag("GameController").GetComponent<GameManager>();
        
        dotController = GameObject.FindWithTag("DotController").GetComponent<DotController>();
        menuController = Menu.GetComponent<MenuController>();

        try
        {
            Moonnote = GameObject.FindWithTag("moonnote").GetComponent<Moonnote>();
            if (Moonnote != null)
            {
                ischeck = false;
            }
            else
            {
                Debug.Log("메인이거나 한번 체크했을 경우");
                ischeck = true;
            }
        }
        catch (System.Exception ex)
        {
            Debug.Log("한번 봤거나 메인일 경우");
            ischeck = true;
        }

        if (ischeck)
        {
            exitbut.SetActive(true);
        }
        else
        {
            exitbut.SetActive(false);
        }
        UpdatePageButtons();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Exit()
    {
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.buttonClick, this.transform.position);
        var scene = SceneManager.GetActiveScene().name;

        // 1) 메인(또는 튜토가 아닌 모든 씬): 그냥 닫기만
        if (scene != "Tutorial")
        {
            gameObject.SetActive(false);
            return;
        }

        // 2) 튜토 씬: 복귀 정리 후 닫기
        menuController?.onlyskipoff();
        menuController?.LaterON();
        menuController?.tuto();
        gameManager?.ScrollManager?.scrollable();
        Moonnote?.disappear();

        ischeck = true;
        gameObject.SetActive(false);
    }

    void UpdatePageButtons()
    {
        // 첫 페이지
        if (currentPage <= 0)
        {
            prevButton.SetActive(false);
            nextButton.SetActive(true);
        }
        // 마지막 페이지
        else if (currentPage >= totalPages - 1)
        {
            prevButton.SetActive(true);
            nextButton.SetActive(false);
        }
        // 중간
        else
        {
            prevButton.SetActive(true);
            nextButton.SetActive(true);
        }
    }
    public void NextPage()
    {
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.buttonClick, this.transform.position);
        if (currentPage >= totalPages - 1) return;
        currentPage++;
        UpdatePageButtons();
    }

    public void PreviousPage()
    {
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.buttonClick, this.transform.position);
        if (currentPage <= 0) return;
        currentPage--;
        UpdatePageButtons();
    }


   
}
