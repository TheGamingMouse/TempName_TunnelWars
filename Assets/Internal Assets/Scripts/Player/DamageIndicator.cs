using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageIndicator : MonoBehaviour
{
    #region Variables

    [Header("Flaots")]
    float fadeStart = 1.5f;
    float fadeTime = 1.5f;
    float fadeMax;

    [Header("Transforms")]
    Transform canvas;
    Transform player;
    Transform damageIndicatorPivot;

    [Header("Vector3s")]
    Vector3 damagePos;

    [Header("Components")]
    CanvasGroup damamgeIndicatorGroup;

    #endregion

    #region StartUpdate

    // Start is called before the first frame update
    void Start()
    {
        canvas = GameObject.FindGameObjectWithTag("UI").transform;
        player = GameObject.FindGameObjectWithTag("Player").transform;

        damageIndicatorPivot = canvas.Find("DamageIndicatorPivot");
        damamgeIndicatorGroup = GetComponent<CanvasGroup>();

        fadeMax = fadeTime;
    }

    // Update is called once per frame
    void Update()
    {
        damagePos = player.GetComponent<PlayerHealth>().dmgDirection;

        if (fadeStart > 0)
        {
            fadeStart -= Time.deltaTime;
        }
        else
        {
            fadeTime -= Time.deltaTime;
            damamgeIndicatorGroup.alpha = fadeTime / fadeMax;
            if (fadeTime <= 0)
            {
                Destroy(gameObject);
            }
        }
        
        damagePos.y = player.position.y;
        Vector3 direction = (damagePos - player.position).normalized;
        float angle = Vector3.SignedAngle(direction, player.forward, Vector3.up);
        damageIndicatorPivot.transform.localEulerAngles = new Vector3(0f, 0f, angle);
    }

    #endregion
}
