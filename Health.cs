using UnityEngine;

public class Health : MonoBehaviour
{
    public float maxHealth = 100f;
    public float minHealth = 0f;
    public float currentHealthPercentage => currentHealth / maxHealth;
    private float currentHealth;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, minHealth, maxHealth);
        Debug.Log($"Took {damageAmount} damage. Current health: {currentHealth}");
    }
    private void Update()
    {
        if (currentHealth <= 0)
        {
            Debug.Log("you are ass lil bro");
        }
    }


}
