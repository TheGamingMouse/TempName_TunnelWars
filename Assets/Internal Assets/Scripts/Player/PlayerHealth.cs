using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    #region Events

    public static event Action OnPlayerDeath;

    #endregion
    
    #region Variables

    [Header("Floats")]
    readonly float cooldown = 1f;

    [Header("Ints")]
    readonly int maxHealth = 100;
    public int health;

    [Header("Bools")]
    bool damageCooldown;
    [SerializeField] bool godMode; // SerializeField is Important!

    #endregion

    #region StartUpdate

    // Start is called before the first frame update
    void Start()
    {
        health = maxHealth;
        damageCooldown = false;
    }

    // Update is called once per frame
    void Update()
    {
        health = Mathf.Clamp(health, 0, maxHealth);
        
        if (!godMode)
        {
            if (health <= 0)
            {
                OnPlayerDeath?.Invoke();
                // print("Player is dead");
            }
        }
    }

    #endregion

    #region Methods

    public void TakeDamage(int damage)
    {
        if (!damageCooldown)
        {
            health -= damage;
        }
        StopCoroutine(nameof(DamageTaken));
        StartCoroutine(DamageTaken());
    }

    IEnumerator DamageTaken()
    {
        damageCooldown = true;

        yield return new WaitForSeconds(cooldown);

        damageCooldown = false;
    }

    #endregion
}
