using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class DragRotate : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 5f;

    private bool isDragging;

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

            isDragging = true;
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            isDragging = false;
        }

        if (isDragging)
        {
            float mouseX = Mouse.current.delta.ReadValue().x;
            transform.Rotate(Vector3.up, -mouseX * rotationSpeed * Time.deltaTime, Space.World);
        }
    }
}