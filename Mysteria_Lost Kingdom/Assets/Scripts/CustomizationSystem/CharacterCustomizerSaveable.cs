using UnityEngine;

[RequireComponent(typeof(CharacterCustomizer))]
[RequireComponent(typeof(SaveableEntity))]
public class CharacterCustomizerSaveable : MonoBehaviour, ISaveable
{
    private CharacterCustomizer customizer;
    private SaveableEntity saveable;

    private void Awake()
    {
        customizer = GetComponent<CharacterCustomizer>();
        saveable = GetComponent<SaveableEntity>();
        SaveManager.Instance?.Register(saveable, this);
    }

    private void OnDestroy()
    {
        SaveManager.Instance?.Unregister(saveable);
    }

    public string GetSaveID() => saveable.ID;
    public object CaptureState()
    {
        return customizer.GetCustomizationData();
    }
    public void RestoreState(object state)
    {
        if (state is CharacterCustomizationData data)
        {
            customizer.ApplyCustomization(data);
        }
    }
}