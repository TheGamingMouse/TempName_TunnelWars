using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Audio;

public class EnemyMovement : MonoBehaviour
{
    #region Variables

    [Header("Enum States")]
    [SerializeField] CombatState cState;

    [Header("Floats")]
    readonly float rotSpeed = 5f;
    float closestDistanceCover;
    float newDistanceCover;
    float closestDistanceCentrePoints;
    float newDistanceCentrePoints;
    float closestDistanceEnemyObjs;
    float newDistanceEnemyObjs;
    readonly float range = 5f;
    readonly float searchTimer = 30f;
    readonly float combattingTimer = 20f;
    float sightRange;

    [Header("Bools")]
    bool identifyingCover;
    bool searching;
    bool startPatrol;
    bool startSearch;
    bool startCombat;
    bool playerNotFound;
    bool exitCombat;
    public bool combatting;
    bool leaveCover;
    bool soundMade;
    bool playWalkingAudio;

    [Header("Arrays")]
    Cover[] covers;
    GameObject[] centrePoints;
    GameObject[] enemyObjs;

    [Header("Lists")]
    readonly List<AudioSource> audioSourcePool = new();

    [Header("GameObjects")]
    [SerializeField] GameObject emptyPoint; // SerializeField is Important!
    
    [Header("Transforms")]
    Transform playerTarget;
    Transform enemyObj;
    Transform coverTarget;
    [SerializeField] Transform centrePoint;
    Transform feet;
    Transform searchCentrePoint;
    Transform player;
    Transform potCentrePoint;
    Transform potEnemyObj;
    
    [Header("Vector3s")]
    Vector3 coverPoint;

    [Header("Navmeshes")]
    NavMeshAgent agent;

    [Header("AudioClips")]
    AudioClip audioWalking;

    [Header("Components")]
    [SerializeField] AudioMixer audioMixer; // SerializeField is Important!
    EnemyMovementAudioStorage emas;
    [SerializeField] AudioMixerGroup sfxVolume; // SerializeField is Important!

    #endregion

    #region StartUpdate

    // Start is called before the first frame update
    void Start()
    {
        covers = FindObjectsByType<Cover>(FindObjectsSortMode.None);
        centrePoints = GameObject.FindGameObjectsWithTag("CenterPoint");
        enemyObjs = GameObject.FindGameObjectsWithTag("Enemy");
        
        foreach(GameObject g in centrePoints)
        {
            potCentrePoint = g.transform;
            newDistanceCentrePoints = Vector3.Distance(potCentrePoint.position, transform.position);

            if (newDistanceCentrePoints < closestDistanceCentrePoints || closestDistanceCentrePoints == 0)
            {
                closestDistanceCentrePoints = newDistanceCentrePoints;
                centrePoint = potCentrePoint;
            }
        }

        foreach(GameObject g in enemyObjs)
        {
            potEnemyObj = g.transform;
            newDistanceEnemyObjs = Vector3.Distance(potEnemyObj.position, transform.position);

            if (newDistanceEnemyObjs < closestDistanceEnemyObjs || closestDistanceEnemyObjs == 0)
            {
                closestDistanceEnemyObjs = newDistanceEnemyObjs;
                enemyObj = potEnemyObj;
            }
        }
        
        agent = enemyObj.gameObject.GetComponent<NavMeshAgent>();
        feet = enemyObj.Find("Body/Feet");
        player = GameObject.FindWithTag("Player").transform;
        emas = GameObject.FindGameObjectWithTag("Storage").transform.Find("AudioStorages/EnemyMovement").GetComponent<EnemyMovementAudioStorage>();

        agent.SetDestination(centrePoint.position);

        sightRange = GetComponent<EnemySight>().range;
        audioWalking = emas.audioWalking;

        startPatrol = true;
        exitCombat = true;
    }
    
    // Update is called once per frame
    void Update()
    {
        playerTarget = GetComponent<EnemySight>().target;
        soundMade = Camera.main.transform.GetComponentInChildren<Rifle>().soundMade;
        transform.position = new Vector3(transform.position.x, feet.position.y + 1.5f, transform.position.z);
        
        IdentifyCover();
        if (!identifyingCover)
        {
            StartCoroutine(RefreshCover());
            identifyingCover = true;
        }

        if (agent.speed != 0 && !playWalkingAudio)
        {
            StartCoroutine(WalkingAudio());
            playWalkingAudio = true;
        }
        if (agent.speed == 0 && playWalkingAudio)
        {
            StopCoroutine(nameof(WalkingAudio));
            StopClip(audioWalking);
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
                    playerNotFound = false;
                    cState = CombatState.Combat;
                }

                Destroy(GameObject.Find("SearchPoint(Clone)"));

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

                    if ((Vector3.Distance(player.position, transform.position) <= sightRange / 2) && soundMade)
                    {
                        GameObject playerLastPos = Instantiate(emptyPoint, player.position, player.rotation);
                        searchCentrePoint = playerLastPos.transform;

                        startPatrol = false;
                        startCombat = false;
                        startSearch = true;
                        startPatrol = false;
                        searching = false;
                        playerNotFound = false;
                        cState = CombatState.Searching;
                    }
                }
                break;
            
