using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InfiltrationEnemyPatrol : MonoBehaviour
{
    public List<Vector2Int> patrolPoints;
    public float moveSpeed = 2f;
    public float waitTime = 1f;

    private InfiltrationEnemyMover _mover;
    private int _index = 0;

    IEnumerator Start()
    {
        _mover = GetComponent<InfiltrationEnemyMover>();
        while (true)
        {
            yield return _mover.MoveToRoutine(patrolPoints[_index], moveSpeed);
            yield return new WaitForSeconds(waitTime);
            _index = (_index + 1) % patrolPoints.Count;
        }
    }
}