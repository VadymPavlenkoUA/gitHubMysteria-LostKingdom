using System.IO;
using UnityEngine;

public class SaveLoadMenu : MonoBehaviour
{
    public Transform contentParent;
    public SaveSlotUI slotPrefab;
    public SaveInfoPanel infoPanel;

    private void OnEnable()
    {
        SaveManager.Instance.OnSaveCompleted += OnSaveCompleted;
        Refresh();
    }

    private void OnDisable()
    {
        if (SaveManager.Instance != null) SaveManager.Instance.OnSaveCompleted -= OnSaveCompleted;
    }

    public void Refresh()
    {
        foreach (Transform child in contentParent) Destroy(child.gameObject);

        var saves = SaveManager.Instance.GetAllSaves();

        foreach (var save in saves)
        {
            var slot = Instantiate(slotPrefab, contentParent);
            slot.Setup(save);

            slot.OnSlotClicked += ShowSlotInfo;
        }
    }

    private void ShowSlotInfo(SaveSlotInfo info)
    {
        if (info == null || !info.exists)
        {
            infoPanel.Show(
                "<color=#ad4402><b>Пустий слот</b></color>",
                "<color=#e80909>У цьому слоті ще немає збереження</color>",
                null
            );
            return;
        }

        Sprite preview = LoadSavePreview(info.slot);

        infoPanel.Show(
            $"<color=#ad4402><b>Збереження {info.slot + 1}</b></color>",
            $"<color=#876207>Дата:</color> {info.saveTime}",
            preview
        );
    }

    private Sprite LoadSavePreview(int slot)
    {
        string path = Path.Combine(Application.persistentDataPath, $"Saves/save_{slot}.png");

        if (!File.Exists(path)) return null;

        byte[] data = File.ReadAllBytes(path);

        Texture2D tex = new Texture2D(2, 2, TextureFormat.RGB24, false);
        tex.LoadImage(data);

        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
    }

    private void OnSaveCompleted(int slotIndex)
    {
        var info = SaveManager.Instance.GetAllSaves().Find(s => s.slot == slotIndex);

        if (info != null) ShowSlotInfo(info);

        Refresh();
    }
}
