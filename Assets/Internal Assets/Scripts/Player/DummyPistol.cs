using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyPistol : MonoBehaviour, IDataPersistence
{
    #region Variables

    [Header("Ints")]
    int cIndex;

    [Header("Lists")]
    List<Color> colors = new();

    [Header("Colors")]
    Color c1 = Color.black;
    Color c2 = Color.blue;
    Color c3 = Color.cyan;
    Color c4 = Color.gray;
    Color c5 = Color.green;
    Color c6 = Color.magenta;
    Color c7 = Color.red;
    Color c8 = Color.white;
    Color c9 = Color.yellow;

    #endregion

    #region StartUpdate

    // Start is called before the first frame update
    void Start()
    {
        AddColorsToList();
    }

    // Update is called once per frame
    void Update()
    {
        UpdatePistolColor();
    }

    #endregion

    #region Methods

    void AddColorsToList()
    {
        colors.Add(c1);
        colors.Add(c2);
        colors.Add(c3);
        colors.Add(c4);
        colors.Add(c5);
        colors.Add(c6);
        colors.Add(c7);
        colors.Add(c8);
        colors.Add(c9);
    }

    void UpdatePistolColor()
    {
        transform.Find("Glock/glock").GetComponent<SkinnedMeshRenderer>().material.color = colors[cIndex];
        transform.Find("Glock/glock/red_dot").GetComponent<SkinnedMeshRenderer>().material.color = colors[cIndex];
        transform.Find("Glock/glock/Supressor").GetComponent<SkinnedMeshRenderer>().material.color = colors[cIndex];
    }

    public void LoadData(GameData data)
    {
        cIndex = data.pistolColor;
    }

    public void SaveData(ref GameData data)
    {
        
    }

    #endregion
}
