using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecretDrop : MonoBehaviour
{
    #region Variables
    
    [Header("Floats")]
    public float ammountRifleAmmo;
    public float ammountPistolAmmo;
    readonly float rotateSpeed = 1f;

    [Header("Ints")]
    public int ammountHealth;

    [Header("GameObjects")]
    [SerializeField] GameObject parentObj; // SerializeField is Important!

    #endregion

    #region StartUpdate

    // Start is called before the first frame update
    void Awake()
    {
        SetAmount();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.Rotate(new Vector3(0f, rotateSpeed, 0f));
    }

    #endregion

    #region Methods

    void SetAmount()
    {
        ammountRifleAmmo = Random.Range(1, 4) * 30;
        ammountPistolAmmo = Random.Range(1, 4) * 14;
        ammountHealth = Random.Range(1, 4) * 10;
    }

    #endregion
}
