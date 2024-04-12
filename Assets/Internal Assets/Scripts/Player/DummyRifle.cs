using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyRifle : MonoBehaviour, IDataPersistence
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
        UpdateRifleColor();
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

    void UpdateRifleColor()
    {
        transform.Find("ak47/ak47").GetComponent<SkinnedMeshRenderer>().material.color = colors[cIndex];
        transform.Find("All_in_one_scopes/red_dot_a_prefab/red_dot_a").GetComponent<MeshRenderer>().material.color = colors[cIndex];
    }

    public void LoadData(GameData data)
    {
        cIndex = data.rifleColor;
    }

    public void SaveData(ref GameData data)
    {
        
    }

    #endregion
}
