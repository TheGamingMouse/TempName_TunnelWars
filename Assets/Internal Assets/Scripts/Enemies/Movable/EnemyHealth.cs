using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    #region Events

    public static event Action OnDamageTaken;

    #endregion
    
    #region Variables

    [Header("Flaots")]
    readonly float maxHealth = 100f;
    float health;

    [Header("GameObjects")]
    GameObject enemyObj;

    #endregion

    #region StartUpdate

    // Start is called before the first frame update
    void Start()
    {
        health = maxHealth;
        enemyObj = GetComponentInParent<enemy>().gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    #endregion

    #region Methods

    public void TakeDamage(float damage)
    {
        health -= damage;
        OnDamageTaken?.Invoke();

        if (health <= 0)
        {
            Destroy(enemyObj);
        }
    }

    #endregion
}
