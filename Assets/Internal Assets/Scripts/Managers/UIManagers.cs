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

    [Header("Strings")]
    string fireMode;

    [Header("GameObjects")]
    GameObject camObj;
    GameObject playerObj;
    GameObject gameOverObj;
    
    [Header("Transforms")]
    Transform canvas;
    
    [Header("TMP_Texts")]
    TMP_Text ammoText;
    TMP_Text fireModeText;
    TMP_Text healthText;

    [Header("Images")]
    Image healthImg;

    #endregion

    #region Subscriptions

    void OnEnable()
    {
        PlayerHealth.OnPlayerDeath += HandlePlayerDeath;
    }

    void OnDisable()
    {
        PlayerHealth.OnPlayerDeath -= HandlePlayerDeath;
    }

    #endregion

    #region StartUpdate

    // Start is called before the first frame update
    void Start()
    {
        canvas = GameObject.FindGameObjectWithTag("UI").transform;
        camObj = Camera.main.gameObject;
        playerObj = GameObject.FindGameObjectWithTag("Player");
        gameOverObj = canvas.Find("DeathScreenPanel").gameObject;

        ammoText = canvas.Find("AmmoText (TMP)").GetComponent<TMP_Text>();
        fireModeText = canvas.Find("FireModeText (TMP)").GetComponent<TMP_Text>();
        healthImg = canvas.Find("Health/Health").GetComponent<Image>();
        healthText = canvas.Find("Health/HealthText (TMP)").GetComponent<TMP_Text>();

        DisableElements();
    }

    // Update is called once per frame
    void Update()
    {
        magAmmo = camObj.GetComponentInChildren<Rifle>().bulletsLeft;
        totalAmmo = camObj.GetComponentInChildren<Rifle>().totalAmmo;
        fmState = (FireModeState)camObj.GetComponentInChildren<Rifle>().fmState;
        health = playerObj.GetComponentInChildren<PlayerHealth>().health;
        
        UpdateAmmoText();
        UpdateFireModeText();
        UpdateHealthFill();

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

    #endregion

    #region SubscriptionHandlers

    void HandlePlayerDeath()
    {
        gameOverObj.SetActive(true);
        Time.timeScale = 0f;
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
