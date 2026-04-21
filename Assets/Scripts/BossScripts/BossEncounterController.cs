using System.Collections;
using UnityEngine;

public class BossEncounterController : MonoBehaviour
{
    [Header("Boss Settings")]
    [SerializeField] private string bossId;

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

    public IEnumerator StartEncounter()
    {
        if (isRunning) yield break;

        isRunning = true;
        OnEncounterStart?.Invoke();
        OnPhaseChanged?.Invoke(0);// Phase 0 = Intro

        if (freezeScrolling)
            BackgroundSpawner.Instance?.SetScrolling(false);

        if (freezePlayer) 
            PlayerController.Instance?.FreezeMovement(0.5f);

        if (showBossUI)
        {
            string bossNmae = GetBossName();
            UIManager.Instance?.ShowBossUI(bossNmae);
        }

        yield return new WaitForSeconds(introDelay);
        OnPhaseChanged?.Invoke(1); // Phase 1 = Boss Spawned

        yield return EnemySpawner.Instance.SpawnBossRoutine(bossId);

        BindBossHealthEvent();

        OnPhaseChanged?.Invoke(2); // Phase 2 = Boss Active - fight phase

        yield return WaitForBossDefeat();
        OnPhaseChanged?.Invoke(3); // Phase 3 = outro phase
        
        yield return new WaitForSeconds(outroDelay);

        if (showBossUI)
            UIManager.Instance?.HideBossUIAfterDelay();

        if (freezeScrolling)
            BackgroundSpawner.Instance?.SetScrolling(true);

        OnEncounterEnd?.Invoke();
        isRunning = false;
    }

    private IEnumerator WaitForBossDefeat()
    {
        EntityTracker tracker = EntityTracker.Instance;
        while (tracker != null && tracker.HasBoss)
        {
            yield return null;
        }
    }

    private string GetBossName()
    {
        WaveDatabase database = EnemySpawner.Instance?.waveDatabase;
        if (database == null) return "Unknown Boss";

        BossData data = database.GetBossData(bossId);
        return data != null ? data.bossName : "Unknown Boss";
    }

    private void BindBossHealthEvent()
    {
        Boss boss = FindAnyObjectByType<Boss>();
        if(boss != null && UIManager.Instance != null)
        {
            boss.OnHealthChanged -= UIManager.Instance.SetBossHealth;
            boss.OnHealthChanged += UIManager.Instance.SetBossHealth;
        }
    }

    public bool isComplete => !isRunning;
}
