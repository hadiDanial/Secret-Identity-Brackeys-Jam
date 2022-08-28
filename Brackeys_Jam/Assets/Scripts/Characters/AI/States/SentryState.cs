using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SentryState : State
{
    [SerializeField] private Waypoint waypoint;

    public override void StartState(State previousState, object data = null)
    {
        base.StartState(previousState);
        aiController.MoveToTarget(waypoint.transform);
    }

    public override string ToString()
    {
        return "Sentry State";
    }

    public override void UpdateState()
    {
        if(timer < 0.1f || aiController.DistanceBiggerThanStoppingDistance)
            aiController.MoveToTarget(waypoint.transform);
    }

    public override void OnDetected()
    {
        detected = true;
        if (reactToPlayer && nextState != null)
            ActivateNextState();
    }
    public override void OnLostDetection()
    {
        detected = false;
    }
}
