using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Moonnote : MonoBehaviour
{
    [SerializeField] GameObject MoonnoteUI;

    public Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        MoonnoteUI = Resources.Load<GameObject>("MoonnoteUI");
        animator = this.transform.GetChild(0).GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void NoteClick()
    {
        MoonnoteUI.SetActive(true);
        animator.SetBool("Light",false);
    }

    public void anion()
    {
        animator.SetBool("Light",true);
    }
}
