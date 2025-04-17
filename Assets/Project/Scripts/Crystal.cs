using UnityEngine;

public class Crystal : MonoBehaviour
{
    [SerializeField] private int maxHealth = 500;
    [SerializeField] private GameObject damageEffect;
    [SerializeField] private GameObject destroyEffect;
    
    private int currentHealth;
    
    private void Start()
    {
        currentHealth = maxHealth;
        UIManager.Instance.UpdateCrystalHealth(currentHealth);
    }
    
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log("水晶血量" + currentHealth);
        if (damageEffect != null)
        {
            Instantiate(damageEffect, transform.position, Quaternion.identity);
        }
        
        UIManager.Instance.UpdateCrystalHealth(currentHealth);
        
        if (currentHealth <= 0)
        {
            DestroyCrystal();
        }
    }
    
    private void DestroyCrystal()
    {
        if (destroyEffect != null)
        {
            Instantiate(destroyEffect, transform.position, Quaternion.identity);
        }
        
        GameManager.Instance.GameOver();
        gameObject.SetActive(false);
    }
}