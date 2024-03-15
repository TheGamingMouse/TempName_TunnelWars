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
    readonly float rotSpeed = 5f;
    float closestDistance;
    float newDistance;
    readonly float range = 5f;
    readonly float searchTimer = 30f;

    [Header("Bools")]
    bool identifyingCover;
    bool searching;
    bool startPatrol;
    bool startSearch;
    bool startCombat;

    [Header("Arrays")]
    Cover[] covers;

    [Header("GameObjects")]
    [SerializeField] GameObject emptyPoint; // SerializeField is Important!
    
    [Header("Transforms")]
    [SerializeField] Transform playerTarget;
    Transform enemyObj;
    Transform coverTarget;
    Transform centrePoint;
    Transform feet;
    Transform searchCentrePoint;
    Transform player;
    
    [Header("Vector3s")]
    Vector3 coverPoint;

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
        player = GameObject.FindWithTag("Player").transform;

        agent.SetDestination(centrePoint.position);

        startPatrol = true;
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
                    startPatrol = false;
                    startCombat = true;
                    startSearch = false;
                    searching = false;
                    cState = CombatState.Combat;
                }

                Destroy(GameObject.Find("EmptyPoint(Clone)"));

                if (startPatrol)
                {
                    agent.SetDestination(centrePoint.position);

                    if (agent.remainingDistance <= agent.stoppingDistance)
                    {
                        startPatrol = false;
                    }
                }
                else
                {
                    Patrol();
                }
                break;
            
            case CombatState.Combat:

                // TODO - Add Complexity:
                //          - Enemy should only leave CombatState.Combat when it has not seen the player in a longer amount of time.
                //          - Enemy should seek cover that breaks line of sight to player.
                //          - When player comes too close, enemy should relocate to a different coverPoint, breaking line of sight again.
                //          - Enemy should periodically leave cover to shoot at player, then re-enter cover.

                GameObject playerLastPos = Instantiate(emptyPoint, player.position, player.rotation);
                searchCentrePoint = playerLastPos.transform;
                
                if (startCombat)
                {
                    agent.SetDestination(coverPoint);
                    startCombat = false;
                    Destroy(GameObject.Find("EmptyPoint(Clone)"));
                }
                
                if (!playerTarget)
                {
                    startPatrol = false;
                    startCombat = false;
                    startSearch = true;
                    cState = CombatState.Searching;
                    return;
                }

                Destroy(playerLastPos);
                RotateToPlayer();
                break;
            
            case CombatState.Searching:
                if (playerTarget)
                {
                    startPatrol = false;
                    startCombat = true;
                    startSearch = false;
                    searching = false;
                    cState = CombatState.Combat;
                }

                if (!searching)
                {
                    StartCoroutine(BeginSearchTimer());
                    searching = true;
                }

                if (startSearch)
                {
                    agent.SetDestination(searchCentrePoint.position);
                    if (agent.remainingDistance <= agent.stoppingDistance)
                    {
                        startSearch = false;
                    }
                }
                else
                {
                    SearchForPlayer();
                }
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
                coverPoint = new Vector3(coverTarget.position.x, transform.position.y, coverTarget.position.z);
            }
        }

        identifyingCover = false;
    }

    IEnumerator RefreshCover()
    {
        yield return new WaitForSeconds(0.5f);
        
        closestDistance = 0f;
    }

    void RotateToPlayer()
    {
        Vector3 direction = playerTarget.position - transform.position;
        Vector3 newDirection = Vector3.RotateTowards(transform.forward, direction, rotSpeed * Time.deltaTime, 0);

        transform.rotation = Quaternion.LookRotation(newDirection);
    }

    #endregion

    #region SearchingMethods

    IEnumerator BeginSearchTimer()
    {
        yield return new WaitForSeconds(searchTimer);

        searching = false;
        startPatrol = true;
        startCombat = false;
        startSearch = false;
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
