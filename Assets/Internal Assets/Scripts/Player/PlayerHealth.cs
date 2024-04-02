using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour, IDataPersistence
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
    int deathCount;
    int cIndex;
    float recoilTimer;

    [Header("Bools")]
    bool damageCooldown;
    public bool godMode = false;
    [SerializeField] bool mainMenu = false; // SerializeField is Important!
    bool dead;
    bool colorsAdded;
    bool colorChanged;

    [Header("Lists")]
    readonly List<AudioSource> audioSourcePool = new();
    readonly List<Color> colors = new();

    [Header("GameObjects")]
    [SerializeField] GameObject damageOverlay; // SerializeField is Important!

    [Header("Vector3s")]
    public Vector3 dmgDirection;

    [Header("AudioClips")]
    AudioClip audioTakeDamage;

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

    [Header("Sprite")]
    [SerializeField] Sprite damageImage1; // SerializeField is Important!
    [SerializeField] Sprite damageImage2; // SerializeField is Important!
    [SerializeField] Sprite damageImage3; // SerializeField is Important!

    [Header("Components")]
    [SerializeField] AudioMixer audioMixer; // SerializeField is Important!
    [SerializeField] AudioMixerGroup sfxVolume; // SerializeField is Important!
    PlayerHealthAudioStorage phas;
    CinemachineVirtualCamera cVirtCam;

    #endregion

    #region StartUpdate

    // Start is called before the first frame update
    void Start()
    {
        if (!mainMenu)
        {
            phas = GameObject.FindGameObjectWithTag("Storage").transform.Find("AudioStorages/PlayerHealth").GetComponent<PlayerHealthAudioStorage>();

            audioTakeDamage = phas.audioTakeDamage;
        }

        health = maxHealth;
        
        cVirtCam = Camera.main.GetComponent<CinemachineVirtualCamera>();

        damageOverlay.GetComponent<Image>().overrideSprite = damageImage1;

        AddColorsToList();

        damageCooldown = false;
        dead = false;
    }

    // Update is called once per frame
    void Update()
    {
        health = Mathf.Clamp(health, 0, maxHealth);

        if (colorsAdded && !colorChanged)
        {
            UpdatePlayerColor();
            colorChanged = true;
        }
        
        if (!godMode)
        {
            if (health <= 0 && !dead)
            {
                OnPlayerDeath?.Invoke();
                deathCount++;
                
                dead = true;
            }
        }

        if (health > 60f)
        {
            damageOverlay.GetComponent<Image>().overrideSprite = damageImage1;
        }
        else if (health <= 60f && health > 30f)
        {
            damageOverlay.GetComponent<Image>().overrideSprite = damageImage2;
        }
        else
        {
            damageOverlay.GetComponent<Image>().overrideSprite = damageImage3;
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

            Recoil(1f, 0.1f);

            OnPlayerDamage?.Invoke();
        }
        StopCoroutine(DamageTaken());
        StartCoroutine(DamageTaken());
    }

    IEnumerator DamageTaken()
    {
        damageCooldown = true;

        yield return new WaitForSeconds(cooldown);

        damageCooldown = false;
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

    void UpdatePlayerColor()
    {
        GetComponent<MeshRenderer>().material.color = colors[cIndex];
    }

    void Recoil(float magnitude, float duration)
    {
        CinemachineBasicMultiChannelPerlin cmbmcp = cVirtCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        cmbmcp.m_AmplitudeGain = magnitude;
        recoilTimer = duration;
    }

    public void LoadData(GameData data)
    {
        if (!mainMenu)
        {
            deathCount = data.deathCount;
        }
        cIndex = data.playerColor;
    }

    public void SaveData(ref GameData data)
    {
        if (!mainMenu)
        {
            data.deathCount = deathCount;
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
