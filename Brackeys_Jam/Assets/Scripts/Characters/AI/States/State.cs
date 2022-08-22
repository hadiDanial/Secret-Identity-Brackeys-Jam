using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class State : MonoBehaviour
{

    [SerializeField] protected float resetTime = 0.2f;
    [SerializeField] protected State nextState;
    protected NavMeshAgent navMeshAgent;
    protected Coroutine actionCoroutine;
    protected bool hasStarted = false, isPaused = true;
    protected bool alwaysUpdate = false, activeAction = true;
    protected float timer = 0;

    public virtual void Initialize(NavMeshAgent navMeshAgent)
    {
        this.navMeshAgent = navMeshAgent;
    }

    public virtual void Update()
    {
        if (alwaysUpdate || (hasStarted && !isPaused))
            UpdateAction();
        timer += Time.deltaTime;
    }

    public abstract void UpdateAction();


    public virtual void StartAction()
    {
        hasStarted = true;
        isPaused = false;
        timer = 0;
    }

    public virtual void StopAction()
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
        StopAction();
        hasStarted = false;
        Destroy(this);
    }
    public virtual void FinishAction()
    {
        StopAction();
        nextState.StartAction();
    }
    protected virtual bool CanPerformAction()
    {
        return activeAction && timer >= resetTime;
    }
    protected virtual IEnumerator PerformAction()
    {
        yield return new WaitForSeconds(resetTime);
    }
    public void SetActive(bool value)
    {
        activeAction = value;
    }

    private void OnDisable()
    {
        activeAction = false;
    }

}
