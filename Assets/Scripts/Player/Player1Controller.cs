using UnityEngine;

public class Player1Controller : MonoBehaviour
{
    public float attackRange = 1f;
    public float attackPower = 10f;

    public PlayerData playerData;
    public HeartUIController uiController;
    public PlayerHealth playerHealth;

    void Update()
    {
        if (!gameObject.activeSelf) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryAttack();
        }
        if (Input.GetKeyDown(KeyCode.H))
        {
            playerHealth.TakeDamage(2.25f);
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            playerHealth.Heal(3.75f);
        }
    }

    void TryAttack()
    {
        Vector2Int bossGrid = GridManager.Instance.WorldToGrid(Boss.Instance.transform.position);
        Vector2Int myGrid = GridManager.Instance.WorldToGrid(transform.position);

        if (Vector2Int.Distance(myGrid, bossGrid) <= attackRange)
        {
            Boss.Instance.TakeDamage(attackPower);
            Debug.Log("플레이어 2가 공격!");
        }
        else
        {
            Debug.Log("보스가 사거리 밖에 있음");
        }
    }
}
