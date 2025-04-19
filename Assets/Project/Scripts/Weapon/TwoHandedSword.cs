using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// 双手剑武器类，支持双手握持，碰到敌人造成大范围伤害
/// </summary>
public class TwoHandedSword : Weapon
{
    [Header("双手剑特性")]
    [SerializeField] private Transform bladeCenter; // 剑刃中心点
    [SerializeField] private GameObject slashEffect; // 挥砍特效
    [SerializeField] private float sweepRadius = 1.8f; // 横扫范围
    [SerializeField] private float chargeTime = 1.5f; // 蓄力时间
    [SerializeField] private float chargeDamageMultiplier = 2.0f; // 蓄力伤害倍率
    
    [Header("双手握持设置")]
    [SerializeField] private Transform primaryGripPoint; // 主要握柄位置
    [SerializeField] private Transform secondaryGripPoint; // 次要握柄位置
    [SerializeField] private float twoHandedDamageMultiplier = 1.5f; // 双手握持伤害倍率
    
    private Rigidbody rb;
    private float lastSwingTime = 0f;
    private const float swingSoundCooldown = 0.6f; // 挥剑音效冷却时间
    private bool isCharging = false;
    private float chargeStartTime;
    private XRGrabInteractable grabInteractable;
    private bool isTwoHanded = false; // 是否正在双手握持
    private IXRSelectInteractor secondaryInteractor; // 次要手柄交互器
    private Animator anim;
    
    /// <summary>
    /// 初始化组件引用
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody>();
        grabInteractable = GetComponent<XRGrabInteractable>();
        anim = GetComponent<Animator>();
        
        // 如果没有设置剑刃中心点，使用当前物体的变换
        if (bladeCenter == null)
        {
            bladeCenter = transform;
        }
        
