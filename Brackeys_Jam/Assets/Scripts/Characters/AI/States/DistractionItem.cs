using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider)), RequireComponent(typeof(Rigidbody))]
public class DistractionItem : MonoBehaviour, Interactable
{
    [SerializeField] private DistractionPoint distractionPoint;
    [SerializeField] private float releaseForce = 1.5f;
    [SerializeField] private float distractionTimeAtPoint = 5f;
    private Rigidbody rb;
    private Collider col;
    private bool isHeld = false;
    private Transform parent;

    public float DistractionTimeAtPoint { get => distractionTimeAtPoint; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        rb.isKinematic = true;
    }
    private void Update()
    {
        if(parent != null)
            transform.localRotation = parent.rotation;
    }
    public void Interact()
    {
        if (isHeld)
            Release();        
    }

    internal void Release()
    {
        transform.SetParent(null);
        rb.isKinematic = false;
        rb.useGravity = true;
        col.isTrigger = false;
        rb.AddExplosionForce(releaseForce, transform.position + Vector3.down * 0.05f, 0.05f);
        isHeld = false;
        parent = null;
    }

    internal void Grab(GameObject interactableHoldPoint)
    {
        rb.useGravity = false;
        rb.isKinematic = true;
        col.isTrigger = true;
        parent = interactableHoldPoint.transform;
        transform.SetParent(parent);
        transform.localRotation = parent.rotation;
        transform.localPosition = Vector3.zero;
        isHeld = true;
    }

    internal void ChangeHolder(DistractedState ds)
    {
        Release();
        Grab(ds.GetHoldPoint());
        ds.SetDistractionItem(this);
    }

    internal DistractionPoint GetDistractionPoint()
    {
        return distractionPoint;
    }

    internal void Destory()
    {
        Destroy(gameObject);
    }
}
