using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 控制敌人的行为：路径点跟随，水晶攻击，死亡掉落金币
/// </summary>

public class Enemy : MonoBehaviour
{
    [Header("基本属性")] 
    [SerializeField] private float maxHealth = 100f;      // 最大生命值
    [SerializeField] private int reward = 10;             // 击杀奖励金币数量
    [SerializeField] private float moveSpeed = 0.8f;      // 移动速度
    [SerializeField] private float rotationSpeed = 1f;    // 旋转速度

    [Header("战斗属性")] 
    [SerializeField] private float attackRange = 1.5f;    // 攻击范围
    [SerializeField] private float attackRate = 1f;       // 攻击频率
    [SerializeField] private int attackDamage = 10;       // 攻击伤害
    
    [Header("掉落设置")]
    [SerializeField] private GameObject coinPrefab;       // 金币预制体
    [SerializeField] private int minCoins = 1;            // 最少掉落金币数
    [SerializeField] private int maxCoins = 3;            // 最多掉落金币数
    [SerializeField] private float dropRadius = 1f;       // 掉落半径
    
    private float currentHealth;                          // 当前生命值
    private Transform targetWaypoint;                     // 当前目标路径点
    private int waypointIndex = 0;                        // 当前路径点索引
    private Rigidbody rb;                                 // 刚体组件
    private Renderer enemyRenderer;                       // 渲染器组件
    private Material originalMaterial;                    // 原始材质
    private bool isDead = false;                          // 是否已死亡
    private Animator anim;                                // 动画控制器
    
    private Transform crystalTarget;                      // 水晶目标
    private float nextAttackTime = 0f;                         // 下次攻击时间
    private bool isAttacking = false;                     // 是否正在攻击
    
    //属性访问器
    public bool IsDead => isDead;
    public int Reward => reward;
    public float MoveSpeed => moveSpeed;

    /// <summary>
    /// 初始化组件引用
    /// </summary>
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        enemyRenderer = GetComponentInChildren<Renderer>();
        anim = GetComponent<Animator>();
        if (enemyRenderer != null)
        {
            originalMaterial = enemyRenderer.material;
        }
    }

    /// <summary>
    /// 初始化状态和目标
    /// </summary>
    private void Start()
    {
        currentHealth = maxHealth;
        targetWaypoint = WaypointManager.Instance.GetWaypoint(waypointIndex);
        waypointIndex = 0;
    }
    
    /// <summary>
    /// 每帧更新敌人行为
    /// </summary>
    private void Update()
    {
        if (isDead) return;

        if (crystalTarget != null)
        {
            AttackCrystal();
        }
        else
        {
            MovementBehavior();
        }
    }

    
    /// <summary>
    /// 沿路径点移动的行为
    /// </summary>
    private void MovementBehavior()
    {
        if (targetWaypoint == null) return;
        
        // 计算方向
        Vector3 direction = (targetWaypoint.position - transform.position).normalized;
        direction.y = 0;

        // 旋转朝向目标
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        
        // 移动
        transform.Translate(Vector3.forward * (moveSpeed * Time.deltaTime));

        // 检查是否到达路径点
        if (Vector3.Distance(transform.position, targetWaypoint.position) < 0.5f)
        {
            GetNextWaypoint();
        }
    }

    /// <summary>
    /// 获取下一个路径点，如果没有则寻找水晶
    /// </summary>
    private void GetNextWaypoint()
    {
        waypointIndex++;
        targetWaypoint = WaypointManager.Instance.GetWaypoint(waypointIndex);
        
        // 如果没有下一个路径点，到达终点
        if (targetWaypoint == null)
        {
            // 没有更多路径点，寻找水晶
            GameObject crystal = GameObject.FindGameObjectWithTag("Tower");
            if (crystal != null)
            {
                crystalTarget = crystal.transform;
            }
        }
    }

    /// <summary>
    /// 攻击水晶的行为
    /// </summary>
    private void AttackCrystal()
    {
        if (crystalTarget == null) return;
        
        // 面向目标
        Vector3 direction = (crystalTarget.position - transform.position).normalized;
        direction.y = 0;
        
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        
        // 停止移动
        rb.velocity = Vector3.zero;

        //按攻击频率进行攻击
        if (Time.time >= nextAttackTime)
        {
            AttackTarget();
            nextAttackTime = Time.time + 1f / attackRate;
        }
        
        /*
        // 检查是否在攻击范围内
        float distance = Vector3.Distance(transform.position, crystalTarget.position);
        if (distance <= attackRange)
        {
            if (Time.time >= nextAttackTime)
            {
                AttackTarget();
                nextAttackTime = Time.time + 1f / attackRate;
            }
        }
        else
        {
            // 不在攻击范围内，向目标移动
            transform.Translate(Vector3.forward * (moveSpeed * Time.deltaTime));
            isAttacking = false;
        }
        */
    }
    
    /// <summary>
    /// 执行攻击目标的动作
    /// </summary>
    private void AttackTarget()
    {
        isAttacking = true;
        
        // 触发攻击动画
        if (anim != null)
        {
            anim.SetBool("IsAttacking", isAttacking);
        }
        
        // 对水晶造成伤害
        if (crystalTarget != null && crystalTarget.CompareTag("Tower"))
        {
            Crystal crystal = crystalTarget.GetComponent<Crystal>();
            if (crystal != null)
            {
                crystal.TakeDamage(attackDamage);
            }
        }
    }

    /// <summary>
    /// 受到伤害的处理
    /// </summary>
    /// <param name="damage">伤害值</param>
    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        Debug.Log(currentHealth);
        
        // 检查死亡
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }
    
    /// <summary>
    /// 敌人死亡处理
    /// </summary>
    private void Die()
    {
        isDead = true;
        
        // 奖励玩家金币
        GameManager.Instance.AddCurrency(reward);
        
        // 掉落金币物品
        DropCoins();
        
        // 禁用碰撞器和渲染器
        if (TryGetComponent<Collider>(out var collider))
        {
            collider.enabled = false;
        }
        
        if (enemyRenderer != null)
        {
            enemyRenderer.enabled = false;
        }
        
        // 停止移动
        rb.velocity = Vector3.zero;
        
        // 播放死亡动画
        if (anim != null)
        {
            anim.SetTrigger("Die");
        }
        
        // 销毁对象
        Destroy(gameObject, 2f);
    }

    /// <summary>
    /// 掉落金币物品
    /// </summary>
    private void DropCoins()
    {
        if (coinPrefab == null) return;

        // 随机确定掉落的金币数量
        int coinCount = UnityEngine.Random.Range(minCoins, maxCoins + 1);

        for (int i = 0; i < coinCount; i++)
        {
            // 在敌人周围随机位置生成金币
            Vector2 randomCircle = UnityEngine.Random.insideUnitCircle * dropRadius;
            Vector3 randomPosition = transform.position + new Vector3(randomCircle.x, 0.1f, randomCircle.y);

            // 实例化金币对象
            GameObject coin = Instantiate(coinPrefab, randomPosition, Quaternion.identity);

            // 可以给金币添加一些随机的物理效果，比如弹跳效果
            if (coin.TryGetComponent<Rigidbody>(out var coinRb))
            {
                coinRb.AddForce(new Vector3(
                    UnityEngine.Random.Range(-1f, 1f),
                    UnityEngine.Random.Range(2f, 4f),
                    UnityEngine.Random.Range(-1f, 1f)
                ) * 1.5f, ForceMode.Impulse);
            }
        }
    }

    // 用于调试的Gizmos
    private void OnDrawGizmos()
    {
        if (targetWaypoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, targetWaypoint.position);
        }
    }
}