using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickingArrowToSurface : MonoBehaviour
{
    [SerializeField] 
    private Rigidbody rb;
    [SerializeField] 
    private SphereCollider myCollider;
    
    [SerializeField] 
    private GameObject stickingArrow;

    private void OnCollisionEnter(Collision collision)
    {
        rb.isKinematic = true;
        myCollider.isTrigger = true;

        GameObject arrow = Instantiate(stickingArrow);
        arrow.transform.position = transform.position;
        arrow.transform.forward = transform.forward;

        if (collision.collider.attachedRigidbody != null)
        {
            arrow.transform.parent = collision.collider.attachedRigidbody.transform;
            if (collision.collider.gameObject == GameObject.FindGameObjectWithTag("Enemy"))
            {
                collision.collider.gameObject.GetComponent<Enemy>().TakeDamage(10);
            }
        }

        collision.collider.GetComponent<IHittable>()?.GetHit();
        
        Destroy(gameObject);
        Destroy(arrow.gameObject, 1);
    }
}
