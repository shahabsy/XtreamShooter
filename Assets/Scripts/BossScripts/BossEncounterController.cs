using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class BossEncounterController : MonoBehaviour
{
    [Header("Arena Controls")]
    [SerializeField] private bool freezeScrolling = true;
    [SerializeField] private bool freezePlayer = true;

    [Header("Timing")]
    [SerializeField] private float introDelay = 1f;
    [SerializeField] private float outroDelay = 1f;

    [Header("UI")]
    [SerializeField] private bool showBossUI = true;

    public System.Action OnEncounterStart;
    public System.Action OnEncounterEnd;
    public System.Action<int> OnPhaseChanged;

    private bool isRunning = false;

    public IEnumerator StartEncounter(string bossId)
    {
        if (isRunning) yield break;
        isRunning = true;

        OnEncounterStart?.Invoke();
        OnPhaseChanged?.Invoke(0);// Phase 0 = Intro

        if (freezeScrolling) BackgroundSpawner.Instance?.SetScrolling(false);

        if (freezePlayer) PlayerController.Instance?.FreezeMovement(0.5f);

        if (showBossUI)
        {
            string bossNmae = GetBossName(bossId);
            UIManager.Instance?.ShowBossUI(bossNmae);
        }

        yield return new WaitForSeconds(introDelay);
        OnPhaseChanged?.Invoke(1); // Phase 1 = Boss Spawned

        yield return SpawnAndWaitForBoss(bossId);
        
        OnPhaseChanged?.Invoke(2); // Phase 2 = Boss Active - fight phase
        
        yield return new WaitForSeconds(outroDelay);

        if (showBossUI) UIManager.Instance?.HideBossUIAfterDelay();

        if (freezeScrolling) BackgroundSpawner.Instance?.SetScrolling(true);

        OnEncounterEnd?.Invoke();
        isRunning = false;
    }
    private IEnumerator SpawnAndWaitForBoss(string bossId)
    {
        var spawner = EnemySpawner.Instance;
        if (spawner == null) yield break;

        yield return spawner.SpawnBossRoutine(bossId);

        while (EntityTracker.Instance != null && EntityTracker.Instance.HasBoss)
            yield return null;
    }
    private void BindBossHealthEvent(Boss boss)
    {
        if (boss == null || UIManager.Instance == null) return;

        boss.OnHealthChanged -= UIManager.Instance.SetBossHealth;
        boss.OnHealthChanged += UIManager.Instance.SetBossHealth;
    }

    private string GetBossName(string bossId)
    {
        WaveDatabase database = EnemySpawner.Instance?.waveDatabase;
        if (database == null) return "Unknown Boss";

        BossData data = database.GetBossData(bossId);
        return data != null ? data.bossName : "Unknown Boss";
    }

    public bool IsRunning => isRunning;
}
