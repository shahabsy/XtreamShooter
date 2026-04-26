using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;

public class StageController : MonoBehaviour
{
    public static StageController Instance {  get; private set; }

    [Header("Mission")]
    public MissionData currentMission;

    [Header("References")]
    public BackgroundSpawner backgroundSpawner;
    public EnemySpawner enemySpawner;
    public DialogManager dialogManager;
    public PlayerController playerController;
    public BossEncounterController bossEncounter;

    //private bool isStartingMission = false;// Prevents re-entry

    private enum MissionPhase
    {
        Intro,
        Gameplay,
        BossIntro,
        BossFight,
        Outro,
        Completed
    }

    private MissionPhase currentPhase = MissionPhase.Intro;

    private int actionIndex = -1;
    private bool missionComplete = false;
    
    private Dictionary<string, Action<string>> triggerHandlers = new Dictionary<string, Action<string>>();

    private string lastSpawnedWaveId = null;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (currentMission == null) { Debug.LogError("No mission assigned."); return; }
        if (backgroundSpawner == null) backgroundSpawner = FindAnyObjectByType<BackgroundSpawner>(); 
        if (enemySpawner == null) enemySpawner = FindAnyObjectByType<EnemySpawner>();
        if (dialogManager == null) dialogManager = FindAnyObjectByType<DialogManager>();
        if (playerController == null) playerController = FindAnyObjectByType<PlayerController>();
        if (bossEncounter == null) bossEncounter = FindAnyObjectByType<BossEncounterController>();
        StartMission();
    }

    private void StartMission()
    {
        //if (isStartingMission) return;
        //isStartingMission = true;

        Debug.Log($"Starting mission: {currentMission.missionName} ID: {currentMission.missionId}");
        // Play music

        // Begin intro phase
        ClearAllTriggerListeners();
        EntityTracker.Instance?.ResetTracker();
        enemySpawner.ResetSpawner();
        backgroundSpawner.ResetSpawner();
        playerController.ResetPlayer();
        


        if (backgroundSpawner != null && currentMission.tileSet != null)
        {
            Debug.Log($"Initializing background spawner with tile set: {currentMission.tileSet.name}");
            backgroundSpawner.Initialize(currentMission.tileSet);
            backgroundSpawner.SetScrolling(true);
            backgroundSpawner.SetScrollingMultiplier(1);
        }

        currentPhase = MissionPhase.Intro;
        actionIndex = -1;
        missionComplete = false;
        lastSpawnedWaveId = null;

        ProceedToNextAction();
        //isStartingMission = false;
    }
    private void ProceedToNextAction()
    {
        if (missionComplete) return;
        StageAction[] actions = GetCurrentPhaseActions();
        actionIndex++;

        if (actions == null || actionIndex >= actions.Length)
        {
            OnPhaseComplete();
            return;
        }
        StartCoroutine(ExecuteActionWithDelay(actions[actionIndex]));
    }

    private IEnumerator ExecuteActionWithDelay(StageAction action)
    {
        if(action.delayTime > 0)
            yield return new WaitForSeconds(action.delayTime);

        yield return ExecuteAction(action);
    }

    private IEnumerator ExecuteAction(StageAction action)
    {
        switch (action.type)
        {
            case StageAction.ActionType.Idle:
                Debug.Log($"Stage: Idle");
                break;
            case StageAction.ActionType.Dialog:
                Debug.Log("Stage: Dialog");
                yield return ExecuteDialog(action);
                break;
            // -------------------------------------
            // Normal Wave
            // ------------------------------------
            case StageAction.ActionType.SpawnEnemy:
                Debug.Log("Stage: SpawnEnemy");
                lastSpawnedWaveId = enemySpawner.SpawnWaveAndReturnId(action.spawnId);
                break;
            case StageAction.ActionType.WaitUntilWaveDead:
                {
                    Debug.Log($"Stage: Waiting for wave {lastSpawnedWaveId}");
                    if (!string.IsNullOrEmpty(lastSpawnedWaveId))
                    {
                        yield return WaitForWaveClear(lastSpawnedWaveId);
                    }
                    break;
                }
            // -------------------------------------
            // Elite Wave
            // ------------------------------------
            case StageAction.ActionType.SpawnEliteEnemy:
                Debug.Log("Stage: SpawnEliteEnemy");
                lastSpawnedWaveId = enemySpawner.SpawnEliteWaveAndReturnId(action.spawnId);
                break;
            case StageAction.ActionType.WaitUntilEliteWaveDead:
                {
                    Debug.Log($"Stage: Waiting for elite wave {lastSpawnedWaveId}");
                    if (!string.IsNullOrEmpty(lastSpawnedWaveId))
                    {
                        yield return WaitForWaveClear(lastSpawnedWaveId);
                    }
                    break;
                }
            // -------------------------------------
            // Boss
            // ------------------------------------
            case StageAction.ActionType.StartBossEncounter:
                Debug.Log("Stage: StartBossEncounter");
                if (bossEncounter != null)
                    yield return bossEncounter.StartEncounter(action.spawnId);
                else
                    Debug.Log("BossEnounterController: missing");
                break;
            // -------------------------------------
            // Environment
            // ------------------------------------
            case StageAction.ActionType.FreezeScrolling:
                Debug.Log("Stage: FreenzScrolling");
                SetScrolling(false);
                break;
            case StageAction.ActionType.UnfreezeScrolling:
                Debug.Log("Stage: UnfreezeScrolling");
                SetScrolling(true);
                break;
            case StageAction.ActionType.SetScrollSpeedMultiplier:
                Debug.Log("Stage: SetScrollSpeedMltiplier");
                backgroundSpawner?.SetScrollingMultiplier(action.scrollSpeedMultiplier);
                break;
            // -------------------------------------
            // Flow Control
            // ------------------------------------
            case StageAction.ActionType.WaitForPlayerTrigger:
                Debug.Log("Stage: WaitForPlayerTrigger");
                yield return WaitForPlayerTrigger(action.completeTrigger);
                break;
            case StageAction.ActionType.CompleteMission:
                Debug.Log("Stage: CompleteMission");
                MissionComplete();
                break;
        }
        ProceedToNextAction();
    }
    private IEnumerator WaitForWaveClear(string waveId)
    {
        if (EntityTracker.Instance == null) yield break;

        while (enemySpawner.IsWaveSpawning(waveId) || EntityTracker.Instance.HasEnemiesInWave(waveId))
        {
            yield return null;
        }
        Debug.Log($"Wave {waveId} fully cleared.");
    }

    private StageAction[] GetCurrentPhaseActions()
    {
        switch (currentPhase)
        {
            case MissionPhase.Intro: return currentMission.introActions;
            case MissionPhase.Gameplay: return currentMission.gameplayActions;
            case MissionPhase.BossIntro: return currentMission.bossIntroActions;
            case MissionPhase.BossFight: return currentMission.bossFightActions;
            case MissionPhase.Outro: return currentMission.outroActions;
            default: return null;
        }
    }
    
    private void OnPhaseComplete()
    {
        switch(currentPhase)
        {
            case MissionPhase.Intro:
                // Start gameplay phase: enable scrolling and enemy spawning
                Debug.Log("Phase: Intro");
                currentPhase = MissionPhase.Gameplay;
                //SetScrolling(true);
                actionIndex = -1;
                ProceedToNextAction();
                break;
            case MissionPhase.Gameplay:
                // Gameplay finished - trigger boss intro
                Debug.Log("Phase: GamePlay");
                currentPhase = MissionPhase.BossIntro;
                //SetScrolling(false);
                actionIndex = -1;
                ProceedToNextAction();
                break;
            case MissionPhase.BossIntro:
                Debug.Log("Phase: BossIntro");
                currentPhase = MissionPhase.BossFight;
                actionIndex = -1;
                ProceedToNextAction();
                break;
            case MissionPhase.BossFight:
                Debug.Log("Phase: BossFight");
                currentPhase = MissionPhase.Outro;
                actionIndex = -1;
                ProceedToNextAction();
                break;
            case MissionPhase.Outro:
                Debug.Log("Phase: Outro");
                MissionComplete();
                break;
        }
    }
    private IEnumerator WaitForPlayerTrigger(string triggerName)
    {
        if (playerController == null) yield break;

        bool triggered = false;
        Action<string> handler = (t) =>
        {
            if (t == triggerName) triggered = true;
        };

        triggerHandlers[triggerName] = handler;
        playerController?.RegisterTriggerListener(handler);

        while (!triggered) yield return null;

        playerController?.UnregisterTriggerListener(handler);
        triggerHandlers.Remove(triggerName);
    }
    private IEnumerator ExecuteDialog(StageAction action)
    {
        if (dialogManager == null)
        {
            Debug.Log($"Dialog: {action.text}");
            yield break;
        }
        bool completed = false;
        dialogManager.ShowDialog(action, () => completed = true);

        while (!completed) yield return null;
    }
    private void SetScrolling(bool enabled)
    {
        if (backgroundSpawner != null)
        {
            backgroundSpawner?.SetScrolling(enabled);
        }
        else
        {
            Debug.Log("BackgroundSpawner missing. cannot set scrolling.");
        }

    }
    private void MissionComplete()
    {
        if (missionComplete) return;
        missionComplete = true;
        currentPhase = MissionPhase.Completed;
        Debug.Log($"Mission: {currentMission.missionName} completed.");
        ClearAllTriggerListeners();
        enemySpawner?.ResetSpawner();
        backgroundSpawner.ResetSpawner();
        EntityTracker.Instance?.ClearAllEntities();
        EntityTracker.Instance.ResetTracker();
        // show reward screen, load next mission etc...
        LoadNextMission();
    }
    private void LoadNextMission()
    {
        if (string.IsNullOrEmpty(currentMission.nextMissionId))
        {
            Debug.LogWarning("No next mission specified. Cannot load next mission.");
            return;
        }

        string path = $"DataAssets/Missions/{currentMission.nextMissionId}";
        MissionData nextMission = Resources.Load<MissionData>(path);
        if (nextMission == null)
        {
            Debug.LogError($"Next mission with ID {currentMission.nextMissionId} not found in {path}.");
            return;
        }
        Debug.Log($"Successfully Loaded next mission: {nextMission.missionName} ID: {nextMission.missionId}");
        currentMission = nextMission;
        //missionComplete = false;
        StartMission();
    }

    private void OnDestroy()
    {
        ClearAllTriggerListeners();
    }

    private void ClearAllTriggerListeners()
    {
        if (playerController != null)
        {
            foreach (var trigger in triggerHandlers.Values)
                playerController.UnregisterTriggerListener(trigger);
        }
        triggerHandlers.Clear();
    }
}
