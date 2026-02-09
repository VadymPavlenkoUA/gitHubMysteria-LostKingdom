using UnityEngine;

public class ChestVisual : MonoBehaviour
{
    public Animator animator;
    public AudioSource audioSource;
    public AudioClip openSound;
    public AudioClip closeSound;
    public AudioClip lockSound;

    public void Open()
    {
        Debug.Log(animator.gameObject.name);
        animator.SetBool("IsOpen", true);
        if (audioSource && openSound) audioSource.PlayOneShot(openSound);
    }

    public void Close()
    {
        Debug.Log(animator.gameObject.name);
        animator.SetBool("IsOpen", false);
        if (audioSource && closeSound) audioSource.PlayOneShot(closeSound);
    }

    public void LockSound()
    {
        audioSource.PlayOneShot(lockSound);
    }
}
