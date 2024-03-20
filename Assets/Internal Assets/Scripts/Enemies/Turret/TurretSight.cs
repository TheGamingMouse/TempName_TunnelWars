using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class TurretSight : MonoBehaviour
{
    #region Variables

    [Header("Floats")]
    public float range = 25f;
    readonly float fovAngle = 135f;

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
    AudioClip audioTargetLost;

    [Header("Components")]
    TurretSightAudioStorage tsas;
    [SerializeField] AudioMixer audioMixer; // SerializeField is Important!
    [SerializeField] AudioMixerGroup sfxVolume; // SerializeField is Important!

    #endregion

    #region StartUpdate
    
    // Start is called before the first frame update
    void Start()
    {
        player = Camera.main.transform;
        tsas = GameObject.FindGameObjectWithTag("Storage").transform.Find("AudioStorages/TurretSight").GetComponent<TurretSightAudioStorage>();

        audioTargetFound = tsas.audioTargetFound;
        audioTargetLost = tsas.audioTargetLost;
    }

    // Update is called once per frame
    void Update()
    {
        if (!target)
        {
            tracking = false;
            FindTarget();
            return;
        }

        if (target)
        {
            tracking = true;

            topAngle = DirFromAngle(fovAngle / 2);
            bottomAngle = DirFromAngle(-fovAngle / 2);

            if (!TargetInRange() || TargetObstructed())
            {
                target = null;
                StopClip(audioTargetFound);
                PlayClip(audioTargetLost);
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
            target = player;
            StopClip(audioTargetLost);
            PlayClip(audioTargetFound);
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

    #region AudioMethods

    AudioSource AddNewSourceToPool()
    {
        audioMixer.GetFloat("sfxVolume", out float dBSFX);
        float SFXVolume = Mathf.Pow(10.0f, dBSFX / 20.0f);

        audioMixer.GetFloat("masterVolume", out float dBMaster);
        float masterVolume = Mathf.Pow(10.0f, dBMaster / 20.0f);
        
        float realVolume = (SFXVolume + masterVolume) / 2 * 0.5f;
        
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

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, range);

        if (player)
        {
            Gizmos.DrawLine(transform.position, player.position);
        }

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
