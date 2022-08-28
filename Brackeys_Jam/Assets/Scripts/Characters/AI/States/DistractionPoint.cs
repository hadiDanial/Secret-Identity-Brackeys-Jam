using DG.Tweening;
using SensorToolkit;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(RangeSensor)), RequireComponent(typeof(NavMeshObstacle)), RequireComponent(typeof(AudioSource))]
public class DistractionPoint : Waypoint, Interactable
{
    [SerializeField, Range(1, 30)] private int maxAICount = 15;
    [SerializeField, Range(0.15f, 3f)] private float maxStandingDistanceFromPoint = 1f;
    [SerializeField] private List<DistractedState> aiList;
    [SerializeField] private bool isActive = false;
    [SerializeField] private float timer = 0;
    [SerializeField] private float destroyTimeAfterDeactivation = 5;
    private bool hasBeenActivated = false;
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
            if (aiList.Count > 0 && AtLeastOneAIHasStopped())
                timer += Time.deltaTime;
            else
            {
                AttractNPCs();
            }
            if (timer >= waitTime)
                Deactivate();
        }
        else
        {
            if (aiList.Count > 0)
                StopDistracting();
        }
    }

    private bool AtLeastOneAIHasStopped()
    {
        foreach (DistractedState distractedState in aiList)
        {
            if (distractedState.IsStopped())
                return true;
        }
        return false;
    }

    public void Activate()
    {
        if (hasBeenActivated) return;
        timer = 0;
        isActive = true;
        hasBeenActivated = true;
        if (animator != null)
        {
            animator.Play(animationTriggerName);
        }
        AttractNPCs();
        audioSource.Play();
    }

    private void AttractNPCs()
    {
        List<DistractedState> newlyDistracted = new List<DistractedState>();
        foreach (GameObject item in rangeSensor.DetectedObjects)
        {
            DistractedState d;
            if (item.TryGetComponent<DistractedState>(out d))
            {
                if (!d.IsActive())
                    newlyDistracted.Add(d);
            }
        }
        for (int i = 0; i < newlyDistracted.Count && maxAICount - aiList.Count > 0; i++)
        {
            if (AddAIToDistraction(newlyDistracted[i]))
            {
                newlyDistracted[i].SetDistractionPoint(this);
                newlyDistracted[i].StartDistracting();
            }
        }
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
        DOTween.Sequence().AppendInterval(destroyTimeAfterDeactivation).OnComplete(() => Destroy(gameObject));
    }
    internal bool IsActive()
    {
        return isActive;
    }


    private void StopDistracting()
    {
        for (int i = aiList.Count - 1; i >= 0; i--)
        {
            if(aiList[i].IsStopped() && Vector3.Distance(aiList[i].transform.position, transform.position) <= maxStandingDistanceFromPoint + 0.5f)
            {
                aiList[i].StopDistraction();
                aiList.Remove(aiList[i]);
            }

        }
        
        //aiList.Clear();
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
