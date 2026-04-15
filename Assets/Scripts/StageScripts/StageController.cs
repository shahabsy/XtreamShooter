using UnityEngine;
using System.Collections;
using System;
using UnityEditor.Experimental.GraphView;
using NUnit.Framework;
using System.Collections.Generic;

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

    private enum MissionPhase
    {
        Intro,
        Gameplay,
        BossIntro,
        BossFight,
        Completed
    }

    private MissionPhase currenPhase = MissionPhase.Intro;
    private int actionIndex = -1;
    private float actionTimer = 0f;
    private bool waitingForTrigger = false;
    private string requiredTrigger;
    private List<string> activeTriggers = new List<string>();
    private bool scrollingFrozen = false;
    private bool missionComplete = false;

    public bool IsScrollingForzen => scrollingFrozen;
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

        StartMission();
    }

    private void StartMission()
    {
        // Play music
        //if (!string.IsNullOrEmpty(currentMission.music)) AudioManger.PlayMusic(currentMission.music);

        // Configure enemy spawner
        if (enemySpawner != null)
        {
            enemySpawner.SetSpawnIntervals(currentMission.quarter1Min, currentMission.quarter1Max,
                                            currentMission.quarter2Min, currentMission.quarter2Max,
                                            currentMission.quarter3Min, currentMission.quarter3Max,
                                            currentMission.quarter4Min, currentMission.quarter4Max);
        }

        // Begin intro phase
        currenPhase = MissionPhase.Intro;
        actionIndex = -1;
        NextAction();
    }

    void Update()
    {
        if (missionComplete) return;
        if (waitingForTrigger) return;
        if(actionTimer >  0)
        {
            actionTimer -= Time.deltaTime;
            if ( actionTimer <= 0)
            {
                ExecuteCurrentAction();
            }
        }
    }
    private void NextAction()
    {
        StageAction[] actions = GetCurrentPhaseActions();
        actionIndex++;
        if (actions == null || actionIndex >= actions.Length)
        {
            OnPhaseComplete();
            return;
        }

        StageAction action = actions[actionIndex];
        actionTimer = action.delayTime;
        waitingForTrigger = false;
    }

    private StageAction[] GetCurrentPhaseActions()
    {
        switch (currenPhase)
        {
            case MissionPhase.Intro: return currentMission.introActions;
            case MissionPhase.Gameplay: return currentMission.gameplayActions;
            case MissionPhase.BossIntro: return currentMission.bossActions;
            case MissionPhase.BossFight: return currentMission.bossActions;
            default: return new StageAction[0];
        }
    }
    
    private void OnPhaseComplete()
    {
        switch(currenPhase)
        {
            case MissionPhase.Intro:
                // Start gameplay phase: enable scrolling and enemy spawning
                backgroundSpawner?.SetScrolling(true);
                enemySpawner?.StartSpawning();
                currenPhase = MissionPhase.Gameplay;
                actionIndex = -1;
                NextAction();
                break;
            case MissionPhase.Gameplay:
                // Gameplay finished - trigger boss intro
                currenPhase = MissionPhase.BossIntro;
                actionIndex = -1;
                NextAction();
                break;
            case MissionPhase.BossIntro:
                currenPhase = MissionPhase.BossFight;
                actionIndex = -1;
                NextAction();
                break;
            case MissionPhase.BossFight:
                MissionComplete();
                break;
        }
    }

    private void ExecuteCurrentAction()
    {
        StageAction[] actions = GetCurrentPhaseActions();
        if (actions == null || actionIndex >= actions.Length) return;
        StageAction action = actions[actionIndex];

        switch(action.type)
        {
            case StageAction.ActionType.Idle:
                NextAction();
                break;
            case StageAction.ActionType.Dialog:
                ShowDialog(action, () => NextAction());
                break;
            case StageAction.ActionType.SpawnEnemy:
                enemySpawner?.SpawnWave(action.spawnId);
                NextAction();
                break;
            case StageAction.ActionType.SpawnEliteEnemy:
                enemySpawner.SpawnEliteWave(action.spawnId);
                NextAction();
                break;
            case StageAction.ActionType.SpawnBoss:
                SpawnBoss(action.spawnId);
                NextAction();
                break;
            case StageAction.ActionType.FreezeScrolling:
                backgroundSpawner?.SetScrolling(false);
                scrollingFrozen = true;
                NextAction();
                break;
            case StageAction.ActionType.UnfreezeScrolling:
                backgroundSpawner?.SetScrolling(true);
                scrollingFrozen = false;
                NextAction();
                break;
            case StageAction.ActionType.SetScrollSpeedMultiplier:
                backgroundSpawner?.SetScrollingMultiplier(action.floatValue);
                NextAction();
                break;
            case StageAction.ActionType.WaitUntilAllEnemiesDead:
                StartCoroutine(WaitForEnemiesDead());
                break;
            case StageAction.ActionType.WaitForPlayerTrigger:
                StartCoroutine(WaitForPlayerTrigger(action.completeTrigger));
                break;
            case StageAction.ActionType.NPCFlyIn:
                NextAction();
                break;
            case StageAction.ActionType.NPCFlyOut:
                NextAction();
                break;
            case StageAction.ActionType.CompleteMission:
                MissionComplete();
                break;
        }
    }

    private void ShowDialog(StageAction action, System.Action onComplete)
    {
        if (dialogManager != null)
        {
            dialogManager.ShowDialog(action, onComplete);
        }
        else
        {
            Debug.Log($"Dialog: {action.text}");
            onComplete?.Invoke();
        }
    }

    private void SpawnBoss(string bossId)
    {
        // Lookup boss prefab from a batabase (single resources load for demo
        GameObject bossPrefab = Resources.Load<GameObject>($"Bosses/{bossId}");
        if(bossPrefab != null)
        {
            Vector3 spawnPos = Camera.main.ViewportToWorldPoint(new Vector3(1.2f, 0.5f, 0));
            spawnPos.z = 0f;
            Instantiate(bossPrefab, spawnPos, Quaternion.identity);
        }
        else
        {
            Debug.LogError($"Boss prefab not found: {bossId}");
        }
    }

    private IEnumerator WaitForEnemiesDead()
    {
        while(enemySpawner != null && enemySpawner.ActiveEnemyCount > 0)
        {
            yield return null;
        }
        NextAction();
    }

    private IEnumerator WaitForPlayerTrigger(string triggerName)
    {
        if (playerController == null) { NextAction(); yield break; }
        bool triggered = false;
        System.Action<string> handler = (t) => { if (t == triggerName) triggered = true; };
        playerController.RegisterTriggerListener(handler);
        activeTriggers.Add(triggerName);
        while (!triggered) yield return null;
        playerController.UnregisterTriggerListener(handler);
        activeTriggers.Remove(triggerName);
        NextAction();
    }

    private void MissionComplete()
    {
        missionComplete = true;
        enemySpawner?.StopSpawning();
        backgroundSpawner?.SetScrolling(false);
        Debug.Log($"Mission {currentMission.missionName} completed!");
        // load next mission or show rewards
    }

    private void OnDestroy()
    {
        if (playerController != null)
        {
            foreach (var trigger in activeTriggers)
                playerController.UnregisterAllListeners(trigger);
        }
    }
}
