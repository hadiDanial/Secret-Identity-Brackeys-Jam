using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIUtils
{
    public static Vector3 GetRandomPointInRadius(Vector3 position, float radius)
    {
        NavMeshHit hit;
        Vector3 point = Vector3.zero;
        bool flag = false;
        while (!flag)
        {
            Vector3 randomPoint = position + UnityEngine.Random.insideUnitSphere * radius;
            if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
            {
                point = hit.position;
                flag = true;
            }
        }
        return point;
    }
}
