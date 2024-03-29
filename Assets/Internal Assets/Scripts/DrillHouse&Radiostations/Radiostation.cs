using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Radiostation : MonoBehaviour
{
    #region Variables

    [Header("Ints")]
    public int id;

    [Header("Bools")]
    public bool destroyed;

    [Header("Lists")]
    List<Color> colors = new();

    [Header("GameObjects")]
    public GameObject indicator; //SerializeField is Important!
    [SerializeField] GameObject bomb; //SerializeField is Important!

    [Header("Colors")]
    Color c0 = Color.blue;
    Color c1 = Color.cyan;
    Color c2 = Color.green;
    Color c3 = Color.magenta;
    Color c4 = Color.red;
    Color c5 = Color.yellow;

    #endregion

    #region StartUpdate

    // Start is called before the first frame update
    void Start()
    {
        AddColors();
    }

    // Update is called once per frame
    void Update()
    {
        if (!destroyed)
        {
            indicator.GetComponent<MeshRenderer>().material.color = colors[id];
        }
    }
    
    #endregion

    #region Methods

    void AddColors()
    {
        colors.Add(c0);
        colors.Add(c1);
        colors.Add(c2);
        colors.Add(c3);
        colors.Add(c4);
        colors.Add(c5);
    }

    #endregion
}
