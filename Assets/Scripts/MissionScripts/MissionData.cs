using UnityEngine;

[CreateAssetMenu(fileName = "MissionData_", menuName = "Game/MissionData")]
public class MissionData : ScriptableObject
{
    public string missionName;
    public string missionId;
    public bool enabled = true;
    public string[] tileSets;
    public string music;

    [Header("Enemy Spawn Intervals (Seconds) - used when no explicity waves are available")]
    public float startWait = 3.5f;
    public float quarter1Min = 2.0f;
    public float quarter1Max = 2.5f;
    public float quarter2Min = 1.8f;
    public float quarter2Max = 2.3f;
    public float quarter3Min = 1.5f;
    public float quarter3Max = 2.0f;
    public float quarter4Min = 1.3f;
    public float quarter4Max = 1.8f;

    [Header("Wave")]
    public string[] enemyWaves;
    public string[] eliteEnemyWaves;
    public string[] bosses;

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
