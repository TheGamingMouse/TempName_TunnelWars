using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class EnemyShooting : MonoBehaviour
{
    #region Variables

    [Header("Enum States")]
    ReloadState rState;

    [Header("Ints")]
    readonly int damage = 10;
    
    [Header("Floats")]
    readonly float cooldown = 0.17f;
    readonly float magSize = 45f;
    float bulletsLeft;
    float reloadTime = 0f;
    readonly float reloadCooldown = 3f;
    float waitTime;
    
    [Header("Bools")]
    bool canShoot;
    bool reloading;
    bool inFov;
    bool prepShooting;
    bool shooting;
    bool combatting;

    [Header("Lists")]
    readonly List<AudioSource> audioSourcePool = new();

    [Header("Transforms")]
    Transform spawnedPrefabs;
    Transform spawnedImpacts;

    [Header("AudioClips")]
    AudioClip audioShoot;
    AudioClip audioReload;

    [Header("Components")]
    ParticleSystem muzzleFlash;
    [SerializeField] AudioMixer audioMixer; // SerializeField is Important!
    [SerializeField] AudioMixerGroup sfxVolume; // SerializeField is Important!

    #endregion

    #region StartUpdate

    // Start is called before the first frame update
    void Start()
    {
        spawnedPrefabs = GameObject.FindGameObjectWithTag("Prefabs").transform;
        spawnedImpacts = spawnedPrefabs.Find("SpawnedImpacts").transform;
        
        muzzleFlash = GetComponentInChildren<ParticleSystem>();
        
        EnemyShootingAudioStorage audioStorage = GameObject.FindGameObjectWithTag("Storage").transform.Find("AudioStorages/EnemyShooting").GetComponent<EnemyShootingAudioStorage>();
        audioShoot = audioStorage.audioShoot;
        audioReload = audioStorage.audioReload;

        canShoot = true;

        bulletsLeft = magSize;
        reloadTime = reloadCooldown;
    }

    // Update is called once per frame
    void Update()
    {
        inFov = GetComponent<EnemySight>().inFov;
        if (inFov && !prepShooting)
        {
            StartCoroutine(Shooting());

            prepShooting = true;
        }

        combatting = GetComponent<EnemyMovement>().combatting;
        if (combatting)
        {
            waitTime = 0f;
        }
        else
        {
            waitTime = 1f;
        }

        if (shooting && canShoot && bulletsLeft > 0 && inFov)
        {
            Shoot();
        }
        if (!inFov)
        {
            StopCoroutine(nameof(Shooting));
            
            shooting = false;
            prepShooting = false;
        }

        switch (rState)
        {
            case ReloadState.Ready:
                if (bulletsLeft == 0 && !reloading)
                {
                    PlayClip(audioReload);
                    
                    reloading = true;
                    canShoot = false;
                    
                    rState = ReloadState.ReloadStart;
                }
                break;
            case ReloadState.ReloadStart:
                reloadTime -= Time.deltaTime;

                if (reloadTime <= 0)
                {
                    rState = ReloadState.Reloadfinishing;
                }
                break;
            case ReloadState.Reloadfinishing:
                reloadTime = reloadCooldown;

                FinishReload();

                rState = ReloadState.Ready;
                break;
        }
    }

    #endregion

    #region Methods

    void Shoot()
    {
        muzzleFlash.Play();
        PlayClip(audioShoot);

        canShoot = false;

        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit))
        {
            if (hit.collider.gameObject.TryGetComponent<PlayerHealth>(out PlayerHealth pComp))
            {
                pComp.TakeDamage(damage, transform.position);
            }
        }

        bulletsLeft--;

        Invoke(nameof(ResetShot), cooldown);
    }

    void ResetShot()
    {
        canShoot = true;
    }

    void FinishReload()
    {
        reloading = false;
        canShoot = true;

        bulletsLeft = magSize;
    }

    IEnumerator Shooting()
    {
        yield return new WaitForSeconds(waitTime);

        shooting = true;
        // print("Shooting");
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

    void PlayClip(AudioClip clip)
    {
        AudioSource source = GetAvailablePoolSource();
        source.clip = clip;
        source.Play();
    }

    #endregion

    #region Enums

    enum ReloadState
    {
        Ready,
        ReloadStart,
        Reloadfinishing
    }

    #endregion
}
