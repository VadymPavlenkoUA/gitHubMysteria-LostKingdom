using UnityEngine;



[System.Serializable]
public class EnemySaveData
{
    public string uniqueID;
    public Vector3 position;
    public Quaternion rotation;

    public EnemyAIController.State currentState;

    public Vector3 patrolTarget;
    public bool isPatrolling;
    public float patrolTimer;

    public bool isAttacking;
    public bool isHit;

    public float currentHealth;

    public bool lootDropped;
}
