using SensorToolkit;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FollowState : State
{
    [SerializeField]
    private bool allowSprinting = false;
    [SerializeField, Range(1.3f, 3), Tooltip("The enemy will move to the next state if it gets closer than this distance to the player. (Only if detected)")]
    private float nextStateDistance = 1.3f;
    [SerializeField, Range(5, 25), Tooltip("The enemy will give up and return to its previous state if the player gets farther away from it than this distance")]
    private float giveUpDistance = 10;
    [SerializeField, Tooltip("The enemy will stop following the player if they lose line of sight")]
    private bool followOnlyIfLOS = false;
    [SerializeField, Tooltip("What state to go to if we lose line of sight. If null, returns to previous state.\n(Only works if followOnlyIfLOS is active)")]
    private State lostLOSState;

    [SerializeField, Tooltip("Read-only. The current distance towards the player.")]
    private float currentDistanceToPlayer;

    private Transform target;
    private Vector3 lastSeenPosition;
    private bool triggeredLOSState = false;
    public override void Initialize(Transform playerTransform, NavMeshAgent navMeshAgent, TriggerSensor triggerSensor, AIController aiController)
    {
        base.Initialize(playerTransform, navMeshAgent, triggerSensor, aiController);
        target = playerTransform;
    }

    public override void StartState(State previousState)
    {
        base.StartState(previousState);
        lastSeenPosition = target.position;
    }

    public override void UpdateState()
    {
        currentDistanceToPlayer = Vector3.Distance(transform.position, target.position);
        aiController.inputs.sprint = allowSprinting;
        if (followOnlyIfLOS)
        {
            if (triggerSensor.DetectedObjects.Count < 1)
            {
                if (lostLOSState != null)
                {
                    triggeredLOSState = true;
                    ActivateState(lostLOSState, true, lastSeenPosition);
                }
                else
                {
                    ActivatePreviousStateWithData();
                    Debug.Log("Prev");
                }
            }
            else
                aiController.MoveToTarget(target);
        }
        else
        {
            if (currentDistanceToPlayer <= nextStateDistance)
            {
                aiController.inputs.move = Vector2.zero;
                ActivateNextState();
                Debug.Log("Next");
            }
            else if (currentDistanceToPlayer >= giveUpDistance)
            {
                if (triggeredLOSState)
                {
                    aiController.inputs.move = Vector2.zero;
                    ActivatePreviousStateWithData();
                    Debug.Log("Prev Gave up");
                }
                else
                {
                    triggeredLOSState = true;
                    aiController.inputs.move = Vector2.zero;
                    ActivateState(lostLOSState, true, lastSeenPosition);
                    Debug.Log("Lost LOS");
                }
            }
            else
                aiController.MoveToTarget(target);
        }
        lastSeenPosition = target.position;
        //aiController.MoveToTarget(target);
    }
    public override string ToString()
    {
        return "Follow State (N=" + nextStateDistance + ", G=" + giveUpDistance + ")";
    }

    public override void StartState(State previousState, object data = null)
    {
        if (data != null)
        {
            try
            {
                target = (Transform)data;
            }
            catch (InvalidCastException e)
            {
                PrintDebugErrorMessageInvalidData(data, "Should be a Transform for the target to follow.");
            }
        }
        StartState(previousState);
    }

    public override void StartState()
    {
        if (triggeredLOSState && triggerSensor.DetectedObjects.Count < 1)
        {
            triggeredLOSState = false;
            ActivatePreviousStateWithData();
            Debug.Log("Prev in start state");
        }
        else
        {
            triggeredLOSState = false;
            base.StartState();
        }
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
