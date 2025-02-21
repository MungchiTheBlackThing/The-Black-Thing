using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuButton : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(menuon());
    }
    public IEnumerator menuon()
    {
        yield return new WaitForSeconds(0.2f);
        this.transform.parent.GetComponent<MenuController>().tuto();
    }

}
