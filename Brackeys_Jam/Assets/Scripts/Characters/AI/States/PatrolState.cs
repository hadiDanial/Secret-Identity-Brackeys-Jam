using SensorToolkit;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PatrolState : State
{
    [SerializeField, Tooltip("Use a custom/predefined path?\nIf true, you have to manually add the waypoints to the list.\nOtherwise, this state will query the PathContainer in the scene for a random path.")]
    private bool usePredefinedPath;
    [SerializeField, Tooltip("Move towards a random waypoint?")]
    private bool randomizePath;
    [SerializeField, Tooltip("If randomizePath is on, should the same waypoint be selectable again after it was already selected?")]
    private bool allowSameWaypointInARow;

    public List<Waypoint> waypoints;

    private int index = 0, newIndex;
    private bool hasResetTimer = false;
    private Animator animator;

    public override void Initialize(Transform playerTransform, NavMeshAgent navMeshAgent, TriggerSensor triggerSensor, AIController aiController)
    {
        base.Initialize(playerTransform, navMeshAgent, triggerSensor, aiController);
        animator = GetComponent<Animator>();
        if(!usePredefinedPath)
        {
            PathContainer pathContainer = FindObjectOfType<PathContainer>();
            if (pathContainer != null)
                waypoints = pathContainer.GetRandomPath();
        }
    }

    public override void UpdateState()
    {
        if (waypoints.Count < 1 || waypoints[index].transform == null)
            return;
        aiController.inputs.sprint = waypoints[index].sprintToWaypoint;
        
        aiController.MoveToTarget(waypoints[index].transform);
        if(aiController.IsStopped())
        {
            if (!hasResetTimer)
            {
                timer = 0;
                hasResetTimer = true;
                aiController.CanMove = false;
                if(!waypoints[index].animationTriggerName.Equals(""))
                    animator?.SetBool(waypoints[index].animationTriggerName, true);
            }
            if(timer >= waypoints[index].waitTime)
            {
                if (!waypoints[index].animationTriggerName.Equals(""))
                    animator?.SetBool(waypoints[index].animationTriggerName, false);
                if(randomizePath)
                {
                    newIndex = UnityEngine.Random.Range(0, waypoints.Count);
                    if (!allowSameWaypointInARow)
                    {
                        while(newIndex == index)
                            newIndex = UnityEngine.Random.Range(0, waypoints.Count);
                    }
                    index = newIndex;
                }
                else
                {
                    index = (index + 1) % waypoints.Count;
                }
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
            catch (InvalidCastException)
            {
                PrintDebugErrorMessageInvalidData(data, "Should be List<Waypoint>");
            }
        }
        StartState(previousState);
    }
}
