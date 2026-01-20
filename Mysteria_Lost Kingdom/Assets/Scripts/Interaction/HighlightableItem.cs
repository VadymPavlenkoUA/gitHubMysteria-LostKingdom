using UnityEngine;

public class HighlightableItem : MonoBehaviour, IHighlightable
{
    private Outline outline;

    void Awake()
    {
        outline = GetComponent<Outline>();
        if (outline != null) outline.enabled = false;
    }

    public void SetHighlight(bool state)
    {
        if (outline != null)
        {
            outline.enabled = state;
        }
    }
}
