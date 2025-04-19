using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public abstract class Weapon : MonoBehaviour
{
    [Header("基础武器属性")]
    [SerializeField] protected int baseDamage = 10;
    [SerializeField] protected float attackCooldown = 0.3f;
    
    [Header("基础效果")]
    [SerializeField] protected GameObject hitEffect;
    [SerializeField] protected AudioClip hitSound;
    [SerializeField] protected AudioClip useSound;
    
    [Header("左右手抓取设置")]
    [SerializeField] private Transform leftHandAttachTransform;
    [SerializeField] private Transform rightHandAttachTransform;

    protected AudioSource audioSource;
    protected XRGrabInteractable interactable;
    protected bool isHeld = false;
    protected Dictionary<Enemy, float> hitEnemies = new Dictionary<Enemy, float>();
    
    protected virtual void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        interactable = GetComponent<XRGrabInteractable>();
        leftHandAttachTransform = transform.Find("leftHandAttachTransform");
        rightHandAttachTransform = transform.Find("rightHandAttachTransform");

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // 配置XR交互
        if (interactable != null)
        {
            interactable.selectEntered.AddListener(OnGrab);
            interactable.selectExited.AddListener(OnRelease);
            interactable.activated.AddListener(SpecialEvent);
        }
    }

    
    protected virtual void OnGrab(SelectEnterEventArgs args)
    {
        isHeld = true;
        
        Transform interactor = args.interactorObject.transform; 
        if (interactor.name.ToLower().Contains("left"))
        {
            interactable.attachTransform = leftHandAttachTransform;
        }
        else if (interactor.name.ToLower().Contains("right"))
        {
            interactable.attachTransform = rightHandAttachTransform;
        }
    }

    protected virtual void OnRelease(SelectExitEventArgs args)
    {
        isHeld = false;
    }
    
    protected virtual void SpecialEvent(ActivateEventArgs arg0)
    {
        //事件，eg：扣动扳机
    }
    
    protected virtual void PlayHitSound()
    {
        if (hitSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hitSound);
        }
    }
    
    protected virtual void PlayUseSound()
    {
        if (useSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(useSound);
        }
    }
    
    protected virtual void SpawnHitEffect(Vector3 position)
    {
        if (hitEffect != null)
        {
            Instantiate(hitEffect, position, Quaternion.identity);
        }
    }
    
    // 伤害计算基础方法，子类可重写
    protected virtual int CalculateDamage()
    {
        return baseDamage;
    }
    
    // 对敌人造成伤害
    protected virtual void DamageEnemy(Enemy enemy, int damage)
    {
        if (enemy != null && !enemy.IsDead)
        {
            // 检查冷却时间
            if (hitEnemies.TryGetValue(enemy, out float lastHitTime))
            {
                if (Time.time - lastHitTime < attackCooldown)
                {
                    return;
                }
            }
            
            // 造成伤害
            enemy.TakeDamage(damage);
            
            // 记录击中时间
            hitEnemies[enemy] = Time.time;
            
            // 音效和视觉效果
            PlayHitSound();
        }
    }
    
    // 清理已击中敌人字典
    protected virtual void CleanupHitEnemies()
    {
        List<Enemy> enemiesToRemove = new List<Enemy>();
        
        foreach (var pair in hitEnemies)
        {
            if (pair.Key == null || pair.Key.IsDead || Time.time - pair.Value > attackCooldown * 3)
            {
                enemiesToRemove.Add(pair.Key);
            }
        }
        
        foreach (var enemy in enemiesToRemove)
        {
            hitEnemies.Remove(enemy);
        }
    }
    
    // 用于施加特殊效果的虚方法
    protected virtual void ApplyEffects(Enemy enemy) 
    {
        // 基类不实现任何效果，由子类实现
    }
} 