using UnityEngine;

public class InteractionDistanceWatcher : MonoBehaviour
{
    public static InteractionDistanceWatcher Instance;

    public float closeDistance = 3.5f;

    private Transform player;
    private IClosableInteraction activeInteraction;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        //DontDestroyOnLoad(gameObject);
        player = GameObject.FindWithTag("Player").transform;
    }

    void Update()
    {
        if (activeInteraction == null) return;

        float dist = Vector3.Distance(
            player.position,
            activeInteraction.InteractionTransform.position
        );

        if (dist > closeDistance)
        {
            CloseInteraction();
        }
    }

    public void StartWatching(IClosableInteraction interaction)
    {
        activeInteraction = interaction;
    }

    public void CloseInteraction()
    {
        if (activeInteraction == null) return;

        activeInteraction.OnInteractionClosed();
        activeInteraction = null;
    }
}
