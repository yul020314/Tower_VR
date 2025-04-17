using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    private int currency = 100;
    private int playerHealth = 10;
    
    // 添加公共属性访问器
    public int CurrentCurrency => currency;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void AddCurrency(int amount)
    {
        currency += amount;
        // 更新UI
        UIManager.Instance.UpdateCurrencyText(currency);
    }
    
    // 花费货币的方法，返回是否成功
    public bool SpendCurrency(int amount)
    {
        if (currency >= amount)
        {
            currency -= amount;
            UIManager.Instance.UpdateCurrencyText(currency);
            return true;
        }
        return false;
    }
    
    public void PlayerTakeDamage(int damage)
    {
        playerHealth -= damage;
        // 更新UI
        UIManager.Instance.UpdateHealthText(playerHealth);
        
        if (playerHealth <= 0)
        {
            GameOver();
        }
    }
    
    public void GameOver()
    {
        // 处理游戏结束逻辑
        Debug.Log("Game Over!");
        UIManager.Instance.ShowGameOver();
    }
}