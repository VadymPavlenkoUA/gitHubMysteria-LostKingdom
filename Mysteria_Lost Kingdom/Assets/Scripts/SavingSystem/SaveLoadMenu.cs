using UnityEngine;

public class SaveLoadMenu : MonoBehaviour
{
    public Transform contentParent;
    public SaveSlotUI slotPrefab;

    private void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        foreach (Transform child in contentParent) Destroy(child.gameObject);

        var saves = SaveManager.Instance.GetAllSaves();

        foreach (var save in saves)
        {
            var slot = Instantiate(slotPrefab, contentParent);
            slot.Setup(save);
        }
    }
}
