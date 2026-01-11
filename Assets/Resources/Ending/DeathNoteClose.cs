using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathNoteClose : MonoBehaviour
{
    public void OnClose()
    {
        // 1) 런타임 플래그
        DeathNoteClick.readDeathnote = true;

        // 2) 영구 저장 (PlayerInfo)
        var pc = GameObject.Find("PlayerController")?.GetComponent<PlayerController>();
        if (pc != null)
        {
            pc.GetPlayerInfo().deathnoteState = 1;
            pc.SavePlayerInfo();
        }

        var menu = GameObject.Find("Menu")?.GetComponent<MenuController>();
        if (menu != null)
        {
            menu.ApplyEndingOverride();
            menu.StartCoroutine(ApplyNextFrame(menu));
        }

        Destroy(gameObject);
    }

    private IEnumerator ApplyNextFrame(MenuController menu)
    {
        yield return null; // 다음 프레임
        menu.ApplyEndingOverride();
    }
}

