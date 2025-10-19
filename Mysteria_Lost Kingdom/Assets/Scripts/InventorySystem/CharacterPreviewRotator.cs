using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CharacterPreviewRotator : MonoBehaviour
{
    public float rotationSpeed = 100f;
    public RectTransform panelRect;
    private bool isDragging;
    public GraphicRaycaster graphicRaycaster;
    public EventSystem eventSystem;
    public LayerMask uiCharacterLayer;

    void Update()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;

        Vector2 mousePos = mouse.position.ReadValue();

        if (mouse.leftButton.wasPressedThisFrame)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(panelRect, mousePos))
            {
                PointerEventData pointerData = new PointerEventData(eventSystem);
                pointerData.position = mousePos;

                List<RaycastResult> results = new List<RaycastResult>();
                graphicRaycaster.Raycast(pointerData, results);

                foreach (var result in results)
                {
                    var go = result.gameObject;

                    if (go == panelRect.gameObject) continue;
                    if (((1 << go.layer) & uiCharacterLayer.value) != 0)
                    {
                        return;
                    }
                }
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
