using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class UIManagers : MonoBehaviour, IDataPersistence
{
    #region Events

    public static event Action OnPause;
    public static event Action OnUnpause;

    #endregion

    #region Variables

    [Header("Enum States")]
    FireModeState fmState;

    [Header("Floats")]
    float magAmmo;
    float totalAmmo;
    float health;
    float playerReloadTime;
    float playerReloadCooldown;
    float masterVolume;
    float musicVolume;
    float sfxVolume;
    public float mouseSens;
    float uiScale;
    float masterSliderValue;
    float musicSliderValue;
    float sfxSliderValue;
    float sensSliderValue;
    float uiScaleSliderValue;
    float toSaveMasterSliderValue;
    float toSaveMusicSliderValue;
    float toSaveSFXSliderValue;
    float toSaveSensSliderValue;
    float toSaveUIScaleSliderValue;

    [Header("Bools")]
    bool reloading;
    bool gameStarted;
    bool scriptFound;
    public bool paused;
    bool slidersUpdated;

    [Header("Strings")]
    string fireMode;

    [Header("Arrays")]
    Resolution[] resolutions;

    [Header("GameObjects")]
    GameObject camObj;
    GameObject gameOverObj;
    [SerializeField] GameObject damageIndicatorPrefab; // SerializeField is Important!
    GameObject levelCompleteObj;
    GameObject pauseObj;
    GameObject optionsMenu;
    
    [Header("Transforms")]
    Transform canvas;
    Transform player;
    Transform spawnedDamageIndicators;
    
    [Header("TMP_Texts")]
    TMP_Text ammoText;
    TMP_Text fireModeText;
    TMP_Text healthText;
    TMP_Text masterVolumeText;
    TMP_Text musicVolumeText;
    TMP_Text sfxVolumeText;
    TMP_Text mouseSensText;
    TMP_Text uiScaleText;
    TMP_Text promtText;

    [Header("Dropdowns")]
    TMP_Dropdown resolutionDropdown;

    [Header("Sliders")]
    Slider masterVolumeSlider;
    Slider musicVolumeSlider;
    Slider SFXVolumeSlider;
    Slider mouseSensSlider;
    Slider uiScaleSlider;

    [Header("Images")]
    Image healthImg;
    Image reloadImg;

    [Header("Components")]
    Rifle rifleScript;
    PlayerHealth  playerHealthScript;
    [SerializeField] AudioMixer audioMixer; // SerializeField is Important!

    #endregion

    #region Subscriptions

    void OnEnable()
    {
        PlayerHealth.OnPlayerDeath += HandlePlayerDeath;
        PlayerHealth.OnPlayerDamage += HandleDamageTaken;
        PlayerMovement.OnLevelComplete += HandleLevelComplete;
        StartCamMovement.OnGameStart += HandleGameStart;
    }

    void OnDisable()
    {
        PlayerHealth.OnPlayerDeath -= HandlePlayerDeath;
        PlayerHealth.OnPlayerDamage -= HandleDamageTaken;
        PlayerMovement.OnLevelComplete -= HandleLevelComplete;
        StartCamMovement.OnGameStart -= HandleGameStart;
    }

    #endregion

    #region StartUpdate

    // Start is called before the first frame update
    void Start()
    {
        canvas = GameObject.FindGameObjectWithTag("UI").transform.Find("CanvasScale");
        camObj = Camera.main.gameObject;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        optionsMenu = canvas.Find("PauseMenu/OptionsMenu").gameObject;
        
        gameOverObj = canvas.Find("DeathScreenPanel").gameObject;
        levelCompleteObj = canvas.Find("LevelCompletePanel").gameObject;
        pauseObj = canvas.Find("PauseMenu").gameObject;
        spawnedDamageIndicators = canvas.Find("SpawnedDamageIndicators");
        
        playerHealthScript = player.GetComponentInChildren<PlayerHealth>();

        ammoText = canvas.Find("AmmoText (TMP)").GetComponent<TMP_Text>();
        fireModeText = canvas.Find("FireModeText (TMP)").GetComponent<TMP_Text>();
        promtText = canvas.Find("PromtText (TMP)").GetComponent<TMP_Text>();
        
        masterVolumeText = optionsMenu.transform.Find("VolumeObject/VolumeText (TMP)/MasterVolumeText (TMP)/MasterVolumeText (TMP)").GetComponent<TMP_Text>();
        musicVolumeText = optionsMenu.transform.Find("VolumeObject/VolumeText (TMP)/MusicVolumeText (TMP)/MusicVolumeText (TMP)").GetComponent<TMP_Text>();
        sfxVolumeText = optionsMenu.transform.Find("VolumeObject/VolumeText (TMP)/SFXVolumeText (TMP)/SFXVolumeText (TMP)").GetComponent<TMP_Text>();
        mouseSensText = optionsMenu.transform.Find("MouseSensObject/SensText (TMP)/SensText (TMP)").GetComponent<TMP_Text>();
        uiScaleText = optionsMenu.transform.Find("UISclaeObject/UIText (TMP)/UIText (TMP)").GetComponent<TMP_Text>();

        resolutionDropdown = optionsMenu.transform.Find("ResolutionObject/ResolutionDropdown").GetComponent<TMP_Dropdown>();

        masterVolumeSlider = optionsMenu.transform.Find("VolumeObject/VolumeSliders/MasterSlider").GetComponent<Slider>();
        musicVolumeSlider = optionsMenu.transform.Find("VolumeObject/VolumeSliders/MusicSlider").GetComponent<Slider>();
        SFXVolumeSlider = optionsMenu.transform.Find("VolumeObject/VolumeSliders/SFXSlider").GetComponent<Slider>();
        mouseSensSlider = optionsMenu.transform.Find("MouseSensObject/SensSlider").GetComponent<Slider>();
        uiScaleSlider = optionsMenu.transform.Find("UISclaeObject/UISlider").GetComponent<Slider>();

        healthImg = canvas.Find("Health/Health").GetComponent<Image>();
        healthText = canvas.Find("Health/HealthText (TMP)").GetComponent<TMP_Text>();
        reloadImg = canvas.Find("Reload").GetComponent<Image>();

        gameStarted = false;

        DisableElements();

        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        List<string> options = new();

        int currentResolution = 0;
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolution = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolution;
        resolutionDropdown.RefreshShownValue();

        SetMasterVolume(masterVolume);
        SetMusicVolume(musicVolume);
        SetSFXVolume(sfxVolume);

        SetSens(mouseSens * 20);
        SetUIScale(uiScale * 5);

        SetResolution(currentResolution);
    }

    // Update is called once per frame
    void Update()
    {
        if (gameStarted)
        {
            if (!scriptFound)
            {
                rifleScript = camObj.GetComponentInChildren<Rifle>();
                if (rifleScript != null)
                {
                    scriptFound = true;
                }
            }

            health = playerHealthScript.health;

            if (rifleScript != null)
            {
                magAmmo = rifleScript.bulletsLeft;
                totalAmmo = rifleScript.totalAmmo;

                fmState = (FireModeState)rifleScript.fmState;
                
                playerReloadTime = rifleScript.reloadTime;
                playerReloadCooldown = rifleScript.reloadCooldown;
                reloading = rifleScript.reloading;
            }
            
            UpdateAmmoText();
            UpdateFireModeText();
            UpdateHealthFill();
            UpdateReload();
            UpdatePromt();

            switch (fmState)
            {
                case FireModeState.FullAuto:
                    fireMode = "Full-Auto";
                    break;
                
                case FireModeState.SemiAuto:
                    fireMode = "Semi-Autio";
                    break;

                case FireModeState.Burst:
                    fireMode = "Burst Fire";
                    break;
            }

            if (Input.GetKeyDown(KeyCode.Escape) && !paused)
            {
                Pause();
            }
            else if (Input.GetKeyDown(KeyCode.Escape) && paused)
            {
                Unpause();
            }

            if (masterSliderValue > -100 && !slidersUpdated)
            {
                UpdateMasterVolumeSlider(masterSliderValue);
                UpdateMusicVolumeSlider(musicSliderValue);
                UpdateSFXVolumeSlider(sfxSliderValue);

                UpdateSensSlider(sensSliderValue);
                UpdateUIScaleSlider(uiScaleSliderValue);

                slidersUpdated = true;
            }
        }
    }

    #endregion

    #region GeneralMethods

    void DisableElements()
    {
        gameOverObj.SetActive(false);
        levelCompleteObj.SetActive(false);
        pauseObj.SetActive(false);
        promtText.gameObject.SetActive(false);

        canvas.gameObject.SetActive(false);
    }

    #endregion
    
    #region UpdateMethods

    void UpdateAmmoText()
    {
        ammoText.text = $"{magAmmo} / {totalAmmo}";
    }

    void UpdateFireModeText()
    {
        fireModeText.text = $"{fireMode}";
    }

    void UpdateHealthFill()
    {
        healthText.text = $"{health}";
        healthImg.fillAmount = health / 100;
    }

    void UpdateReload()
    {
        if (reloading)
        {
            reloadImg.fillAmount = playerReloadTime / playerReloadCooldown;
        }
        else
        {
            reloadImg.fillAmount = 0f;
        }
    }

    void UpdatePromt()
    {
        if (player.GetComponent<Interactor>().promtFound && !player.GetComponent<Interactor>().colliders[0].GetComponent<PlaceBomb>().bombPlanted)
        {
            promtText.gameObject.SetActive(true);
            promtText.text = $"{player.GetComponent<Interactor>().colliders[0].GetComponent<IInteractable>().promt}";
        }
        else
        {
            promtText.gameObject.SetActive(false);
        }
    }

    public void Pause()
    {
        OnPause?.Invoke();
        pauseObj.SetActive(true);

        paused = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 0f;
    }

    public void Unpause()
    {
        OnUnpause?.Invoke();
        pauseObj.SetActive(false);

        paused = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Time.timeScale = 1f;
    }

    public void LoadData(GameData data)
    {
        masterVolume = data.masterVolume;
        musicVolume = data.musicVolume;
        sfxVolume = data.sfxVolume;

        masterSliderValue = data.masterSliderValue;
        musicSliderValue = data.musicSliderValue;
        sfxSliderValue = data.sfxSliderValue;

        mouseSens = data.mouseSens;
        sensSliderValue = data.sensSliderValue;

        uiScale = data.uiScale;
        uiScaleSliderValue = data.uiScaleSliderValue;
    }

    public void SaveData(ref GameData data)
    {
        data.masterVolume = masterVolume;
        data.musicVolume = musicVolume;
        data.sfxVolume = sfxVolume;

        data.masterSliderValue = toSaveMasterSliderValue;
        data.musicSliderValue = toSaveMusicSliderValue;
        data.sfxSliderValue = toSaveSFXSliderValue;

        data.mouseSens = mouseSens;
        data.sensSliderValue = toSaveSensSliderValue;

        data.uiScale = uiScale;
        data.uiScaleSliderValue = toSaveUIScaleSliderValue;
    }

    #endregion

    #region OptionsMethods

    public void SetMasterVolume(float volume)
    {
        audioMixer.SetFloat("masterVolume", volume);

        float volumePercent = (volume + 30) * 2;
        masterVolumeText.text = $"{volumePercent}%";
        
        masterVolume = volume;
        toSaveMasterSliderValue = masterVolumeSlider.value;
    }

    public void SetMusicVolume(float volume)
    {
        audioMixer.SetFloat("musicVolume", volume);

        float volumePercent = (volume + 30) * 2;
        musicVolumeText.text = $"{volumePercent}%";
        
        musicVolume = volume;
        toSaveMusicSliderValue = musicVolumeSlider.value;
    }

    public void SetSFXVolume(float volume)
    {
        audioMixer.SetFloat("sfxVolume", volume);

        float volumePercent = (volume + 30) * 2;
        sfxVolumeText.text = $"{volumePercent}%";
        
        sfxVolume = volume;
        toSaveSFXSliderValue = SFXVolumeSlider.value;
    }

    public void SetSens(float sens)
    {
        mouseSensText.text = $"{sens}%";

        mouseSens = sens / 20;
        toSaveSensSliderValue = mouseSensSlider.value;
    }

    public void SetUIScale(float scale)
    {
        uiScaleText.text = $"{scale * 10}%";

        uiScale = scale / 5;
        toSaveUIScaleSliderValue = uiScaleSlider.value;
    }

    public void ApplyUIScale()
    {
        canvas.localScale = new Vector3(uiScale, uiScale, uiScale);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    void UpdateMasterVolumeSlider(float volume)
    {
        masterVolumeSlider.value = volume;
    }

    void UpdateMusicVolumeSlider(float volume)
    {
        musicVolumeSlider.value = volume;
    }

    void UpdateSFXVolumeSlider(float volume)
    {
        SFXVolumeSlider.value = volume;
    }

    void UpdateSensSlider(float sens)
    {
        mouseSensSlider.value = sens;
    }

    void UpdateUIScaleSlider(float scale)
    {
        uiScaleSlider.value = scale;
    }

    #endregion

    #region SubscriptionHandlers

    void HandlePlayerDeath()
    {
        gameOverObj.SetActive(true);
        Time.timeScale = 0f;
    }

    void HandleDamageTaken()
    {
        Instantiate(damageIndicatorPrefab, spawnedDamageIndicators);
    }

    void HandleLevelComplete()
    {
        levelCompleteObj.SetActive(true);
    }

    void HandleGameStart()
    {
        canvas.gameObject.SetActive(true);
        gameStarted = true;
    }

    #endregion

    #region Enums

    enum FireModeState
    {
        FullAuto,
        SemiAuto,
        Burst
    }

    #endregion
}
