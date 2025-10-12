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

    }
}
