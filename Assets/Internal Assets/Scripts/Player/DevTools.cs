using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DevTools : MonoBehaviour
{
    #region Variables

    [Header("Vector3s")]
    Vector3 spawn = new Vector3(0f, 0.1f, -5.96046448e-07f);
    Vector3 radioOne = new Vector3(58.1619377f, 0.1f, 178.152679f);
    Vector3 radioTwo = new Vector3(-260.995331f, 0.1f, 228.232391f);
    Vector3 radioThree = new Vector3(-248.607117f, 0.1f, 83.9364548f);
    Vector3 drillHouse = new Vector3(-139.120743f, 0.1f, 186.229919f);

    [Header("Components")]
    [SerializeField] PlayerHealth playerHealth;
    [SerializeField] GameObject player;

    #endregion

    void Update()
    {
        if (SceneManager.GetActiveScene().buildIndex == 2)
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                playerHealth.godMode = !playerHealth.godMode;
            }
            if (Input.GetKeyDown(KeyCode.O))
            {
                player.transform.position = spawn;
            }
            if (Input.GetKeyDown(KeyCode.L))
            {
                player.transform.position = radioOne;
            }
            if (Input.GetKeyDown(KeyCode.I))
            {
                player.transform.position = radioTwo;
            }
            if (Input.GetKeyDown(KeyCode.K))
            {
                player.transform.position = radioThree;
            }
            if (Input.GetKeyDown(KeyCode.U))
            {
                player.transform.position = drillHouse;
            }
        }
    }
}
