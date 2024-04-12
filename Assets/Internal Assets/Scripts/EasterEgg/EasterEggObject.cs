using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EasterEggObject : MonoBehaviour
{
    float rotateSpeed = 1f;

    void FixedUpdate()
    {
        transform.Rotate(new Vector3(0f, rotateSpeed, 0f));
    }
}
