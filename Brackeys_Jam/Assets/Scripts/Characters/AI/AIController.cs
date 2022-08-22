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
    [SerializeField] private Transform target;
    [SerializeField] private float recalculatePathDelta = 0.1f;
    private Vector3 previousLocation;

    private NavMeshAgent navMeshAgent;
    private MovementInputs inputs;
    private MovementController movementController;
    private CharacterController characterController;
    private LineRenderer lineRenderer;
    [SerializeField] private Vector3 desiredVelocity;
    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        inputs = GetComponent<MovementInputs>();
        characterController = GetComponent<CharacterController>();
        movementController = GetComponent<MovementController>();
        lineRenderer = GetComponent<LineRenderer>();
        previousLocation = target.position;
    }

    void Update()
    {
        if (Vector3.Distance(target.position, previousLocation) > recalculatePathDelta)
            navMeshAgent.SetDestination(target.position);

        if (navMeshAgent.pathStatus == NavMeshPathStatus.PathComplete)
        {
            desiredVelocity = navMeshAgent.desiredVelocity.normalized;
            navMeshAgent.updatePosition = false;
            navMeshAgent.updateRotation = false;
            if (Vector3.Distance(transform.position, target.position) > navMeshAgent.stoppingDistance)
            {
                inputs.MoveInput(desiredVelocity);
                DrawPath();
            }
            else
            {
                inputs.MoveInput(Vector2.zero);
            }
        }
        else
        {
            inputs.MoveInput(Vector2.zero);

        }
        navMeshAgent.velocity = characterController.velocity;
        navMeshAgent.nextPosition = transform.position;
        previousLocation = target.position;
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
