using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class PlayerHealth : MonoBehaviour
{
    #region Events

    public static event Action OnPlayerDeath;
    public static event Action OnPlayerDamage;

    #endregion
    
    #region Variables

    [Header("Floats")]
    readonly float cooldown = 2f;

    [Header("Ints")]
    readonly int maxHealth = 100;
    public int health;

    [Header("Bools")]
    bool damageCooldown;
    [SerializeField] bool godMode; // SerializeField is Important!

    [Header("Lists")]
    readonly List<AudioSource> audioSourcePool = new();

    [Header("Vector3s")]
    public Vector3 dmgDirection;

    [Header("AudioClips")]
    AudioClip audioTakeDamage;

    [Header("Components")]
    [SerializeField] AudioMixer audioMixer; // SerializeField is Important!
    [SerializeField] AudioMixerGroup sfxVolume; // SerializeField is Important!
    PlayerHealthAudioStorage phas;

    #endregion

    #region StartUpdate

    // Start is called before the first frame update
    void Start()
    {
        phas = GameObject.FindGameObjectWithTag("Storage").transform.Find("AudioStorages/PlayerHealth").GetComponent<PlayerHealthAudioStorage>();
        audioTakeDamage = phas.audioTakeDamage;

        health = maxHealth;
        damageCooldown = false;
    }

    // Update is called once per frame
    void Update()
    {
        health = Mathf.Clamp(health, 0, maxHealth);
        
        if (!godMode)
        {
            if (health <= 0)
            {
                OnPlayerDeath?.Invoke();
                // print("Player is dead");
            }
        }
    }

    #endregion

    #region Methods

    public void TakeDamage(int damage, Vector3 direction)
    {
        if (!damageCooldown)
        {
            health -= damage;
            dmgDirection = direction;
            PlayClip(audioTakeDamage);

            OnPlayerDamage?.Invoke();
        }
        StopCoroutine(nameof(DamageTaken));
        StartCoroutine(DamageTaken());
    }

    IEnumerator DamageTaken()
    {
        damageCooldown = true;

        yield return new WaitForSeconds(cooldown);

        damageCooldown = false;
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
}
