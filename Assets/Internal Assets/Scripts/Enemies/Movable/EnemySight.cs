using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class EnemySight : MonoBehaviour
{
    #region Events

    public static event Action OnTargetFound;

    #endregion
    
    #region Variables

    [Header("Floats")]
    public float range = 25f;
    readonly float fovAngle = 135f;
    readonly float engangementRange = 50f;

    [Header("Bools")]
    public bool tracking;
    public bool inFov;

    [Header("Lists")]
    readonly List<AudioSource> audioSourcePool = new();
    
    [Header("Transforms")]
    public Transform target;
    Transform player;

    [Header("Vector3s")]
    Vector3 topAngle;
    Vector3 bottomAngle;

    [Header("LayerMasks")]
    [SerializeField] LayerMask playerMask; // SerializeField is Important!
    [SerializeField] LayerMask obstructionMask; // SerializeField is Important!

    [Header("AudioClips")]
    AudioClip audioTargetFound;

    [Header("Components")]
    EnemySightAudioStorage esas;
    [SerializeField] AudioMixer audioMixer; // SerializeField is Important!
    [SerializeField] AudioMixerGroup sfxVolume; // SerializeField is Important!
    EnemySight enemySight;

    #endregion

    #region Subscriptions

    void OnEnable()
    {
        EnemyHealth.OnDamageTaken += HandleDamageTaken;
        EnemySight.OnTargetFound += HandleTargetFound;
    }

    void OnDisable()
    {
        EnemyHealth.OnDamageTaken -= HandleDamageTaken;
        EnemySight.OnTargetFound += HandleTargetFound;
    }
    
    #endregion
    
    #region StartUpdate

    // Start is called before the first frame update
    void Start()
    {
        enemySight = this.GetComponent<EnemySight>();

        player = GameObject.FindGameObjectWithTag("Player").transform;
        esas = GameObject.FindGameObjectWithTag("Storage").transform.Find("AudioStorages/EnemySight").GetComponent<EnemySightAudioStorage>();

        audioTargetFound = esas.audioTargetFound;
    }

    // Update is called once per frame
    void Update()
    {
        topAngle = DirFromAngle(fovAngle / 2);
        bottomAngle = DirFromAngle(-fovAngle / 2);

        if (!target)
        {
            tracking = false;
            FindTarget();
            return;
        }

        if (target)
        {
            tracking = true;

            if (!TargetInRange() || TargetObstructed())
            {
                target = null;
            }
        }

        inFov = TargetInFOV();
    }

    #endregion

    #region Methods

    void FindTarget()
    {
        RaycastHit[] hits = Physics.SphereCastAll(transform.position, range, transform.position, playerMask);
        
        if ((hits.Length > 0) && TargetInRange() && !TargetObstructed())
        {
            OnTargetFound?.Invoke();
        }
    }

    bool TargetInRange()
    {
        if (Vector3.Distance(player.position, transform.position) <= range)
        {
            return true;
        }
        return false;
    }

    bool TargetInFOV()
    {
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, range, playerMask);

        if (rangeChecks.Length > 0 && TargetInRange() && !TargetObstructed())
        {
            Vector3 direction = (player.position - transform.position).normalized;

            if (Vector3.Angle(transform.forward, direction) < fovAngle / 2)
            {
                return true;
            }
            return false;
        }
        return false;
    }

    bool TargetObstructed()
    {
        bool hit = Physics.Linecast(transform.position, player.position, obstructionMask);

        if (hit)
        {
            return true;
        }
        return false;
    }

    Vector3 DirFromAngle(float angleIndDegrees)
    {
        return new Vector3(Mathf.Sin(angleIndDegrees * Mathf.Deg2Rad), 0f, Mathf.Cos(angleIndDegrees * Mathf.Deg2Rad));
    }

    #endregion

    #region SubscriptionHandlers

    void HandleDamageTaken()
    {
        if (!target && (Vector3.Distance(transform.position, player.position) < engangementRange) && enemySight != null)
        {
            target = player;
            StartCoroutine(PlayAudioTargetFound());
        }
    }

    void HandleTargetFound()
    {
        if (!target && (Vector3.Distance(transform.position, player.position) < engangementRange) && enemySight != null)
        {
            target = player;
            StartCoroutine(PlayAudioTargetFound());
        }
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

    IEnumerator PlayAudioTargetFound()
    {
        PlayClip(audioTargetFound);

        yield return new WaitForSeconds(2f);

        StopClip(audioTargetFound);

        // Walkie-Talkie SoundFX by HolmesAudio -- https://freesound.org/s/318783/ -- License: Attribution 3.0
    }

    #endregion

    #region Gizmos

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, range);

        if (player)
        {
            Gizmos.DrawLine(transform.position, player.position);
        }

        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, engangementRange);

        if (target)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, target.position);
        }

        Gizmos.matrix = transform.localToWorldMatrix;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(0f, 0f, 0f), transform.localPosition + topAngle * range);
        Gizmos.DrawLine(new Vector3(0f, 0f, 0f), transform.localPosition + bottomAngle * range);
    }

    #endregion
}
