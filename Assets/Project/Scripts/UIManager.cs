using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("游戏状态UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject levelCompletePanel;
    [SerializeField] private TextMeshProUGUI waveText;

    [Header("玩家状态UI")]
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI currencyText;
    [SerializeField] private Image healthBar;
    [SerializeField] private Image energyBar; // 可选：用于VR特殊能力

    [Header("建造UI")]
    [SerializeField] private GameObject buildMenu;
    [SerializeField] private TowerButton[] towerButtons;

    [Header("VR特定UI")]
    [SerializeField] private Transform vrUIAnchor; // VR中UI的锚点
    [SerializeField] private float uiDistance = 2f; // UI距离玩家的距离
    [SerializeField] private float uiHeight = 1.5f; // UI的高度
    
    [Header("水晶UI")]
    [SerializeField] private TextMeshProUGUI crystalHealthText;
    [SerializeField] private Image crystalHealthBar;
    
    [Header("消息系统")]
    [SerializeField] private GameObject messagePanel;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private float messageDuration = 3f; // 消息显示持续时间
    
    private Coroutine messageCoroutine;
    
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

    private void Start()
    {
        // 初始化VR UI位置
        if (vrUIAnchor == null)
        {
            vrUIAnchor = Camera.main.transform; // 默认使用主相机
        }

        PositionVrUI();
        
        // 默认隐藏游戏状态面板
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (levelCompletePanel != null) levelCompletePanel.SetActive(false);
    }

    // 定位VR UI
    private void PositionVrUI()
    {
        if (vrUIAnchor != null)
        {
            transform.position = vrUIAnchor.position + vrUIAnchor.forward * uiDistance;
            transform.position = new Vector3(transform.position.x, uiHeight, transform.position.z);
            transform.LookAt(vrUIAnchor);
            transform.Rotate(0, 180, 0); // 使UI面向玩家
        }
    }

    #region 玩家状态更新
    public void UpdateHealthText(int health)
    {
        if (healthText != null)
        {
            healthText.text = $"生命: {health}";
        }

        if (healthBar != null)
        {
            healthBar.fillAmount = Mathf.Clamp01((float)health / 10f); // 假设最大血量为10
        }
    }

    public void UpdateCurrencyText(int currency)
    {
        if (currencyText != null)
        {
            currencyText.text = $"金币: {currency}";
        }
    }

    public void UpdateWaveText(int currentWave, int totalWaves)
    {
        if (waveText != null)
        {
            waveText.text = $"波数: {currentWave}/{totalWaves}";
        }
    }

    public void UpdateEnergy(float energy, float maxEnergy)
    {
        if (energyBar != null)
        {
            energyBar.fillAmount = Mathf.Clamp01(energy / maxEnergy);
        }
    }
    #endregion

    #region 游戏状态UI
    public void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            // 确保在VR中面对玩家
            if (vrUIAnchor != null)
            {
                gameOverPanel.transform.position = vrUIAnchor.position + vrUIAnchor.forward * uiDistance;
                gameOverPanel.transform.rotation = Quaternion.LookRotation(-vrUIAnchor.forward);
            }
        }
    }

    public void ShowLevelComplete()
    {
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(true);
            // 确保在VR中面对玩家
            if (vrUIAnchor != null)
            {
                levelCompletePanel.transform.position = vrUIAnchor.position + vrUIAnchor.forward * uiDistance;
                levelCompletePanel.transform.rotation = Quaternion.LookRotation(-vrUIAnchor.forward);
            }
        }
    }
    #endregion

    #region 建造UI
    public void ToggleBuildMenu(bool show)
    {
        if (buildMenu != null)
        {
            buildMenu.SetActive(show);
            if (show) PositionVrUI(); // 显示时重新定位
        }
    }

    public void UpdateTowerButtons(int currentCurrency)
    {
        foreach (TowerButton button in towerButtons)
        {
            button.UpdateButtonState(currentCurrency);
        }
    }
    #endregion

    #region VR交互方法
    // 用于VR控制器交互
    public void OnVrPointerEnter(Button button)
    {
        // 可以添加悬停效果
        button.transform.localScale = Vector3.one * 1.1f;
    }

    public void OnVrPointerExit(Button button)
    {
        // 恢复原始大小
        button.transform.localScale = Vector3.one;
    }
    #endregion

    #region 水晶

    public void UpdateCrystalHealth(int health)
    {
        if (crystalHealthText != null)
        {
            crystalHealthText.text = $"水晶: {health}";
        }

        if (crystalHealthBar != null)
        {
            crystalHealthBar.fillAmount = Mathf.Clamp01((float)health / 500f); // 假设水晶最大血量为500
        }
    }

    #endregion

    #region 消息系统
    
    // 显示临时消息
    public void ShowMessage(string message)
    {
        if (messagePanel == null || messageText == null) return;
        
        // 停止之前的消息协程
        if (messageCoroutine != null)
        {
            StopCoroutine(messageCoroutine);
        }
        
        // 开始新的消息显示
        messageCoroutine = StartCoroutine(ShowMessageCoroutine(message));
    }
    
    private IEnumerator ShowMessageCoroutine(string message)
    {
        // 设置消息文本
        messageText.text = message;
        messagePanel.SetActive(true);
        
        // 等待指定时间
        yield return new WaitForSeconds(messageDuration);
        
        // 隐藏消息面板
        messagePanel.SetActive(false);
        messageCoroutine = null;
    }
    
    #endregion
}

// 塔按钮类
[System.Serializable]
public class TowerButton
{
    public Button button;
    public int cost;
    public TextMeshProUGUI costText;

    public void UpdateButtonState(int currentCurrency)
    {
        bool canAfford = currentCurrency >= cost;
        button.interactable = canAfford;
        
        if (costText != null)
        {
            costText.color = canAfford ? Color.white : Color.red;
        }
    }
}