            case CombatState.Combat:
                if (!combatting)
                {
                    if (startCombat)
                    {
                        Destroy(GameObject.Find("SearchPoint(Clone)"));
                        GameObject playerLastPos = Instantiate(emptyPoint, player.position, player.rotation);
                        searchCentrePoint = playerLastPos.transform;
                        agent.SetDestination(coverPoint);
                        startCombat = false;
                    }
                    
                    if (playerTarget)
                    {
                        StopCoroutine(nameof(CoverFireTimer));
                        playerNotFound = false;
                        exitCombat = true;
                    }
                    else
                    {
                        if (exitCombat)
                        {
                            StartCoroutine(CoverFireTimer());
                            combatting = true;
                            exitCombat = false;
                        }
                        
                        if (playerNotFound)
                        {
                            startPatrol = false;
                            startCombat = false;
                            startSearch = true;
                            cState = CombatState.Searching;
                            return;
                        }
                    }
                }
                else
                {
                    if (playerTarget)
                    {
                        combatting = true;
                        leaveCover = true;
                        StopCoroutine(nameof(CoverFireTimer));
                        StartCoroutine(CoverFireTimer());
                        StartCoroutine(FindCover());
                    }
                    else
                    {
                        if (leaveCover)
                        {
                            StartCoroutine(LeaveCover());
                            leaveCover = false;
                        }
                    }
                }

                if (Vector3.Distance(player.position, enemyObj.position) <= range)
                {
                    agent.SetDestination(coverPoint);
                }

                if (playerTarget)
                {
                    Destroy(GameObject.Find("SearchPoint(Clone)"));
                    StartCoroutine(RememberPlayer());
                    RotateToPlayer();
                }
                break;
            
            case CombatState.Searching:
                if (playerTarget)
                {
                    startPatrol = false;
                    startCombat = true;
                    startSearch = false;
                    searching = false;
                    playerNotFound = false;
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

    IEnumerator WalkingAudio()
    {
        PlayClip(audioWalking);
        
        yield return new WaitForSeconds(audioWalking.length);

        playWalkingAudio = false;
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
            newDistanceCover = Vector3.Distance(c.transform.position, transform.position);
            coverTarget = c.transform;

            if (newDistanceCover < closestDistanceCover || closestDistanceCover == 0)
            {
                if (!c.LOS)
                {
                    closestDistanceCover = newDistanceCover;
                    coverPoint = new Vector3(coverTarget.position.x, transform.position.y, coverTarget.position.z);
                }
            }
        }

        identifyingCover = false;
    }

    IEnumerator RefreshCover()
    {
        yield return new WaitForSeconds(0.5f);
        
        closestDistanceCover = 0f;
    }

    IEnumerator RememberPlayer()
    {
        GameObject playerLastPos = Instantiate(emptyPoint, player.position, player.rotation);
        searchCentrePoint = playerLastPos.transform;

        yield return new WaitForNextFrameUnit();

        if (playerTarget)
        {
            Destroy(playerLastPos);
        }
    }

    void RotateToPlayer()
    {
        Vector3 direction = playerTarget.position - transform.position;
        Vector3 newDirection = Vector3.RotateTowards(transform.forward, direction, rotSpeed * Time.deltaTime, 0);

        transform.rotation = Quaternion.LookRotation(newDirection);
    }

    IEnumerator CoverFireTimer()
    {
        yield return new WaitForSeconds(combattingTimer);

        playerNotFound = true;
        combatting = false;
    }

    IEnumerator LeaveCover()
    {
        yield return new WaitForSeconds(2f);

        agent.SetDestination(searchCentrePoint.position);
        // print("Agent leaving cover");
    }

    IEnumerator FindCover()
    {
        agent.SetDestination(enemyObj.position);
        // print("Agent standing still");
        
        yield return new WaitForSeconds(2f);

        agent.SetDestination(coverPoint);
        // print("Agent returning to cover");
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

    #region AudioMethods

    AudioSource AddNewSourceToPool()
    {
        audioMixer.GetFloat("sfxVolume", out float dBSFX);
        float SFXVolume = Mathf.Pow(10.0f, dBSFX / 20.0f);

        audioMixer.GetFloat("masterVolume", out float dBMaster);
        float masterVolume = Mathf.Pow(10.0f, dBMaster / 20.0f);
        
        float realVolume = (SFXVolume + masterVolume) / 2 * 0.05f;
        
        AudioSource newSource = gameObject.AddComponent<AudioSource>();
        newSource.playOnAwake = false;
        newSource.volume = realVolume;
        newSource.spatialBlend = 1f;
        newSource.outputAudioMixerGroup = sfxVolume;
        audioSourcePool.Add(newSource);
        return newSource;
    }

    AudioSource GetAvailablePoolSource()
    {
        //Fetch the first source in the pool that is not currently playing anything
        foreach (var source in audioSourcePool)
        {
            if (!source.isPlaying)
            {
                return source;
            }
        }
 
        //No unused sources. Create and fetch a new source
        return AddNewSourceToPool();
    }

    AudioSource GetUnavailablePoolSource()
    {
        //Fetch the first source in the pool that is not currently playing anything
        foreach (var source in audioSourcePool)
        {
            if (source.isPlaying)
            {
                return source;
            }
        }
        return null;
    }

    void PlayClip(AudioClip clip)
    {
        AudioSource source = GetAvailablePoolSource();
        source.clip = clip;
        source.Play();
    }

    void StopClip(AudioClip clip)
    {
        AudioSource source = GetUnavailablePoolSource();
        if (source == null)
        {
            return;
        }
        source.clip = clip;
        source.Stop();
    }

    #endregion
    
    #region Gizmos

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawCube(coverPoint, Vector3.one * 0.3f);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, range);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange / 2);
    }

    #endregion
}
