using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretMovement : MonoBehaviour
{
    readonly float rotSpeed = 5f;
    Transform playerTarget;
    Transform lookPoint;

    // Start is called before the first frame update
    void Start()
    {
        lookPoint = GameObject.FindGameObjectWithTag("Turret").transform.Find("LookPoint");
    }

    // Update is called once per frame
    void Update()
    {
        playerTarget = GetComponent<TurretSight>().target;

        if (playerTarget)
        {
            RotateToPlayer();
        }
        else
        {
            RotateToDefault();
        }
    }

    void RotateToPlayer()
    {
        Vector3 direction = playerTarget.position - transform.position;
        Vector3 newDirection = Vector3.RotateTowards(transform.forward, direction, rotSpeed * Time.deltaTime, 0);

        transform.rotation = Quaternion.LookRotation(newDirection);
    }

    void RotateToDefault()
    {
        Vector3 direction = lookPoint.position - transform.position;
        Vector3 newDirection = Vector3.RotateTowards(transform.forward, direction, rotSpeed * Time.deltaTime, 0);

        transform.rotation = Quaternion.LookRotation(newDirection);
    }
}
