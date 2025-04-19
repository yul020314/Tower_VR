using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// 长矛武器类，碰到敌人造成固定伤害，有较长的攻击距离
/// </summary>
public class Spear : Weapon
{
    [Header("长矛特性")]
    [SerializeField] private Transform spearTip; // 矛尖位置
    [SerializeField] private float thrustDamageMultiplier = 1.5f; // 刺击伤害倍率
    [SerializeField] private float lungeDistance = 1.5f; // 突刺距离
    [SerializeField] private LayerMask enemyLayer; // 敌人层级
    [SerializeField] private GameObject pierceEffect; // 穿刺特效
    
    private Rigidbody rb;
    private float lastThrustTime = 0f;
    private const float thrustCooldown = 0.8f; // 刺击冷却时间
    private bool isThrusting = false;
    private Vector3 thrustDirection;
    private Animator anim;
    
    /// <summary>
    /// 初始化组件引用
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        
        // 如果没有设置矛尖位置，使用当前物体的前方
        if (spearTip == null)
        {
            spearTip = transform;
        }
    }
    
    /// <summary>
    /// 当武器被抓取时的事件处理
    /// </summary>
    protected override void OnGrab(SelectEnterEventArgs args)
    {
        base.OnGrab(args);
        isThrusting = false;
    }
    
    /// <summary>
    /// 当武器被释放时的事件处理
    /// </summary>
    protected override void OnRelease(SelectExitEventArgs args)
    {
        base.OnRelease(args);
        isThrusting = false;
    }
    
    /// <summary>
    /// 特殊按钮事件处理（用于触发突刺）
    /// </summary>
    protected override void SpecialEvent(ActivateEventArgs args)
    {
        base.SpecialEvent(args);
        
        // 如果冷却完成则执行突刺
        if (Time.time - lastThrustTime > thrustCooldown && !isThrusting)
        {
            StartCoroutine(PerformThrust());
        }
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
    /// 执行突刺动作的协程
    /// </summary>
    private IEnumerator PerformThrust()
    {
        isThrusting = true;
        lastThrustTime = Time.time;
        /*
        // 记录突刺方向
        thrustDirection = spearTip.forward;
        
        // 播放突刺动画
        if (anim != null)
        {
            anim.SetTrigger("Thrust");
        }
        
        // 播放突刺音效
        if (useSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(useSound);
        }
        
        // 突刺检测
        RaycastHit[] hits = Physics.SphereCastAll(spearTip.position, 0.2f, thrustDirection, lungeDistance, enemyLayer);
        foreach (var hit in hits)
        {
            Enemy enemy = hit.collider.GetComponent<Enemy>();
            if (enemy != null && !enemy.IsDead && !HasHitEnemy(enemy))
            {
                // 计算伤害
                int damage = CalculateDamage(true);
                
                // 对敌人造成伤害
                DamageEnemy(enemy, damage);
                
                // 生成击中特效
                SpawnHitEffect(hit.point);
                
                // 应用特殊效果
                ApplyEffects(enemy);
                
                // 显示穿刺特效
                if (pierceEffect != null)
                {
                    GameObject effect = Instantiate(pierceEffect, hit.point, Quaternion.LookRotation(thrustDirection));
                    Destroy(effect, 1f);
                }
            }
        }
        */
        // 突刺冷却
        yield return new WaitForSeconds(0.5f);
        isThrusting = false;
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
            int damage = CalculateDamage(false);
            
            // 对敌人造成伤害
            DamageEnemy(enemy, damage);
            
            // 生成击中特效
            SpawnHitEffect(other.ClosestPoint(spearTip.position));
            
            // 播放击中音效
            if (useSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(useSound);
            }
            
            // 应用特殊效果
            ApplyEffects(enemy);
        }
    }
    
    /// <summary>
    /// 计算伤害值
    /// </summary>
    protected int CalculateDamage(bool isThrust)
    {
        // 如果是突刺，增加伤害
        if (isThrust)
        {
            return Mathf.RoundToInt(baseDamage * thrustDamageMultiplier);
        }
        
        return baseDamage;
    }
    
    /// <summary>
    /// 计算基础伤害
    /// </summary>
    protected override int CalculateDamage()
    {
        return baseDamage;
    }
    
    /// <summary>
    /// 应用长矛特有的效果
    /// </summary>
    protected override void ApplyEffects(Enemy enemy)
    {
        // 长矛可以有穿透或推后效果，但这里简化处理
    }
}