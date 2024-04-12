using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class PlaceBomb : MonoBehaviour, IInteractable
{
    #region Variables

    [Header("Bools")]
    public bool bombPlanted;
    public bool exploded;
    bool playingAudio;

    [Header("Strings")]
    readonly string promtString = "Plant Bomb (Press E)";

    [Header("Lists")]
    readonly List<AudioSource> audioSourcePool = new();

    [Header("GameObjects")]
    [SerializeField] GameObject bomb;

    [Header("AudioClips")]
    AudioClip audioBombTimer;

    [Header("Components")]
    PlaceBombAudioStorage pcas;
    [SerializeField] AudioMixer audioMixer; // SerializeField is Important!
    [SerializeField] AudioMixerGroup sfxVolume; // SerializeField is Important!

    #endregion

    #region StartUpdate

    // Start is called before the first frame update
    void Start()
    {
        pcas = GameObject.FindGameObjectWithTag("Storage").transform.Find("AudioStorages/PlaceBomb").GetComponent<PlaceBombAudioStorage>();
        audioBombTimer = pcas.audioBombTimer;

        bombPlanted = false;
        exploded = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (bombPlanted && !playingAudio)
        {
            StartCoroutine(BombTimerAudio());
            playingAudio = true;
        }
        
        if (exploded)
        {
            StopCoroutine(BombTimerAudio());
        }
    }

    #endregion

    #region Methods

    public string promt => promtString;

    public bool Interact(Interactor interactor)
    {
        bomb.SetActive(true);
        StartCoroutine(BombTimer());
        bombPlanted = true;
        return true;
    }

    IEnumerator BombTimer()
    {
        yield return new WaitForSeconds(5f);

        exploded = true;
    }

    IEnumerator BombTimerAudio()
    {
        PlayClip(audioBombTimer);
        yield return new WaitForSeconds(audioBombTimer.length + 0.2f);

        playingAudio = false;
    }

    #endregion

    #region AudioMethods

    AudioSource AddNewSourceToPool()
    {
        audioMixer.GetFloat("sfxVolume", out float dBSFX);
        float SFXVolume = Mathf.Pow(10.0f, dBSFX / 20.0f);

        audioMixer.GetFloat("masterVolume", out float dBMaster);
        float masterVolume = Mathf.Pow(10.0f, dBMaster / 20.0f);
        
        float realVolume = (SFXVolume + masterVolume) / 2 * 2f;
        
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
    }

    #endregion
}
