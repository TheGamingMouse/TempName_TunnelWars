using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class TurretShooting : MonoBehaviour
{
    #region Variables

    [Header("Enum States")]
    ReloadState rState;

    [Header("Ints")]
    readonly int damage = 10;
    
    [Header("Floats")]
    readonly float waitTime = 0.5f;
    readonly float cooldown = 0.12f;
    readonly float magSize = 35f;
    float bulletsLeft;
    float reloadTime = 0f;
    readonly float reloadCooldown = 1.5f;
    
    [Header("Bools")]
    bool canShoot;
    bool reloading;
    bool inFov;
    bool prepShooting;
    bool shooting;
    bool bursting;

    [Header("Lists")]
    readonly List<AudioSource> audioSourcePool = new();

    [Header("Transforms")]
    Transform spawnedPrefabs;
    Transform spawnedImpacts;
    Transform firePoint;

    [Header("AudioClips")]
    AudioClip audioShoot;
    AudioClip audioReload;

    [Header("Components")]
    ParticleSystem muzzleFlash;
    [SerializeField] AudioMixer audioMixer; // SerializeField is Important!

    #endregion

    #region StartUpdate

    // Start is called before the first frame update
    void Start()
    {
        spawnedPrefabs = GameObject.FindGameObjectWithTag("Prefabs").transform;
        spawnedImpacts = spawnedPrefabs.Find("SpawnedImpacts").transform;
        firePoint = transform.Find("TurretBarrel/FirePoint");
        
        muzzleFlash = GetComponentInChildren<ParticleSystem>();
        
        TurretShootingAudioStorage audioStorage = GameObject.FindGameObjectWithTag("Storage").transform.Find("AudioStorages/TurretShooting").GetComponent<TurretShootingAudioStorage>();
        audioShoot = audioStorage.audioShoot;
        audioReload = audioStorage.audioReload;

        canShoot = true;

        bulletsLeft = magSize;
        reloadTime = reloadCooldown;
    }

    // Update is called once per frame
    void Update()
    {
        inFov = GetComponent<TurretSight>().inFov;
        if (inFov && !prepShooting)
        {
            StartCoroutine(Shooting());

            prepShooting = true;
        }

        if (shooting && canShoot && bulletsLeft > 0 && inFov && !bursting)
        {
            StartCoroutine(ShootBurst());
            bursting = true;
        }
        if (!inFov)
        {
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

        if (Physics.Raycast(firePoint.position, firePoint.transform.forward, out RaycastHit hit))
        {
            if (hit.collider.gameObject.TryGetComponent<PlayerHealth>(out PlayerHealth pComp))
            {
                pComp.TakeDamage(damage, transform.position);
            }
        }

        bulletsLeft--;

        Invoke(nameof(ResetShot), cooldown);
    }

    void BurstShot()
    {
        if (bulletsLeft > 0f)
        {
            Shoot();
        }
        else
        {
            return;
        }
    }

    IEnumerator ShootBurst()
    {
        WaitForSeconds wait = new(cooldown * 0.75f);

        BurstShot();

        yield return wait;

        BurstShot();

        yield return wait;

        BurstShot();

        yield return new WaitForSeconds(cooldown * 1.5f);

        bursting = false;
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

    IEnumerator Shooting()
    {
        yield return new WaitForSeconds(waitTime);

        shooting = true;
        // print("Shooting");
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
