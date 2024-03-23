using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class StartCamMovement : MonoBehaviour
{
    #region Events

    public static event Action OnGameStart;

    #endregion

    #region Variables

    [Header("Flaots")]
    readonly float lookSpeed = 1.8f;
    readonly float moveSpeed = 0.65f;

    [Header("Bools")]
    bool moveToSecondPos;
    bool startMovement;

    [Header("Transforms")]
    [SerializeField] Transform startCam; // SerializeField is Important!
    [SerializeField] Transform lookAt; // SerializeField is Important!
    [SerializeField] Transform control; // SerializeField is Important!

    [Header("Vector3s")]
    Vector3 lookAtEndPos;
    Vector3 transformEndPos;

    [Header("Components")]
    [SerializeField] CinemachineVirtualCamera mainCam; // SerializeField is Important!

    #endregion

    #region StartUpdate

    // Start is called before the first frame update
    void Start()
    {
        lookAtEndPos = new Vector3(-3.5f, -4f, -12f);
        transformEndPos = new Vector3(3f, 9f, 15f);

        Invoke(nameof(StartMove), 2f);

        moveToSecondPos = false;
        startMovement = false;
    }

    void FixedUpdate()
    {
        if (moveToSecondPos)
        {
            lookAt.localPosition = Vector3.Slerp(lookAt.localPosition, lookAtEndPos, lookSpeed * Time.deltaTime);
        }

        if (startMovement)
        {
            transform.position = new Vector3(control.position.x, control.position.y, Vector3.Slerp(transform.position, transformEndPos, moveSpeed * Time.deltaTime).z);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    #endregion

    #region Methods

    void StartMove()
    {
        StartMovementRoutine();
        moveToSecondPos = true;
    }

    void StartMovementRoutine()
    {
        startMovement = true;
        StartCoroutine(FinishMovementRoutine());
    }

    IEnumerator FinishMovementRoutine()
    {
        yield return new WaitForSeconds(5f);

        OnGameStart?.Invoke();

        mainCam.enabled = true;
        transform.gameObject.SetActive(false);
    }
    
    #endregion
}
