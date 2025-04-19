using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class BowController : MonoBehaviour
{
    [SerializeField] private BowString bowStringRenderer;
    
    [SerializeField] private XRGrabInteractable grabbable,interactable;

    [SerializeField] private Transform midPointGrabObject, midPointVisualObject, midPointParent;

    [SerializeField] private float bowStringStretchLimit = 0.25f;
    
    [Header("左右手抓取设置")]
    [SerializeField] private Transform leftHandAttachTransform;
    [SerializeField] private Transform rightHandAttachTransform;
    
    private Transform interactor;

    private float strength;

    public UnityEvent OnBowPulled;
    public UnityEvent<float> OnBowReleased;

    private void Awake()
    {
        leftHandAttachTransform = transform.Find("leftHandAttachTransform");
        rightHandAttachTransform = transform.Find("rightHandAttachTransform");
        grabbable = GetComponent<XRGrabInteractable>();
        interactable = midPointGrabObject.GetComponent<XRGrabInteractable>();
    }

    private void Start()
    {
        grabbable.selectEntered.AddListener(OnGrab);
        interactable.selectEntered.AddListener(PreparBowString);
        interactable.selectExited.AddListener(ResetBowString);
    }

    private void OnGrab(SelectEnterEventArgs arg0)
    {
        interactor = arg0.interactorObject.transform; 
        if (interactor.name.ToLower().Contains("left"))
        {
            grabbable.attachTransform = leftHandAttachTransform;
        }
        else if (interactor.name.ToLower().Contains("right"))
        {
            grabbable.attachTransform = rightHandAttachTransform;
        }
    }
    private void PreparBowString(SelectEnterEventArgs arg0)
    {
        interactor = arg0.interactableObject.transform;
        OnBowPulled?.Invoke();
    }

    private void ResetBowString(SelectExitEventArgs arg0)
    {
        OnBowReleased?.Invoke(strength);
        strength = 0;
        
        interactor = null;
        midPointGrabObject.localPosition = Vector3.zero;
        midPointVisualObject.localPosition = Vector3.zero;
        bowStringRenderer.CreateString(null);
    }

    private void Update()
    {
        if (interactor != null)
        {
            Vector3 midPointLocalSpace = 
                midPointParent.InverseTransformPoint(midPointGrabObject.position);

            float midPointLocalZAbs = Mathf.Abs(midPointLocalSpace.z);

            HandleStringPushedBackToStart(midPointLocalSpace);

            HandleStringPulledBackTolimit(midPointLocalZAbs, midPointLocalSpace);

            HandlePullingString(midPointLocalZAbs, midPointLocalSpace);
            
            bowStringRenderer.CreateString(midPointVisualObject.position);
        }
    }

    private void HandleStringPushedBackToStart(Vector3 midPointLocalSpace)
    {
        if (midPointLocalSpace.z >= 0)
        {
            strength = 0;
            midPointVisualObject.localPosition = Vector3.zero;
        }
    }

    private void HandleStringPulledBackTolimit(float midPointLocalZAbs, Vector3 midPointLocalSpace)
    {
        if (midPointLocalSpace.z < 0 && midPointLocalZAbs >= bowStringStretchLimit)
        {
            strength = 1;
            //Vector3 direction = midPointParent.TransformDirection(new Vector3(0, 0, midPointLocalSpace.z));
            midPointVisualObject.localPosition = new Vector3(0, 0, -bowStringStretchLimit);
        }
    }

    private void HandlePullingString(float midPointLocalZAbs, Vector3 midPointLocalSpace)
    {
        if (midPointLocalSpace.z < 0 && midPointLocalZAbs < bowStringStretchLimit)
        {
            strength = Remap(midPointLocalZAbs, 0, bowStringStretchLimit, 0, 1);
            midPointVisualObject.localPosition = new Vector3(0, 0, midPointLocalSpace.z);
        }
    }

    private float Remap(float value, int fromMin, float fromMax, int toMin, int toMax)
    {
        return (value - fromMin) / (fromMax - fromMin) * (toMax - toMin) + toMin;
    }
}
