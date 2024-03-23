using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorChange : MonoBehaviour
{
    #region Variables

    [Header("Enum States")]
    [SerializeField] ColorState cState; // SerializeField is Important!
    
    [Header("Images")]
    Image bodyColor;
    Image rifleColor;

    #endregion

    #region StartUpdate

    // Start is called before the first frame update
    void Start()
    {
        bodyColor = GameObject.FindGameObjectWithTag("MainCamera").transform.Find("Canvas/Menus/CustomizeMenu/PlayerCustom/PlayerCustomImage").GetComponentInChildren<Image>();
        rifleColor = GameObject.FindGameObjectWithTag("MainCamera").transform.Find("Canvas/Menus/CustomizeMenu/RifleCustom/RifleCustomImage").GetComponentInChildren<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        switch (cState)
        {
            case ColorState.body:
                GetComponent<MeshRenderer>().material.SetColor("_Color", bodyColor.color);
                break;
            
            case ColorState.rifle:
                GetComponent<MeshRenderer>().material.SetColor("_Color", rifleColor.color);
                break;
        }
    }

    #endregion

    #region Enums

    enum ColorState
    {
        body,
        rifle
    }

    #endregion
}
