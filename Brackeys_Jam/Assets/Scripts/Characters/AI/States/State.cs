using SensorToolkit;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class State : MonoBehaviour
{

    [SerializeField, Tooltip("Should the state react to the player when it is detected?\nIf true, detection will trigger the next state.")]
    protected bool reactToPlayer = true;
    [SerializeField] protected State nextState, previousState;
    [SerializeField, Tooltip("Should we go back to the previous state when this state is done (if it isn't null)?")] 
    protected bool preferReturningToPreviousState = false;
    [SerializeField] protected float resetTime = 0.2f;

    protected NavMeshAgent navMeshAgent;
    protected Transform playerTransform;
    protected TriggerSensor triggerSensor;
    protected AIController aiController;
    protected Coroutine actionCoroutine;

    protected bool hasStarted = false, isPaused = true;
    protected bool alwaysUpdate = false, stateIsActive = true;
    protected float timer = 0;
    protected bool detected = false;

    public virtual void Initialize(Transform playerTransform, NavMeshAgent navMeshAgent, TriggerSensor triggerSensor, AIController aiController)
    {
        this.navMeshAgent = navMeshAgent;
        this.playerTransform = playerTransform;
        this.triggerSensor = triggerSensor;
        this.aiController = aiController;
    }

    public virtual void Update()
    {
        if (alwaysUpdate || (hasStarted && !isPaused))
            UpdateState();
        timer += Time.deltaTime;
    }

    public abstract void UpdateState();

    public virtual void OnDetected()
    {
        detected = true;
        if (reactToPlayer)
        {
            FinishAction();
        }
    }

    public virtual void OnLostDetection()
    {
        detected = false;
        if (reactToPlayer)
        {
            FinishAction();
        }
    }

    public virtual void StartState()
    {
        hasStarted = true;
        isPaused = false;
        timer = 0;
    }
    public virtual void StartState(State previousState)
    {
        this.previousState = previousState;
        StartState();
    }

    public abstract void StartState(State previousState, object data = null);

    public virtual void StopState()
    {
        isPaused = true;
        hasStarted = false;
        timer = resetTime;
    }

    public virtual void Resume()
    {
        isPaused = false;
    }

    public virtual void Pause()
    {
        isPaused = true;
    }

    public virtual void KillAction()
    {
        StopState();
        hasStarted = false;
        Destroy(this);
    }
    public virtual void FinishAction()
    {
        StopState();
        if(previousState != null && preferReturningToPreviousState)
        {
            previousState?.StartState();
        }
        else
        {
            nextState?.StartState(this);
        }
    }

    public virtual void ActivateNextState(object data = null)
    {
        Debug.Log("Moving from " + this + " to Next State " + nextState + ", Data = " + data);
        ActivateState(nextState, data);
    }
    public virtual void ActivatePreviousState(object data = null)
    {
        Debug.Log("Moving from " + this + " to Previous State " + nextState + ", Data = " + data);
        ActivateState(previousState, data);
    }
    public virtual void ActivateState(State customState, object data = null)
    {
        StopState();
        Debug.Log("Moving from " + this + " to " + customState);
        if(data == null)
            customState?.StartState(this);
        else
            customState?.StartState(this, data);
    }


    protected virtual bool CanPerformAction()
    {
        return stateIsActive && timer >= resetTime;
    }
    protected virtual IEnumerator PerformAction()
    {
        yield return new WaitForSeconds(resetTime);
    }
    public void SetActive(bool value)
    {
        stateIsActive = value;
    }

    private void OnDisable()
    {
        stateIsActive = false;
    }

    public abstract override string ToString();
    protected void PrintDebugErrorMessageInvalidData(object data, string message)
    {
        Debug.LogError("Invalid data passed to state " + ToString() + "\n" + data + "\n" + message);
    }
}
