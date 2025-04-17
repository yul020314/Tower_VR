using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 弩箭类，处理飞行和伤害计算
/// </summary>
public class CrossbowBolt : MonoBehaviour
{
    [Header("箭属性")]
    [SerializeField] private int damage = 25; // 默认伤害值
    [SerializeField] private float speed = 30f; // 默认速度
    [SerializeField] private float maxRange = 50f; // 最大射程
    [SerializeField] private float destroyDelay = 5f; // 射中后销毁延迟
    [SerializeField] private float lifeTime = 10f; // 箭的最大存在时间
    
    [Header("特效")]
    [SerializeField] private GameObject hitEffect; // 命中特效
    [SerializeField] private AudioClip hitSound; // 命中音效
    
    private Rigidbody rb;
    private bool hasHit = false;
    private float distanceTraveled = 0f;
    private Vector3 lastPosition;
    private AudioSource audioSource;
    
    /// <summary>
    /// 初始化组件引用
    /// </summary>
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        
        // 如果没有音频源，创建一个
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1.0f; // 3D音效
            audioSource.volume = 0.7f;
        }
    }
    
    /// <summary>
    /// 初始化箭的状态
    /// </summary>
    private void Start()
    {
        lastPosition = transform.position;
        
        // 设置初始速度
        if (rb != null)
        {
            rb.velocity = transform.forward * speed;
        }
        
        // 设置最大存在时间
        Destroy(gameObject, lifeTime);
    }
    
    /// <summary>
    /// 初始化箭的参数
    /// </summary>
    public void Initialize(int newDamage, float newSpeed, float newMaxRange)
    {
        damage = newDamage;
        speed = newSpeed;
        maxRange = newMaxRange;
        
        // 应用速度
        if (rb != null)
        {
            rb.velocity = transform.forward * speed;
        }
    }
    
    /// <summary>
    /// 每帧更新，处理飞行轨迹和距离计算
    /// </summary>
    private void Update()
    {
        if (hasHit) return;
        
        // 让箭头始终朝向飞行方向
        if (rb.velocity.sqrMagnitude > 0.1f)
        {
            transform.rotation = Quaternion.LookRotation(rb.velocity);
        }
        
        // 计算已飞行距离
        distanceTraveled += Vector3.Distance(transform.position, lastPosition);
        lastPosition = transform.position;
        
        // 检查是否超出最大射程
        if (distanceTraveled >= maxRange)
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// 处理碰撞事件
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;
        
        // 检测敌人
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null && !enemy.IsDead)
        {
            // 对敌人造成伤害
            enemy.TakeDamage(damage);
            
            // 播放命中音效
            if (hitSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(hitSound);
            }
            
            // 生成命中特效
            if (hitEffect != null)
            {
                Instantiate(hitEffect, transform.position, Quaternion.identity);
            }
            
            // 处理箭的附着
            AttachArrowToTarget(other.transform);
        }
        else if (!other.isTrigger && other.CompareTag("Environment"))
        {
            // 如果击中环境，停止移动并附着
            AttachArrowToEnvironment(other);
        }
    }
    
    /// <summary>
    /// 将箭附着到目标上
    /// </summary>
    private void AttachArrowToTarget(Transform target)
    {
        hasHit = true;
        
        // 停止物理模拟
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.velocity = Vector3.zero;
        }
        
        // 附着到目标
        transform.SetParent(target);
        
        // 延迟销毁
        Destroy(gameObject, destroyDelay);
    }
    
    /// <summary>
    /// 将箭附着到环境中
    /// </summary>
    private void AttachArrowToEnvironment(Collider surface)
    {
        hasHit = true;
        
        // 停止物理模拟
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.velocity = Vector3.zero;
        }
        
        // 找到击中点
        RaycastHit hit;
        if (Physics.Raycast(transform.position - transform.forward * 0.5f, transform.forward, out hit, 1f))
        {
            // 将箭头稍微嵌入表面
            transform.position = hit.point - transform.forward * 0.1f;
            
            // 根据表面法线调整箭的方向
            transform.rotation = Quaternion.LookRotation(Vector3.Reflect(transform.forward, hit.normal));
        }
        
        // 延迟销毁
        Destroy(gameObject, destroyDelay);
    }
}