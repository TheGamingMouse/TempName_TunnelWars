using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour, IDataPersistence
{
    #region Variables

    [Header("Ints")]
    int cIndex1;
    int cIndex2;
    int cIndex3;
    int levelCount;

    [Header("Floats")]
    float masterVolume;
    float musicVolume;
    float sfxVolume;
    float mouseSens;
    float uiScale = 5f;
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
    bool watchCursor;
    bool slidersUpdated;

    [Header("Lists")]
    List<Color> colors1 = new();
    List<Color> colors2 = new();
    List<Color> colors3 = new();
    readonly List<AudioSource> audioSourcePool = new();

    [Header("Arrays")]
    Resolution[] resolutions;
    
    [Header("GameObjects")]
    [SerializeField] GameObject mainCam; // SerializeField is Important!
    [SerializeField] GameObject customCam; // SerializeField is Important!
    [SerializeField] GameObject playerLeftArrow; // SerializeField is Important!
    [SerializeField] GameObject playerRightArrow; // SerializeField is Important!
    [SerializeField] GameObject rifleLeftArrow; // SerializeField is Important!
    [SerializeField] GameObject rifleRightArrow; // SerializeField is Important!
    [SerializeField] GameObject pistolLeftArrow; // SerializeField is Important!
    [SerializeField] GameObject pistolRightArrow; // SerializeField is Important!
    [SerializeField] GameObject playerModel; // SerializeField is Important!
    [SerializeField] GameObject rifleModel; // SerializeField is Important!
    [SerializeField] GameObject rifleModelPivot; // SerializeField is Important!
    GameObject playerObject;

    [Header("Buttons")]
    [SerializeField] Button continueButton; // SerializeField is Important!
    [SerializeField] Button loadButton; // SerializeField is Important!

    [Header("Texts")]
    [SerializeField] TMP_Text masterVolumeText; // SerializeField is Important!
    [SerializeField] TMP_Text musicVolumeText; // SerializeField is Important!
    [SerializeField] TMP_Text sfxVolumeText; // SerializeField is Important!
    [SerializeField] TMP_Text mouseSensText; // SerializeField is Important!
    [SerializeField] TMP_Text uiScaleText; // SerializeField is Important!

    [Header("Dropdowns")]
    [SerializeField] TMP_Dropdown resolutionDropdown; // SerializeField is Important!

    [Header("Sliders")]
    [SerializeField] Slider masterVolumeSlider; // SerializeField is Important!
    [SerializeField] Slider musicVolumeSlider; // SerializeField is Important!
    [SerializeField] Slider sfxVolumeSlider; // SerializeField is Important!
    [SerializeField] Slider mouseSensSlider; // SerializeField is Important!
    [SerializeField] Slider uiScaleSlider;  // SerializeField is Important!

    [Header("Vector3s")]
    Vector3 mousePos;
    Vector3 pressPoint;

    [Header("Quaternions")]
    Quaternion startRotation;
    Quaternion initialRotation;

    [Header("Images")]
    Image bodyColor;
    Image rifleColor;
    Image pistolColor;

    [Header("AudioClips")]
    AudioClip audioUIClick;
    AudioClip audioUIHover;
    AudioClip audioWarning;
    AudioClip audioSliderChange;
    AudioClip audioButtonSelect;

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

    [Header("Components")]
    [SerializeField] SaveSlotsMenu saveSlotsMenu; // SerializeField is Important!
    [SerializeField] AudioMixer audioMixer; // SerializeField is Important!
    [SerializeField] AudioMixerGroup sfxVolumeMixer; // SerializeField is Important!
    MenuManagerAudioStorage phmm;
    
    #endregion

    #region StartUpdate

    // Start is called before the first frame update
    void Start()
    {
        mainCam.GetComponent<CinemachineVirtualCamera>().enabled = true;
        customCam.GetComponent<CinemachineVirtualCamera>().enabled = false;

        bodyColor = transform.Find("CustomizeMenu/PlayerCustom/PlayerCustomImage").GetComponentInChildren<Image>();
        rifleColor = transform.Find("CustomizeMenu/RifleCustom/RifleCustomImage").GetComponentInChildren<Image>();
        pistolColor = transform.Find("CustomizeMenu/PistolCustom/PistolCustomImage").GetComponentInChildren<Image>();

        playerObject = GameObject.FindWithTag("Player");

        initialRotation = playerObject.transform.rotation;

        AddColorsToList();

        phmm = GameObject.FindGameObjectWithTag("Storage").transform.Find("AudioStorages/MenuManager").GetComponent<MenuManagerAudioStorage>();

        audioUIClick = phmm.audioUIClick;
        audioUIHover = phmm.audioUIHover;
        audioWarning = phmm.audioWarning;
        audioSliderChange = phmm.audioSliderChange;
        audioButtonSelect = phmm.audioButtonSelect;

        if (!DataPersistenceManager.Instance.HasGameData())
        {
            continueButton.interactable = false;
            loadButton.interactable = false;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

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

        watchCursor = true;
        slidersUpdated = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (watchCursor)
        {
            FollowCursor();

            playerObject.transform.rotation = initialRotation;
        }
        else
        {
            playerModel.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);
            rifleModelPivot.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);

            DragWithMouse();
        }

        UpdatePlayerColor();
        UpdateRifleColor();
        UpdatePistolColor();

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
    
    #endregion

    #region Methods

    public void EnableCustomCam()
    {
        mainCam.GetComponent<CinemachineVirtualCamera>().enabled = false;
        customCam.GetComponent<CinemachineVirtualCamera>().enabled = true;

        watchCursor = false;
    }

    public void EnableMainCam()
    {
        mainCam.GetComponent<CinemachineVirtualCamera>().enabled = true;
        customCam.GetComponent<CinemachineVirtualCamera>().enabled = false;

        watchCursor = true;
    }

    void AddColorsToList()
    {
        colors1.Add(c1);
        colors1.Add(c2);
        colors1.Add(c3);
        colors1.Add(c4);
        colors1.Add(c5);
        colors1.Add(c6);
        colors1.Add(c7);
        colors1.Add(c8);
        colors1.Add(c9);

        colors2.Add(c1);
        colors2.Add(c2);
        colors2.Add(c3);
        colors2.Add(c4);
        colors2.Add(c5);
        colors2.Add(c6);
        colors2.Add(c7);
        colors2.Add(c8);
        colors2.Add(c9);

        colors3.Add(c1);
        colors3.Add(c2);
        colors3.Add(c3);
        colors3.Add(c4);
        colors3.Add(c5);
        colors3.Add(c6);
        colors3.Add(c7);
        colors3.Add(c8);
        colors3.Add(c9);
    }

    public void PlayerColorGoRight()
    {
        cIndex1++;
        if (cIndex1 > colors1.Count - 1)
        {
            cIndex1 = 0;
        }
    }

    public void PlayerColorGoLeft()
    {
        cIndex1--;
        if (cIndex1 < 0)
        {
            cIndex1 = colors1.Count - 1;
        }
    }

    public void RifleColorGoRight()
    {
        cIndex2++;
        if (cIndex2 > colors2.Count - 1)
        {
            cIndex2 = 0;
        }
    }

    public void RifleColorGoLeft()
    {
        cIndex2--;
        if (cIndex2 < 0)
        {
            cIndex2 = colors2.Count - 1;
        }
    }

    public void PistolColorGoRight()
    {
        cIndex3++;
        if (cIndex3 > colors3.Count - 1)
        {
            cIndex3 = 0;
        }
    }

    public void PistolColorGoLeft()
    {
        cIndex3--;
        if (cIndex3 < 0)
        {
            cIndex3 = colors3.Count - 1;
        }
    }

    void UpdatePlayerColor()
    {
        bodyColor.color = colors1[cIndex1];
    }

    void UpdateRifleColor()
    {
        rifleColor.color = colors2[cIndex2];
    }

    void UpdatePistolColor()
    {
        pistolColor.color = colors3[cIndex3];
    }

    void FollowCursor()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            mousePos = hit.point;
        }
        
        Vector3 playerDirection = new(mousePos.x - playerModel.transform.position.x, playerModel.transform.position.y, playerModel.transform.position.z);
        playerModel.transform.forward = playerDirection;

        rifleModelPivot.transform.LookAt(mousePos);
    }

    void DragWithMouse()
    {
        if (Input.GetMouseButtonDown(0))
        {
            pressPoint = Input.mousePosition;
            startRotation = playerObject.transform.rotation;
        }
        else if (Input.GetMouseButton(0))
        {
            float DistToNewMousePos = (pressPoint - Input.mousePosition).x;
            playerObject.transform.rotation = startRotation * Quaternion.Euler(Vector3.up * DistToNewMousePos);
        }
    }

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
        toSaveSFXSliderValue = sfxVolumeSlider.value;
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
        GetComponentInParent<CanvasScaler>().scaleFactor = uiScale;
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
        sfxVolumeSlider.value = volume;
    }

    void UpdateSensSlider(float sens)
    {
        mouseSensSlider.value = sens;
    }

    void UpdateUIScaleSlider(float scale)
    {
        uiScaleSlider.value = scale;
    }

    public void UIClick()
    {
        PlayClip(audioUIClick);
    }

    public void UIClickWarning()
    {
        PlayClip(audioWarning);
    }

    public void UIHover()
    {
        PlayClip(audioUIHover);
    }

    public void UISliderChange()
    {
        PlayClip(audioSliderChange);
    }

    public void UIButtonSelect()
    {
        PlayClip(audioButtonSelect);
    }

    public void ContinueGame()
    {
        SceneManager.LoadScene(levelCount);
    }

    public void LoadGameMenu()
    {
        saveSlotsMenu.ActivateMenu(true);
    }

    public void NewGameMenu()
    {
        saveSlotsMenu.ActivateMenu(false);
    }

    public void QuitGame()
    {
        Debug.Log("Quiter");
        Application.Quit();
    }

    public void LoadData(GameData data)
    {
        cIndex1 = data.playerColor;
        cIndex2 = data.rifleColor;
        cIndex3 = data.pistolColor;

        levelCount = data.levelCount;

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
        data.playerColor = cIndex1;
        data.rifleColor = cIndex2;
        data.pistolColor = cIndex3;

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

    #region Credits

    // Main Menu music by Luca Francini
    
    #endregion
}
