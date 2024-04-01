using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactor : MonoBehaviour
{
    #region Variables

    [Header("Ints")]
    [SerializeField] int numFound;

    [Header("Floats")]
    float radius = 0.5f;

    [Header("Bools")]
    public bool promtFound;

    [Header("Transforms")]
    [SerializeField] Transform point; // SerializeField is Important!

    [Header("Arrays")]
    public readonly Collider[] colliders = new Collider[3];

    [Header("LayerMasks")]
    [SerializeField] LayerMask interactableMask; // SerializeField is Important!

    #endregion

    #region StartUpdate

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        numFound = Physics.OverlapSphereNonAlloc(point.position, radius, colliders, interactableMask);

        if (numFound > 0)
        {
            var interactable = colliders[0].GetComponent<IInteractable>();
            var planter = colliders[0].GetComponent<PlaceBomb>();
            if (interactable != null && Input.GetKeyDown(KeyCode.E) && !planter.bombPlanted)
            {
                interactable.Interact(this);
            }

            promtFound = true;
        }
        else
        {
            promtFound = false;
        }
    }

    #endregion

    #region Gizmos

    void OnDrawGizmos()
    {
        if (point)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(point.position, radius);
        }
    }

    #endregion
}
