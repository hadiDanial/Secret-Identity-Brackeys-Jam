using SensorToolkit;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(RangeSensor)), RequireComponent(typeof(NavMeshObstacle)), RequireComponent(typeof(AudioSource))]
public class DistractionPoint : Waypoint, Interactable
{
    [SerializeField, Range(1, 10)] private int maxAICount = 2;
    [SerializeField, Range(0.15f, 3f)] private float maxStandingDistanceFromPoint = 1f;
    [SerializeField] private List<DistractedState> aiList;
    [SerializeField] private bool isActive = false;
    [SerializeField] private float timer = 0;

    private Animator animator;
    private RangeSensor rangeSensor;
    private AudioSource audioSource;
    private void Awake()
    {
        TryGetComponent<Animator>(out animator);
        rangeSensor = GetComponent<RangeSensor>();
        audioSource = GetComponent<AudioSource>();
        audioSource.loop = true;
    }
    private void Update()
    {
        if (isActive)
        {
            if (aiList.Count > 0)
                timer += Time.deltaTime;
            if (timer >= waitTime)
                Deactivate();
        }
        else
        {
            if (aiList.Count > 0)
                StopDistracting();
        }
    }

    public void Activate()
    {
        timer = 0;
        isActive = true;
        if(animator != null)
        {
            animator.Play(animationTriggerName);
        }
        List<DistractedState> newlyDistracted = new List<DistractedState>();
        foreach (GameObject item in rangeSensor.DetectedObjects)
        {
            DistractedState d;
            if(item.TryGetComponent<DistractedState>(out d))
            {
                newlyDistracted.Add(d);
            }
        }
        for (int i = 0; i < newlyDistracted.Count && maxAICount - aiList.Count > 0; i++)
        {
            if(AddAIToDistraction(newlyDistracted[i]))
            {
                newlyDistracted[i].SetDistractionPoint(this);
                newlyDistracted[i].StartDistracting();
            }            
        }
        audioSource.Play();
    }
  
    public void Deactivate()
    {
        if (animator != null)
        {
            animator.StopPlayback();
        }
        isActive = false;
        StopDistracting();
        audioSource.Stop();
    }
    internal bool IsActive()
    {
        return isActive;
    }


    private void StopDistracting()
    {
        foreach (DistractedState state in aiList)
        {
            state.StopDistraction();
        }
        aiList.Clear();
    }

    public bool AddAIToDistraction(DistractedState distractedState)
    {
        if (aiList.Count < maxAICount && !aiList.Contains(distractedState))
        {
            aiList.Add(distractedState);
            return aiList.Contains(distractedState);
        }
        return false;
    }

    public bool RemoveAIFromDistraction(DistractedState distractedState)
    {
        bool res = aiList.Remove(distractedState);
        if(res || aiList.Count == 0)
        {
            Deactivate();
        }
        return res;
    }

    public Vector3 GetStandingPosition()
    {
        return AIUtils.GetRandomPointInRadius(transform.position, maxStandingDistanceFromPoint);
    }

    public void Interact()
    {
        Activate();
    }
}
