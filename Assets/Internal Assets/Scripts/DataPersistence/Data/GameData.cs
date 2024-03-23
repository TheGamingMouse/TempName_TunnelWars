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

    public GameData()
    {
        levelCount = 0;
        deathCount = 0;
        playerColor = 7;
        rifleColor = 7;
    }

    public int GetLevelCount()
    {
        return levelCount;
    }
}
