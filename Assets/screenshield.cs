using UnityEngine;

public class ScreenShield : MonoBehaviour
{
    private static ScreenShield _instance;

    private void Awake()
    {
        _instance = this;

        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        transform.SetAsLastSibling();
    }

    public static void Set(bool on)
    {
        if (_instance == null)
        {
            return;
        }

        _instance.gameObject.SetActive(on);

        if (on)
        {
            _instance.transform.SetAsLastSibling();
        }
    }

    public static void On()
    {
        Set(true);
    }

    public static void Off()
    {
        Set(false);
    }
}
