using System;
using UnityEngine;
using DG.Tweening;
using UnityEngine.AI;
using System.Collections.Generic;

public class SearchState : State
{
    
    [SerializeField] private int maxSearchSteps = 4;
    [SerializeField] private int minSearchSteps = 1;
    [SerializeField] private float maxSearchStepDistance = 5f;
    [SerializeField] private float minSearchStepDistance = 1f;
    [SerializeField, Range(0, 75)] private float lookAroundAngle = 45f;
    [SerializeField, Range(0.5f, 5f)] private float lookAroundTime = 2f;
    [SerializeField, Range(0f, 3f)] private float delayBetweenLooks = 1f;
    [SerializeField] private bool sprint = false;

    private Vector3 lastSeenPosition;
    private int countSteps = 0, countLooks = 0;
    private int requiredSteps;
    private bool isLookingAround = false, isMoving = false, doneSearching = false;
    private GameObject currentTarget;
    private Vector3 rightDir, leftDir;
    private Vector3 finalDestination;
    private HashSet<Sequence> sequences;
    public override void StartState(State previousState, object data = null)
    {
        try
        {
            lastSeenPosition = (Vector3)data;
        }
        catch (InvalidCastException)
        {
            PrintDebugErrorMessageInvalidData(data, "Must be a Vector3 position.");
        }
        StartState(previousState);
    }
    public override void StartState()
    {
        ResetSearchState();
        base.StartState();
    }

    private void ResetSearchState()
    {
        detected = false;
        isLookingAround = false;
        isMoving = true;
        doneSearching = false;
        countSteps = -1;
        countLooks = 0;
        requiredSteps = UnityEngine.Random.Range(minSearchSteps, maxSearchSteps + 1);
        if (currentTarget == null)
            currentTarget = new GameObject("Search Target");
        currentTarget.transform.position = lastSeenPosition;
        aiController.inputs.move = Vector3.zero;
        sequences = new HashSet<Sequence>();
        //triggerSensor.Pulse();
    }

    public override void UpdateState()
    {

        //    CalculateLookConeVectors(transform.forward);
        //    LookAround(() => { isLookingAround = false;
        //        ChooseNewLocationToSearch();
        //    });

        //return;
        triggerSensor.Pulse(); 
        aiController.inputs.sprint = sprint;
        if (detected && timer >= 0.5f)
        {
            OnSearchSuccess();
            return;
        }
        aiController.inputs.move = Vector3.zero;
        navMeshAgent.nextPosition = transform.position;
        if (doneSearching || (countSteps == requiredSteps && countSteps != 0))
        {
            Debug.Log("Done searching, final look!");

                LookAround(() =>
                {
                    if (detected)
                    {
                        Debug.Log("Search successful, returning to " + previousState);
                        OnSearchSuccess();
                    }
                    else
                    {
                        Debug.Log("Gave up searching, returning to " + previousState + "'s previous state");
                        previousState?.ActivatePreviousStateWithData();
                    }
                });
            
            doneSearching = true;
        }
        else
        {
            if (!isLookingAround && !isMoving && countLooks >= countSteps)
            {
                ChooseNewLocationToSearch();
            }
            else
            {
                if (isMoving)
                {
                    //Debug.Log("Moving to target");

                    aiController.MoveToTarget(currentTarget.transform);
                    isMoving = !aiController.IsStopped();
                    if (!isMoving)
                        countSteps++;
                    //if (navMeshAgent.enabled && navMeshAgent.remainingDistance - navMeshAgent.radius < navMeshAgent.stoppingDistance)
                    //{
                    //    Debug.Log("Done moving");
                    //    countSteps++;
                    //    isMoving = false;
                    //}
                }

                else if (!isMoving && !isLookingAround)
                {
                    LookAround(() => isLookingAround = false);
                }
            }
        }

    }

    private void OnSearchSuccess()
    {
        StopSearching();
        Debug.Log("Search successful, returning to " + previousState);
        ActivatePreviousStateWithData();
    }

    private void StopSearching()
    {
        foreach (Sequence sequence in sequences)
        {
            sequence.Kill();
        }
        ResetSearchState();
    }

