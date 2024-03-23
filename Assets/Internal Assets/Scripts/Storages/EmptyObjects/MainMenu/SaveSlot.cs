using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SaveSlot : MonoBehaviour
{
    [Header("Ints")]
    public int levelCount;

    [Header("Strings")]
    [SerializeField] string profileId = "";

    [Header("GameObjects")]
    [SerializeField] GameObject noDataContent;
    [SerializeField] GameObject hasDataContent;

    [Header("Buttons")]
    [SerializeField] Button saveSlotButton;

    public void SetData(GameData data)
    {
        if (data == null)
        {
            noDataContent.SetActive(true);
            hasDataContent.SetActive(false);
        }
        else
        {
            noDataContent.SetActive(false);
            hasDataContent.SetActive(true);
            levelCount = data.levelCount;
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
