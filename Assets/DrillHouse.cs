
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DrillHouse : MonoBehaviour
{
    #region Variables

    [Header("Bools")]
    public bool sortedRadiostations;

    [Header("Lists")]
    public List<Radiostation> radiostations = new();

    [Header("GameObjects")]
    [SerializeField] GameObject door; // SerializeField is Important!

    [Header("Components")]
    DrillHouseIdicators drillIndicators;

    #endregion

    #region StartUpdate

    // Start is called before the first frame update
    void Start()
    {
        FindRadiostations();

        drillIndicators = GetComponentInChildren<DrillHouseIdicators>();
    }

    // Update is called once per frame
    void Update()
    {
        if (drillIndicators.activeIndicators.Count() == 0)
        {
            door.SetActive(false);
        }
    }

    #endregion

    #region Methods

    void FindRadiostations()
    {
        GameObject[] findAllArray = GameObject.FindGameObjectsWithTag("Radiostation");

        foreach (GameObject g in findAllArray)
        {
            radiostations.Add(g.GetComponent<Radiostation>());
        }

        radiostations.OrderBy(o => o.id).ToList();
        
        int i = 0;
        foreach (Radiostation r in radiostations)
        {
            r.id = i;
            i++;
        }

        sortedRadiostations = true;
    }

    #endregion
}
