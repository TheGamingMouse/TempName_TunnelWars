using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

public class Radiostation : MonoBehaviour
{
    #region Variables

    [Header("Ints")]
    public int id;

    [Header("Floats")]
    float explosionRadiusLethal = 5f;
    float explosionRadiusSemiLethal = 10f;
    float explosionRadiusNearNonLethal = 15f;

    [Header("Bools")]
    public bool destroyed;

    [Header("Lists")]
    readonly List<Color> colors = new();
    readonly List<AudioSource> audioSourcePool = new();

    [Header("GameObjects")]
    public GameObject indicator;
    [SerializeField] GameObject bomb; //SerializeField is Important!
    [SerializeField] GameObject intake; //SerializeField is Important!
    GameObject player;

    [Header("AudioClips")]
    AudioClip audioExplosion;

    [Header("Colors")]
    Color c0 = Color.blue;
    Color c1 = Color.cyan;
    Color c2 = Color.green;
    Color c3 = Color.magenta;
    Color c4 = Color.red;
    Color c5 = Color.yellow;

    [Header("Components")]
    [SerializeField] PlaceBomb placeBomb;
    [SerializeField] ParticleSystem explosion;
    [SerializeField] AudioMixer audioMixer; // SerializeField is Important!
    [SerializeField] AudioMixerGroup sfxVolume; // SerializeField is Important!
    RadiostationAudioStorage ras;

    #endregion

    #region StartUpdate

    // Start is called before the first frame update
    void Start()
    {
        AddColors();

        player = GameObject.FindWithTag("Player");
        ras = GameObject.FindGameObjectWithTag("Storage").transform.Find("AudioStorages/Radiostation").GetComponent<RadiostationAudioStorage>();

        audioExplosion = ras.audioExplosion;
    }

    // Update is called once per frame
    void Update()
    {
        if (!destroyed)
        {
            indicator.GetComponent<MeshRenderer>().material.color = colors[id];
        }

        if (placeBomb.exploded && !destroyed)
        {
            explosion.Play();
            // TODO - Add explosion sound.
            
            intake.SetActive(false);
            bomb.SetActive(false);
            indicator.SetActive(false);

            float distance = Vector3.Distance(player.transform.position, transform.position);
            if (distance <= explosionRadiusLethal)
            {
                player.GetComponent<PlayerHealth>().TakeDamage(100, bomb.transform.position);
            }
            else if (distance <= explosionRadiusSemiLethal)
            {
                player.GetComponent<PlayerHealth>().TakeDamage(40, bomb.transform.position);
            }
            else if (distance <= explosionRadiusNearNonLethal)
            {
                player.GetComponent<PlayerHealth>().TakeDamage(10, bomb.transform.position);
            }

            destroyed = true;
        }
    }
    
    #endregion

    #region Methods

    void AddColors()
    {
        colors.Add(c0);
        colors.Add(c1);
        colors.Add(c2);
        colors.Add(c3);
        colors.Add(c4);
        colors.Add(c5);
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

    #region Gizmos

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(intake.transform.position, explosionRadiusLethal);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(intake.transform.position, explosionRadiusSemiLethal);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(intake.transform.position, explosionRadiusNearNonLethal);
    }

    #endregion
}
