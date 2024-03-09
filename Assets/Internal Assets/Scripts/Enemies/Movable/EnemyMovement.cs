using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    #region Variables

    [Header("Floats")]
    readonly float rotSpeed = 30f;
    readonly float moveSpeed = 5f;

    [Header("Transforms")]
    Transform target;

    [Header("Bools")]
    bool tracking;

    #endregion

    #region StartUpdate

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        target = GetComponent<EnemySight>().target;
        tracking = GetComponent<EnemySight>().tracking;

        RotateToTarget();
    }

    #endregion

    #region Methods

    void RotateToTarget()
    {
        if (target)
        {
            // transform.LookAt(target);

            Vector3 relativePos = target.position - transform.position;
            Quaternion rotation = Quaternion.LookRotation(relativePos);

            Quaternion current = transform.localRotation;

            transform.localRotation = Quaternion.Slerp(current, rotation, Time.deltaTime * rotSpeed);
        }
    }

    #endregion
}
