using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// 双手斧武器类，碰到敌人造成固定伤害，并有强力眩晕效果
/// </summary>
public class TwoHandedAxe : Weapon
{
    [Header("双手斧特性")]
    [SerializeField] private float stunDuration = 3.5f; // 眩晕效果持续时间，比普通斧更长
    [SerializeField] private Transform axeHead; // 斧头部分
    [SerializeField] private GameObject stunEffect; // 眩晕效果预制体
    [SerializeField] private float cleaveRadius = 1.5f; // 范围伤害半径
    
    private Rigidbody rb;
    private float lastSwingTime = 0f;
    private const float swingSoundCooldown = 0.6f; // 挥斧音效冷却时间
    
    /// <summary>
    /// 初始化组件引用
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody>();
        
        // 如果没有设置斧头位置，使用当前物体的变换
        if (axeHead == null)
        {
            axeHead = transform;
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
            SpawnHitEffect(other.ClosestPoint(axeHead.position));
            
            // 播放击中音效
            PlayHitSound();
            
            // 应用特殊效果
            ApplyEffects(enemy);
            
            // 范围伤害
            ApplyCleaveDamage(enemy);
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
    /// 播放挥斧音效
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
    /// 应用双手斧特有的效果：强力眩晕
    /// </summary>
    protected override void ApplyEffects(Enemy enemy)
    {
        // 眩晕效果
        StartCoroutine(ApplyStun(enemy));
    }
    
    /// <summary>
    /// 应用范围伤害效果
    /// </summary>
    private void ApplyCleaveDamage(Enemy primaryTarget)
    {
        // 寻找范围内的其他敌人
        Collider[] hitColliders = Physics.OverlapSphere(primaryTarget.transform.position, cleaveRadius);
        foreach (var hitCollider in hitColliders)
        {
            Enemy nearbyEnemy = hitCollider.GetComponent<Enemy>();
            
            // 如果找到另一个敌人（不是主要目标）且未击中过
            /*if (nearbyEnemy != null && nearbyEnemy != primaryTarget && !nearbyEnemy.IsDead && !HasHitEnemy(nearbyEnemy))
            {
                // 造成一半伤害
                int cleaveDamage = baseDamage / 2;
                DamageEnemy(nearbyEnemy, cleaveDamage);
                
                // 轻微眩晕效果
                // nearbyEnemy.ModifyMoveSpeed(0.7f, stunDuration / 2);
            }*/
        }
    }
    
    /// <summary>
    /// 应用眩晕效果的协程
    /// </summary>
    private IEnumerator ApplyStun(Enemy enemy)
    {
        if (enemy == null || enemy.IsDead) yield break;
        
        // 应用眩晕效果 - 使敌人减速
        // enemy.ModifyMoveSpeed(0.3f, stunDuration);
        
        // 显示眩晕特效
        GameObject effect = null;
        if (stunEffect != null)
        {
            effect = Instantiate(stunEffect, enemy.transform.position + Vector3.up * 2f, Quaternion.identity);
            effect.transform.SetParent(enemy.transform);
        }
        
        // 等待眩晕结束
        yield return new WaitForSeconds(stunDuration);
        
        // 清理特效
        if (effect != null)
        {
            Destroy(effect);
        }
    }
}