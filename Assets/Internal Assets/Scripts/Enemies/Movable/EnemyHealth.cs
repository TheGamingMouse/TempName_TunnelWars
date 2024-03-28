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
    [SerializeField] GameObject ammoDrop; // SerializeField is Important!
    GameObject spawnedPrefabs;

    [Header("Transforms")]
    Transform spawnedDrops;

    #endregion

    #region StartUpdate

    // Start is called before the first frame update
    void Start()
    {
        health = maxHealth;
        enemyObj = GetComponentInParent<enemy>().gameObject;
        spawnedPrefabs = GameObject.FindGameObjectWithTag("Prefabs");
        spawnedDrops = spawnedPrefabs.transform.Find("SpawnedDrops");
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
            int i = UnityEngine.Random.Range(1, 101);
            if (i > 20)
            {
                Instantiate(ammoDrop, new Vector3(transform.position.x, 0.4f, transform.position.z), Quaternion.identity, spawnedDrops);
            } 
            Destroy(enemyObj);
        }
    }

    #endregion
}
