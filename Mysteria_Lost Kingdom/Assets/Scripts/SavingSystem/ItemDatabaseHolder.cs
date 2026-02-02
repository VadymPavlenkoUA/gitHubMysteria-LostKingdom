using UnityEngine;

public class ItemDatabaseHolder : MonoBehaviour
{
    public static ItemDatabaseHolder Instance { get; private set; }

    public ItemDatabase database;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        database.Init();
    }

    public Item GetItem(string id)
    {
        return database.Get(id);
    }
}
