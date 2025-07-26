using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SplashController : MonoBehaviour
{
    // Start is called before the first frame updat

    public void EndAnimation()
    {
        StartCoroutine("GoDialogue");
    }

    IEnumerator GoDialogue()
    {
        yield return new WaitForSeconds(3f); //다음에는 데이터베이스 호출 
        //씬이동
        SceneManager.LoadScene("01.Scenes/MainScene"); 
    }
}