        // 设置XRGrabInteractable组件
        SetupInteractable();
    }
    
    /// <summary>
    /// 配置交互组件以支持双手握持
    /// </summary>
    private void SetupInteractable()
    {
        if (grabInteractable != null)
        {
            // 允许多个交互器选择（必要设置）
            grabInteractable.selectMode = InteractableSelectMode.Multiple;
            
            // 注册次要抓取事件
            grabInteractable.selectEntered.AddListener(OnSecondaryGrab);
            grabInteractable.selectExited.AddListener(OnSecondaryRelease);
        }
    }
    
    /// <summary>
    /// 当武器被抓取时的事件处理
    /// </summary>
    protected override void OnGrab(SelectEnterEventArgs args)
    {
        base.OnGrab(args);
        
        // 重置状态
        isCharging = false;
    }
    
    /// <summary>
    /// 当次要手柄抓取武器
    /// </summary>
    private void OnSecondaryGrab(SelectEnterEventArgs args)
    {
        // 如果已经有一个交互器，而这是第二个，则设置为双手模式
        if (grabInteractable.interactorsSelecting.Count > 1)
        {
            // 获取次要交互器（不是第一个抓取的那个）
            foreach (var interactor in grabInteractable.interactorsSelecting)
            {
                if (interactor != grabInteractable.firstInteractorSelecting)
                {
                    secondaryInteractor = interactor;
                    break;
                }
            }
            
            // 启用双手模式
            isTwoHanded = true;
            
            // 播放双手持剑动画
            if (anim != null)
            {
                anim.SetBool("TwoHanded", true);
            }
        }
    }
    
    /// <summary>
    /// 当武器被释放时的事件处理
    /// </summary>
    protected override void OnRelease(SelectExitEventArgs args)
    {
        // 如果是主要手柄释放，调用基类方法
        if (args.interactorObject == grabInteractable.firstInteractorSelecting)
        {
            base.OnRelease(args);
            isTwoHanded = false;
            secondaryInteractor = null;
            
            // 结束蓄力
            isCharging = false;
            
            // 重置动画
            if (anim != null)
            {
                anim.SetBool("TwoHanded", false);
                anim.SetBool("Charging", false);
            }
        }
        // 如果是次要手柄释放
        else if (isTwoHanded && args.interactorObject == secondaryInteractor)
        {
            isTwoHanded = false;
            secondaryInteractor = null;
            
            // 更新动画
            if (anim != null)
            {
                anim.SetBool("TwoHanded", false);
            }
        }
    }
    
    /// <summary>
    /// 当次要交互器释放武器
    /// </summary>
    private void OnSecondaryRelease(SelectExitEventArgs args)
    {
        if (isTwoHanded && args.interactorObject == secondaryInteractor)
        {
            isTwoHanded = false;
            secondaryInteractor = null;
            
            // 更新动画
            if (anim != null)
            {
                anim.SetBool("TwoHanded", false);
            }
        }
    }
    
    /// <summary>
    /// 特殊按钮事件（用于蓄力攻击）
    /// </summary>
    protected override void SpecialEvent(ActivateEventArgs args)
    {
        base.SpecialEvent(args);
        
        // 只有双手持剑时才能蓄力
        if (isTwoHanded && !isCharging)
        {
            StartCharging();
        }
        else if (isCharging)
        {
            ReleaseCharge();
        }
    }
    
    /// <summary>
    /// 开始蓄力
    /// </summary>
    private void StartCharging()
    {
        isCharging = true;
        chargeStartTime = Time.time;
        
        // 播放蓄力动画
        if (anim != null)
        {
            anim.SetBool("Charging", true);
        }
    }
    
    /// <summary>
    /// 释放蓄力攻击
    /// </summary>
    private void ReleaseCharge()
    {
        isCharging = false;
        
        // 计算蓄力时间
        float chargeDuration = Time.time - chargeStartTime;
        
        // 播放攻击动画
        if (anim != null)
        {
            anim.SetBool("Charging", false);
            anim.SetTrigger("Attack");
        }
        
        // 播放攻击音效
        if (useSound != null && audioSource != null)
        {
            audioSource.pitch = 0.8f + Mathf.Min(chargeDuration / chargeTime, 1f) * 0.4f;
            audioSource.PlayOneShot(useSound);
        }
        
        // 执行横扫攻击
        float chargeRatio = Mathf.Min(chargeDuration / chargeTime, 1f);
        PerformSweepAttack(chargeRatio);
    }
    
    /// <summary>
    /// 执行横扫攻击
    /// </summary>
    private void PerformSweepAttack(float chargeRatio)
    {
        // 计算横扫范围
        float effectiveSweepRadius = sweepRadius * (0.5f + chargeRatio * 0.5f);
        
        // 生成横扫特效
        if (slashEffect != null)
        {
            GameObject effect = Instantiate(slashEffect, bladeCenter.position, Quaternion.identity);
            effect.transform.rotation = Quaternion.LookRotation(bladeCenter.forward, bladeCenter.up);
            effect.transform.localScale *= (1f + chargeRatio);
            Destroy(effect, 0.5f);
        }
        /*
        // 寻找范围内的敌人
        Collider[] hitColliders = Physics.OverlapSphere(bladeCenter.position, effectiveSweepRadius, enemyLayer);
        foreach (var hitCollider in hitColliders)
        {
            Enemy enemy = hitCollider.GetComponent<Enemy>();
            
            // 如果找到敌人且未击中过
            if (enemy != null && !enemy.IsDead && !HasHitEnemy(enemy))
            {
                // 计算伤害
                int damage = CalculateDamage(chargeRatio);
                
                // 对敌人造成伤害
                DamageEnemy(enemy, damage);
                
                // 生成击中特效
                SpawnHitEffect(hitCollider.ClosestPoint(bladeCenter.position));
                
                // 应用特殊效果
                ApplyEffects(enemy, chargeRatio);
            }
        }*/
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
        if (!isHeld || isCharging) return; // 蓄力时不造成普通碰撞伤害
        
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null && !enemy.IsDead)
        {
            // 计算伤害
            int damage = CalculateDamage(0);
            
            // 对敌人造成伤害
            DamageEnemy(enemy, damage);
            
            // 生成击中特效
            SpawnHitEffect(other.ClosestPoint(bladeCenter.position));
            
            // 播放击中音效
            PlayHitSound();
            
            // 应用特殊效果
            ApplyEffects(enemy, 0);
        }
    }
    
    /// <summary>
    /// 计算伤害值，考虑双手握持和蓄力状态
    /// </summary>
    protected int CalculateDamage(float chargeRatio)
    {
        float calculatedDamage = baseDamage;
        
        // 双手握持加成
        if (isTwoHanded)
        {
            calculatedDamage *= twoHandedDamageMultiplier;
        }
        
        // 蓄力加成
        if (chargeRatio > 0)
        {
            calculatedDamage *= (1f + chargeRatio * (chargeDamageMultiplier - 1f));
        }
        
        return Mathf.RoundToInt(calculatedDamage);
    }
    
    /// <summary>
    /// 计算基础伤害，实现基类要求
    /// </summary>
    protected override int CalculateDamage()
    {
        return CalculateDamage(0);
    }
    
    /// <summary>
    /// 播放挥剑音效
    /// </summary>
    private void PlayHitSound()
    {
        if (useSound != null && audioSource != null && Time.time - lastSwingTime > swingSoundCooldown)
        {
            audioSource.pitch = isTwoHanded ? 0.8f : 1.0f;
            audioSource.PlayOneShot(useSound);
            lastSwingTime = Time.time;
        }
    }
    
    /// <summary>
    /// 应用双手剑特有的效果
    /// </summary>
    protected void ApplyEffects(Enemy enemy, float chargeRatio)
    {
        if (enemy == null || enemy.IsDead) return;
        
        // 施加击退效果
        Rigidbody enemyRb = enemy.GetComponent<Rigidbody>();
        if (enemyRb != null)
        {
            Vector3 direction = (enemy.transform.position - transform.position).normalized;
            direction.y = 0.1f;
            float force = 3f + chargeRatio * 4f;
            enemyRb.AddForce(direction * force, ForceMode.Impulse);
        }
    }
    
    /// <summary>
    /// 应用基类要求的效果方法
    /// </summary>
    protected override void ApplyEffects(Enemy enemy)
    {
        ApplyEffects(enemy, 0);
    }
    
    /// <summary>
    /// 可视化双手握持点和范围
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // 显示横扫范围
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        if (bladeCenter != null)
        {
            Gizmos.DrawSphere(bladeCenter.position, sweepRadius);
        }
        
        // 显示握持点
        Gizmos.color = Color.blue;
        if (primaryGripPoint != null)
        {
            Gizmos.DrawSphere(primaryGripPoint.position, 0.03f);
        }
        
        Gizmos.color = Color.cyan;
        if (secondaryGripPoint != null)
        {
            Gizmos.DrawSphere(secondaryGripPoint.position, 0.03f);
        }
    }
}