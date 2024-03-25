using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaveSlot : MonoBehaviour
{
    [Header("Bools")]
    public bool hasData;

    [Header("Ints")]
    public int levelCount;
    public int deathCount;

    [Header("Strings")]
    [SerializeField] string profileId = "";

    [Header("GameObjects")]
    [SerializeField] GameObject noDataContent;
    [SerializeField] GameObject hasDataContent;

    [Header("TMP_Pros")]
    [SerializeField] TMP_Text levelText;
    [SerializeField] TMP_Text deathText;

    [Header("Buttons")]
    [SerializeField] Button saveSlotButton;

    public void SetData(GameData data)
    {
        if (data == null)
        {
            noDataContent.SetActive(true);
            hasDataContent.SetActive(false);

            hasData = false;
        }
        else
        {
            noDataContent.SetActive(false);
            hasDataContent.SetActive(true);

            levelCount = data.levelCount;
            deathCount = data.deathCount;

            levelText.text = $"Level: {levelCount}";
            deathText.text = $"Deaths: {deathCount}";

            hasData = true;
        }
    }

    public string GetProfileId()
    {
        return profileId;
    }

    public void SetInteractable(bool interactable)
    {
        saveSlotButton.interactable = interactable;
    }
}
