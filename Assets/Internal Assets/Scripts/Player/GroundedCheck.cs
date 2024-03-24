using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class GroundedCheck : MonoBehaviour
{
    #region Variables
    
    [Header("Floats")]
    float jumpHeight;
    float airTime;
    [SerializeField] float recAirTime;
    
    [Header("Bools")]
    public bool isOnGround;
    public bool soundMade;
    [SerializeField] bool mainMenu = false; // SerializeField is Important!
    bool moveBool;

    [Header("Lists")]
    readonly List<AudioSource> audioSourcePool = new();

    [Header("AudioClips")]
    AudioClip audioJumpingLow;
    AudioClip audioJumpingHigh;

    [Header("Components")]
    Rigidbody rb;
    [SerializeField] AudioMixer audioMixer; // SerializeField is Important!
    PlayerMovementAudioStorage pmas;
    [SerializeField] AudioMixerGroup sfxVolume; // SerializeField is Important!

    #endregion

    #region StartUpdate

    void Start()
    {
        rb = GetComponentInParent<Rigidbody>();
        jumpHeight = GetComponentInParent<PlayerMovement>().jumpHeight;
        
        if (!mainMenu)
        {
            pmas = GameObject.FindGameObjectWithTag("Storage").transform.Find("AudioStorages/PlayerMovement").GetComponent<PlayerMovementAudioStorage>();
            
            audioJumpingLow = pmas.audioJumpingLow;
            audioJumpingHigh = pmas.audioJumpingHigh;

            moveBool = GameObject.FindWithTag("Player").GetComponent<PlayerMovement>().moveBool;
        }

        soundMade = false;
    }
    
    void Update()
    {
        if (Input.GetKey(KeyCode.Space) && isOnGround && moveBool)
        {
            rb.AddForce(Vector3.up * jumpHeight, ForceMode.Impulse);
            isOnGround = false;
        }

        if (!isOnGround)
        {
            airTime += Time.deltaTime;
            recAirTime = airTime;
        }
    }

    #endregion

    #region Methods

    void OnTriggerStay(Collider coll)
    {
        if (coll.transform.CompareTag("Obstruction") || coll.transform.CompareTag("Cover"))
        {
            isOnGround = true;
        }
    }
    
    void OnTriggerEnter(Collider coll)
    {
        if (coll.transform.CompareTag("Obstruction") || coll.transform.CompareTag("Cover"))
        {
            airTime = 0;

            if (recAirTime > 1)
            {
                PlayClip(audioJumpingHigh);
                // print("High Jump!");
            }
            else
            {
                PlayClip(audioJumpingLow);
                // print("Low Jump!");
            }
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
}
