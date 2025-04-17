using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private GameObject damageEffect;
    
    private int currentHealth;
    
    private void Start()
    {
        currentHealth = maxHealth;
        UIManager.Instance.UpdateHealthText(currentHealth);
    }
    
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        
        if (damageEffect != null)
        {
            Instantiate(damageEffect, transform.position, Quaternion.identity);
        }
        
        UIManager.Instance.UpdateHealthText(currentHealth);
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    private void Die()
    {
        // 玩家死亡逻辑
        GameManager.Instance.GameOver();
    }
}