using DG.Tweening;
using SensorToolkit;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DistractedState : State, Interactable
{
    [SerializeField, Tooltip("The animation trigger/bool parameter name. It should trigger a looping animation.")] private string animationTriggerName;
    [SerializeField] DistractionPoint distractionPoint;
    private Animator animator;
    private GameObject target;
    private bool isMoving = true;
    private bool hasLostInterest = false;
    private DistractionItem distractionItem;
    private float distractionItemTimer = 0;
    public override void Initialize(Transform playerTransform, NavMeshAgent navMeshAgent, TriggerSensor triggerSensor, AIController aiController)
    {
        base.Initialize(playerTransform, navMeshAgent, triggerSensor, aiController);
        animator = GetComponent<Animator>();
        resetTime = 0;
    }

    public void Interact()
    {
        aiController.ReplaceActiveState(this, true);
        //StartState();
    }

    public override void StartState(State previousState, object data = null)
    {
        StartState(previousState);
        //
    }
    public override void StartState()
    {
        base.StartState();
        if (target == null)
            target = new GameObject("Distraction Position " + transform.name);
        isMoving = true;
        distractionItemTimer = 0;
        if(distractionPoint == null)
        {
            DistractionPoint[] points = FindObjectsOfType<DistractionPoint>();
            if (points != null && points.Length > 0)
            {
                DistractionPoint chosenPoint = null;
                foreach (DistractionPoint point in points)
                {
                    if (point.IsActive())
                    {
                        chosenPoint = point;
                        break;
                    }
                }
                if (chosenPoint == null)
                {
                    do
                    {
                        chosenPoint = points[UnityEngine.Random.Range(0, points.Length)];
                    } while (!chosenPoint.AddAIToDistraction(this));
                }
                this.distractionPoint = chosenPoint;
                target.transform.position = distractionPoint.GetStandingPosition();
            }
            else
            {
                DOTween.Sequence().AppendInterval(5).Play().OnComplete(() => ActivatePreviousState());
            }
        }
        else if(distractionItem != null)
        {
            distractionPoint = distractionItem.GetDistractionPoint();
            target.transform.position = distractionPoint.GetStandingPosition();
        }
    }

    internal GameObject GetHoldPoint()
    {
        return aiController.GetHoldPoint();
    }

    internal void StartDistracting()
    {
        aiController.ReplaceActiveState(this, true);
    }

    internal void StopDistraction()
    {
        if (animationTriggerName != "")
            animator.SetBool(animationTriggerName, false);
        distractionPoint = null;
        ActivateNextState();
    }

    public override string ToString()
    {
        return "Distracted State";
    }

    public override void UpdateState()
    {
        reactToPlayer = false;
        if(isMoving)
        {
            if(!aiController.IsStopped() || aiController.DistanceBiggerThanStoppingDistance)//Vector3.Distance(transform.position, target.transform.position) > navMeshAgent.stoppingDistance + navMeshAgent.radius + 0.2f)
            {
                aiController.MoveToTarget(target.transform);
            }
            else
            {
                isMoving = false;
                timer = 0;
                if (animationTriggerName != "")
                    animator.SetBool(animationTriggerName, true);
            }
        }
        else if(distractionPoint != null)
        {
            if(distractionItem != null)
            {
                distractionItemTimer += Time.deltaTime;
                if(distractionItemTimer >= distractionItem.DistractionTimeAtPoint)
                {
                    hasLostInterest = false;
                    distractionItem.Release();
                    distractionItem.Destory();
                    distractionPoint = null;
                    distractionItem = null;
                    if (previousState != null)
                        ActivatePreviousState();
                    else
                        ActivateNextState();
                }
            }
            else if(!distractionPoint.IsActive() && !hasLostInterest)
            {

            hasLostInterest = true;
            DOTween.Sequence().AppendInterval(2f).OnComplete(() =>
            {
                hasLostInterest = false;
                distractionPoint = null;
                if (previousState != null)
                    ActivatePreviousState();
                else
                    ActivateNextState();
            });
            }
        }
    }

    internal void SetDistractionPoint(DistractionPoint distractionPoint)
    {
        this.distractionPoint = distractionPoint;
        if (target == null)
            target = new GameObject("Distraction Position " + transform.name);
        target.transform.position = distractionPoint.GetStandingPosition();
        isMoving = true;
        aiController.MoveToTarget(target.transform);
    }

    internal void SetDistractionItem(DistractionItem distractionItem)
    {
        this.distractionItem = distractionItem;
        this.distractionPoint = distractionItem.GetDistractionPoint();
    }
    public override void OnDetected()
    {
        detected = true;
    }
    public override void OnLostDetection()
    {
        detected = false;
    }
}
