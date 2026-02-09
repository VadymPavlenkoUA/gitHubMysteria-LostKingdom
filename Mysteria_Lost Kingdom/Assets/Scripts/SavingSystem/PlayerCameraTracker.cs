using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(SaveableEntity))]
public class PlayerCameraTracker : MonoBehaviour, ISaveable
{
    public static PlayerCameraTracker Instance;

    public CinemachineOrbitalFollow orbitalFollow;

    private SaveableEntity saveableEntity;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        saveableEntity = GetComponent<SaveableEntity>();
        if (saveableEntity == null)
        {
            Debug.LogError("[PlayerCameraTracker] Missing SaveableEntity!");
        }
    }

    public string GetSaveID() => saveableEntity.ID;

    public object CaptureState()
    {
        if (orbitalFollow == null) return null;

        return new CameraSaveData
        {
            horizontalAxis = orbitalFollow.HorizontalAxis.Value,
            verticalAxis = orbitalFollow.VerticalAxis.Value
        };
    }

    public void RestoreState(object state)
    {
        if (orbitalFollow == null || state == null) return;

        var data = (CameraSaveData)state;

        orbitalFollow.HorizontalAxis.Value = data.horizontalAxis;
        orbitalFollow.VerticalAxis.Value = data.verticalAxis;
    }
}
