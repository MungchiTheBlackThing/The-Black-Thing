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
        yield return new WaitForSeconds(3f); //�������� �����ͺ��̽� ȣ�� 
        //���̵�
        SceneManager.LoadScene("01.Scenes/MainScene");
    }
}
