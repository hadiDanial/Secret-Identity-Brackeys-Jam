using System;
using UnityEngine;

[Serializable]
public struct Waypoint
{
    [SerializeField]
    public Transform location;
    [SerializeField]
    public float waitTime;
    [SerializeField]
    public bool sprintToWaypoint;
    [SerializeField, Tooltip("Animation to play after reaching this waypoint")]
    public string animationName;

    public Waypoint(Transform location, float waitTime, bool sprintToWaypoint, string animationName) 
    {
        this.location = location;
        this.waitTime = waitTime;
        this.sprintToWaypoint = sprintToWaypoint;
        this.animationName = animationName;
    }
}
