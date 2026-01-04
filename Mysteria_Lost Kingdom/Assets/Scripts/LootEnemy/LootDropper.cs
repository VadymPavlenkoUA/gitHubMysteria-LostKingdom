using UnityEngine;

public static class LootDropper
{
    public static void Drop(LootTable table, Vector3 position)
    {
        if (table == null) return;

        foreach (var loot in table.lootItems)
        {
            if (Random.value > loot.dropChance)
                continue;

            int amount = Random.Range(loot.minAmount, loot.maxAmount + 1);

            for (int i = 0; i < amount; i++)
            {
                Vector3 offset = Random.insideUnitSphere * 0.5f;
                offset.y = Mathf.Abs(offset.y) + 0.3f;

                GameObject go = Object.Instantiate(
                    loot.worldPrefab,
                    position + offset,
                    Quaternion.identity
                );

                // візуальний "виліт"
                if (go.TryGetComponent<Rigidbody>(out var rb))
                {
                    rb.AddForce(
                        new Vector3(
                            Random.Range(-0.3f, 0.3f),
                            Random.Range(1f, 2f),
                            Random.Range(-0.3f, 0.3f)
                        ),
                        ForceMode.Impulse
                    );
                }
            }
        }
    }
}
