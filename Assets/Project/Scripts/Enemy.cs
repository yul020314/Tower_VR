using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("基本属性")] 
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private int reward = 10; // 击杀奖励
    [SerializeField] private float moveSpeed = 0.5f;
    [SerializeField] private float rotationSpeed = 1f;

    [Header("战斗属性")] 
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackRate = 1f;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private float playerDetectionRange = 5f;
    
    [Header("效果")] 
    [SerializeField] private Material hitMaterial;
    [SerializeField] private float hitEffectDuration = 0.2f;
    [SerializeField] private GameObject deathEffect;
    [SerializeField] private GameObject attackEffect;

    [Header("音效")]
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioClip hurtSound;
    
    
    private float currentHealth;
    private Transform targetWaypoint;
    private int waypointIndex = 0;
    private Rigidbody rb;
    private Renderer enemyRenderer;
    private Material originalMaterial;
    private bool isDead = false;
    private Animator anim;
    
    private Transform currentTarget;
    private float nextAttackTime;
    private bool isAttacking = false;
    
    //属性访问器
    public bool IsDead => isDead;
    public int Reward => reward;
    public float MoveSpeed => moveSpeed;

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

    private void Start()
    {
        currentHealth = maxHealth;
        targetWaypoint = WaypointManager.Instance.GetWaypoint(waypointIndex);
        waypointIndex = 0;
    }

    private void Update()
    {
        if (isDead) return;

        // 检查是否有可攻击的目标
        CheckForTargets();
        
        if (currentTarget != null)
        {
            AttackBehavior();
        }
        else
        {
            MovementBehavior();
        }
        
        UpdateAnimator();
    }

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
        // rb.velocity = transform.forward * moveSpeed;
        // transform.position += transform.forward * (moveSpeed * Time.deltaTime);
        transform.Translate(Vector3.forward * (moveSpeed * Time.deltaTime));

        // 检查是否到达路径点
        if (Vector3.Distance(transform.position, targetWaypoint.position) < 0.5f)
        {
            GetNextWaypoint();
        }
    }

    private void GetNextWaypoint()
    {
        waypointIndex++;
        targetWaypoint = WaypointManager.Instance.GetWaypoint(waypointIndex);
        
        // 如果没有下一个路径点，到达终点
        if (targetWaypoint == null)
        {
            // 没有更多路径点，寻找水晶
            GameObject crystal = GameObject.FindGameObjectWithTag("Crystal");
            if (crystal != null)
            {
                currentTarget = crystal.transform;
            }
        }
    }
    
    private void CheckForTargets()
    {
        // 优先攻击玩家
        if (currentTarget == null || currentTarget.CompareTag("Player"))
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null && Vector3.Distance(transform.position, player.transform.position) <= playerDetectionRange)
            {
                currentTarget = player.transform;
                return;
            }
        }
        
        // 如果没有玩家目标，检查是否到达水晶
        if (targetWaypoint == null && currentTarget == null)
        {
            GameObject crystal = GameObject.FindGameObjectWithTag("Crystal");
            if (crystal != null)
            {
                currentTarget = crystal.transform;
            }
        }
    }

    private void AttackBehavior()
    {
        if (currentTarget == null) return;
        
        // 面向目标
        Vector3 direction = (currentTarget.position - transform.position).normalized;
        direction.y = 0;
        
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        
        // 停止移动
        rb.velocity = Vector3.zero;
        
        // 检查是否在攻击范围内
        float distance = Vector3.Distance(transform.position, currentTarget.position);
        if (distance <= attackRange)
        {
            if (Time.time >= nextAttackTime)
            {
                Attack();
                nextAttackTime = Time.time + 1f / attackRate;
            }
        }
        else
        {
            // 不在攻击范围内，向目标移动
            //rb.velocity = transform.forward * moveSpeed;
            transform.Translate(Vector3.forward * (moveSpeed * Time.deltaTime));
            isAttacking = false;
        }
    }
    
    private void Attack()
    {
        isAttacking = true;
        
        // 触发攻击动画
        if (anim != null)
        {
            anim.SetBool("IsAttacking", isAttacking);
        }
        
        // 攻击效果
        if (attackEffect != null)
        {
            Instantiate(attackEffect, transform.position + Vector3.up, Quaternion.identity);
        }
        
        // 对目标造成伤害
        if (currentTarget.CompareTag("Player"))
        {
            Player player = currentTarget.GetComponent<Player>();
            if (player != null)
            {
                player.TakeDamage(attackDamage);
            }
        }
        else if (currentTarget.CompareTag("Crystal"))
        {
            Crystal crystal = currentTarget.GetComponent<Crystal>();
            if (crystal != null)
            {
                crystal.TakeDamage(attackDamage);
            }
        }
    }
    
    private void UpdateAnimator()
    {
        if (anim == null) return;
        
        anim.SetBool("IsMoving", rb.velocity.magnitude > 0.1f && !isAttacking);
        anim.SetBool("IsAttacking", isAttacking);
    }
    
    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        
        // 受伤效果
        StartCoroutine(HitEffect());
        
        // 检查死亡
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }
    private IEnumerator HitEffect()
    {
        if (enemyRenderer != null && hitMaterial != null)
        {
            enemyRenderer.material = hitMaterial;
            yield return new WaitForSeconds(hitEffectDuration);
            enemyRenderer.material = originalMaterial;
        }
    }
    
    private void Die()
    {
        isDead = true;
        
        // 死亡效果
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }
        
        // 奖励玩家
        GameManager.Instance.AddCurrency(reward);
        
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
        
        // 销毁对象（可以替换为对象池回收）
        Destroy(gameObject, 2f);
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

    // 修改移动速度的方法
    public void ModifyMoveSpeed(float speedModifier, float duration)
    {
        StartCoroutine(TempModifySpeed(speedModifier, duration));
    }
    
    // 临时修改速度的协程
    private IEnumerator TempModifySpeed(float speedModifier, float duration)
    {
        float originalSpeed = moveSpeed;
        moveSpeed *= speedModifier;
        
        yield return new WaitForSeconds(duration);
        
        if (!isDead)
        {
            moveSpeed = originalSpeed;
        }
    }
}
