using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SaveableEntity : MonoBehaviour
{
    [SerializeField] private string uniqueID;

    public string ID => uniqueID;

    private ISaveable saveable;


    private void Awake()
    {
        if (string.IsNullOrEmpty(uniqueID))
        {
            GenerateID();
        }

        saveable = GetComponent<ISaveable>();
        if (saveable == null)
        {
            Debug.LogWarning($"[SaveableEntity] {gameObject.name} не має ISaveable компонента!");
        }
    }

    private void OnEnable()
    {
        SaveManager.Instance?.Register(this, saveable);
    }

    private void OnDisable()
    {
        SaveManager.Instance?.Unregister(this);
    }

    public void GenerateID()
    {
        uniqueID = System.Guid.NewGuid().ToString();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        //if (Application.isPlaying) return;

        //if (string.IsNullOrEmpty(uniqueID))
        //{
        //    uniqueID = System.Guid.NewGuid().ToString();
        //    EditorUtility.SetDirty(this);
        //}
    }
#endif
}