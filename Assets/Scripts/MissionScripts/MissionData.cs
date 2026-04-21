using UnityEngine;

[CreateAssetMenu(fileName = "MissionData_", menuName = "Game/MissionData")]
public class MissionData : ScriptableObject
{
    [Header("Mission Info")]
    public string missionName;
    public string missionId;
    public bool enabled = true;

    [Header("Next Mission")]
    public string nextMissionId; // for chaining missions together, can be empty

    [Header("Visuals & Audio")]
    public string[] tileSets; // used for parallax layers, in order from back to front
    public string music; // music track name or path

    [Header("Mission Sequence")]
    public StageAction[] introActions;
    public StageAction[] gameplayActions;
    public StageAction[] bossIntroActions;
    public StageAction[] bossFightActions;
    public StageAction[] outroActions;

    [Header("Rewards")]
    public string[] rewards;
    public int killCredit = 10;
    public int levelCompleteCredit = 200;
    public string bossName;
    public string bossIcon;
    public string[] bossTaunts;
}
