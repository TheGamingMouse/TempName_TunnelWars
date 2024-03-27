using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public long lastUpdated;

    public int levelCount;
    public int deathCount;
    public int playerColor;
    public int rifleColor;

    public float masterVolume;
    public float musicVolume;
    public float sfxVolume;
    public float mouseSens;
    public float uiScale;
    public float masterSliderValue;
    public float musicSliderValue;
    public float sfxSliderValue;
    public float sensSliderValue;
    public float uiScaleSliderValue;

    public GameData()
    {
        levelCount = 0;
        deathCount = 0;
    }

    public int GetLevelCount()
    {
        return levelCount;
    }

    public int GetDeathCount()
    {
        return deathCount;
    }
}
