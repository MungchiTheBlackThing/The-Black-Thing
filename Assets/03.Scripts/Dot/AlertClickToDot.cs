using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlertClickToDot : MonoBehaviour
{
    public DotController.AlertType type;
    public DotController dot;

    private void OnMouseDown()
    {
        if (InputGuard.BlockWorldInput()) return;

        if (dot == null) return;
        dot.OnAlertClicked(type);
    }
}
