using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoDrop : MonoBehaviour
{
    #region Variables
    
    [Header("Floats")]
    public float ammount;
    readonly float moveSpeed = 0.2f;
    readonly float rotateSpeed = 1f;

    [Header("Bools")]
    bool direction;

    [Header("GameObjects")]
    [SerializeField] GameObject parentObj; // SerializeField is Important!

    [Header("Vector3s")]
    Vector3 directionTranslation;

    #endregion

    #region StartUpdate

    // Start is called before the first frame update
    void Awake()
    {
        SetAmount();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        directionTranslation = direction ? transform.up : -transform.up;
        directionTranslation *= Time.deltaTime * moveSpeed;

        transform.Translate(directionTranslation);
        transform.Rotate(new Vector3(0f, rotateSpeed, 0f));
    }

    #endregion

    #region Methods

    void SetAmount()
    {
        ammount = Random.Range(10, 31);
    }

    void OnTriggerEnter(Collider coll)
    {
        if (coll.transform.CompareTag("MoveCollider"))
        {
            direction = !direction;
        }
    }

    #endregion
}
