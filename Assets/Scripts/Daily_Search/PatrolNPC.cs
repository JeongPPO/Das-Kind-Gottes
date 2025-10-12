using UnityEngine;

public class PatrolNPC : MonoBehaviour
{
    public Transform[] patrolPoints;
    public float moveSpeed = 2f;
    public float visionRadius = 2f;
    public LayerMask playerLayer;

    private int currentPoint = 0;

    void Update()
    {
        Patrol();
        CheckPlayer();
    }

    void Patrol()
    {
        if (patrolPoints.Length == 0) return;

        Transform target = patrolPoints[currentPoint];
        transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target.position) < 0.1f)
            currentPoint = (currentPoint + 1) % patrolPoints.Length;
    }

    void CheckPlayer()
    {
        Collider2D player = Physics2D.OverlapCircle(transform.position, visionRadius, playerLayer);
        if (player != null)
        {
            Debug.Log($"{name}에게 {player.name} 발각됨!");
        }
    }
}