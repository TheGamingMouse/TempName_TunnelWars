using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class headPosFollow : MonoBehaviour
{
    #region Variables

    [Header("Transforms")]
    Transform headPos;

    #endregion

    #region StartUpdate

    // Start is called before the first frame update
    void Start()
    {
        headPos = GameObject.FindGameObjectWithTag("Enemy").transform.Find("Head").transform;
    }
    
    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(headPos.position.x, transform.position.y ,headPos.position.z);
    }

    #endregion
}
