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
    public UIManager uiManager;
    public EntityTracker entityTracker;

    private Coroutine missionRoutine;
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
        if (uiManager == null) uiManager = FindAnyObjectByType<UIManager>();
        if (entityTracker == null) entityTracker = EntityTracker.Instance;
    }

    private void StartMission()
    {
        Debug.Log($"Starting mission: {currentMission.missionName} ID: {currentMission.missionId}");
        // Play music

        // Begin intro phase
        if (missionRoutine != null)
            StopCoroutine(missionRoutine);
        
        StopAllCoroutines();
        CancelInvoke();
        ClearAllTriggerListeners();

        // UI Timer System
        uiManager.ResetTimer();
        uiManager.StartTimer();
        uiManager.SetMissionName(currentMission.missionName);

        entityTracker.ResetTracker();
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

        missionComplete = false;
        pendingWaveId = null;
        isLoadingNext = false;

        missionRoutine = StartCoroutine(RunMission());
    }
    private IEnumerator RunMission()
    {
        // Wait one frame to ensure all obects are setled
        yield return null;

        foreach(PhaseData phase in currentMission.phases)
        {
            if (missionComplete) yield break;
            yield return RunPhase(phase);
        }
        MissionComplete();
    }
    private IEnumerator RunPhase(PhaseData phase)
    {
        Debug.Log($"=== Starting phase: {phase.phaseId} ===");
        if(phase.actions == null || phase.actions.Length == 0 )
        {
            Debug.Log($"Phase {phase.phaseId} has no actions. skipping.");
            yield break;
        }

        // Execute all actions in the phase sequentially
        for(int i = 0; i < phase.actions.Length; i++)
        {
            if(missionComplete) yield break;

            StageAction action = phase.actions[i];
            if (action.delayTime > 0)
            {
                yield return new WaitForSeconds(action.delayTime);
            }
            yield return ExecuteAction(action);
        }
        // Handle non-auto-complete phases (wait for trigger)
        if(!phase.autoComplete && !string.IsNullOrEmpty(phase.completeTrigger))
        {
            Debug.Log($"Phase {phase.phaseId} waiting for trigger: {phase.completeTrigger}");
            yield return WaitForGlobalTrigger(phase.completeTrigger);
        }
        Debug.Log($"=== PhaseCompleted: {phase.phaseId}");
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
    private IEnumerator WaitForGlobalTrigger(string triggerName)
    {
        bool triggered = false;
        Action<string> handler = (t) => { if (t == triggerName) triggered = true; };
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

        uiManager.StopTimer();
        uiManager.SetMissionName("");

        enemySpawner?.ResetSpawner();
        backgroundSpawner.ResetSpawner();
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
