using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoonnoteUI : MonoBehaviour
{
    [SerializeField] GameObject Menu;

    private MenuController menuController;
    // Start is called before the first frame update
    void Start()
    {
        Menu = GameObject.FindWithTag("Menu");
        menuController = Menu.GetComponent<MenuController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Exit()
    {
        menuController.onlyskipoff();
        menuController.tuto();
        this.gameObject.SetActive(false);
    }
}
