using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

public class MagicWand : Weapon
{
    [Header("魔法杖特性")]
    [SerializeField] private GameObject magicProjectilePrefab; // 魔法弹预制体
    [SerializeField] private Transform spellSpawnPoint; // 法术生成点
    [SerializeField] private float projectileSpeed = 15f; // 魔法弹速度
    [SerializeField] private float projectileLifetime = 5f; // 魔法弹存活时间
    [SerializeField] private float castingCooldown = 0.5f; // 施法冷却时间
    
    [Header("魔法效果")]
    [SerializeField] private ParticleSystem chargingEffect; // 充能特效
    [SerializeField] private ParticleSystem castingEffect; // 施法特效
    [SerializeField] private AudioClip chargingSound; // 充能音效
    [SerializeField] private Color magicColor = Color.blue; // 魔法颜色
    
    [Header("次要魔法")]
    [SerializeField] private GameObject secondarySpellPrefab; // 次要法术预制体
    [SerializeField] private float secondaryCooldown = 5f; // 次要法术冷却时间
    [SerializeField] private float chargeDuration = 1.5f; // 充能持续时间
    
    private float nextPrimaryFireTime = 0f;
    private float nextSecondaryFireTime = 0f;
    private bool isCharging = false;
    private float chargeStartTime = 0f;
    private InputAction primaryAction;
    private InputAction secondaryAction;
    
    protected override void Awake()
    {
        base.Awake();
        
        if (spellSpawnPoint == null)
        {
            spellSpawnPoint = transform.Find("SpellSpawnPoint");
            if (spellSpawnPoint == null)
            {
                spellSpawnPoint = transform;
            }
        }
        
        // 设置充能效果为禁用状态
        if (chargingEffect != null)
        {
            chargingEffect.Stop();
        }
    }
    
    private void Start()
    {
        // 获取输入动作引用
        SetupInputActions();
    }
    
    private void OnEnable()
    {
        // 启用输入动作
        EnableInputActions();
    }
    
    private void OnDisable()
    {
        // 禁用输入动作
        DisableInputActions();
    }
    
    protected override void OnGrab(SelectEnterEventArgs args)
    {
        base.OnGrab(args);
        
        // 启用输入动作
        EnableInputActions();
    }
    
    protected override void OnRelease(SelectExitEventArgs args)
    {
        base.OnRelease(args);
        
        // 停止充能
        StopCharging();
        
        // 禁用输入动作
        DisableInputActions();
    }
    
    private void Update()
    {
        if (!isHeld) return;
        
        // 处理充能状态
        if (isCharging)
        {
            float chargeTime = Time.time - chargeStartTime;
            
            // 如果充能完成，释放次要法术
            if (chargeTime >= chargeDuration)
            {
                CastSecondarySpell();
                StopCharging();
            }
        }
    }
    
    // 设置输入动作
    private void SetupInputActions()
    {
        // 尝试获取现代 ActionBasedController
        ActionBasedController actionController = interactable.GetOldestInteractorSelecting()?.transform.GetComponent<ActionBasedController>();
        if (actionController != null)
        {
            // 尝试不同的属性名称 (不同版本的XR Interaction Toolkit可能使用不同名称)
            if (actionController.activateAction != null && actionController.selectAction != null)
            {
                // 使用activateAction和selectAction (较新版本的XR Toolkit)
                primaryAction = actionController.activateAction.action;
                secondaryAction = actionController.selectAction.action;
                
                primaryAction.performed += OnPrimaryAction;
                secondaryAction.performed += OnSecondaryAction;
                secondaryAction.canceled += OnSecondaryActionReleased;
            }
            else
            {
                Debug.LogWarning("无法找到ActionBasedController的标准输入引用，尝试使用反射");
            }
        }

        // 如果上述方法失败，使用反射尝试获取属性
        if (primaryAction == null || secondaryAction == null)
        {
            // 尝试使用通用方法获取输入动作
            var controller = interactable.GetOldestInteractorSelecting();
            if (controller != null)
            {
                // 尝试反射获取所有可能的属性名称
                var possibleActivateNames = new[] { "activateAction", "activateActionReference", "activateInput" };
                var possibleSelectNames = new[] { "selectAction", "selectActionReference", "selectInput" };
                
                InputAction foundActivateAction = null;
                InputAction foundSelectAction = null;
                
                // 尝试不同的属性名称
                foreach (var activateName in possibleActivateNames)
                {
                    var property = controller.GetType().GetProperty(activateName);
                    if (property != null)
                    {
                        var actionObj = property.GetValue(controller);
                        if (actionObj != null)
                        {
                            // 尝试直接获取action属性或转换为InputAction
                            var actionProperty = actionObj.GetType().GetProperty("action");
                            if (actionProperty != null)
                            {
                                foundActivateAction = (InputAction)actionProperty.GetValue(actionObj);
                                break;
                            }
                            else if (actionObj is InputAction)
                            {
                                foundActivateAction = (InputAction)actionObj;
                                break;
                            }
                        }
                    }
                }
                
                // 同样尝试select动作
                foreach (var selectName in possibleSelectNames)
                {
                    var property = controller.GetType().GetProperty(selectName);
                    if (property != null)
                    {
                        var actionObj = property.GetValue(controller);
                        if (actionObj != null)
                        {
                            // 尝试直接获取action属性或转换为InputAction
                            var actionProperty = actionObj.GetType().GetProperty("action");
                            if (actionProperty != null)
                            {
                                foundSelectAction = (InputAction)actionProperty.GetValue(actionObj);
                                break;
                            }
                            else if (actionObj is InputAction)
                            {
                                foundSelectAction = (InputAction)actionObj;
                                break;
                            }
                        }
                    }
                }
                
                // 如果找到了两个动作，使用它们
                if (foundActivateAction != null && foundSelectAction != null)
                {
                    primaryAction = foundActivateAction;
                    secondaryAction = foundSelectAction;
                    
                    primaryAction.performed += OnPrimaryAction;
                    secondaryAction.performed += OnSecondaryAction;
                    secondaryAction.canceled += OnSecondaryActionReleased;
                }
            }
        }
        
        // 如果上述方法都失败，使用备用输入方法
        if (primaryAction == null || secondaryAction == null)
        {
            Debug.LogWarning("无法获取XR控制器输入动作，将使用键盘输入作为备用");
            
            // 创建备用输入动作
            primaryAction = new InputAction("PrimaryAction", InputActionType.Button);
            primaryAction.AddBinding("<Keyboard>/space");
            
            secondaryAction = new InputAction("SecondaryAction", InputActionType.Button);
            secondaryAction.AddBinding("<Keyboard>/e");
            
            primaryAction.performed += OnPrimaryAction;
            secondaryAction.performed += OnSecondaryAction;
            secondaryAction.canceled += OnSecondaryActionReleased;
            
            primaryAction.Enable();
            secondaryAction.Enable();
        }
    }
    
