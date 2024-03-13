using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    #region Variables

    [Header("Enum States")]
    [SerializeField] CombatState cState;

    [Header("Floats")]
    readonly float rotSpeed = 30f;
    readonly float moveSpeed = 1f;
    float closestDistance;
    float newDistance;
    readonly float range = 5f;
    readonly float searchTimer = 30f;

    [Header("Bools")]
    bool identifyingCover;
    bool searching;
    bool startPatrol;
    bool startSearch;

    [Header("Arrays")]
    Cover[] covers;

    [Header("GameObjects")]
    [SerializeField] GameObject emptyPoint; // [SerializeField] Important!
    
    [Header("Transforms")]
    Transform playerTarget;
    Transform enemyObj;
    Transform target;
    Transform coverTarget;
    Transform centrePoint;
    Transform feet;
    Transform searchCentrePoint;

    [Header("Navmeshes")]
    NavMeshAgent agent;

    #endregion

    #region StartUpdate

    // Start is called before the first frame update
    void Start()
    {
        enemyObj = GameObject.FindGameObjectWithTag("Enemy").transform;
        agent = enemyObj.gameObject.GetComponent<NavMeshAgent>();
        covers = FindObjectsByType<Cover>(FindObjectsSortMode.None);
        centrePoint = GameObject.FindGameObjectWithTag("CenterPoint").transform;
        feet = enemyObj.Find("Body/Feet");

        agent.SetDestination(centrePoint.position);
    }

    void FixedUpdate()
    {
        if (startPatrol)
        {
            transform.position = Vector3.Slerp(transform.position, centrePoint.position, Time.deltaTime * moveSpeed);
        }
        if (startSearch)
        {
            transform.position = Vector3.Slerp(transform.position, searchCentrePoint.position, Time.deltaTime * moveSpeed);
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        playerTarget = GetComponent<EnemySight>().target;

        transform.position = new Vector3(transform.position.x, feet.position.y + 1.5f, transform.position.z);
        
        IdentifyCover();
        if (!identifyingCover)
        {
            StartCoroutine(RefreshCover());
            identifyingCover = true;
        }

        switch (cState)
        {
            case CombatState.Patroling:
                if (playerTarget)
                {
                    target = playerTarget;
                    cState = CombatState.Combat;
                }

                Destroy(GameObject.Find("EmptyPoint(Clone)"));

                Patrol();
                break;
            
            case CombatState.Combat:
                searching = false;

                GameObject playerLastPos = Instantiate(emptyPoint, target.position, Quaternion.identity);
                searchCentrePoint = playerLastPos.transform;
                
                if (playerTarget)
                {
                    target = playerTarget;
                }
                else
                {
                    target = null;
                }
                
                if (!target)
                {
                    startSearch = true;
                    cState = CombatState.Searching;
                    return;
                }

                Destroy(playerLastPos);
                break;
            
            case CombatState.Searching:
                if (playerTarget)
                {
                    target = playerTarget;
                    cState = CombatState.Combat;
                }

                if (!searching)
                {
                    StartCoroutine(BeginSearchTimer());
                    searching = true;
                }

                SearchForPlayer();
                break;
        }
    }

    #endregion

    #region GeneralMethods

    bool RandomPoint(Vector3 center, float range, out Vector3 result)
    {
        Vector3 randomPoint = center + Random.insideUnitSphere * range;
        if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
        {
            result = hit.position;
            // print("RandomPoint = true");
            return true;
        }

        result = Vector3.zero;
        // print("RandomPoint = false");
        return false;
    }

    #endregion

    #region PatrolMethods

    void Patrol()
    {
        if(agent.remainingDistance <= agent.stoppingDistance || startPatrol)
        {
            if (RandomPoint(centrePoint.position, range, out Vector3 patrolPoint))
            {
                Vector3 patrolPosition = new(patrolPoint.x, transform.position.y, patrolPoint.z);
                agent.SetDestination(patrolPosition);

                startPatrol = false;
            }
        }
    }

    #endregion

    #region CombatMethods

    void IdentifyCover()
    {
        foreach(Cover c in covers)
        {
            newDistance = Vector3.Distance(c.transform.position, transform.position);
            coverTarget = c.transform;

            if (newDistance <= closestDistance || closestDistance == 0)
            {
                closestDistance = newDistance;
                agent.SetDestination(new Vector3(coverTarget.position.x, transform.position.y, coverTarget.position.z));
            }
        }

        identifyingCover = false;
    }

    IEnumerator RefreshCover()
    {
        yield return new WaitForSeconds(0.5f);
        
        closestDistance = 0f;
    }

    #endregion

    #region SearchingMethods

    IEnumerator BeginSearchTimer()
    {
        yield return new WaitForSeconds(searchTimer);

        searching = false;
        startPatrol = true;
        cState = CombatState.Patroling;
    }

    void SearchForPlayer()
    {
        if(agent.remainingDistance <= agent.stoppingDistance || startSearch)
        {
            if (RandomPoint(searchCentrePoint.position, range, out Vector3 searchPoint))
            {
                Vector3 searchPosition = new(searchPoint.x, transform.position.y, searchPoint.z);
                agent.SetDestination(searchPosition);

                startSearch = false;
            }
        }
    }
    
    #endregion
    
    #region Enums

    enum CombatState
    {
        Patroling,
        Combat,
        Searching
    }

    #endregion
}
