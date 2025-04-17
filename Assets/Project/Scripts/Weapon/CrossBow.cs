using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

/// <summary>
/// 弩武器类，可以发射弩箭造成远程伤害
/// </summary>
public class CrossBow : Weapon
{
    [Header("弩设置")]
    [SerializeField] private GameObject arrowPrefab; // 弩箭预制体
    [SerializeField] private Transform arrowSpawnPoint; // 弩箭生成位置
    [SerializeField] private float arrowSpeed = 30f; // 弩箭速度
    [SerializeField] private float reloadTime = 1.5f; // 装弹时间
    [SerializeField] private int arrowDamage = 25; // 弩箭伤害
    [SerializeField] private float maxRange = 50f; // 最大射程
    
    [Header("音效")]
    [SerializeField] private AudioClip reloadSound; // 装弹音效
    [SerializeField] private AudioClip fireSound; // 发射音效
    
    private Animator animator; // 动画控制器
    private bool isLoaded = false; // 是否已装弹
    private bool isReloading = false; // 是否正在装弹
    private XRGrabInteractable grabInteractable; // 抓取交互组件

    // 动画参数名称
    private static readonly int IsReady = Animator.StringToHash("isReady");
    private static readonly int IsFire = Animator.StringToHash("isFire");


    /// <summary>
    /// 初始化组件引用
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        animator = GetComponent<Animator>();
        grabInteractable = GetComponent<XRGrabInteractable>();
    }
    
    /// <summary>
    /// 当武器被抓取时的事件处理
    /// </summary>
    protected override void OnGrab(SelectEnterEventArgs args)
    {
        base.OnGrab(args);
        
        // 进入装弹状态
        if (!isLoaded && !isReloading)
        {
            StartCoroutine(ReloadCrossBow());
        }
    }
    
    /// <summary>
    /// 当武器被释放时的事件处理
    /// </summary>
    protected override void OnRelease(SelectExitEventArgs args)
    {
        base.OnRelease(args);
    }

    /// <summary>
    /// 当Trigger按钮被按下时触发发射弩箭
    /// </summary>
    protected override void SpecialEvent(ActivateEventArgs args)
    {
        base.SpecialEvent(args);

        if (isLoaded && !isReloading)
        {
            FireArrow();
        }
    }
    
    /// <summary>
    /// 装弹过程
    /// </summary>
    private IEnumerator ReloadCrossBow()
    {
        isReloading = true;
        
        // 播放装弹动画
        if (animator != null)
        {
            animator.SetBool(IsReady, true);
            animator.SetBool(IsFire, false);
        }
        
        // 播放装弹音效
        if (reloadSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(reloadSound);
        }
        
        // 等待装弹时间
        yield return new WaitForSeconds(reloadTime);
        
        isLoaded = true;
        isReloading = false;
    }
    
    /// <summary>
    /// 发射弩箭
    /// </summary>
    private void FireArrow()
    {
        // 播放发射动画
        if (animator != null)
        {
            animator.SetBool(IsFire, true);
            animator.SetBool(IsReady, false);
            // 强制更新动画状态机
            animator.Update(0); 
        }
        
        // 播放发射音效
        if (fireSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(fireSound);
        }
        
        // 生成弩箭
        if (arrowPrefab != null)
        {
            GameObject arrowObj = Instantiate(arrowPrefab, arrowSpawnPoint.position, arrowSpawnPoint.rotation);
            CrossbowBolt arrow = arrowObj.GetComponent<CrossbowBolt>();
            
            if (arrow != null)
            {
                // 设置弩箭属性
                arrow.Initialize(arrowDamage, arrowSpeed, maxRange);
            }
            else
            {
                // 如果没有Arrow组件，直接给予物理冲力
                Rigidbody arrowRb = arrowObj.GetComponent<Rigidbody>();
                if (arrowRb != null)
                {
                    arrowRb.velocity = arrowSpawnPoint.forward * arrowSpeed;
                }
            }
        }
        
        // 重置状态
        isLoaded = false;
        
        // 开始重新装弹
        StartCoroutine(ReloadCrossBow());
    }
}