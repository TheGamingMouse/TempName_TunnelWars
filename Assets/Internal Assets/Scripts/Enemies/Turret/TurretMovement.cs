using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretMovement : MonoBehaviour
{
    #region Variables

    [Header("Enum States")]
    [SerializeField] CombatState cState;
    [SerializeField] TurnState tState;

    [Header("Floats")]
    float rotSpeedCurr;
    readonly float rotSpeedCombat = 3f;
    readonly float rotSpeedSeeking = 1f;
    float leftAngle;
    float rightAngle;

    [Header("Bools")]
    [SerializeField] bool isLookingRight;

    [Header("Transforms")]
    Transform playerTarget;
    [SerializeField] Transform lookPoint;
    Transform self;
    Transform lookPoint1;
    Transform lookPoint2;

    #endregion

    #region StartUpdate

    // Start is called before the first frame update
    void Start()
    {
        self = GetComponentInParent<Turret>().transform;
        lookPoint1 = self.Find("LookPoint1");
        lookPoint2 = self.Find("LookPoint2");

        rotSpeedCurr = rotSpeedSeeking;

        isLookingRight = true;
    }

    // Update is called once per frame
    void Update()
    {
        playerTarget = GetComponent<TurretSight>().target;

        switch (cState)
        {
            case CombatState.Seeking:
                rotSpeedCurr = rotSpeedSeeking;

                if (playerTarget)
                {
                    cState = CombatState.Combat;
                    return;
                }

                if (isLookingRight)
                {
                    lookPoint = lookPoint1;
                }
                else
                {
                    lookPoint = lookPoint2;
                }
                RotateToDefault();
                break;
            
            case CombatState.Combat:
                rotSpeedCurr = rotSpeedCombat;

                if (!playerTarget)
                {
                    cState = CombatState.Seeking;
                    return;
                }

                RotateToPlayer();
                break;
        }
        
        switch (tState)
        {
            case TurnState.zero:
                leftAngle = 135f;
                rightAngle = 225f;
                break;
            
            case TurnState.ninety:
                leftAngle = 135f + 90f;
                rightAngle = 225f + 90f;
                break;
            
            case TurnState.oneEighty:
                leftAngle = 135f + 180f;
                rightAngle = 225f + 180f;
                break;
            
            case TurnState.twoSeventy:
                leftAngle = 135f - 90f;
                rightAngle = 225f - 90;
                break;
        }
    }

    #endregion

    #region Methods

    void RotateToPlayer()
    {
        Vector3 direction = playerTarget.position - transform.position;
        Vector3 newDirection = Vector3.RotateTowards(transform.forward, direction, rotSpeedCurr * Time.deltaTime, 0);

        transform.rotation = Quaternion.LookRotation(newDirection);
    }

    void RotateToDefault()
    {
        Vector3 direction = lookPoint.position - transform.position;
        Vector3 newDirection = Vector3.RotateTowards(transform.forward, direction, rotSpeedCurr * Time.deltaTime, 0);

        transform.rotation = Quaternion.LookRotation(newDirection);

        if (Quaternion.LookRotation(newDirection).eulerAngles.y >= rightAngle || Quaternion.LookRotation(newDirection).eulerAngles.y <= leftAngle)
        {
            if (isLookingRight)
            {
                isLookingRight = false;
            }
            else
            {
                isLookingRight = true;
            }
        }
    }

    #endregion

    #region Enums

    enum CombatState
    {
        Seeking,
        Combat
    }

    enum TurnState
    {
        zero,
        ninety,
        oneEighty,
        twoSeventy
    }

    #endregion
}
