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
    float moveSpeedCurr;
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
    readonly float shoulderZRot = -22.5f;
    readonly float shoulderXRot = 5;

    [Header("Bools")]
    bool isOnGround;
    public bool crouched;
    public bool moveBool;
    bool playWalking;
    bool playRunning;
    bool scriptFound;
    [SerializeField] bool mainMenu = false; // SerializeField is Important!
    bool aimingRifle;
    bool aimingPistol;
    bool speedAltered;
    bool speedAlteredSprint;
    bool speedAlteredCrouched;
    public bool rifleActive;
    bool canSwap;
    bool sprinting;
    bool aiming;

    [Header("Lists")]
    readonly List<AudioSource> audioSourcePool = new();

    [Header("AudioClips")]
    AudioClip audioWalking;
    AudioClip audioRunning;
    AudioClip audioPickup;

    [Header("GameObjects")]
    public GameObject rifle; // SerializeField is Important!
    public GameObject pistol; // SerializeField is Important!
    [SerializeField] GameObject dummyRifle; // SerializeField is Important!
    [SerializeField] GameObject dummyPistol; // SerializeField is Important!

    [Header("Transforms")]
    Transform orientation;
    [SerializeField] Transform spine; // SerializeField is Important!
    [SerializeField] Transform rightShoulder; // SerializeField is Important!
    [SerializeField] Transform leftShoulder; // SerializeField is Important!

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
    Animator animator;

    #endregion

    #region Subscriptions

    void OnEnable()
    {
        PlayerHealth.OnPlayerDeath += HandlePlayerDeath;
        StartCamMovement.OnGameStart += HandleGameStart;
        UIManagers.OnPause += HandlePause;
        UIManagers.OnUnpause += HandleUnpause;
        Rifle.OnShooting += HandleShooting;
    }

    void OnDisable()
    {
        PlayerHealth.OnPlayerDeath -= HandlePlayerDeath;
        StartCamMovement.OnGameStart -= HandleGameStart;
        UIManagers.OnPause -= HandlePause;
        UIManagers.OnUnpause -= HandleUnpause;
        Rifle.OnShooting -= HandleShooting;
    }
    
    #endregion

    #region StartUpdate

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        orientation = transform.Find("Orientation");
        cam = Camera.main;
        animator = GetComponent<Animator>();

        if (dummyRifle != null)
        {
            dummyRifle.SetActive(false);
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

            if ((move.x != 0 || move.z != 0) && moveSpeedCurr != sprintSpeed && !playWalking && !playRunning)
            {
                StartCoroutine(WalkingAudio());
                playWalking = true;
            }
            else if ((move.x != 0 || move.z != 0) && moveSpeedCurr == sprintSpeed && !playRunning && !playWalking)
            {
                StartCoroutine(RunningAudio());
                playRunning = true;
            }
            else if (((move.x == 0 && move.z == 0) || Input.GetKeyDown(KeyCode.LeftShift)) && (playWalking || playRunning))
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
            if (!crouched)
            {
                xRot = Mathf.Clamp(xRot, -50f, 65f);
            }
            else if (crouched)
            {
                xRot = Mathf.Clamp(xRot, -35f, 65f);
            }

            if (moveBool)
            {
                cam.transform.rotation = Quaternion.Euler(xRot, yRot, 0f);
                transform.rotation = Quaternion.Euler(0f, yRot, 0f);
            }

            if (Input.GetKey(KeyCode.LeftShift) && isOnGround && moveBool && !Input.GetKey(KeyCode.LeftControl) && !aimingRifle && !aimingPistol && (move.x != 0 || move.z != 0))
            {
                moveSpeedCurr = sprintSpeed;
                speedAlteredSprint = true;
                sprinting = true;
            }
            else if (speedAlteredSprint)
            {
                moveSpeedCurr = moveSpeedBase;
                speedAlteredSprint = false;
                speedAltered = false;
                sprinting = false;
            }

            if (Input.GetKeyDown(KeyCode.LeftControl) && moveBool && !Input.GetKey(KeyCode.LeftShift))
            {
                crouched = true;
                speedAltered = false;
            }
            else if (Input.GetKeyUp(KeyCode.LeftControl) && moveBool && !Input.GetKey(KeyCode.LeftShift))
            {
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
                    pistol.GetComponent<Pistol>().aiming = false;

                    rifleActive = false;

                    StartCoroutine(StartSwapTimer());
                }
            }

            if (!PlayerInBounds())
            {
                transform.position = lastOnGround;
            }

            // Animator booleans
            if (moveBool)
            {
                animator.SetBool("isWalkingForward", Input.GetKey(KeyCode.W));
                animator.SetBool("isWalkingBackwards", Input.GetKey(KeyCode.S));
                animator.SetBool("isWalkingLeft", Input.GetKey(KeyCode.A));
                animator.SetBool("isWalkingRight", Input.GetKey(KeyCode.D));

                animator.SetBool("isCrouching", crouched);
                animator.SetBool("isSprinting", sprinting);

                if (aimingRifle || aimingPistol)
                {
                    aiming = true;
                }
                else
                {
                    aiming = false;
                }
                animator.SetBool("isAiming", aiming);
            }
        }
    }

    void LateUpdate()
    {
        if (moveBool && aiming)
            {
                cam.transform.rotation = Quaternion.Euler(xRot, yRot, 0f);
                spine.rotation = Quaternion.Euler(xRot, yRot, 0f);
                leftShoulder.rotation *= Quaternion.Euler(shoulderXRot, 1f, shoulderZRot);
                rightShoulder.rotation *= Quaternion.Euler(shoulderXRot, 1f, shoulderZRot);
            }
    }

    #endregion

    #region Methods

    IEnumerator WalkingAudio()
    {
        PlayClip(audioWalking, true);
        
        yield return new WaitForSeconds(audioWalking.length);

        playWalking = false;
    }

    IEnumerator RunningAudio()
    {
        PlayClip(audioRunning, true);
        
        yield return new WaitForSeconds(audioRunning.length);

        playRunning = false;
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
        dummyPistol.SetActive(true);
        rifle.SetActive(true);
        pistol.SetActive(false);
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

    IEnumerator Recoil()
    {
        leftShoulder.position += new Vector3(0f, -0.15f, 0f);
        rightShoulder.position += new Vector3(0f, -0.15f, 0f);

        yield return new WaitForSeconds(0.1f);

        leftShoulder.position += new Vector3(0f, 0.15f, 0f);
        rightShoulder.position += new Vector3(0f, 0.15f, 0f);
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

    void HandleShooting()
    {
        StartCoroutine(Recoil());
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
