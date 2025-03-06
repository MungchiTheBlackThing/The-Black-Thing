using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathNoteClick : MonoBehaviour
{
    public static bool checkdeath = false;
    public static int SUN_COUNT = 0;
    public static int MOON_COUNT = 0;
    [SerializeField]
    GameObject _deathnote;

    [SerializeField]
    GameObject canvas;

    [SerializeField]
    MenuController menu;
    // Start is called before the first frame update
    void Start()
    {
        menu = GameObject.Find("Menu").GetComponent<MenuController>();
        canvas = GameObject.Find("Canvas");
    }

    // Update is called once per frame
    public void Onclick()
    {
        if (!checkdeath)
        {
            if (SUN_COUNT >= MOON_COUNT)
            {
                _deathnote = Instantiate(Resources.Load<GameObject>("Sun_deathnote"), canvas.transform);
            }
            else
            {
                _deathnote = Instantiate(Resources.Load<GameObject>("Moon_deathnote"), canvas.transform);
            }
            Destroy(this.transform.GetChild(0).gameObject);
        }
        checkdeath = true;
        _deathnote.SetActive(true);
        menu.replayON();
    }
}