using SensorToolkit;
using StarterAssets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(NavMeshObstacle))]
[RequireComponent(typeof(MovementInputs))]
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(LineRenderer))]
public class AIController : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    [SerializeField, Tooltip("The default state for this AI.")] private State initialState;

  
    //[SerializeField] private float recalculatePathDelta = 0.1f;

    [SerializeField] private Vector3 desiredVelocity;
    private Vector3 previousLocation;

    private NavMeshAgent navMeshAgent;
    private NavMeshObstacle navMeshObstacle;
    public MovementInputs inputs;
    private MovementController movementController;
    private CharacterController characterController;
    private LineRenderer lineRenderer;
    private TriggerSensor triggerSensor;

    private State[] states;
    private State currentState;
    private bool isStopped = true;

    public static event Detected OnDetectedEvent;
    public delegate void Detected(AIController aiController);
    public static event LostDetection OnLostDetectionEvent;
    public delegate void LostDetection(AIController aiController);

    private float agentDiameter;

    private bool canMove = true;
    private bool distanceBiggerThanStoppingDistance;
    public bool CanMove { get => canMove; set => canMove = value; }

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshObstacle = GetComponent<NavMeshObstacle>();
        navMeshAgent.enabled = true;
        navMeshObstacle.enabled = false;
        agentDiameter = navMeshAgent.radius / 2;
        inputs = GetComponent<MovementInputs>();
        characterController = GetComponent<CharacterController>();
        movementController = GetComponent<MovementController>();
        lineRenderer = GetComponent<LineRenderer>();
        triggerSensor = GetComponentInChildren<TriggerSensor>();
        previousLocation = playerTransform.position;
        navMeshAgent.SetDestination(playerTransform.position);
        InitializeStates();
    }

    private void InitializeStates()
    {
        states = GetComponents<State>();
        foreach (State state in states)
        {
            state.Initialize(playerTransform, navMeshAgent, triggerSensor, this);
        }
        currentState = initialState;
        currentState?.StartState();
    }

    void Update()
    {
        //if (Vector3.Distance(playerTransform.position, previousLocation) > recalculatePathDelta)
        //    navMeshAgent.SetDestination(playerTransform.position);

        // Follow the player if no states have been added
        if(currentState == null)
            MoveToTarget(playerTransform);
    }

    public void MoveToTarget(Transform target)
    {
        distanceBiggerThanStoppingDistance = Vector3.Distance(transform.position, target.position) > navMeshAgent.stoppingDistance + agentDiameter;
        if (distanceBiggerThanStoppingDistance)
        {
            TurnNavMeshAgentOn();
        }
        if (!canMove)
            return;
        if(navMeshAgent.enabled)
            navMeshAgent.destination = (target.position);
        else if (target.CompareTag("Player"))
        {
            Vector3 targetPosition = new Vector3(target.position.x,
                                        transform.position.y,
                                        target.position.z);
            transform.LookAt(targetPosition);
        }
        if (navMeshAgent.pathStatus != NavMeshPathStatus.PathInvalid)
        {
            desiredVelocity = navMeshAgent.desiredVelocity.normalized;
            navMeshAgent.updatePosition = false;
            navMeshAgent.updateRotation = false;
            if (distanceBiggerThanStoppingDistance)
            {
                inputs.MoveInput(desiredVelocity);
                isStopped = false;
                DrawPath();
            }
            else
            {
                inputs.MoveInput(Vector2.zero);
                
                TurnNavMeshAgentOff();
            }
        }
        else
        {
            inputs.MoveInput(Vector2.zero);
            isStopped = true;
        }
        navMeshAgent.velocity = characterController.velocity;
        navMeshAgent.nextPosition = transform.position;
        previousLocation = target.position;
    }

    private void TurnNavMeshAgentOff()
    {
        navMeshAgent.enabled = false;
        navMeshObstacle.enabled = true;
        isStopped = true;
    }

    private void TurnNavMeshAgentOn()
    {
        navMeshObstacle.enabled = false;
        navMeshAgent.enabled = true;
        isStopped = false;
    }

    public void OnDetected()
    {
        currentState?.OnDetected();
        OnDetectedEvent?.Invoke(this);
    }
    public void OnLostDetection()
    {
        currentState?.OnLostDetection();
        OnLostDetectionEvent?.Invoke(this);
    }
    internal bool IsStopped()
    {
        return isStopped;
    }
    private void DrawPath()
    {
        NavMeshPath path = navMeshAgent.path;
        lineRenderer.positionCount = path.corners.Length;
        lineRenderer.SetPosition(0, transform.position);
        if (path.corners.Length < 2)
            return;
        for (int i = 1; i < path.corners.Length; i++)
        {
            Vector3 point = path.corners[i];
            lineRenderer.SetPosition(i, point);
        }
    }
}
