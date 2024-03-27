using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class SaveSlotsMenu : MonoBehaviour
{
    [Header("Lists")]
    readonly List<AudioSource> audioSourcePool = new();
    
    SaveSlot[] saveSlots;

    [SerializeField] bool isLoadingGame = false;
    [SerializeField] int levelCount;

    [SerializeField] AudioClip audioWarning;
    [SerializeField] AudioClip audioClick;

    [SerializeField] GameObject confirmNewGame;

    [SerializeField] AudioMixer audioMixer; // SerializeField is Important!
    [SerializeField] AudioMixerGroup sfxVolumeMixer; // SerializeField is Important!

    void Awake()
    {
        saveSlots = GetComponentsInChildren<SaveSlot>();
    }

    public void OnSaveSlotClicked(SaveSlot saveSlot)
    {
        DataPersistenceManager.Instance.ChangeSelectedProfileId(saveSlot.GetProfileId());

        if (!isLoadingGame)
        {
            if (saveSlot.hasData)
            {
                confirmNewGame.SetActive(true);
                PlayClip(audioWarning);
                return;
            }
            OnConfirmNewGame();
            PlayClip(audioClick);
            return;
        }

        DataPersistenceManager.Instance.LoadGame();
        SceneManager.LoadSceneAsync(saveSlot.levelCount);
    }

    public void OnConfirmNewGame()
    {
        DataPersistenceManager.Instance.NewGame();
        SceneManager.LoadSceneAsync("Level1");
    }

    public void ActivateMenu(bool isLoadingGame)
    {
        this.isLoadingGame = isLoadingGame;

        Dictionary<string, GameData> profilesGameData = DataPersistenceManager.Instance.GetAllProfilesGameData();

        foreach (SaveSlot saveSlot in saveSlots)
        {
            profilesGameData.TryGetValue(saveSlot.GetProfileId(), out GameData profileData);
            saveSlot.SetData(profileData);
            if (profileData == null && isLoadingGame)
            {
                saveSlot.SetInteractable(false);
            }
            else
            {
                saveSlot.SetInteractable(true);
            }
        }
    }

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
        newSource.outputAudioMixerGroup = sfxVolumeMixer;
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
