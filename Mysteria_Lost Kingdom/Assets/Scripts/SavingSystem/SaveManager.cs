using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    private string savePath;

    private Dictionary<string, ISaveable> saveables = new();

    public event Action<int> OnSaveCompleted;

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

    public void Register(SaveableEntity entity, ISaveable saveable)
    {
        if (entity == null || saveable == null) return;
        saveables[entity.ID] = saveable;
    }

    public void Unregister(SaveableEntity entity)
    {
        if (entity == null) return;
        saveables.Remove(entity.ID);
    }

    public void SaveGame(int slot)
    {
        StartCoroutine(SaveGameRoutine(slot));
    }

    private IEnumerator SaveGameRoutine(int slot)
    {
        yield return CaptureScreenshot(slot);

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

        if (DroppedItemRegistry.Instance != null)
        {
            wrapper.droppedItems = DroppedItemRegistry.Instance.Capture();
            Debug.Log($"[SAVE] Dropped items count: {wrapper.droppedItems?.Count}");
        }

        string path = GetSaveFilePath(slot);
        File.WriteAllText(path, JsonUtility.ToJson(wrapper, true));

        OnSaveCompleted?.Invoke(slot);
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

        foreach (var pickup in FindObjectsByType<ItemPickup>(FindObjectsSortMode.None))
        {
            pickup.ForceHide();
        }

        var saveables = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<ISaveable>();

        foreach (var entry in wrapper.objects)
        {
            var saveable = saveables.FirstOrDefault(s => s.GetSaveID() == entry.id);
            if (saveable == null) continue;

            Type type = Type.GetType(entry.type);
            object data = JsonUtility.FromJson(entry.json, type);

            saveable.RestoreState(data);
        }

        if (DroppedItemRegistry.Instance != null && wrapper.droppedItems != null)
        {
            DroppedItemRegistry.Instance.Restore(wrapper.droppedItems);
            DroppedItemRegistry.Instance.RestoreAll();
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

    private IEnumerator CaptureScreenshot(int slot)
    {
        yield return new WaitForEndOfFrame();

        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("No camera for screenshot");
            yield break;
        }

        int width = 512;
        int height = 288;

        RenderTexture rt = new RenderTexture(width, height, 24);
        cam.targetTexture = rt;

        Texture2D screenshot = new Texture2D(width, height, TextureFormat.RGB24, false);

        cam.Render();
        RenderTexture.active = rt;
        screenshot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        screenshot.Apply();

        cam.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);

        byte[] bytes = screenshot.EncodeToPNG();
        Destroy(screenshot);

        string path = GetScreenshotPath(slot);
        File.WriteAllBytes(path, bytes);

        Debug.Log($"[SAVE] Screenshot saved: {path}");
    }


    private string GetScreenshotPath(int slot)
    {
        return Path.Combine(savePath, $"save_{slot}.png");
    }
}
