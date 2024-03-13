using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySight : MonoBehaviour
{
    #region Variables

    [Header("Floats")]
    [SerializeField] readonly float range = 25f;
    readonly float fovAngle = 135f;

    [Header("Bools")]
    public bool tracking;
    public bool inFov;
    
    [Header("Transforms")]
    public Transform target;
    [SerializeField] Transform player;

    [Header("Vector3s")]
    Vector3 topAngle;
    Vector3 bottomAngle;

    [Header("LayerMasks")]
    [SerializeField] LayerMask playerMask;
    [SerializeField] LayerMask obstructionMask;

    #endregion
    
    #region StartUpdate

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (!target)
        {
            tracking = false;
            FindTarget();
            return;
        }

        if (target)
        {
            tracking = true;

            topAngle = DirFromAngle(fovAngle / 2);
            bottomAngle = DirFromAngle(-fovAngle / 2);

            if (!TargetInRange() || TargetObstructed())
            {
                target = null;
            }
        }

        inFov = TargetInFOV();
    }

    #endregion

    #region Methods

    void FindTarget()
    {
        RaycastHit[] hits = Physics.SphereCastAll(transform.position, range, transform.position, playerMask);
        
        if ((hits.Length > 0) && TargetInRange() && !TargetObstructed())
        {
            target = player;
        }
    }

    bool TargetInRange()
    {
        if (Vector3.Distance(player.position, transform.position) <= range)
        {
            return true;
        }
        return false;
    }

    bool TargetInFOV()
    {
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, range, playerMask);

        if (rangeChecks.Length > 0 && TargetInRange() && !TargetObstructed())
        {
            Vector3 direction = (player.position - transform.position).normalized;

            if (Vector3.Angle(transform.forward, direction) < fovAngle / 2)
            {
                return true;
            }
            return false;
        }
        return false;
    }

    bool TargetObstructed()
    {
        bool hit = Physics.Linecast(transform.position, player.position, obstructionMask);

        if (hit)
        {
            return true;
        }
        return false;
    }

    Vector3 DirFromAngle(float angleIndDegrees)
    {
        return new Vector3(Mathf.Sin(angleIndDegrees * Mathf.Deg2Rad), 0f, Mathf.Cos(angleIndDegrees * Mathf.Deg2Rad));
    }

    #endregion

    #region Gizmos

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, range);

        if (player)
        {
            Gizmos.DrawLine(transform.position, player.position);
        }

        if (target)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, target.position);
        }

        Gizmos.matrix = transform.localToWorldMatrix;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(0f, 0f, 0f), transform.localPosition + topAngle * range);
        Gizmos.DrawLine(new Vector3(0f, 0f, 0f), transform.localPosition + bottomAngle * range);
    }

    #endregion
}
