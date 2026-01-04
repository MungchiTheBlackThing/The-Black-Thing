using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathNoteClick : MonoBehaviour
{ 
    [SerializeField]
    PlayerController player;
    [SerializeField]
    GameObject _deathnote;

    [SerializeField]
    GameObject canvas;

    [SerializeField]
    MenuController menu;
    public static bool readDeathnote = false;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("PlayerController").GetComponent<PlayerController>();
        menu = GameObject.Find("Menu").GetComponent<MenuController>();
        canvas = GameObject.Find("Canvas");
    }

    private void OnMouseDown()
    {
        Onclick();
    }
    // Update is called once per frame
    public void Onclick()
    {

        string resourcePath = (player.GetSunMoon().sun >= player.GetSunMoon().moon)
            ? "Ending/Sun_deathnote"
            : "Ending/Moon_deathnote";

        _deathnote = Instantiate(Resources.Load<GameObject>(resourcePath), canvas.transform);

        if (_deathnote == null)
        {
            Debug.LogError($"Error: {resourcePath} ���ҽ��� ã�� �� �����ϴ�!");
            return;
        }

        // �ڽ� ������Ʈ�� ���� ��쿡�� ����
        if (this.transform.childCount > 0)
        {
            Destroy(this.transform.GetChild(0).gameObject);
        }

        _deathnote.SetActive(true);
    }

}