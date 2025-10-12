using UnityEngine;
public class StaminaSystem : MonoBehaviour
{
    public float MaxStamina = 100f;
    public float currentStamina = 100f;

    public void UseStamina(float amount)
    {
        currentStamina = Mathf.Max(currentStamina - amount, 0);
    }

    public void RecoverStamina(float amount)
    {
        currentStamina = Mathf.Min(currentStamina + amount, MaxStamina);
    }
}
