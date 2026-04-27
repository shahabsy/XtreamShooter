using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;


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
    public EntityTracker entityTracker;

    private enum MissionPhase { Intro, Gameplay, BossIntro, BossFight, Outro, Completed }

    private MissionPhase currentPhase = MissionPhase.Intro;
    private int actionIndex = -1;
    private bool missionComplete = false;
    private string pendingWaveId = null;
    private bool isLoadingNext = false;

    private Dictionary<string, Action<string>> triggerHandlers = new Dictionary<string, Action<string>>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (currentMission == null) { Debug.LogError("No mission assigned."); return; }
        RefreshReferences();
        StartMission();
    }
    private void RefreshReferences()
    {
        if (backgroundSpawner == null) backgroundSpawner = FindAnyObjectByType<BackgroundSpawner>();
        if (enemySpawner == null) enemySpawner = FindAnyObjectByType<EnemySpawner>();
        if (dialogManager == null) dialogManager = FindAnyObjectByType<DialogManager>();
        if (playerController == null) playerController = FindAnyObjectByType<PlayerController>();
        if (bossEncounter == null) bossEncounter = FindAnyObjectByType<BossEncounterController>();
        if (entityTracker == null) entityTracker = EntityTracker.Instance;
    }

    private void StartMission()
    {
        Debug.Log($"Starting mission: {currentMission.missionName} ID: {currentMission.missionId}");
        // Play music

        // Begin intro phase
        StopAllCoroutines();
        CancelInvoke();
        ClearAllTriggerListeners();

        EntityTracker.Instance?.ResetTracker();
        enemySpawner.ResetSpawner();
        backgroundSpawner.ResetSpawner();
        playerController.ResetPlayer();

        if (backgroundSpawner != null && currentMission.tileSet != null)
        {
            //Debug.Log($"Initializing background spawner with tile set: {currentMission.tileSet.name}");
            backgroundSpawner.Initialize(currentMission.tileSet);
            backgroundSpawner.SetScrolling(true);
            backgroundSpawner.SetScrollingMultiplier(1);
        }

        currentPhase = MissionPhase.Intro;
        actionIndex = -1;
        missionComplete = false;
        pendingWaveId = null;
        isLoadingNext = false;

        StartCoroutine(RunMissionFlow());
    }
    private IEnumerator RunMissionFlow()
    {
        // Wait one frame to ensure all obects are setled
        yield return null;
        while(!missionComplete)
        {
            StageAction[] actions = GetCurrentPhaseActions();
            if(actions == null || actions.Length == 0)
            {
                // No actions in this phase , move to next phase
                AdvanceToNextPhase();
                continue;
            }

            // Execute all action in the current phase sequentially
            for (actionIndex = 0; actionIndex < actions.Length; actionIndex++)
            {
                if (missionComplete) yield break;

                StageAction action = actions[actionIndex];
                if(action.delayTime > 0)
                {
                    yield return new WaitForSeconds(action.delayTime);
                }
                yield return ExecuteAction(action);
            }
            // All actions in this phase complted , move to next phase
            AdvanceToNextPhase();
        }
    }
    private IEnumerator ExecuteAction(StageAction action)
    {
        switch (action.type)
        {
            case StageAction.ActionType.Idle:
                Debug.Log($"[Action] Idle {action.delayTime}s");
                break;
            case StageAction.ActionType.Dialog:
                Debug.Log("[Action] Dialog");
                yield return ExecuteDialog(action);
                break;
            // -------------------------------------
            // Normal And Elite Wave
            // ------------------------------------
            case StageAction.ActionType.SpawnEnemy:
                Debug.Log($"[Action] SpawnEnemy -> {action.spawnId}");
                pendingWaveId = enemySpawner.SpawnWaveAndReturnId(action.spawnId);
                break;
            case StageAction.ActionType.SpawnEliteEnemy:
                Debug.Log($"[Action] SpawnEliteEnemy -> {action.spawnId}");
                pendingWaveId = enemySpawner.SpawnEliteWaveAndReturnId(action.spawnId);
                break;
            case StageAction.ActionType.WaitUntilWaveDead:
            case StageAction.ActionType.WaitUntilEliteWaveDead:
                Debug.Log($"[Action] Waiting for wave {pendingWaveId}");
                if (!string.IsNullOrEmpty(pendingWaveId))
                    yield return WaitForWaveClear(pendingWaveId);
                pendingWaveId = null;
                break;
            // -------------------------------------
            // Boss
            // ------------------------------------
            case StageAction.ActionType.StartBossEncounter:
                Debug.Log($"[Action] StartBossEncounter -> {action.spawnId}");
                if (bossEncounter != null)
                    yield return bossEncounter.StartEncounter(action.spawnId);
                else
                    Debug.Log("BossEnounterController: missing");
                break;
            // -------------------------------------
            // Environment
            // ------------------------------------
            case StageAction.ActionType.FreezeScrolling:
                Debug.Log($"[Action] FreenzScrolling");
                SetScrolling(false);
                break;
            case StageAction.ActionType.UnfreezeScrolling:
                Debug.Log($"[Action] UnfreezeScrolling");
                SetScrolling(true);
                break;
            case StageAction.ActionType.SetScrollSpeedMultiplier:
                Debug.Log($"[Action] SetScrollSpeedMltiplier -> {action.scrollSpeedMultiplier}");
                backgroundSpawner?.SetScrollingMultiplier(action.scrollSpeedMultiplier);
                break;
            // -------------------------------------
            // Flow Control
            // ------------------------------------
            case StageAction.ActionType.WaitForPlayerTrigger:
                Debug.Log($"[Action] WaitForPlayerTrigger -> {action.completeTrigger}");
                yield return WaitForPlayerTrigger(action.completeTrigger);
                break;
            case StageAction.ActionType.CompleteMission:
                Debug.Log($"[Action] CompleteMission");
                MissionComplete();
                break;
        }
    }
    private IEnumerator WaitForWaveClear(string waveId)
    {
        if (string.IsNullOrEmpty(waveId)) yield break;
        if (EntityTracker.Instance == null) yield break;

        while (enemySpawner.IsWaveSpawning(waveId) || EntityTracker.Instance.HasEnemiesInWave(waveId))
        {
            yield return null;
        }
        Debug.Log($"Wave {waveId} fully cleared.");
    }
    private IEnumerator WaitForPlayerTrigger(string triggerName)
    {
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
    private void AdvanceToNextPhase()
    {
        Debug.Log($"Advancing from phase {currentPhase}");
        switch (currentPhase)
        {
            case MissionPhase.Intro:
                currentPhase = MissionPhase.Gameplay;
                break;
            case MissionPhase.Gameplay:
                currentPhase = MissionPhase.BossIntro;
                break;
            case MissionPhase.BossIntro:
                currentPhase = MissionPhase.BossFight;
                break;
            case MissionPhase.BossFight:
                currentPhase = MissionPhase.Outro;
                break;
            case MissionPhase.Outro:
                MissionComplete();
                break;
            default:
                break;
        }
        Debug.Log($"New phase: {currentPhase}");
        actionIndex = -1;
    }
    private void SetScrolling(bool enabled)
    {
        if (backgroundSpawner != null) backgroundSpawner?.SetScrolling(enabled);
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

    private void MissionComplete()
    {
        if (missionComplete || isLoadingNext) return;
        missionComplete = true;
        isLoadingNext = true;
        
        Debug.Log($"Mission: {currentMission.missionName} completed.");

        //Cleanup
        StopAllCoroutines();
        CancelInvoke();
        ClearAllTriggerListeners();
        
        enemySpawner?.ResetSpawner();
        backgroundSpawner.ResetSpawner();
        EntityTracker.Instance?.ClearAllEntities();
        EntityTracker.Instance?.ResetTracker();
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
        Debug.Log($"Loading next mission: {nextMission.missionName}");
        currentMission = nextMission;
        RefreshReferences();
        isLoadingNext = false;
        StartMission();
    }

    private void OnDestroy()
    {
        ClearAllTriggerListeners();
    }

    
}
