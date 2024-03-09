using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class Rifle : MonoBehaviour
{
    #region Variables

    [Header("Enum States")]
    ReloadState rState;
    public FireModeState fmState;

    [Header("Floats")]
    readonly float aimSpeed = 20f;
    readonly float zoomSpeed = 75f;
    readonly float zoom = 40f;
    public readonly float damage = 10f;
    readonly float cooldown = 0.12f;
    float reloadTime = 0f;
    readonly float reloadCooldown = 2.5f;
    public float bulletsLeft;
    readonly float magSize = 30f;
    public float totalAmmo = 120;

    [Header("Bools")]
    bool crouching;
    bool crouched;
    public bool aiming;
    bool isAutomatic = true;
    bool shooting;
    bool canShoot;
    bool reloading;
    bool burstMode;
    bool bursting;

    [Header("Lists")]
    private List<AudioSource> audioSourcePool = new();

    [Header("GameObjects")]
    [SerializeField] GameObject impact;

    [Header("Transforms")]
    Transform spawnedPrefabs;
    Transform spawnedImpacts;

    [Header("Vector3s")]
    [SerializeField] Vector3 nPos, aPos;

    [Header("AudioClips")]
    AudioClip audioShoot;
    AudioClip audioReload;

    [Header("Components")]
    Camera cam;
    ParticleSystem muzzleFlash;
    [SerializeField] AudioMixer audioMixer;

    #endregion

    #region StartUpdate

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
        spawnedPrefabs = GameObject.FindGameObjectWithTag("Prefabs").transform;
        spawnedImpacts = spawnedPrefabs.Find("SpawnedImpacts").transform;
        
        muzzleFlash = GetComponentInChildren<ParticleSystem>();

        RifleAudioStorage audioStorage = GameObject.FindGameObjectWithTag("Storage").transform.Find("AudioStorages/Rifle").GetComponent<RifleAudioStorage>();
        audioShoot = audioStorage.audioShoot;
        audioReload = audioStorage.audioReload;

        canShoot = true;

        bulletsLeft = magSize;
        reloadTime = reloadCooldown;
    }

    // Update is called once per frame
    void Update()
    {
        crouching = FindObjectOfType<PlayerMovement>().crouched;

        if (crouching && crouched)
        {
            aPos = new Vector3(aPos.x, aPos.y - 0.5f, aPos.z);
            crouched = true;
        }
        else
        {
            aPos = new Vector3(aPos.x, aPos.y, aPos.z);
            crouched = false;
        }

        if (Input.GetMouseButton(1))
        {
            transform.localPosition = Vector3.Slerp(transform.localPosition, aPos, aimSpeed * Time.deltaTime);
            cam.fieldOfView -= zoomSpeed * Time.deltaTime;
            cam.fieldOfView = Mathf.Clamp(cam.fieldOfView, zoom, 60);
            aiming = true;
        }
        else
        {
            transform.localPosition = Vector3.Slerp(transform.localPosition, nPos, aimSpeed * Time.deltaTime);
            cam.fieldOfView += zoomSpeed * Time.deltaTime;
            cam.fieldOfView = Mathf.Clamp(cam.fieldOfView, zoom, 60);
            aiming = false;
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            switch (fmState)
            {
                case FireModeState.FullAuto:
                    isAutomatic = false;
                    burstMode = false;
                    fmState = FireModeState.SemiAuto;
                    break;

                case FireModeState.SemiAuto:
                    isAutomatic = false;
                    burstMode = true;
                    fmState = FireModeState.Burst;
                    break;

                case FireModeState.Burst:
                    isAutomatic = true;
                    burstMode = false;
                    fmState = FireModeState.FullAuto;
                    break;
            }
        }
        if (isAutomatic)
        {
            shooting = Input.GetMouseButton(0);
        }
        else
        {
            shooting = Input.GetMouseButtonDown(0);
        }

        if (canShoot && shooting && !reloading && bulletsLeft > 0 && !burstMode)
        {
            Shoot();
        }

        if (canShoot && shooting && !reloading && bulletsLeft > 0 && burstMode && !bursting)
        {
            StartCoroutine(ShootBurst());
            bursting = true;
        }

        switch (rState)
        {
            case ReloadState.Ready:
                if (bulletsLeft < magSize && !reloading && (Input.GetKeyDown(KeyCode.R) || bulletsLeft == 0) && totalAmmo > 0)
                {
                    PlayClip(audioReload);

                    reloading = true;
                    
                    rState = ReloadState.ReloadStart;
                }
                break;
            case ReloadState.ReloadStart:
                reloadTime -= Time.deltaTime;

                if (reloadTime <= 0)
                {
                    reloadTime = 0;

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

        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit))
        {
            //Enemy TakeDamage()
            if (hit.collider.gameObject.TryGetComponent<EnemyHealth>(out EnemyHealth eComp))
            {
                eComp.TakeDamage(damage);
            }

            GameObject impactObj = Instantiate(impact, hit.point, Quaternion.LookRotation(hit.normal), spawnedImpacts);
            Destroy(impactObj, 2f);
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

        float tempTotalAmmo = totalAmmo;
        if (totalAmmo >= 30f)
        {
            totalAmmo -= magSize - bulletsLeft;
            bulletsLeft = magSize;
        }
        else
        {
            totalAmmo = 0f;
            bulletsLeft = tempTotalAmmo;
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
        newSource.spatialBlend = 0.5f;
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

    public enum FireModeState
    {
        FullAuto,
        SemiAuto,
        Burst
    }

    #endregion
}
