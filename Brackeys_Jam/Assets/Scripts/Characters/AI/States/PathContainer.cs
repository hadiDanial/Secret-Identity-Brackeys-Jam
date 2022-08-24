using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathContainer : MonoBehaviour
{
    private List<List<Waypoint>> paths;

    private void Awake()
    {
        paths = new List<List<Waypoint>>();
        GeneratePaths();
    }
    public void GeneratePaths()
    {
        int count = transform.childCount;
        for (int i = 0; i < count; i++)
        {
            Waypoint[] w = transform.GetChild(i).GetComponentsInChildren<Waypoint>();
            if (w == null || w.Length == 0)
                continue;
            List<Waypoint> waypoints = new List<Waypoint>();
            waypoints.AddRange(w);
            paths.Add(waypoints);
        }
    }
    public List<Waypoint> GetRandomPath()
    {
        return paths[Random.Range(0, paths.Count)];
    }
}
