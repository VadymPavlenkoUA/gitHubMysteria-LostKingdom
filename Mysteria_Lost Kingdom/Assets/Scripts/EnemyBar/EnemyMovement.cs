using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float rotationSpeed = 10f;

    private bool isMoving = false;
    private Vector3 target;

    void Update()
    {
        if (!isMoving) return;

        Vector3 dir = (target - transform.position).normalized;
        dir.y = 0;

        transform.position += dir * moveSpeed * Time.deltaTime;

        Quaternion rot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, rot, rotationSpeed * Time.deltaTime);
    }

    public void MoveTo(Vector3 pos)
    {
        target = pos;
        isMoving = true;
    }

    public void Stop()
    {
        isMoving = false;
    }
}
