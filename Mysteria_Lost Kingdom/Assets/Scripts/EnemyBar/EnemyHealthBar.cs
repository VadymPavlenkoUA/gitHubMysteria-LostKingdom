using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    public Slider fillSlider;
    public TextMeshProUGUI enemyName;
    public Canvas canvas;

    private Transform target;
    private Camera cam;
    private EnemyStats enemy;

    [Header("Visibility Settings")]
    public float hideDistance = 12f;

    public void Init(EnemyStats enemyStats, Transform targetTransform)
    {
        enemy = enemyStats;
        target = targetTransform;
        cam = Camera.main;
        enemyName.text = enemy.enemyName;
    }

    void LateUpdate()
    {
        if (enemy == null || cam == null) return;
        float dist = Vector3.Distance(cam.transform.position, target.position);
        if (dist > hideDistance)
        {
            if (canvas.enabled) canvas.enabled = false;
            return;
        }

        if (!canvas.enabled) canvas.enabled = true;
        transform.forward = cam.transform.forward;
    }

    public void UpdateHealth(float normalizedValue)
    {
        fillSlider.value = normalizedValue;
    }

    public void OnDeath()
    {
        Destroy(gameObject);
    }
}
