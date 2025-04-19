using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// 匕首武器类，碰到敌人造成固定伤害，有较高的攻击速度和暴击率
/// </summary>
public class Dagger : Weapon
{
    [Header("匕首特性")]
    [SerializeField] private Transform bladePoint; // 刀刃位置
    [SerializeField] private float criticalChance = 0.15f; // 暴击几率
    [SerializeField] private float criticalMultiplier = 2.0f; // 暴击伤害倍率
    [SerializeField] private GameObject criticalEffect; // 暴击特效
    [SerializeField] private AudioClip criticalSound; // 暴击音效
    
    private Rigidbody rb;
    private float lastStrikeTime = 0f;
    private const float strikeCooldown = 0.2f; // 攻击冷却时间（较短）
    
    /// <summary>
    /// 初始化组件引用
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody>();
        
        // 如果没有设置刀刃位置，使用当前物体的前端
        if (bladePoint == null)
        {
            bladePoint = transform;
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
        
        // 检查攻击冷却
        if (Time.time - lastStrikeTime < strikeCooldown) return;
        
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null && !enemy.IsDead)
        {
            // 检查是否暴击
            bool isCritical = Random.value < criticalChance;
            
            // 计算伤害
            int damage = CalculateDamage(isCritical);
            
            // 对敌人造成伤害
            DamageEnemy(enemy, damage);
            
            // 生成击中特效
            Vector3 hitPoint = other.ClosestPoint(bladePoint.position);
            SpawnHitEffect(hitPoint);
            
            // 如果是暴击，播放暴击特效和音效
            if (isCritical)
            {
                PlayCriticalEffects(hitPoint);
            }
            else
            {
                // 播放普通击中音效
                if (useSound != null && audioSource != null)
                {
                    audioSource.PlayOneShot(useSound);
                }
            }
            
            // 应用特殊效果
            ApplyEffects(enemy);
            
            // 更新冷却时间
            lastStrikeTime = Time.time;
        }
    }
    
    /// <summary>
    /// 计算伤害值
    /// </summary>
    protected int CalculateDamage(bool isCritical)
    {
        if (isCritical)
        {
            return Mathf.RoundToInt(baseDamage * criticalMultiplier);
        }
        return baseDamage;
    }
    
    /// <summary>
    /// 实现基类计算伤害接口
    /// </summary>
    protected override int CalculateDamage()
    {
        return baseDamage;
    }
    
    /// <summary>
    /// 播放暴击特效和音效
    /// </summary>
    private void PlayCriticalEffects(Vector3 position)
    {
        // 生成暴击特效
        if (criticalEffect != null)
        {
            GameObject effect = Instantiate(criticalEffect, position, Quaternion.identity);
            Destroy(effect, 1f);
        }
        
        // 播放暴击音效
        if (criticalSound != null && audioSource != null)
        {
            audioSource.pitch = 1.2f; // 提高音调
            audioSource.PlayOneShot(criticalSound);
            audioSource.pitch = 1.0f; // 恢复音调
        }
        else if (useSound != null && audioSource != null)
        {
            audioSource.pitch = 1.2f;
            audioSource.PlayOneShot(useSound);
            audioSource.pitch = 1.0f;
        }
    }
    
    /// <summary>
    /// 应用匕首特有的效果
    /// </summary>
    protected override void ApplyEffects(Enemy enemy)
    {
        // 匕首没有额外效果
    }
}