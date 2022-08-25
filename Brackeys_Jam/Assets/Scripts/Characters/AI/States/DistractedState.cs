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

    public override void Initialize(Transform playerTransform, NavMeshAgent navMeshAgent, TriggerSensor triggerSensor, AIController aiController)
    {
        base.Initialize(playerTransform, navMeshAgent, triggerSensor, aiController);
        animator = GetComponent<Animator>();
        resetTime = 0;
    }

    public void Interact()
    {
        aiController.ReplaceActiveState(this);
        StartState();
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
        if(distractionPoint == null)
        {
            DistractionPoint[] points = FindObjectsOfType<DistractionPoint>();
            DistractionPoint chosenPoint = null;
            foreach (DistractionPoint point in points)
            {
                if (point.IsActive())
                { 
                    chosenPoint = point;
                    break;
                }
            }
            if(chosenPoint == null)
            {
                do
                {
                    chosenPoint = points[UnityEngine.Random.Range(0, points.Length)];
                } while (!chosenPoint.AddAIToDistraction(this));
            }
            this.distractionPoint = chosenPoint;
            target.transform.position = distractionPoint.GetStandingPosition();
        }
    }

    internal void StartDistracting()
    {
        aiController.ReplaceActiveState(this);
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
            if(!aiController.IsStopped())
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
    }

    internal void SetDistractionPoint(DistractionPoint distractionPoint)
    {
        this.distractionPoint = distractionPoint;
        if (target == null)
            target = new GameObject("Distraction Position " + transform.name);
        target.transform.position = distractionPoint.GetStandingPosition();
        isMoving = true;
    }
}
