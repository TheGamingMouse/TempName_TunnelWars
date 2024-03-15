using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cover : MonoBehaviour
{
    #region Variables

    [Header("Bools")]
    public bool LOS;
    bool LOSBody;
    bool LOSHead;
    bool LOSRight;
    bool LOSLeft;
    bool LOSForward;
    bool LOSBackward;

    [Header("GameObjects")]
    GameObject head;
    GameObject right;
    GameObject left;
    GameObject forward;
    GameObject backward;
    [SerializeField] GameObject emptyPrefab;

    [Header("Transforms")]
    Transform player;
    Transform playerHead;

    [Header("Vector3s")]
    Vector3 headVector;
    Vector3 rightVector;
    Vector3 leftVector;
    Vector3 forwardVector;
    Vector3 backwardVector;

    [Header("LayerMasks")]
    LayerMask obstructionMask;

    #endregion

    #region StartUpdate

    void Start()
    {
        playerHead = Camera.main.transform;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        obstructionMask = LayerMask.GetMask("Terrain");
        
        headVector = new Vector3(transform.position.x, playerHead.position.y, transform.position.z);
        rightVector = new Vector3(transform.position.x + 0.45f, transform.position.y, transform.position.z);
        leftVector = new Vector3(transform.position.x - 0.45f, transform.position.y, transform.position.z);
        forwardVector = new Vector3(transform.position.x, transform.position.y, transform.position.z + 0.45f);
        backwardVector = new Vector3(transform.position.x, transform.position.y, transform.position.z - 0.45f);

        head = Instantiate(emptyPrefab, headVector, Quaternion.identity, transform);
        right = Instantiate(emptyPrefab, rightVector, Quaternion.identity, transform);
        left = Instantiate(emptyPrefab, leftVector, Quaternion.identity, transform);
        forward = Instantiate(emptyPrefab, forwardVector, Quaternion.identity, transform);
        backward = Instantiate(emptyPrefab, backwardVector, Quaternion.identity, transform);
    }

    void Update()
    {
        LOSBody = LineOfSightBody();
        LOSHead = LineOfSightHead();
        LOSRight = LineOfSightRight();
        LOSLeft = LineOfSightLeft();
        LOSForward = LineOfSightForward();
        LOSBackward = LineOfSightBackward();

        if (!LOSBody && !LOSHead && !LOSRight && !LOSLeft && !LOSForward && !LOSBackward)
        {
            LOS = false;
        }
        else
        {
            LOS = true;
        }
    }

    #endregion

    #region Methods

    bool LineOfSightBody()
    {
        bool hit = Physics.Linecast(transform.position, player.position, obstructionMask);

        if (hit)
        {
            return false;
        }
        return true;
    }

    bool LineOfSightHead()
    {
        bool hit = Physics.Linecast(head.transform.position, player.position, obstructionMask);

        if (hit)
        {
            return false;
        }
        return true;
    }

    bool LineOfSightRight()
    {
        bool hit = Physics.Linecast(right.transform.position, player.position, obstructionMask);

        if (hit)
        {
            return false;
        }
        return true;
    }

    bool LineOfSightLeft()
    {
        bool hit = Physics.Linecast(left.transform.position, player.position, obstructionMask);

        if (hit)
        {
            return false;
        }
        return true;
    }

    bool LineOfSightForward()
    {
        bool hit = Physics.Linecast(forward.transform.position, player.position, obstructionMask);

        if (hit)
        {
            return false;
        }
        return true;
    }

    bool LineOfSightBackward()
    {
        bool hit = Physics.Linecast(backward.transform.position, player.position, obstructionMask);

        if (hit)
        {
            return false;
        }
        return true;
    }

    #endregion

    #region Gizmos

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawCube(transform.position, Vector3.one * 0.3f);
        
        if (player)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, player.position);

            if (LineOfSightBody())
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, player.position);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
       if (player)
       {
            Gizmos.DrawCube(rightVector, Vector3.one * 0.1f);
            Gizmos.DrawCube(leftVector, Vector3.one * 0.1f);
            Gizmos.DrawCube(forwardVector, Vector3.one * 0.1f);
            Gizmos.DrawCube(backwardVector, Vector3.one * 0.1f);
            Gizmos.DrawCube(headVector, Vector3.one * 0.1f);

            if (LineOfSightHead())
            {
                Gizmos.color = Color.red;
            }
            else
            {
                Gizmos.color = Color.green;
            }
            Gizmos.DrawLine(head.transform.position, playerHead.position);

            if (LineOfSightRight())
            {
                Gizmos.color = Color.red;
            }
            else
            {
                Gizmos.color = Color.green;
            }
            Gizmos.DrawLine(right.transform.position, playerHead.position);

            if (LineOfSightLeft())
            {
                Gizmos.color = Color.red;
            }
            else
            {
                Gizmos.color = Color.green;
            }
            Gizmos.DrawLine(left.transform.position, playerHead.position);

            if (LineOfSightForward())
            {
                Gizmos.color = Color.red;
            }
            else
            {
                Gizmos.color = Color.green;
            }
            Gizmos.DrawLine(forward.transform.position, playerHead.position);

            if (LineOfSightBackward())
            {
                Gizmos.color = Color.red;
            }
            else
            {
                Gizmos.color = Color.green;
            }
            Gizmos.DrawLine(backward.transform.position, playerHead.position);
       }
    }

    #endregion
}
