using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ButtonSwap : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public TMP_Text buttonText; // È¤Àº TMP_Text
    public Color normalColor = Color.white;
    public Color pressedColor = Color.black;

    public void OnPointerDown(PointerEventData eventData)
    {
        buttonText.color = pressedColor;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        buttonText.color = normalColor;
    }
}