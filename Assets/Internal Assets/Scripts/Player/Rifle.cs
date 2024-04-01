using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Cinemachine;

public class Rifle : MonoBehaviour, IDataPersistence
{
    #region Variables

    [Header("Enum States")]
    public ReloadState rState;
    public FireModeState fmState;

    [Header("Ints")]
    int cIndex;

    [Header("Floats")]
    readonly float aimSpeed = 20f;
    readonly float zoomSpeed = 140f;
    readonly float zoom = 50f;
    public readonly float damage = 10f;
    readonly float cooldown = 0.12f;
    public float reloadTime = 0f;
    public readonly float reloadCooldown = 2.1f;
    public float bulletsLeft;
    readonly float magSize = 30f;
    public float totalAmmo = 120f;
    float recoilTimer;
    readonly float holsterSpeed = 0.1f;

    [Header("Bools")]
    bool crouching;
    bool crouched;
    public bool aiming;
    bool isAutomatic = true;
    bool shooting;
    bool canShoot;
    public bool reloading;
    bool burstMode;
    bool bursting;
    bool actionBool;
    public bool soundMade;
    bool colorsAdded;
    bool colorChanged;

    [Header("Lists")]
    readonly List<AudioSource> audioSourcePool = new();
    List<Color> colors = new();

    [Header("GameObjects")]
    [SerializeField] GameObject impact; // SerializeField is Important!

    [Header("Transforms")]
    Transform spawnedPrefabs;
    Transform spawnedImpacts;

    [Header("Vector3s")]
    [SerializeField] Vector3 nPos, aPos, hPos;

    [Header("AudioClips")]
    AudioClip audioShoot;
    AudioClip audioReload;
    AudioClip audioHitMarker;

    [Header("Colors")]
    Color c1 = Color.black;
    Color c2 = Color.blue;
    Color c3 = Color.cyan;
    Color c4 = Color.gray;
    Color c5 = Color.green;
    Color c6 = Color.magenta;
    Color c7 = Color.red;
    Color c8 = Color.white;
    Color c9 = Color.yellow;

    [Header("Components")]
    Camera cam;
    CinemachineVirtualCamera cVirtCam;
    ParticleSystem muzzleFlash;
    RifleAudioStorage ras;
    [SerializeField] AudioMixer audioMixer; // SerializeField is Important!
    [SerializeField] AudioMixerGroup sfxVolume; // SerializeField is Important!

    #endregion

    #region Subscriptions

    void OnEnable()
    {
        PlayerHealth.OnPlayerDeath += HandlePlayerDeath;
        PlayerMovement.OnLevelComplete += HandleLevelComplete;
    }

    void OnDisable()
    {
        PlayerHealth.OnPlayerDeath -= HandlePlayerDeath;
        PlayerMovement.OnLevelComplete -= HandleLevelComplete;
    }

    #endregion

    #region StartUpdate

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
        cVirtCam = Camera.main.GetComponent<CinemachineVirtualCamera>();
        spawnedPrefabs = GameObject.FindGameObjectWithTag("Prefabs").transform;
        spawnedImpacts = spawnedPrefabs.Find("SpawnedImpacts").transform;
        
        muzzleFlash = GetComponentInChildren<ParticleSystem>();

        ras = GameObject.FindGameObjectWithTag("Storage").transform.Find("AudioStorages/Rifle").GetComponent<RifleAudioStorage>();
        audioShoot = ras.audioShoot;
        audioReload = ras.audioReload;
        audioHitMarker = ras.audioHitMarker;

        AddColorsToList();

        canShoot = true;
        soundMade = false;

