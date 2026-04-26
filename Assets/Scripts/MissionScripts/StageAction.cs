using UnityEngine;
using System;
[System.Serializable]
public class StageAction
{
    public enum ActionType
    {
        Idle,
        Dialog,
        SpawnEnemy,
        WaitUntilWaveDead,
        SpawnEliteEnemy,
        WaitUntilEliteWaveDead,
        StartBossEncounter,
        FreezeScrolling,
        UnfreezeScrolling,
        SetScrollSpeedMultiplier,
        WaitForPlayerTrigger,
        CompleteMission
    }

    public ActionType type;
    public string spawnId;
    public float delayTime;
    public float floatValue;
    public float scrollSpeedMultiplier;

    public string text;
    public string openSFX;
    public string closeSFX;
    public string overridePrefab;
    public string completeTrigger;
    public string characterPortrait;
    public string characterName;
    public bool skippable;
}
