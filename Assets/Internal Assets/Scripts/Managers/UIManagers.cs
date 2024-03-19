using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManagers : MonoBehaviour
{
    #region Variables

    [Header("Enum States")]
    FireModeState fmState;

    [Header("Floats")]
    float magAmmo;
    float totalAmmo;
    float health;
    float playerReloadTime;
    float playerReloadCooldown;

    [Header("Bools")]
    bool reloading;

    [Header("Strings")]
    string fireMode;

    [Header("GameObjects")]
    GameObject camObj;
    GameObject gameOverObj;
    [SerializeField] GameObject damageIndicatorPrefab; // SerializeField is Important!
    GameObject levelCompleteObj;
    
    [Header("Transforms")]
    Transform canvas;
    Transform player;
    Transform damageIndicatorPivot;
    
    [Header("TMP_Texts")]
    TMP_Text ammoText;
    TMP_Text fireModeText;
    TMP_Text healthText;

    [Header("Images")]
    Image healthImg;
    Image reloadImg;

    [Header("Components")]
    Rifle rifleScript;
    PlayerHealth  playerHealthScript;

    #endregion

    #region Subscriptions

    void OnEnable()
    {
        PlayerHealth.OnPlayerDeath += HandlePlayerDeath;
        PlayerHealth.OnPlayerDamage += HandleDamageTaken;
        PlayerMovement.OnLevelComplete += HandleLevelComplete;
    }

    void OnDisable()
    {
        PlayerHealth.OnPlayerDeath -= HandlePlayerDeath;
        PlayerHealth.OnPlayerDamage -= HandleDamageTaken;
        PlayerMovement.OnLevelComplete -= HandleLevelComplete;
    }

    #endregion

    #region StartUpdate

    // Start is called before the first frame update
    void Start()
    {
        canvas = GameObject.FindGameObjectWithTag("UI").transform;
        camObj = Camera.main.gameObject;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        
        gameOverObj = canvas.Find("DeathScreenPanel").gameObject;
        levelCompleteObj = canvas.Find("LevelCompletePanel").gameObject;
        damageIndicatorPivot = canvas.Find("DamageIndicatorPivot");
        
        playerHealthScript = player.GetComponentInChildren<PlayerHealth>();
        rifleScript = camObj.GetComponentInChildren<Rifle>();

        ammoText = canvas.Find("AmmoText (TMP)").GetComponent<TMP_Text>();
        fireModeText = canvas.Find("FireModeText (TMP)").GetComponent<TMP_Text>();
        healthImg = canvas.Find("Health/Health").GetComponent<Image>();
        healthText = canvas.Find("Health/HealthText (TMP)").GetComponent<TMP_Text>();
        reloadImg = canvas.Find("Crosshair/Reload").GetComponent<Image>();

        DisableElements();
    }

    // Update is called once per frame
    void Update()
    {
        health = playerHealthScript.health;

        magAmmo = rifleScript.bulletsLeft;
        totalAmmo = rifleScript.totalAmmo;

        fmState = (FireModeState)rifleScript.fmState;
        
        playerReloadTime = rifleScript.reloadTime;
        playerReloadCooldown = rifleScript.reloadCooldown;
        reloading = rifleScript.reloading;
        
        UpdateAmmoText();
        UpdateFireModeText();
        UpdateHealthFill();
        UpdateReload();

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
    }

    #endregion

    #region GeneralMethods

    void DisableElements()
    {
        gameOverObj.SetActive(false);
        levelCompleteObj.SetActive(false);
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

    #endregion

    #region SubscriptionHandlers

    void HandlePlayerDeath()
    {
        gameOverObj.SetActive(true);
        Time.timeScale = 0f;
    }

    void HandleDamageTaken()
    {
        Instantiate(damageIndicatorPrefab, damageIndicatorPivot);
    }

    void HandleLevelComplete()
    {
        levelCompleteObj.SetActive(true);
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
