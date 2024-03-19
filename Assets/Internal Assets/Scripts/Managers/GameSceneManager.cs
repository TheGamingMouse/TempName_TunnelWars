using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneManager : MonoBehaviour
{
    #region Variables

    [Header("Bools")]
    bool goingToNextLevel;
    
    #endregion

    #region Subscriptions

    void OnEnable()
    {
        PlayerMovement.OnLevelComplete += HandleNextLevel;
    }

    void OnDisable()
    {
        PlayerMovement.OnLevelComplete -= HandleNextLevel;
    }

    #endregion
    
    #region StartUpdate

    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1f;

        goingToNextLevel = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    #endregion

    #region Methods

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void NextLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        Time.timeScale = 1f;
    }

    IEnumerator NextLevelRoutine()
    {
        yield return new WaitForSeconds(3f);
        NextLevel();
    }

    public void QuitGame()
    {
        Debug.Log("Quiter");
        Application.Quit();
    }

    #endregion

    #region SubscriptionHandlers

    void HandleNextLevel()
    {
        if (!goingToNextLevel)
        {
            StartCoroutine(NextLevelRoutine());
            goingToNextLevel = true;
        }
    }

    #endregion
}
