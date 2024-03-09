using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class camPosFollow : MonoBehaviour
{
    #region Variables

    [Header("Transforms")]
    Transform camPos;

    #endregion

    #region StartUpdate

    // Start is called before the first frame update
    void Start()
    {
        camPos = GameObject.FindGameObjectWithTag("CamPos").transform;
    }
    
    // Update is called once per frame
    void Update()
    {
        transform.position = camPos.position;
    }

    #endregion
}
