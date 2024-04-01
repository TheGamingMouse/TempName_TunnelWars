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
    Image pistolColor;

    #endregion

    #region StartUpdate

    // Start is called before the first frame update
    void Start()
    {
        bodyColor = GameObject.FindGameObjectWithTag("MainCamera").transform.Find("Canvas/Menus/CustomizeMenu/PlayerCustom/PlayerCustomImage").GetComponentInChildren<Image>();
        rifleColor = GameObject.FindGameObjectWithTag("MainCamera").transform.Find("Canvas/Menus/CustomizeMenu/RifleCustom/RifleCustomImage").GetComponentInChildren<Image>();
        pistolColor = GameObject.FindGameObjectWithTag("MainCamera").transform.Find("Canvas/Menus/CustomizeMenu/PistolCustom/PistolCustomImage").GetComponentInChildren<Image>();
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
                transform.Find("tar21/Tar21").GetComponent<SkinnedMeshRenderer>().material.color = rifleColor.color;
                transform.Find("All_in_one_scopes/red_dot_d_prefab/red_dot_d").GetComponent<MeshRenderer>().material.color = rifleColor.color;
                break;

            case ColorState.pistol:
                transform.Find("Glock/glock").GetComponent<SkinnedMeshRenderer>().material.color = pistolColor.color;
                transform.Find("Glock/glock/red_dot").GetComponent<SkinnedMeshRenderer>().material.color = pistolColor.color;
                transform.Find("Glock/glock/Supressor").GetComponent<SkinnedMeshRenderer>().material.color = pistolColor.color;
                break;
        }
    }

    #endregion

    #region Enums

    enum ColorState
    {
        body,
        rifle,
        pistol
    }

    #endregion
}
