using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Indicator : MonoBehaviour
{
    #region Variables

    [Header("Ints")]
    public int id;

    [Header("Bools")]
    bool activatedIndicators;
    bool colorCanged;

    [Header("Components")]
    [SerializeField] DrillHouseIdicators drillIndicators; //SerializeField is Important!
    Color radioColor;

    #endregion

    #region StartUpdate

    void Start()
    {

    }

    void Update()
    {
        activatedIndicators = drillIndicators.activatedIndicators;

        if (activatedIndicators && !colorCanged)
        {
            foreach (Radiostation r in drillIndicators.radiostations)
            {
                if (r.id == id)
                {
                    radioColor = r.indicator.GetComponent<MeshRenderer>().material.color;
                }
            }

            GetComponent<MeshRenderer>().material.color = radioColor;

            colorCanged = true;
        }
    }

    #endregion
}
