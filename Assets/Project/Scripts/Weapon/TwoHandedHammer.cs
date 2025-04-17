using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// 双手锤武器类，碰到敌人造成固定伤害，并有范围眩晕和击飞效果
/// </summary>
public class TwoHandedHammer : Weapon
{
    [Header("双手锤特性")]
    [SerializeField] private float stunDuration = 2.5f; // 眩晕效果持续时间
    [SerializeField] private Transform hammerHead; // 锤头部分
    [SerializeField] private GameObject smashEffect; // 锤击效果预制体
    [SerializeField] private float aoeRadius = 2.5f; // 范围效果半径
    [SerializeField] private float knockUpForce = 5.0f; // 击飞力度
    [SerializeField] private LayerMask enemyLayer; // 敌人层级
    
    private Rigidbody rb;
    private float lastSwingTime = 0f;
    private const float swingSoundCooldown = 0.7f; // 挥锤音效冷却时间
    
    /// <summary>
    /// 初始化组件引用
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody>();
        
        // 如果没有设置锤头位置，使用当前物体的变换
        if (hammerHead == null)
        {
            hammerHead = transform;
        }
    }
    
    /// <summary>
    /// 当武器被抓取时的事件处理
    /// </summary>
    protected override void OnGrab(SelectEnterEventArgs args)
    {
        base.OnGrab(args);
        // 重置状态
    }
    
    /// <summary>
    /// 当武器被释放时的事件处理
    /// </summary>
    protected override void OnRelease(SelectExitEventArgs args)
    {
        base.OnRelease(args);
    }
    
    /// <summary>
    /// 固定时间更新，用于清理超时的敌人记录
    /// </summary>
    private void FixedUpdate()
    {
        if (!isHeld) return;
        
        // 清理超时的敌人记录
        CleanupHitEnemies();
    }
    
    /// <summary>
    /// 检测碰撞，对敌人造成伤害
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        // 只有被持有时才能造成伤害
        if (!isHeld) return;
        
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null && !enemy.IsDead)
        {
            // 计算伤害
            int damage = CalculateDamage();
            
            // 对敌人造成伤害
            DamageEnemy(enemy, damage);
            
            // 生成击中特效
            Vector3 hitPoint = other.ClosestPoint(hammerHead.position);
            SpawnHitEffect(hitPoint);
            
            // 播放击中音效
            PlayHitSound();
            
            // 应用范围效果
            ApplyAreaEffect(hitPoint);
            
            // 应用特殊效果
            ApplyEffects(enemy);
        }
    }
    
    /// <summary>
    /// 计算伤害值
    /// </summary>
    protected override int CalculateDamage()
    {
        // 返回固定的基础伤害值
        return baseDamage;
    }
    
    /// <summary>
    /// 播放挥锤音效
    /// </summary>
    private void PlayHitSound()
    {
        if (useSound != null && audioSource != null && Time.time - lastSwingTime > swingSoundCooldown)
        {
            audioSource.PlayOneShot(useSound);
            lastSwingTime = Time.time;
        }
    }
    
    /// <summary>
    /// 应用双手锤特有的效果：单体击飞
    /// </summary>
    protected override void ApplyEffects(Enemy enemy)
    {
        if (enemy == null || enemy.IsDead) return;
        
        // 计算击飞方向（主要是向上）
        Vector3 knockupDirection = Vector3.up + (enemy.transform.position - transform.position).normalized * 0.5f;
        
        // 应用击飞力
        Rigidbody enemyRb = enemy.GetComponent<Rigidbody>();
        if (enemyRb != null)
        {
            enemyRb.AddForce(knockupDirection * knockUpForce, ForceMode.Impulse);
        }
        
        // 眩晕效果
        // enemy.ModifyMoveSpeed(0.2f, stunDuration);
    }
    
    /// <summary>
    /// 应用范围效果
    /// </summary>
    private void ApplyAreaEffect(Vector3 center)
    {
        // 生成震地特效
        if (smashEffect != null)
        {
            Instantiate(smashEffect, center, Quaternion.identity);
        }
        
        // 寻找范围内的所有敌人
        Collider[] hitColliders = Physics.OverlapSphere(center, aoeRadius, enemyLayer);
        foreach (var hitCollider in hitColliders)
        {
            Enemy nearbyEnemy = hitCollider.GetComponent<Enemy>();
            
            // 如果找到敌人且未击中过
            /*if (nearbyEnemy != null && !nearbyEnemy.IsDead && !HasHitEnemy(nearbyEnemy))
            {
                // 计算与中心的距离
                float distance = Vector3.Distance(center, nearbyEnemy.transform.position);
                float damageMultiplier = 1 - (distance / aoeRadius); // 距离越近伤害越高
                
                // 最少造成30%伤害
                damageMultiplier = Mathf.Max(0.3f, damageMultiplier);
                
                // 造成范围伤害
                int areaDamage = Mathf.RoundToInt(baseDamage * damageMultiplier);
                DamageEnemy(nearbyEnemy, areaDamage);
                
                // 轻微眩晕和击退
                StartCoroutine(ApplyAreaStun(nearbyEnemy, distance));
            }*/
        }
    }
    
    /// <summary>
    /// 应用范围眩晕效果的协程
    /// </summary>
    private IEnumerator ApplyAreaStun(Enemy enemy, float distance)
    {
        if (enemy == null || enemy.IsDead) yield break;
        
        // 距离越近，眩晕效果越强
        float stunMultiplier = 1 - (distance / aoeRadius);
        float actualStunDuration = stunDuration * stunMultiplier;
        
        // 应用眩晕减速效果
        // enemy.ModifyMoveSpeed(0.5f, actualStunDuration);
        
        // 轻微击退
        Rigidbody enemyRb = enemy.GetComponent<Rigidbody>();
        if (enemyRb != null)
        {
            Vector3 direction = (enemy.transform.position - transform.position).normalized;
            direction.y = 0.2f;
            float force = knockUpForce * 0.5f * stunMultiplier;
            enemyRb.AddForce(direction * force, ForceMode.Impulse);
        }
        
        yield return null;
    }
    
    /// <summary>
    /// 在编辑器中可视化范围效果
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (hammerHead != null)
        {
            Gizmos.color = new Color(1, 0, 0, 0.3f);
            Gizmos.DrawSphere(hammerHead.position, aoeRadius);
        }
    }
}