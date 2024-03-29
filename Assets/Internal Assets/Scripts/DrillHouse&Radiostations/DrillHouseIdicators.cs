using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrillHouseIdicators : MonoBehaviour
{
    #region Variables

    [Header("Bools")]
    bool sortedRadiostations;
    public bool activatedIndicators;

    [Header("Lists")]
    public List<Radiostation> radiostations = new();
    List<Indicator> indicators = new();
    public List<ActivatorIndicator> activatorIndicators = new();

    [Header("Components")]
    [SerializeField] DrillHouse drillHouse; //SerializeField is Important!

    #endregion

    #region StartUpdate

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        sortedRadiostations = drillHouse.sortedRadiostations;

        if (sortedRadiostations && !activatedIndicators)
        {
            GameObject[] findAllArray = GameObject.FindGameObjectsWithTag("Indicator");

            foreach (GameObject g in findAllArray)
            {
                indicators.Add(g.GetComponent<Indicator>());
            }

            radiostations = drillHouse.radiostations;

            foreach (Indicator i in indicators)
            {
                if (i.id > (radiostations.Count - 1))
                {
                    i.gameObject.SetActive(false);
                }
                else
                {
                    i.GetComponentInChildren<ActivatorIndicator>().GetComponent<MeshRenderer>().material.color = Color.red;
                    activatorIndicators.Add(i.GetComponentInChildren<ActivatorIndicator>());
                }
            }

            activatedIndicators = true;
        }

        foreach (Radiostation r in radiostations)
        {
            if (r.destroyed)
            {
                foreach (Indicator i in indicators)
                {
                    if (i.id == r.id)
                    {
                        i.GetComponentInChildren<ActivatorIndicator>().GetComponent<MeshRenderer>().material.color = Color.green;
                        activatorIndicators.Remove(i.GetComponentInChildren<ActivatorIndicator>());
                    }
                }
            }
        }
    }

    #endregion
}
