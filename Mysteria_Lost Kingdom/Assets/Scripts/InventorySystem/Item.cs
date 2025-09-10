using UnityEngine;

[CreateAssetMenu(menuName = "RPG/Item")]
public class Item : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public int maxStack = 1;
    public float weight = 1f;
}
