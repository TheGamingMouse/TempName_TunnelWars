using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveSlotsMenu : MonoBehaviour
{
    SaveSlot[] saveSlots;

    [SerializeField] bool isLoadingGame = false;
    [SerializeField] int levelCount;

    [SerializeField] GameObject confirmNewGame;

    void Awake()
    {
        saveSlots = GetComponentsInChildren<SaveSlot>();
    }

    public void OnSaveSlotClicked(SaveSlot saveSlot)
    {
        DataPersistenceManager.Instance.ChangeSelectedProfileId(saveSlot.GetProfileId());

        if (!isLoadingGame)
        {
            if (saveSlot.hasData)
            {
                confirmNewGame.SetActive(true);
                return;
            }
            OnConfirmNewGame();
            return;
        }

        DataPersistenceManager.Instance.LoadGame();
        SceneManager.LoadSceneAsync(saveSlot.levelCount);
    }

    public void OnConfirmNewGame()
    {
        DataPersistenceManager.Instance.NewGame();
        SceneManager.LoadSceneAsync("Level1");
    }

    public void ActivateMenu(bool isLoadingGame)
    {
        this.isLoadingGame = isLoadingGame;

        Dictionary<string, GameData> profilesGameData = DataPersistenceManager.Instance.GetAllProfilesGameData();

        foreach (SaveSlot saveSlot in saveSlots)
        {
            profilesGameData.TryGetValue(saveSlot.GetProfileId(), out GameData profileData);
            saveSlot.SetData(profileData);
            if (profileData == null && isLoadingGame)
            {
                saveSlot.SetInteractable(false);
            }
            else
            {
                saveSlot.SetInteractable(true);
            }
        }
    }
}
