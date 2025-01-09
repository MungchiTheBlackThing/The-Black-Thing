using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoonnoteUI : MonoBehaviour
{
    [SerializeField] GameObject Menu;

    private MenuController menuController;

    private DotController dotController;
    // Start is called before the first frame update
    void Start()
    {
        Menu = GameObject.FindWithTag("Menu");
        dotController = GameObject.FindWithTag("DotController").GetComponent<DotController>();
        menuController = Menu.GetComponent<MenuController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Exit()
    {
        if (dotController.tutorial)
        {
            menuController.skipon();
            menuController.tuto();
        }
        
        this.gameObject.SetActive(false);
    }
}