    private void EnableInputActions()
    {
        if (primaryAction != null)
        {
            primaryAction.performed += OnPrimaryAction;
            primaryAction.Enable();
        }
        
        if (secondaryAction != null)
        {
            secondaryAction.performed += OnSecondaryAction;
            secondaryAction.canceled += OnSecondaryActionReleased;
            secondaryAction.Enable();
        }
    }
    
    private void DisableInputActions()
    {
        if (primaryAction != null)
        {
            primaryAction.performed -= OnPrimaryAction;
            primaryAction.Disable();
        }
        
        if (secondaryAction != null)
        {
            secondaryAction.performed -= OnSecondaryAction;
            secondaryAction.canceled -= OnSecondaryActionReleased;
            secondaryAction.Disable();
        }
    }
    
    // 主要攻击动作
    private void OnPrimaryAction(InputAction.CallbackContext context)
    {
        if (!isHeld) return;
        
        if (Time.time > nextPrimaryFireTime)
        {
            CastPrimarySpell();
            nextPrimaryFireTime = Time.time + castingCooldown;
        }
    }
    
    // 次要攻击动作
    private void OnSecondaryAction(InputAction.CallbackContext context)
    {
        if (!isHeld) return;
        
        if (Time.time > nextSecondaryFireTime && !isCharging)
        {
            StartCharging();
        }
    }
    
    // 次要攻击按钮释放
    private void OnSecondaryActionReleased(InputAction.CallbackContext context)
    {
        if (!isHeld) return;
        
        // 如果正在充能但未完成，取消充能
        if (isCharging && (Time.time - chargeStartTime) < chargeDuration)
        {
            StopCharging();
        }
    }
    
    // 施放主要法术（魔法弹）
    private void CastPrimarySpell()
    {
        if (magicProjectilePrefab == null) return;
        
        // 播放施法音效
        PlayUseSound();
        
        // 播放施法特效
        if (castingEffect != null)
        {
            castingEffect.Play();
        }
        
        // 创建魔法弹
        GameObject projectile = Instantiate(magicProjectilePrefab, spellSpawnPoint.position, spellSpawnPoint.rotation);
        
        // 设置魔法弹颜色
        Renderer projectileRenderer = projectile.GetComponent<Renderer>();
        if (projectileRenderer != null)
        {
            projectileRenderer.material.color = magicColor;
        }
        
        // 设置魔法弹伤害和效果
        /*MagicProjectile magicProjectileScript = projectile.GetComponent<MagicProjectile>();
        if (magicProjectileScript != null)
        {
            magicProjectileScript.Initialize(baseDamage, projectileLifetime, projectileSpeed, magicColor);
        }
        else
        {
            // 如果没有魔法弹脚本，直接添加力
            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = spellSpawnPoint.forward * projectileSpeed;
            }
            
            // 设置自动销毁
            Destroy(projectile, projectileLifetime);
        }*/
    }
    
    // 开始充能次要法术
    private void StartCharging()
    {
        isCharging = true;
        chargeStartTime = Time.time;
        
        // 播放充能特效
        if (chargingEffect != null)
        {
            chargingEffect.Play();
        }
        
        // 播放充能音效
        if (chargingSound != null && audioSource != null)
        {
            audioSource.clip = chargingSound;
            audioSource.Play();
        }
    }
    
    // 停止充能
    private void StopCharging()
    {
        isCharging = false;
        
        // 停止充能特效
        if (chargingEffect != null)
        {
            chargingEffect.Stop();
        }
        
        // 停止充能音效
        if (audioSource != null && audioSource.clip == chargingSound)
        {
            audioSource.Stop();
        }
    }
    
    // 施放次要法术（区域效果）
    private void CastSecondarySpell()
    {
        if (secondarySpellPrefab == null) return;
        
        // 播放施法音效
        PlayUseSound();
        
        // 播放施法特效
        if (castingEffect != null)
        {
            castingEffect.Play();
        }
        
        // 创建次要法术
        GameObject secondarySpell = Instantiate(secondarySpellPrefab, spellSpawnPoint.position + spellSpawnPoint.forward * 5f, Quaternion.identity);
        
        // 设置次要法术伤害和效果
        /*AreaSpell areaSpell = secondarySpell.GetComponent<AreaSpell>();
        if (areaSpell != null)
        {
            areaSpell.Initialize(baseDamage * 2, 5f, magicColor);
        }
        else
        {
            // 设置自动销毁
            Destroy(secondarySpell, 5f);
        }*/
        
        // 设置冷却时间
        nextSecondaryFireTime = Time.time + secondaryCooldown;
    }
} 