using SensorToolkit;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PatrolState : State
{
    public List<Waypoint> waypoints;

    private int index = 0;
    private bool hasResetTimer = false;
    private Animator animator;

    public override void Initialize(Transform playerTransform, NavMeshAgent navMeshAgent, TriggerSensor triggerSensor, AIController aiController)
    {
        base.Initialize(playerTransform, navMeshAgent, triggerSensor, aiController);
        animator = GetComponent<Animator>();
    }

    public override void UpdateState()
    {
        if (waypoints.Count < 1 || waypoints[index].location == null)
            return;
        aiController.inputs.sprint = waypoints[index].sprintToWaypoint;
        
        aiController.MoveToTarget(waypoints[index].location);
        if(aiController.IsStopped())
        {
            if (!hasResetTimer)
            {
                timer = 0;
                hasResetTimer = true;
                aiController.CanMove = false;
                animator?.SetBool(waypoints[index].animationName, true);
            }
            if(timer >= waypoints[index].waitTime)
            {
                animator?.SetBool(waypoints[index].animationName, false);
                index = (index + 1) % waypoints.Count;
                hasResetTimer = false;
                aiController.CanMove = true;
            }
        }
    }
    public override string ToString()
    {
        return "Patrol State";
    }

    public override void StartState(State previousState, object data = null)
    {
        if(data != null)
        {
            try
            {
                waypoints = (List<Waypoint>)data;
            }
            catch (InvalidCastException e)
            {
                PrintDebugErrorMessageInvalidData(data, "Should be List<Waypoint>");
            }
        }
        StartState(previousState);
    }
}
