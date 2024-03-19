using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GizmoEffects : MonoBehaviour
{
    public float range = 5f;

    void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