        bulletsLeft = magSize;
        reloadTime = reloadCooldown;
    }

    // Update is called once per frame
    void Update()
    {
        crouching = FindObjectOfType<PlayerMovement>().crouched;
        actionBool = FindObjectOfType<PlayerMovement>().moveBool;

        if (colorsAdded && !colorChanged)
        {
            UpdateRifleColor();
            colorChanged = true;
        }

        if (recoilTimer > 0)
        {
            recoilTimer -= Time.deltaTime;

            if (recoilTimer <= 0f)
            {
                CinemachineBasicMultiChannelPerlin cmbmcp = cVirtCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

                cmbmcp.m_AmplitudeGain = 0f;
            }
        }

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

        if (Input.GetMouseButton(1) && actionBool)
        {
            Aim();
        }
        else
        {
            UnAim();
        }

        if (Input.GetKeyDown(KeyCode.F) && actionBool)
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

        if (isAutomatic && actionBool)
        {
            shooting = Input.GetMouseButton(0);
        }
        else if (!isAutomatic && actionBool)
        {
            shooting = Input.GetMouseButtonDown(0);
        }

        if (canShoot && shooting && !reloading && bulletsLeft > 0 && !burstMode && actionBool)
        {
            Shoot();
        }

        if (canShoot && shooting && !reloading && bulletsLeft > 0 && burstMode && !bursting && actionBool)
        {
            StartCoroutine(ShootBurst());
            bursting = true;
        }

        switch (rState)
        {
            case ReloadState.Ready:
                reloadTime = reloadCooldown;
                reloading = false;
                if (bulletsLeft < magSize && !reloading && (Input.GetKeyDown(KeyCode.R) || bulletsLeft == 0) && totalAmmo > 0 && actionBool)
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
                FinishReload();

                rState = ReloadState.Ready;
                break;
        }
    }

    #endregion

    #region Methods

    void Aim()
    {
        transform.localPosition = Vector3.Slerp(transform.localPosition, aPos, aimSpeed * Time.deltaTime);
        cVirtCam.m_Lens.FieldOfView -= zoomSpeed * Time.deltaTime;
        cVirtCam.m_Lens.FieldOfView = Mathf.Clamp(cVirtCam.m_Lens.FieldOfView, zoom, 90);
        
        aiming = true;
    }

    void UnAim()
    {
        transform.localPosition = Vector3.Slerp(transform.localPosition, nPos, aimSpeed * Time.deltaTime);
        cVirtCam.m_Lens.FieldOfView += zoomSpeed * Time.deltaTime;
        cVirtCam.m_Lens.FieldOfView = Mathf.Clamp(cVirtCam.m_Lens.FieldOfView, zoom, 90);
        
        aiming = false;
    }

    public void Holser()
    {
        transform.localPosition = Vector3.Slerp(transform.localPosition, hPos, holsterSpeed * Time.deltaTime);
    }

    public void UnHolster()
    {
        transform.localPosition = Vector3.Slerp(hPos, nPos, holsterSpeed * Time.deltaTime);
    }

    void Shoot()
    {
        muzzleFlash.Play();
        PlayClip(audioShoot);

        Recoil(2.5f, 0.1f);

        canShoot = false;

        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit))
        {
            //Enemy TakeDamage()
            if (hit.collider.gameObject.TryGetComponent<EnemyHealth>(out EnemyHealth eComp))
            {
                eComp.TakeDamage(damage);
                PlayClip(audioHitMarker);
            }

            //Turret TakeDamage()
            if (hit.collider.gameObject.TryGetComponent<TurretHealth>(out TurretHealth tComp))
            {
                tComp.TakeDamage(damage);
                PlayClip(audioHitMarker);
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

    void Recoil(float magnitude, float duration)
    {
        CinemachineBasicMultiChannelPerlin cmbmcp = cVirtCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        cmbmcp.m_AmplitudeGain = magnitude;
        recoilTimer = duration;
    }

    void AddColorsToList()
    {
        colors.Add(c1);
        colors.Add(c2);
        colors.Add(c3);
        colors.Add(c4);
        colors.Add(c5);
        colors.Add(c6);
        colors.Add(c7);
        colors.Add(c8);
        colors.Add(c9);

        colorsAdded = true;
    }

    void UpdateRifleColor()
    {
        transform.Find("tar21/Tar21").GetComponent<SkinnedMeshRenderer>().material.color = colors[cIndex];
        transform.Find("All_in_one_scopes/red_dot_d_prefab/red_dot_d").GetComponent<MeshRenderer>().material.color = colors[cIndex];
    }

    public void LoadData(GameData data)
    {
        cIndex = data.rifleColor;
    }

    public void SaveData(ref GameData data)
    {
        
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
        StopCoroutine(nameof(SoundMade));
        StartCoroutine(SoundMade());
    }

    IEnumerator SoundMade()
    {
        soundMade = true;

        yield return new WaitForSeconds(0.01f);

        soundMade = false;
    }

    #endregion

    #region Enums

    public enum ReloadState
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

    #region SubscriptionHandlers

    void HandlePlayerDeath()
    {
        actionBool = false;
    }

    void HandleLevelComplete()
    {
        actionBool = false;
    }

    #endregion

}
