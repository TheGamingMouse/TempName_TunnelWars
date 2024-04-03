using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;

public class PlayerMovement : MonoBehaviour
{
    #region Events

    public static event Action OnLevelComplete;
    
    #endregion

    #region Variables

    [Header("Floats")]
    readonly float moveSpeedBase = 5f;
    [SerializeField] float moveSpeedCurr;
    readonly float crouchSpeed = 4f; 
    public float jumpHeight = 2f;
    readonly float sprintSpeed = 7.5f;
    public float sensX;
    public float sensY;
    float mouseX;
    float mouseY;
    float xRot;
    float yRot;
    readonly float swapSpeed = 0.2f;

    [Header("Bools")]
    bool isOnGround;
    public bool crouched;
    public bool moveBool;
    bool playWalkingAudio;
    bool playRunningAudio;
    bool scriptFound;
    [SerializeField] bool mainMenu = false; // SerializeField is Important!
    bool aimingRifle;
    bool aimingPistol;
    bool speedAltered;
    bool speedAlteredSprint;
    bool speedAlteredCrouched;
    public bool rifleActive;
    bool canSwap;

    [Header("Lists")]
    readonly List<AudioSource> audioSourcePool = new();

    [Header("AudioClips")]
    AudioClip audioWalking;
    AudioClip audioRunning;
    AudioClip audioPickup;

    [Header("GameObjects")]
    [SerializeField] GameObject rifle; // SerializeField is Important!
    [SerializeField] GameObject pistol; // SerializeField is Important!
    [SerializeField] GameObject dummyRifle; // SerializeField is Important!
    [SerializeField] GameObject dummyPistol; // SerializeField is Important!

    [Header("Transforms")]
    Transform orientation;

    [Header("Vector3s")]
    Vector3 move;
    Vector3 moveDir;
    Vector3 lastOnGround;

    [Header("Components")]
    Rigidbody rb;
    Camera cam;
    PlayerMovementAudioStorage pmas;
    ConsumableAudioStorage cas;
    [SerializeField] AudioMixer audioMixer; // SerializeField is Important!
    [SerializeField] AudioMixerGroup sfxVolume; // SerializeField is Important!
    GroundedCheck groundCheck;

    #endregion

    #region Subscriptions

    void OnEnable()
    {
        PlayerHealth.OnPlayerDeath += HandlePlayerDeath;
        StartCamMovement.OnGameStart += HandleGameStart;
        UIManagers.OnPause += HandlePause;
        UIManagers.OnUnpause += HandleUnpause;
    }

    void OnDisable()
    {
        PlayerHealth.OnPlayerDeath -= HandlePlayerDeath;
        StartCamMovement.OnGameStart -= HandleGameStart;
        UIManagers.OnPause -= HandlePause;
        UIManagers.OnUnpause -= HandleUnpause;
    }
    
    #endregion

    #region StartUpdate

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        orientation = transform.Find("Orientation");
        cam = Camera.main;

        if (rifle != null)
        {
            rifle.SetActive(false);
        }
        if (pistol != null)
        {
            pistol.SetActive(false);
        }
        
        if (!mainMenu)
        {
            pmas = GameObject.FindGameObjectWithTag("Storage").transform.Find("AudioStorages/PlayerMovement").GetComponent<PlayerMovementAudioStorage>();

            audioWalking = pmas.audioWalking;
            audioRunning = pmas.audioRunning;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            cas = GameObject.FindGameObjectWithTag("Storage").transform.Find("AudioStorages/Consumable").GetComponent<ConsumableAudioStorage>();
            audioPickup = cas.audioAmmoPickup;
        }

        moveSpeedCurr = moveSpeedBase;

