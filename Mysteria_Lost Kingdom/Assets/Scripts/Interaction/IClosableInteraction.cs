using UnityEngine;


public interface IClosableInteraction
{
    Transform InteractionTransform { get; }
    void OnInteractionClosed();
}
