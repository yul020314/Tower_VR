using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// 弯刀武器类，碰到敌人造成固定伤害，并有流血效果
/// </summary>
public class CurvedDagger : Weapon
{
    [Header("弯刀特性")]
    [SerializeField] private Transform bladePoint; // 刀刃位置
    [SerializeField] private bool causesBleeding = true; // 是否造成流血
    [SerializeField] private int bleedingDamage = 2; // 流血每次伤害
    [SerializeField] private float bleedingDuration = 5f; // 流血持续时间
    [SerializeField] private float bleedingTickRate = 1f; // 流血伤害频率（秒）
    [SerializeField] private GameObject bleedingEffect; // 流血特效
    [SerializeField] private AudioClip bleedingSound; // 流血音效
    
    private Rigidbody rb;
    private float lastStrikeTime = 0f;
    private const float strikeCooldown = 0.3f; // 攻击冷却时间
    
    // 记录正在流血的敌人
    private Dictionary<Enemy, Coroutine> bleedingEnemies = new Dictionary<Enemy, Coroutine>();
    
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
            // 计算伤害
            int damage = CalculateDamage();
            
            // 对敌人造成伤害
            DamageEnemy(enemy, damage);
            
            // 生成击中特效
            Vector3 hitPoint = other.ClosestPoint(bladePoint.position);
            SpawnHitEffect(hitPoint);
            
            // 播放击中音效
            if (useSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(useSound);
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
    protected override int CalculateDamage()
    {
        return baseDamage;
    }
    
    /// <summary>
    /// 应用弯刀特有的效果：流血
    /// </summary>
    protected override void ApplyEffects(Enemy enemy)
    {
        if (!causesBleeding || enemy == null || enemy.IsDead) return;
        
        // 如果敌人已经在流血，停止之前的流血效果
        if (bleedingEnemies.ContainsKey(enemy))
        {
            if (bleedingEnemies[enemy] != null)
            {
                StopCoroutine(bleedingEnemies[enemy]);
            }
            bleedingEnemies.Remove(enemy);
        }
        
        // 开始新的流血效果
        Coroutine bleedingRoutine = StartCoroutine(ApplyBleedingEffect(enemy));
        bleedingEnemies.Add(enemy, bleedingRoutine);
    }
    
    /// <summary>
    /// 应用流血效果的协程
    /// </summary>
    private IEnumerator ApplyBleedingEffect(Enemy enemy)
    {
        // 生成流血特效
        GameObject effect = null;
        if (bleedingEffect != null)
        {
            effect = Instantiate(bleedingEffect, enemy.transform.position, Quaternion.identity);
            effect.transform.SetParent(enemy.transform);
        }
        
        // 播放流血音效
        if (bleedingSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(bleedingSound);
        }
        
        float elapsedTime = 0f;
        float nextTickTime = 0f;
        
        // 流血持续伤害循环
        while (elapsedTime < bleedingDuration && enemy != null && !enemy.IsDead)
        {
            elapsedTime += Time.deltaTime;
            
            // 定期造成流血伤害
            if (Time.time >= nextTickTime)
            {
                enemy.TakeDamage(bleedingDamage);
                nextTickTime = Time.time + bleedingTickRate;
            }
            
            yield return null;
        }
        
        // 清理特效
        if (effect != null)
        {
            Destroy(effect);
        }
        
        // 移除敌人记录
        if (enemy != null && bleedingEnemies.ContainsKey(enemy))
        {
            bleedingEnemies.Remove(enemy);
        }
    }
    
    /// <summary>
    /// 清理对象时停止所有协程
    /// </summary>
    private void OnDestroy()
    {
        // 停止所有流血协程
        foreach (var routine in bleedingEnemies.Values)
        {
            if (routine != null)
            {
                StopCoroutine(routine);
            }
        }
        bleedingEnemies.Clear();
    }
}