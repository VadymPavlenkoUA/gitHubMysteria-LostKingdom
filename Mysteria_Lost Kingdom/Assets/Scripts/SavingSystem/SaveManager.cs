using System.IO;
using System.Linq;
using UnityEngine;
using System;
using System.Collections.Generic;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    private string savePath;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        savePath = Path.Combine(Application.persistentDataPath, "Saves");
        if (!Directory.Exists(savePath)) Directory.CreateDirectory(savePath);
    }

    public void SaveGame(int slot)
    {
        var saveables = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<ISaveable>();

        SaveGameWrapper wrapper = new()
        {
            saveVersion = 1,
            saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };

        foreach (var saveable in saveables)
        {
            object state = saveable.CaptureState();
            string json = JsonUtility.ToJson(state);

            wrapper.objects.Add(new SaveObjectEntry
            {
                id = saveable.GetSaveID(),
                json = json,
                type = state.GetType().AssemblyQualifiedName
            });
        }

        string path = GetSaveFilePath(slot);
        File.WriteAllText(path, JsonUtility.ToJson(wrapper, true));

        Debug.Log($"[SAVE] Slot {slot} saved");
    }

    public void LoadGame(int slot)
    {
        string path = GetSaveFilePath(slot);
        if (!File.Exists(path))
        {
            Debug.LogWarning("Save file not found");
            return;
        }

        string json = File.ReadAllText(path);
        var wrapper = JsonUtility.FromJson<SaveGameWrapper>(json);

        var saveables = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<ISaveable>();

        foreach (var entry in wrapper.objects)
        {
            var saveable = saveables.FirstOrDefault(s => s.GetSaveID() == entry.id);
            if (saveable == null) continue;

            Type type = Type.GetType(entry.type);
            object data = JsonUtility.FromJson(entry.json, type);

            saveable.RestoreState(data);
        }

        Debug.Log($"[LOAD] Slot {slot} loaded");
    }

    public List<SaveSlotInfo> GetAllSaves()
    {
        List<SaveSlotInfo> list = new();

        for (int i = 0; i < 10; i++)
        {
            string path = GetSaveFilePath(i);
            if (!File.Exists(path))
            {
                list.Add(new SaveSlotInfo(i, false, "", ""));
                continue;
            }

            var wrapper = JsonUtility.FromJson<SaveGameWrapper>(
                File.ReadAllText(path));

            list.Add(new SaveSlotInfo(
                i,
                true,
                wrapper.saveTime,
                path
            ));
        }

        return list;
    }

    private string GetSaveFilePath(int slot)
    {
        return Path.Combine(savePath, $"save_{slot}.json");
    }
}
