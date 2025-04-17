using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// 钉锤武器类，碰到敌人造成固定伤害，并有击退效果
/// </summary>
public class Mace : Weapon
{
    [Header("钉锤特性")]
    [SerializeField] private float knockbackForce = 5.0f; // 击退力度
    [SerializeField] private Transform maceHead; // 锤头部分
    [SerializeField] private GameObject impactEffect; // 击打效果预制体
    
    private Rigidbody rb;
    private float lastSwingTime = 0f;
    private const float swingSoundCooldown = 0.5f; // 挥锤音效冷却时间
    
    /// <summary>
    /// 初始化组件引用
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody>();
        
        // 如果没有设置锤头位置，使用当前物体的变换
        if (maceHead == null)
        {
            maceHead = transform;
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
            SpawnHitEffect(other.ClosestPoint(maceHead.position));
            
            // 播放击中音效
            PlayHitSound();
            
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
    /// 应用钉锤特有的效果：击退
    /// </summary>
    protected override void ApplyEffects(Enemy enemy)
    {
        if (enemy == null || enemy.IsDead) return;
        
        // 计算击退方向（从武器指向敌人）
        Vector3 knockbackDirection = (enemy.transform.position - transform.position).normalized;
        knockbackDirection.y = 0.1f; // 轻微向上，让击退看起来更明显
        
        // 应用击退力
        Rigidbody enemyRb = enemy.GetComponent<Rigidbody>();
        if (enemyRb != null)
        {
            enemyRb.AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse);
        }
        
        // 显示撞击特效
        if (impactEffect != null)
        {
            Instantiate(impactEffect, enemy.transform.position + Vector3.up, Quaternion.identity);
        }
    }
}