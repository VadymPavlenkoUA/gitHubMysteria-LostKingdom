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

[System.Serializable]
public class ItemSoundSet
{
    public ItemCategory itemCategory;

    public AudioClip[] equip;
    public AudioClip[] unequip;
    public AudioClip[] pickup;
}

[System.Serializable]
public class CombatSoundSet
{
    public BlockType blockType;

    [Header("Combat")]
    public AudioClip[] attack;
    public AudioClip[] block;
}

public enum SurfaceAction
{
    Walk,
    Sprint,
    Jump,
    Land,
    Roll
}

public enum ItemAction
{
    Equip,
    Unequip,
    Pickup
}

public enum CombatAction
{
    Attack,
    Block
}

public enum BlockType
{
    None,
    WeaponOneHand,
    WeaponTwoHand,
    Shield
}

public class PlayerAudio : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource footstepsSource;
    public AudioSource actionSource;
    public AudioSource itemSource;

    [Header("Footsteps / Actions")]
    public FootstepSet[] footstepSets;

    [Header("Item Audio")]
    public ItemSoundSet[] itemSoundSets;

    [Header("Combat Audio")]
    public CombatSoundSet[] combatSoundSets;

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

    public void PlayItemSound(ItemCategory type, ItemAction action)
    {
        ItemSoundSet fallback = null;

        foreach (var set in itemSoundSets)
        {
            if (set.itemCategory == ItemCategory.None)
                fallback = set;

            if ((type & set.itemCategory) != 0)
            {
                PlayItem(set, action);
                return;
            }
        }

        if (fallback != null)
            PlayItem(fallback, action);
    }

    void PlayItem(ItemSoundSet set, ItemAction action)
    {
        AudioClip[] bank = null;

        switch (action)
        {
            case ItemAction.Equip: bank = set.equip; break;
            case ItemAction.Unequip: bank = set.unequip; break;
            case ItemAction.Pickup: bank = set.pickup; break;
        }

        if (bank == null || bank.Length == 0) return;

        itemSource.pitch = Random.Range(0.95f, 1.05f);
        itemSource.PlayOneShot(bank[Random.Range(0, bank.Length)]);
    }

    public void PlayCombatSound(BlockType blockType, CombatAction action)
    {
        CombatSoundSet fallback = null;

        foreach (var set in combatSoundSets)
        {
            if (set.blockType == BlockType.None)
                fallback = set;

            if (set.blockType == blockType)
            {
                PlayCombat(set, action);
                return;
            }
        }

        if (fallback != null)
            PlayCombat(fallback, action);
    }


    void PlayCombat(CombatSoundSet set, CombatAction action)
    {
        AudioClip[] bank = null;

        switch (action)
        {
            case CombatAction.Attack: bank = set.attack; break;
            case CombatAction.Block: bank = set.block; break;
        }

        if (bank == null || bank.Length == 0) return;

        actionSource.pitch = Random.Range(0.95f, 1.05f);
        actionSource.PlayOneShot(bank[Random.Range(0, bank.Length)]);
    }


}