using UnityEngine;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance;
    public PlayerStats playerStats;

    private void Awake()
    {
        Instance = this;
    }

    public void PerformAttack(GameObject target)
    {
        var combat = playerStats.CalculateCombatStats();

        // Беремо броню цілі
        float targetArmor = 0f;
        var targetStats = target.GetComponent<EnemyStats>(); // у майбутньому для ворогів
        if (targetStats != null) targetArmor = targetStats.armor;

        // Простий розрахунок шкоди
        float finalDamage = combat.totalDamage - targetArmor;
        finalDamage = Mathf.Max(finalDamage, 1f); // мінімум 1 урон

        // Нанесення урону
        if (targetStats != null)
        {
            targetStats.TakeDamage(finalDamage);
        }

        // Витрата стаміни
        playerStats.UseStamina(10f); // можна зробити залежно від типу зброї

        Debug.Log($"Удар! Нанесено {finalDamage} урону");
    }
}
