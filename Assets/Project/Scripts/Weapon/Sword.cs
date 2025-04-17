using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// 剑武器类
/// </summary>
public class Sword : Weapon
{
    [Header("碰撞检测")]
    [SerializeField] private Transform bladeCenter; // 剑刃中心点，用于特效生成位置
    
    private Rigidbody rb;
    private float lastSwingSoundTime = 0f;
    private const float swingSoundCooldown = 0.5f; // 挥剑音效冷却时间
    
    /// <summary>
    /// 初始化组件引用
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody>();
        
        // 如果没有设置剑刃中心点，使用当前物体的变换
        if (bladeCenter == null)
        {
            bladeCenter = transform;
        }
    }
    
    /// <summary>
    /// 当武器被抓取时的事件处理
    /// </summary>
    protected override void OnGrab(SelectEnterEventArgs args)
    {
        base.OnGrab(args);
        // 重置状态，便于新的交互开始
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
            // 使用固定伤害
            int damage = CalculateDamage();
            
            // 对敌人造成伤害
            DamageEnemy(enemy, damage);
            
            // 生成击中特效
            SpawnHitEffect(other.ClosestPoint(bladeCenter.position));
            
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
    /// 播放挥剑音效
    /// </summary>
    private void PlayHitSound()
    {
        if (useSound != null && audioSource != null && Time.time - lastSwingSoundTime > swingSoundCooldown)
        {
            audioSource.PlayOneShot(useSound);
            lastSwingSoundTime = Time.time;
        }
    }
    
    /// <summary>
    /// 应用剑特有的效果
    /// </summary>
    protected override void ApplyEffects(Enemy enemy)
    {
        // 可以在这里添加剑的特殊效果，如果有的话
        // 由于不再使用速度计算，所以不再有基于速度的击退效果
    }
}