using System;
using UnityEngine;

[Serializable]
public class Waypoint : MonoBehaviour
{
    [SerializeField]
    public float waitTime;
    [SerializeField]
    public bool sprintToWaypoint;
    [SerializeField, Tooltip("Animation to play after reaching this waypoint")]
    public string animationTriggerName;
}
