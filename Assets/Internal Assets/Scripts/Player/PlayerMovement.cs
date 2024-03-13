using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerMovement : MonoBehaviour
{
    #region Variables

    [Header("Floats")]
    readonly float moveSpeedBase = 5f;
    float moveSpeedCurr;
    public float jumpHeight = 2f;
    readonly float sprintSpeed = 7.5f;
    public float sensX = 1.25f;
    public float sensY = 1.25f;
    float mouseX;
    float mouseY;
    float xRot;
    float yRot;

    [Header("Bools")]
    bool isOnGround;
    public bool crouched;

    [Header("GameObjects")]
    GameObject rifle;

    [Header("Transforms")]
    Transform orientation;

    [Header("Vector3s")]
    Vector3 move;
    Vector3 moveDir;

    [Header("Components")]
    Rigidbody rb;
    Camera cam;

    #endregion

    #region StartUpdate

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rifle = FindObjectOfType<Rifle>().gameObject;
        orientation = transform.Find("Orientation");
        cam = Camera.main;

        moveSpeedCurr = moveSpeedBase;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + moveSpeedCurr * Time.fixedDeltaTime * moveDir);
    }

    // Update is called once per frame
    void Update()
    {
        isOnGround = GetComponentInChildren<GroundedCheck>().isOnGround;

        move.x = Input.GetAxisRaw("Horizontal");
        move.z = Input.GetAxisRaw("Vertical");

        moveDir = orientation.forward * move.z + orientation.right * move.x;

        mouseX = Input.GetAxisRaw("Mouse X") * sensX; // * Time.deltaTime;
        mouseY = Input.GetAxisRaw("Mouse Y") * sensY; // * Time.deltaTime;

        yRot += mouseX;
        xRot -= mouseY;
        xRot = Mathf.Clamp(xRot, -80f, 70f);

        cam.transform.rotation = Quaternion.Euler(xRot, yRot, 0f);
        transform.rotation = Quaternion.Euler(0f, yRot, 0f);

        if (Input.GetKey(KeyCode.LeftShift) && isOnGround)
        {
            moveSpeedCurr = sprintSpeed;
        }
        else
        {
            moveSpeedCurr = moveSpeedBase;
        }

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            transform.localScale = new Vector3(1f, 0.5f, 1f);
            transform.position = new Vector3(transform.position.x, transform.position.y - 0.5f, transform.position.z);
            
            crouched = true;
        }
        else if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            transform.localScale = new Vector3(1f, 1f, 1f);
            transform.position = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);
            
            crouched = false;
        }
    }

    #endregion
}
