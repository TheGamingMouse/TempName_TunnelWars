using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretHealth : MonoBehaviour
{
    #region Variables

    [Header("Flaots")]
    readonly float maxHealth = 150f;
    float health;

    #endregion

    #region StartUpdate

    // Start is called before the first frame update
    void Start()
    {
        health = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if (health <= 0)
        {
            Destroy(gameObject);
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
