using UnityEngine;
using FMODUnity;

public class MoonBubbleFX : MonoBehaviour
{
    [SerializeField] private EventReference revealSfx;
    [SerializeField] private Animator animator;

    private static readonly int ShowHash = Animator.StringToHash("Show");

    private void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
    }

    public void PlayReveal()
    {
        if (!revealSfx.IsNull)
            AudioManager.Instance.PlayOneShot(revealSfx, transform.position);

        if (animator != null)
            animator.SetTrigger(ShowHash);
    }
}
