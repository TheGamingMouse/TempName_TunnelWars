using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.VisualScripting;
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
    readonly float crouchSpeed = 2.5f; 
    public float jumpHeight = 2f;
    readonly float sprintSpeed = 7.5f;
    public float sensX = 1.25f;
    public float sensY = 1.25f;
    float mouseX;
    float mouseY;
    float xRot;
    float yRot;

    [Header("Bools")]
    bool isOnGround;
    public bool crouched;
    bool moveBool;
    [SerializeField] bool playWalkingAudio;
    [SerializeField] bool playRunningAudio;
    bool scriptFound;
    [SerializeField] bool mainMenu = false; // SerializeField is Important!

    [Header("Lists")]
    readonly List<AudioSource> audioSourcePool = new();

    [Header("AudioClips")]
    AudioClip audioWalking;
    AudioClip audioRunning;

    [Header("GameObjects")]
    [SerializeField] GameObject rifle; // SerializeField is Important!
    [SerializeField] GameObject dummyRifle; // SerializeField is Important!

    [Header("Transforms")]
    Transform orientation;

    [Header("Vector3s")]
    Vector3 move;
    Vector3 moveDir;

    [Header("Components")]
    Rigidbody rb;
    Camera cam;
    PlayerMovementAudioStorage pmas;
    [SerializeField] AudioMixer audioMixer; // SerializeField is Important!
    [SerializeField] AudioMixerGroup sfxVolume; // SerializeField is Important!
    GroundedCheck groundCheck;

    #endregion

    #region Subscriptions

    void OnEnable()
    {
        PlayerHealth.OnPlayerDeath += HandlePlayerDeath;
        StartCamMovement.OnGameStart += HandleGameStart;
    }

    void OnDisable()
    {
        PlayerHealth.OnPlayerDeath -= HandlePlayerDeath;
        StartCamMovement.OnGameStart -= HandleGameStart;
    }
    
    #endregion

    #region StartUpdate

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        orientation = transform.Find("Orientation");
        cam = Camera.main;
        
        if (!mainMenu)
        {
            pmas = GameObject.FindGameObjectWithTag("Storage").transform.Find("AudioStorages/PlayerMovement").GetComponent<PlayerMovementAudioStorage>();

            audioWalking = pmas.audioWalking;
            audioRunning = pmas.audioRunning;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        moveSpeedCurr = moveSpeedBase;

        moveBool = false;
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
            StopCoroutine(nameof(WalkingAudio));
            StopCoroutine(nameof(RunningAudio));

            StopClip(audioWalking);
            StopClip(audioRunning);
        }

        if (moveBool)
        {
            cam.transform.rotation = Quaternion.Euler(xRot, yRot, 0f);
            transform.rotation = Quaternion.Euler(0f, yRot, 0f);
        }

        if (Input.GetKey(KeyCode.LeftShift) && isOnGround && moveBool && !Input.GetKey(KeyCode.LeftControl))
        {
            moveSpeedCurr = sprintSpeed;
        }
        else
        {
            moveSpeedCurr = moveSpeedBase;
        }

        if (Input.GetKeyDown(KeyCode.LeftControl) && moveBool && !Input.GetKey(KeyCode.LeftShift))
        {
            transform.localScale = new Vector3(1f, 0.5f, 1f);
            transform.position = new Vector3(transform.position.x, transform.position.y - 0.5f, transform.position.z);

            moveSpeedCurr = crouchSpeed;
            
            crouched = true;
        }
        else if (Input.GetKeyUp(KeyCode.LeftControl) && moveBool && !Input.GetKey(KeyCode.LeftShift))
        {
            transform.localScale = new Vector3(1f, 1f, 1f);
            transform.position = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);

            moveSpeedCurr = moveSpeedBase;
            
            crouched = false;
        }
    }

    #endregion

    #region Methods

    IEnumerator WalkingAudio()
    {
        PlayClip(audioWalking);
        
        yield return new WaitForSeconds(audioWalking.length);

        playWalkingAudio = false;
    }

    IEnumerator RunningAudio()
    {
        PlayClip(audioRunning);
        
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
    }

    IEnumerator EnableRifle()
    {
        yield return new WaitForSeconds(2f);

        dummyRifle.SetActive(false);
        rifle.SetActive(true);
        moveBool = true;
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
}
