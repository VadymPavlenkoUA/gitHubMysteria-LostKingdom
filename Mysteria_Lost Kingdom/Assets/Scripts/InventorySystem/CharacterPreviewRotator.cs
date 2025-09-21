using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class CharacterPreviewRotator : MonoBehaviour
{
    public float rotationSpeed = 100f;
    public RectTransform panelRect;
    private bool isDragging;

    void Update()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;

        Vector2 mousePos = mouse.position.ReadValue();

        if (mouse.leftButton.wasPressedThisFrame)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(panelRect, mousePos))
            {
                isDragging = true;
            }
        }

        if (mouse.leftButton.wasReleasedThisFrame)
            isDragging = false;

        if (isDragging)
        {
            float rotX = mouse.delta.ReadValue().x * rotationSpeed * Time.deltaTime;
            transform.Rotate(Vector3.up, -rotX, Space.World);
        }
    }
}
