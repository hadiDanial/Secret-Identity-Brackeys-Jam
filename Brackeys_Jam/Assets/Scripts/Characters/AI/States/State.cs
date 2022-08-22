using SensorToolkit;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class State : MonoBehaviour
{

    [SerializeField] protected float resetTime = 0.2f;
    [SerializeField] protected State nextState;
    protected bool reactToPlayer = true;
    protected NavMeshAgent navMeshAgent;
    protected Transform playerTransform;
    protected TriggerSensor triggerSensor;
    protected Coroutine actionCoroutine;

    protected bool hasStarted = false, isPaused = true;
    protected bool alwaysUpdate = false, stateIsActive = true;
    protected float timer = 0;

    public virtual void Initialize(Transform playerTransform, NavMeshAgent navMeshAgent, TriggerSensor triggerSensor)
    {
        this.navMeshAgent = navMeshAgent;
        this.playerTransform = playerTransform;
        this.triggerSensor = triggerSensor;
    }

    public virtual void Update()
    {
        if (alwaysUpdate || (hasStarted && !isPaused))
            UpdateState();
        timer += Time.deltaTime;
    }

    public abstract void UpdateState();

    public abstract void OnDetected();

    public abstract void OnLostDetection();

    public virtual void StartState()
    {
        hasStarted = true;
        isPaused = false;
        timer = 0;
    }

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
        nextState.StartState();
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
}
