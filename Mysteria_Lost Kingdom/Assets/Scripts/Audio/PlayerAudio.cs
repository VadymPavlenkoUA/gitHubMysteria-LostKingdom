using UnityEngine;


[System.Serializable]
public class FootstepSet
{
    public Surface surface;

    [Header("Movement")]
    public AudioClip[] walk;
    public AudioClip[] sprint;

    [Header("Actions")]
    public AudioClip[] jump;
    public AudioClip[] land;
    public AudioClip[] roll;
}

public enum SurfaceAction
{
    Walk,
    Sprint,
    Jump,
    Land,
    Roll
}

public class PlayerAudio : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource footstepsSource;
    public AudioSource actionSource;

    [Header("Footsteps")]
    public FootstepSet[] footstepSets;

    [Header("Actions")]
    public AudioClip rollClip;
    public AudioClip jumpClip;
    public AudioClip landClip;

    public void PlaySurfaceSound(Surface surface, SurfaceAction action)
    {
        FootstepSet fallback = null;

        foreach (var set in footstepSets)
        {
            if (set.surface == Surface.Default)
                fallback = set;

            if (set.surface == surface)
            {
                Play(set, action);
                return;
            }
        }

        if (fallback != null)
            Play(fallback, action);
    }

    void Play(FootstepSet set, SurfaceAction action)
    {
        AudioClip[] bank = null;
        AudioSource source = action == SurfaceAction.Walk || action == SurfaceAction.Sprint
            ? footstepsSource
            : actionSource;

        switch (action)
        {
            case SurfaceAction.Walk: bank = set.walk; break;
            case SurfaceAction.Sprint: bank = set.sprint; break;
            case SurfaceAction.Jump: bank = set.jump; break;
            case SurfaceAction.Land: bank = set.land; break;
            case SurfaceAction.Roll: bank = set.roll; break;
        }

        if (bank == null || bank.Length == 0) return;

        source.pitch = Random.Range(0.95f, 1.05f);
        source.PlayOneShot(bank[Random.Range(0, bank.Length)]);
    }

    //public void PlayFootstep(Surface surface, bool sprint)
    //{
    //    foreach (var set in footstepSets)
    //    {
    //        if (set.surface == surface)
    //        {
    //            var bank = sprint ? set.sprint : set.walk;
    //            if (bank.Length == 0) return;

    //            footstepsSource.PlayOneShot(
    //                bank[Random.Range(0, bank.Length)]
    //            );
    //            return;
    //        }
    //    }
    //}
    //public void PlayRoll()
    //{
    //    actionSource.PlayOneShot(rollClip);
    //}

    //public void PlayJump()
    //{
    //    actionSource.PlayOneShot(jumpClip);
    //}

    //public void PlayLand()
    //{
    //    actionSource.PlayOneShot(landClip);
    //}
}