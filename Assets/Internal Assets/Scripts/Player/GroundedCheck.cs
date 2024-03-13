using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundedCheck : MonoBehaviour
{
    #region Variables
    
    [Header("Floats")]
    float jumpHeight;
    
    [Header("Bools")]
    public bool isOnGround;

    [Header("Components")]
    Rigidbody rb;

    #endregion

    #region StartUpdate

    void Start()
    {
        rb = GetComponentInParent<Rigidbody>();
        jumpHeight = GetComponentInParent<PlayerMovement>().jumpHeight;
    }
    
    void Update()
    {
        if (Input.GetKey(KeyCode.Space) && isOnGround)
        {
            rb.AddForce(Vector3.up * jumpHeight, ForceMode.Impulse);
            isOnGround = false;
        }
    }

    #endregion

    #region Methods

    void OnTriggerStay(Collider coll)
    {
        if (coll.transform.CompareTag("Obstruction") || coll.transform.CompareTag("Cover"))
        {
            isOnGround = true;
        }
    }

    #endregion
}
