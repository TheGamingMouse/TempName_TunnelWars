using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceBomb : MonoBehaviour, IInteractable
{
    #region Variables

    [Header("Bools")]
    public bool bombPlanted;
    public bool exploded;

    [Header("Strings")]
    readonly string promtString = "Plant Bomb (Press E)";

    [Header("GameObjects")]
    [SerializeField] GameObject bomb;

    #endregion

    #region StartUpdate

    // Start is called before the first frame update
    void Start()
    {
        bombPlanted = false;
        exploded = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    #endregion

    #region Methods

    public string promt => promtString;

    public bool Interact(Interactor interactor)
    {
        bomb.SetActive(true);
        StartCoroutine(BombTimer());
        bombPlanted = true;
        return true;
    }

    IEnumerator BombTimer()
    {
        yield return new WaitForSeconds(5f);

        exploded = true;
    }

    #endregion
}
