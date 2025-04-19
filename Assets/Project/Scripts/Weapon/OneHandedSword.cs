using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// 单手剑武器类，碰到敌人造成固定伤害，有较高的攻击速度和机动性
/// </summary>
public class OneHandedSword : Weapon
{
    [Header("单手剑特性")]
    [SerializeField] private Transform bladeCenter; // 剑刃中心点
    [SerializeField] private GameObject slashEffect; // 挥砍特效
    [SerializeField] private float comboTimeWindow = 0.8f; // 连击窗口时间
    [SerializeField] private float comboDamageMultiplier = 1.2f; // 连击伤害倍率
    
    private Rigidbody rb;
    private float lastHitTime = 0f;
    private int comboCount = 0;
    private float lastSwingSoundTime = 0f;
    private const float swingSoundCooldown = 0.4f; // 挥剑音效冷却时间
    
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
        // 重置连击状态
        comboCount = 0;
    }
    
    /// <summary>
    /// 当武器被释放时的事件处理
    /// </summary>
    protected override void OnRelease(SelectExitEventArgs args)
    {
        base.OnRelease(args);
        comboCount = 0;
    }
    
    /// <summary>
    /// 固定时间更新，用于清理超时的敌人记录和连击计时
    /// </summary>
    private void FixedUpdate()
    {
        if (!isHeld) return;
        
        // 清理超时的敌人记录
        CleanupHitEnemies();
        
        // 检查连击是否超时
        if (comboCount > 0 && Time.time - lastHitTime > comboTimeWindow)
        {
            comboCount = 0;
        }
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
            SpawnHitEffect(other.ClosestPoint(bladeCenter.position));
            
            // 显示挥砍特效
            ShowSlashEffect();
            
            // 播放击中音效
            PlayHitSound();
            
            // 应用特殊效果
            ApplyEffects(enemy);
            
            // 更新连击状态
            UpdateCombo();
        }
    }
    
    /// <summary>
    /// 计算伤害值，考虑连击加成
    /// </summary>
    protected override int CalculateDamage()
    {
        // 基础伤害
        float calculatedDamage = baseDamage;
        
        // 应用连击加成
        if (comboCount > 0)
        {
            calculatedDamage *= (1f + (comboCount * 0.1f * comboDamageMultiplier));
        }
        
        return Mathf.RoundToInt(calculatedDamage);
    }
    
    /// <summary>
    /// 更新连击计数和时间
    /// </summary>
    private void UpdateCombo()
    {
        // 如果在连击窗口内，增加连击计数
        if (Time.time - lastHitTime <= comboTimeWindow)
        {
            comboCount = Mathf.Min(comboCount + 1, 3); // 最多3连击
        }
        else
        {
            comboCount = 1; // 重置为第一击
        }
        
        lastHitTime = Time.time;
    }
    
    /// <summary>
    /// 显示挥砍特效
    /// </summary>
    private void ShowSlashEffect()
    {
        if (slashEffect != null)
        {
            // 在剑刃位置生成挥砍特效
            GameObject effect = Instantiate(slashEffect, bladeCenter.position, Quaternion.identity);
            effect.transform.rotation = Quaternion.LookRotation(bladeCenter.forward, bladeCenter.up);
            
            // 调整特效大小和持续时间
            float scale = 1f + (comboCount * 0.2f);
            effect.transform.localScale *= scale;
            
            // 自动销毁特效
            Destroy(effect, 0.5f);
        }
    }
    
    /// <summary>
    /// 播放挥剑音效
    /// </summary>
    private void PlayHitSound()
    {
        if (useSound != null && audioSource != null && Time.time - lastSwingSoundTime > swingSoundCooldown)
        {
            audioSource.pitch = 1f + (comboCount * 0.1f); // 连击提高音调
            audioSource.PlayOneShot(useSound);
            lastSwingSoundTime = Time.time;
        }
    }
    
    /// <summary>
    /// 应用单手剑特有的效果
    /// </summary>
    protected override void ApplyEffects(Enemy enemy)
    {
        // 单手剑没有特殊效果，但可以考虑添加出血或昏迷效果
    }
}