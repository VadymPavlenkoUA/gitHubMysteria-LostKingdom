using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    public float interactRange = 2f;
    public LayerMask interactionLayer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = new Ray (transform.position, transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactionLayer))
        {
            var interactable = hit.collider.GetComponent<Interactable>();
            if (interactable != null)
            {
                Debug.Log("Interacted with: " + hit.collider.name);
            }
        }
    }
}