    private void ChooseNewLocationToSearch()
    {
        Debug.Log("Choosing new spot to search");
        isMoving = true;
        Vector3 potentialDestination = GetRandomVector3Between(leftDir, rightDir).normalized *
                                     UnityEngine.Random.Range(minSearchStepDistance, maxSearchStepDistance) + transform.position;
        // See if the requested point is on the navmesh
        NavMeshHit hit;
        if (NavMesh.SamplePosition(potentialDestination, out hit, 1.0f, NavMesh.AllAreas))
        {
            finalDestination = hit.position;
            currentTarget.transform.position = finalDestination;
        }
        // If it isn't, keep looking for random points on the navmesh
        else
        {
            finalDestination = AIUtils.GetRandomPointInRadius(transform.position, maxSearchStepDistance);
            currentTarget.transform.position = finalDestination;
            aiController.MoveToTarget(currentTarget.transform);
        }
    }

    

    private void LookAround(TweenCallback callback)
    {        
        if (!isLookingAround)
        {
            CalculateLookConeVectors(transform.forward);
            isLookingAround = true;
            countLooks++;
            Debug.Log("Looking around... " + countLooks + "/" + requiredSteps);

            // (x cos alpha + y sin alpha, -x sin alpha + y cos alpha).        
            Sequence lookSequence = DOTween.Sequence();
            lookSequence.AppendInterval(delayBetweenLooks);
            lookSequence.Append(transform.DOLookAt(transform.position + rightDir, lookAroundTime));
            lookSequence.AppendInterval(delayBetweenLooks);
            lookSequence.Append(transform.DOLookAt(transform.position + leftDir, lookAroundTime));
            lookSequence.AppendInterval(delayBetweenLooks);
            lookSequence.Append(transform.DOLookAt(transform.position + transform.forward, lookAroundTime));
            lookSequence.OnComplete(()=> {
                callback();
                sequences.Remove(lookSequence);
                });
            lookSequence.SetLoops(0);
            sequences.Add(lookSequence);
            lookSequence.Play();
        }
    }

    private void CalculateLookConeVectors(Vector3 forward)
    {
        float angle = Mathf.Deg2Rad * (transform.rotation.y + lookAroundAngle);
        float sin = Mathf.Sin(angle), cos = Mathf.Cos(angle);
        Vector3 v1 = new Vector3(cos, 0, sin), v2 = new Vector3(-sin, 0, cos);
        rightDir = ((forward.x * v1) + (forward.z * v2)).normalized; //new Vector3(forward.x * cos + forward.y * sin, 0, -forward.x * sin + forward.y * cos);
        angle = Mathf.Deg2Rad * (transform.rotation.y - lookAroundAngle);
        sin = Mathf.Sin(angle);
        cos = Mathf.Cos(angle);
        v1 = new Vector3(cos, 0, sin);
        v2 = new Vector3(-sin, 0, cos);
        leftDir = ((forward.x * v1) + (forward.z * v2)).normalized; // new Vector3(forward.x * cos + forward.y * sin, 0, -forward.x * sin + forward.y * cos);
    }
    public Vector3 GetRandomVector3Between(Vector3 min, Vector3 max)
    {
        return min + UnityEngine.Random.Range(0f, 1f) * (max - min);
    }
    public override void OnDetected()
    {
        detected = true;
    }
    public override void OnLostDetection()
    {
        detected = false;
    }
    public override void FinishAction()
    {
        GameObject.Destroy(currentTarget);
        base.FinishAction();
    }

    public override void StopState()
    {
        base.StopState();
        StopSearching();
    }

    public override string ToString()
    {
        return "Search State";
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(finalDestination, 0.5f);
        Gizmos.DrawLine(transform.position, transform.position + rightDir * 5);
        Gizmos.DrawLine(transform.position, transform.position + rightDir * 5);
        Gizmos.DrawLine(transform.position, transform.position + rightDir * 5);
        Gizmos.DrawLine(transform.position, transform.position + rightDir * 5);
        Gizmos.DrawLine(transform.position, transform.position + rightDir * 5);
        Gizmos.DrawLine(transform.position, transform.position + rightDir * 5);
        Gizmos.DrawLine(transform.position, transform.position + rightDir * 5);
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + leftDir * 5);
        Gizmos.DrawLine(transform.position, transform.position + leftDir * 5);
        Gizmos.DrawLine(transform.position, transform.position + leftDir * 5);
        Gizmos.DrawLine(transform.position, transform.position + leftDir * 5);
        Gizmos.DrawLine(transform.position, transform.position + leftDir * 5);
        Gizmos.DrawLine(transform.position, transform.position + leftDir * 5); 
        Gizmos.DrawLine(transform.position, transform.position + leftDir * 5);
    }
}
