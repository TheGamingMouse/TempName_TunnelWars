using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
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
        if (health <= 0)
        {
            Destroy(enemyObj);
        }
    }

    #endregion

    #region Methods

    public void TakeDamage(float damage)
    {
        health -= damage;
    }

    #endregion
}
