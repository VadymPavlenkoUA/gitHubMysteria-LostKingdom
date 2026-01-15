using System.Collections;
using UnityEngine;

public class AmbientZone : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource daySource;
    public AudioSource nightSource;

    [Header("Settings")]
    public float fadeTime = 1f;

    private bool playerInside;

    private Coroutine dayFadeRoutine;
    private Coroutine nightFadeRoutine;

    private void Start()
    {
        daySource.volume = 0f;
        nightSource.volume = 0f;

        daySource.loop = true;
        nightSource.loop = true;

        TimeOfDayManager.Instance.OnDayStarted += OnDayStarted;
        TimeOfDayManager.Instance.OnNightStarted += OnNightStarted;
    }

    private void OnDestroy()
    {
        if (TimeOfDayManager.Instance == null) return;

        TimeOfDayManager.Instance.OnDayStarted -= OnDayStarted;
        TimeOfDayManager.Instance.OnNightStarted -= OnNightStarted;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = true;
        UpdateAmbient();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = false;
        FadeOutAll();
    }

    void OnDayStarted()
    {
        if (playerInside)
            CrossFade(nightSource, daySource);
    }

    void OnNightStarted()
    {
        if (playerInside)
            CrossFade(daySource, nightSource);
    }

    void UpdateAmbient()
    {
        if (TimeOfDayManager.Instance.IsNight)
            CrossFade(daySource, nightSource);
        else
            CrossFade(nightSource, daySource);
    }

    void FadeOutAll()
    {
        Fade(daySource, 0f, ref dayFadeRoutine);
        Fade(nightSource, 0f, ref nightFadeRoutine);
    }

    void CrossFade(AudioSource from, AudioSource to)
    {
        if (from == daySource)
            Fade(daySource, 0f, ref dayFadeRoutine);
        else
            Fade(nightSource, 0f, ref nightFadeRoutine);

        if (to == daySource)
            Fade(daySource, 1f, ref dayFadeRoutine);
        else
            Fade(nightSource, 1f, ref nightFadeRoutine);
    }

    void Fade(AudioSource source, float target, ref Coroutine routine)
    {
        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(FadeRoutine(source, target));
    }

    IEnumerator FadeRoutine(AudioSource source, float target)
    {
        if (!source.isPlaying && target > 0f)
            source.Play();

        float start = source.volume;
        float t = 0f;

        while (t < fadeTime)
        {
            t += Time.deltaTime;
            source.volume = Mathf.Lerp(start, target, t / fadeTime);
            yield return null;
        }

        source.volume = target;

        if (target == 0f)
            source.Stop();
    }
}
