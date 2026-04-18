using UnityEngine;

[CreateAssetMenu(fileName = "StageData_", menuName = "Game/StageData")]
public class StageData : ScriptableObject
{
    public string stageName;
    public float stageLength = 500f;// total scroll distance before boss
    public float baseScrollSpeed = 5f; // reference speed for progress tracking
    public GameObject bossPrefab;
    public AudioClip stageMusic;
    public float bossIntroDelay = 2f;
    public float transitionDelay = 3f;
    public float transitionScrollMultiplier = 2f;
    // just in case if we want to trigger events during mid stage
    public float[] midStageTriggers;
}
