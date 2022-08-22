using SensorToolkit;
using StarterAssets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(MovementInputs))]
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(LineRenderer))]
public class AIController : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float recalculatePathDelta = 0.1f;
    [SerializeField, Tooltip("The default state for this AI.")] private State initialState;

    [SerializeField] private Vector3 desiredVelocity;
    private Vector3 previousLocation;

    private NavMeshAgent navMeshAgent;
    private MovementInputs inputs;
    private MovementController movementController;
    private CharacterController characterController;
    private LineRenderer lineRenderer;
    private TriggerSensor triggerSensor;

    private State[] states;
    private State currentState;

    public static event Detected OnDetectedEvent;
    public delegate void Detected(AIController aiController);
    public static event LostDetection OnLostDetectionEvent;
    public delegate void LostDetection(AIController aiController);

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
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
            state.Initialize(playerTransform, navMeshAgent, triggerSensor);
        }
        currentState = initialState;
        currentState?.StartState();
    }

    void Update()
    {
        if (Vector3.Distance(playerTransform.position, previousLocation) > recalculatePathDelta)
            navMeshAgent.SetDestination(playerTransform.position);

        if (navMeshAgent.pathStatus == NavMeshPathStatus.PathComplete)
        {
            desiredVelocity = navMeshAgent.desiredVelocity.normalized;
            navMeshAgent.updatePosition = false;
            navMeshAgent.updateRotation = false;
            if (Vector3.Distance(transform.position, playerTransform.position) > navMeshAgent.stoppingDistance)
            {
                inputs.MoveInput(desiredVelocity);
                DrawPath();
            }
            else
            {
                inputs.MoveInput(Vector2.zero);
                transform.LookAt(playerTransform);
            }
        }
        else
        {
            inputs.MoveInput(Vector2.zero);

        }
        navMeshAgent.velocity = characterController.velocity;
        navMeshAgent.nextPosition = transform.position;
        previousLocation = playerTransform.position;
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
