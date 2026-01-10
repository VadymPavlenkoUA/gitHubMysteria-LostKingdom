using System.Collections;
using UnityEngine;

public class AmbientZone : MonoBehaviour
{
    public AudioSource source;
    public float fadeTime = 2f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            StartCoroutine(Fade(0f, 1f));
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            StartCoroutine(Fade(1f, 0f));
    }

    IEnumerator Fade(float from, float to)
    {
        source.volume = from;
        source.Play();

        float t = 0;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            source.volume = Mathf.Lerp(from, to, t / fadeTime);
            yield return null;
        }

        if (to == 0f)
            source.Stop();
    }
}