        moveBool = false;
        canSwap = true;
    }

    void FixedUpdate()
    {
        if (moveBool)
        {
            rb.MovePosition(rb.position + moveSpeedCurr * Time.fixedDeltaTime * moveDir);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!mainMenu)
        {
            sensX = GameObject.FindWithTag("Managers").transform.Find("UIManager").GetComponent<UIManagers>().mouseSens;
            sensY = GameObject.FindWithTag("Managers").transform.Find("UIManager").GetComponent<UIManagers>().mouseSens;

            if ((move.x != 0 || move.z != 0) && moveSpeedCurr != sprintSpeed && !playWalkingAudio && !playRunningAudio)
            {
                StartCoroutine(WalkingAudio());
                playWalkingAudio = true;
            }
            else if ((move.x != 0 || move.z != 0) && moveSpeedCurr == sprintSpeed && !playRunningAudio && !playWalkingAudio)
            {
                StartCoroutine(RunningAudio());
                playRunningAudio = true;
            }
            else if (((move.x == 0 && move.z == 0) || Input.GetKeyDown(KeyCode.LeftShift)) && (playWalkingAudio || playRunningAudio))
            {
                StopCoroutine(WalkingAudio());
                StopCoroutine(RunningAudio());

                StopClip(audioWalking, true);
                StopClip(audioRunning, true);
            }

            if (!scriptFound)
            {
                groundCheck = GetComponentInChildren<GroundedCheck>();
                if (groundCheck != null)
                {
                    scriptFound = true;
                }
            }
            if (groundCheck != null)
            {
                isOnGround = GetComponentInChildren<GroundedCheck>().isOnGround;
            }

            move.x = Input.GetAxisRaw("Horizontal");
            move.z = Input.GetAxisRaw("Vertical");

            moveDir = orientation.forward * move.z + orientation.right * move.x;

            mouseX = Input.GetAxisRaw("Mouse X") * sensX;
            mouseY = Input.GetAxisRaw("Mouse Y") * sensY;

            yRot += mouseX;
            xRot -= mouseY;
            xRot = Mathf.Clamp(xRot, -80f, 70f);

            if (moveBool)
            {
                cam.transform.rotation = Quaternion.Euler(xRot, yRot, 0f);
                transform.rotation = Quaternion.Euler(0f, yRot, 0f);
            }

            if (Input.GetKey(KeyCode.LeftShift) && isOnGround && moveBool && !Input.GetKey(KeyCode.LeftControl) && !aimingRifle && !aimingPistol)
            {
                moveSpeedCurr = sprintSpeed;
                speedAlteredSprint = true;
            }
            else if (speedAlteredSprint)
            {
                moveSpeedCurr = moveSpeedBase;
                speedAlteredSprint = false;
                speedAltered = false;
            }

            if (Input.GetKeyDown(KeyCode.LeftControl) && moveBool && !Input.GetKey(KeyCode.LeftShift))
            {
                transform.localScale = new Vector3(1f, 0.5f, 1f);
                transform.position = new Vector3(transform.position.x, transform.position.y - 0.5f, transform.position.z);
                
                crouched = true;
                speedAltered = false;
            }
            else if (Input.GetKeyUp(KeyCode.LeftControl) && moveBool && !Input.GetKey(KeyCode.LeftShift))
            {
                transform.localScale = new Vector3(1f, 1f, 1f);
                transform.position = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);
                
                crouched = false;
                speedAltered = false;
            }

            aimingRifle = rifle.GetComponent<Rifle>().aiming;
            aimingPistol = pistol.GetComponent<Pistol>().aiming;

            if (crouched && !speedAlteredCrouched)
            {
                moveSpeedCurr = crouchSpeed;
                speedAlteredCrouched = true;
            }
            else if (!crouched && speedAlteredCrouched)
            {
                moveSpeedCurr = moveSpeedBase;
                speedAlteredCrouched = false;
            }
            
            if ((aimingRifle || aimingPistol) && !speedAltered)
            {
                moveSpeedCurr *= 0.5f;
                speedAltered = true;
            }
            else if (!aimingRifle && !aimingPistol && speedAltered)
            {
                moveSpeedCurr = moveSpeedBase;
                speedAltered = false;
                
            }

            if ((Input.GetAxisRaw("Mouse ScrollWheel") > 0 || Input.GetAxisRaw("Mouse ScrollWheel") < 0) && moveBool && canSwap)
            {
                rifle.SetActive(!rifle.activeInHierarchy);
                pistol.SetActive(!pistol.activeInHierarchy);

                rifle.GetComponent<Rifle>().rState = Rifle.ReloadState.Ready;
                pistol.GetComponent<Pistol>().rState = Pistol.ReloadState.Ready;

                rifle.GetComponent<Rifle>().UnHolster();
                pistol.GetComponent<Pistol>().UnHolster();

                rifle.GetComponent<Rifle>().aiming = false;
                pistol.GetComponent<Pistol>().aiming = false;

                rifleActive = !rifleActive;

                StartCoroutine(StartSwapTimer());
            }
            if (Input.GetKeyDown(KeyCode.Alpha1) && moveBool && canSwap)
            {
                rifle.SetActive(true);
                pistol.SetActive(false);

                if (!rifleActive)
                {
                    rifle.GetComponent<Rifle>().rState = Rifle.ReloadState.Ready;
                    rifle.GetComponent<Rifle>().UnHolster();
                    rifle.GetComponent<Rifle>().aiming = false;

                    rifleActive = true;

                    StartCoroutine(StartSwapTimer());
                }
            }
            if (Input.GetKeyDown(KeyCode.Alpha2) && moveBool && canSwap)
            {
                rifle.SetActive(false);
                pistol.SetActive(true);

                if (rifleActive)
                {
                    pistol.GetComponent<Pistol>().rState = Pistol.ReloadState.Ready;
                    pistol.GetComponent<Pistol>().UnHolster();
                    pistol.GetComponent<Pistol>().aiming = false;

                    rifleActive = false;

                    StartCoroutine(StartSwapTimer());
                }
            }

            if (!PlayerInBounds())
            {
                transform.position = lastOnGround;
            }
        }
    }

    #endregion

    #region Methods

    IEnumerator WalkingAudio()
    {
        PlayClip(audioWalking, true);
        
        yield return new WaitForSeconds(audioWalking.length);

        playWalkingAudio = false;
    }

    IEnumerator RunningAudio()
    {
        PlayClip(audioRunning, true);
        
        yield return new WaitForSeconds(audioRunning.length);

        playRunningAudio = false;
    }

    void OnTriggerEnter(Collider coll)
    {
        if (coll.transform.CompareTag("NextLevel"))
        {
            OnLevelComplete?.Invoke();

            moveBool = false;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            rb.useGravity = false;
            rb.velocity = Vector3.zero;
        }

        if (coll.transform.CompareTag("ConsumablePickup"))
        {
            if (coll.transform.TryGetComponent<AmmoDrop>(out AmmoDrop ammoComp))
            {
                rifle.GetComponent<Rifle>().totalAmmo += ammoComp.ammount;
            }
            else if (coll.transform.TryGetComponent<SecretDrop>(out SecretDrop secretComp))
            {
                rifle.GetComponent<Rifle>().totalAmmo += secretComp.ammountRifleAmmo;
                pistol.GetComponent<Pistol>().totalAmmo += secretComp.ammountPistolAmmo;
                transform.GetComponent<PlayerHealth>().health += secretComp.ammountHealth;
            }

            PlayClip(audioPickup, false);
            Destroy(coll.gameObject);
        }
    }

    void OnCollisionStay(Collision coll)
    {
        if (coll.transform.CompareTag("Ground"))
        {
            lastOnGround = new Vector3(transform.position.x, 1f, transform.position.z);
        }
    }

    IEnumerator EnableRifle()
    {
        yield return new WaitForSeconds(2f);

        dummyRifle.SetActive(false);
        dummyPistol.SetActive(false);
        rifle.SetActive(true);
        moveBool = true;
        rifleActive = true;
    }

    IEnumerator StartSwapTimer()
    {
        canSwap = false;

        yield return new WaitForSeconds(swapSpeed);

        canSwap = true;
    }

    bool PlayerInBounds()
    {
        if (transform.position.y < -30f)
        {
            return false;
        }
        return true;
    }
    
    #endregion

    #region SubscriptionHandlers

    void HandlePlayerDeath()
    {
        moveBool = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void HandleGameStart()
    {
        StartCoroutine(EnableRifle());
    }

    void HandlePause()
    {
        moveBool = false;
    }

    void HandleUnpause()
    {
        moveBool = true;
    }
    
    #endregion

    #region AudioMethods

    AudioSource AddNewSourceToPool(bool isPmas)
    {
        if (isPmas)
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
        else
        {
            audioMixer.GetFloat("sfxVolume", out float dBSFX);
            float SFXVolume = Mathf.Pow(10.0f, dBSFX / 20.0f);

            audioMixer.GetFloat("masterVolume", out float dBMaster);
            float masterVolume = Mathf.Pow(10.0f, dBMaster / 20.0f);
            
            float realVolume = (SFXVolume + masterVolume) / 2 * 0.3f;
            
            AudioSource newSource = gameObject.AddComponent<AudioSource>();
            newSource.playOnAwake = false;
            newSource.volume = realVolume;
            newSource.spatialBlend = 0f;
            newSource.outputAudioMixerGroup = sfxVolume;
            audioSourcePool.Add(newSource);
            return newSource;
        }
    }

    AudioSource GetAvailablePoolSource(bool isPmas)
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
        return AddNewSourceToPool(isPmas);
    }

    AudioSource GetUnavailablePoolSource(bool isPmas)
    {
        //Fetch the first source in the pool that is not currently playing anything
        foreach (var source in audioSourcePool)
        {
            if (source.isPlaying)
            {
                if (isPmas && source.spatialBlend > 0f)
                {
                    return source;
                }
                else if (!isPmas && source.spatialBlend == 0f)
                {
                    return source;
                }
                
            }
        }
        return null;
    }

    void PlayClip(AudioClip clip, bool isPmas)
    {
        AudioSource source = GetAvailablePoolSource(isPmas);
        source.clip = clip;
        source.Play();
    }

    void StopClip(AudioClip clip, bool isPmas)
    {
        AudioSource source = GetUnavailablePoolSource(isPmas);
        if (source == null)
        {
            return;
        }
        source.clip = clip;
        source.Stop();
    }

    #endregion
}
