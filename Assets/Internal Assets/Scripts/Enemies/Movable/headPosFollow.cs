using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class headPosFollow : MonoBehaviour
{
    #region Variables

    [Header("Floats")]
    float newDistance;
    float closestDistance;

    [Header("Arrays")]
    GameObject[] heads;

    [Header("Transforms")]
    Transform headPos;
    Transform potHead;

    #endregion

    #region StartUpdate

    // Start is called before the first frame update
    void Start()
    {
        heads = GameObject.FindGameObjectsWithTag("Enemy");

        foreach(GameObject g in heads)
        {
            potHead = g.transform.Find("Head");
            newDistance = Vector3.Distance(potHead.position, transform.position);

            if (newDistance < closestDistance || closestDistance == 0)
            {
                closestDistance = newDistance;
                headPos = potHead;
            }
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(headPos.position.x, transform.position.y ,headPos.position.z);
    }

    #endregion
}
