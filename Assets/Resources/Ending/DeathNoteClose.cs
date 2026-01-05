using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathNoteClose : MonoBehaviour
{
    public void OnClose()
    {
        DeathNoteClick.readDeathnote = true;

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

