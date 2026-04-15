using UnityEngine;
using System.Collections;
using System;
using UnityEditor.Experimental.GraphView;

public class StageController : MonoBehaviour
{
    public static StageController Instance {  get; private set; }

    [Header("Stage Data")]
    public StageData currentStage;

    [Header("Reference (auto-assigned")]
    public BackgroundSpawner backgroundSpawner;
    public EnemySpawner enemySpawner;

    // state
    private StageState currentState = StageState.None;
    private float stageProgress = 0f;
    private bool scrollingEnabled = false;
    private GameObject currentBoss;
    private bool[] midTriggersFired;

    // events
    public System.Action<StageState> OnStateChanged;
    public System.Action OnBossDefeated;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (backgroundSpawner == null)
        {
            backgroundSpawner = FindAnyObjectByType<BackgroundSpawner>();
        }
        if (enemySpawner == null)
        {
            enemySpawner = FindAnyObjectByType<EnemySpawner>();
        }

        if ( currentStage == null)
        {
            Debug.LogError("StageController: No StageData assigned.");
            return;
        }

        midTriggersFired = new bool[currentStage.midStageTriggers.Length];
        StartStage();
    }

    void Update()
    {
        if (!scrollingEnabled) return;

        float delta = currentStage.baseScrollSpeed * Time.deltaTime;
        stageProgress += delta;

        // check mid stage trigger
        for (int i = 0; i < currentStage.midStageTriggers.Length; i++)
        {
            if (!midTriggersFired[i] && stageProgress >= currentStage.midStageTriggers[i])
            {
                midTriggersFired[i] = true;
                OnMindStageTrigger(i);
            }
        }

        // stage and condition
        if ( stageProgress >= currentStage.stageLength && currentState != StageState.StageEnd)
        {
            BeginBossPhase();
        }
    }

    private void StartStage()
    {
        currentState = StageState.StageStart;
        stageProgress = 0;
        scrollingEnabled = true;
        midTriggersFired = new bool[currentStage.midStageTriggers.Length];

        // Enable background scrolling and spawning
        backgroundSpawner?.SetScrolling(true);
        backgroundSpawner?.SetSpawning(true);
        backgroundSpawner?.SetScrollingMultiplier(currentStage.baseScrollSpeed);
        enemySpawner?.StartSpawning();

        OnStateChanged?.Invoke(currentState);
        Debug.Log($"Stage started: {currentStage.stageName}");
    }

    private void OnMindStageTrigger(int index)
    {
        Debug.Log($"Mid-stage trigger {index} reached at distance {currentStage.midStageTriggers[index]}");
    }

    private void BeginBossPhase()
    {
        currentState = StageState.StageEnd;
        scrollingEnabled = false;
        enemySpawner?.StopSpawning();

        OnStateChanged?.Invoke(currentState);
        Debug.Log("Stage end reached. Preparing boss...");

        StartCoroutine(SmoothStopScrolling(1f));
        StartCoroutine(BossIntroRoutine());
    }
    private IEnumerator SmoothStopScrolling(float decelerationTime)
    {
        float startSpeed = currentStage.baseScrollSpeed;
        float elapsed = 0f;
        while (elapsed < decelerationTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / decelerationTime;
            float newSpeed = Mathf.Lerp(startSpeed, 0, t);
            float multiplier = newSpeed / startSpeed;
            backgroundSpawner.SetScrollingMultiplier(multiplier);
            yield return null;
        }
        backgroundSpawner?.SetScrolling(false);
        backgroundSpawner?.SetScrollingMultiplier(1f);
    }

    //private IEnumerator SmoothTransitionScroll(float targetMultiplier, float duration)
    //{
        /*
        float startMultiplier = backgroundSpawner.GetCurrentMultiplier();
        float elapsed = 0f;
        while(elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float mult = Mathf.Lerp(startMultiplier, targetMultiplier, t);
            backgroundSpawner?.SetScrollingMultiplier(mult);
            yield return null;
        }
        backgroundSpawner?.SetScrollingMultiplier(targetMultiplier);
        */
    //}

    private IEnumerator BossIntroRoutine()
    {
        currentState = StageState.BossIntro;
        OnStateChanged?.Invoke(currentState);

        yield return new WaitForSeconds(currentStage.bossIntroDelay);

        // Spawn boss at designated position (right edge of screen)
        if (currentStage.bossPrefab != null)
        {
            //Vector3 bossSpawnPos = Camera.main.ViewportToWorldPoint(new Vector3(1.2f, 0.5f, 0));
            Vector3 bossSpawnPos = new Vector3(10f, 0, 0);
            //bossSpawnPos.z = 0;
            currentBoss = Instantiate(currentStage.bossPrefab, bossSpawnPos, Quaternion.identity);

            var bossCtrl = currentBoss.GetComponent<BossController>();
            if (bossCtrl != null)
            {
                bossCtrl.OnDefeat += OnBossDefeatedHandler;
                StartCoroutine(TestAutoDefeat(bossCtrl, 5f));
            }
            else
            {
                Debug.LogWarning("Boss prefab has no BossController component. auto-defeat will not work.");
            }
        }
        else
        {
            Debug.LogWarning("No boss prefab assigned. Skipping boss spawn");
            StartCoroutine(TransitionRoutine());
            yield break;
        }

        currentState = StageState.BossFight;
        OnStateChanged?.Invoke(currentState);
        Debug.Log("Boss fight started.");
    }

    private IEnumerator TestAutoDefeat(BossController boss, float delay)
    {
        yield return new WaitForSeconds(delay);
        Debug.Log("Test: Auto-defeating boss after 5 seconds.");
        boss.TakeDamage(9999);// assumes bosscontroller has takedamage method
    }

    private void OnBossDefeatedHandler()
    {
        currentState = StageState.BossDefeated;
        OnStateChanged?.Invoke(currentState);
        OnBossDefeated?.Invoke();
        Debug.Log("Boss Defeated.");

        StartCoroutine(TransitionRoutine());
    }

    private IEnumerator TransitionRoutine()
    {
        currentState = StageState.Transition;
        OnStateChanged?.Invoke(currentState);

        //Temporarily enable scrolling for the transition
        backgroundSpawner?.SetScrolling(true);
        // apply faster scroll multiplier
        backgroundSpawner?.SetScrollingMultiplier(currentStage.transitionScrollMultiplier);

        Debug.Log($"Transition: scrolling at {currentStage.transitionScrollMultiplier}");

        yield return new WaitForSeconds(currentStage.transitionDelay);

        // Reset multiplier back to normal
        backgroundSpawner?.SetScrollingMultiplier(1f);
        backgroundSpawner?.SetScrolling(false);

        LoadNextStage();
    }

    private void LoadNextStage()
    {
        Debug.Log("Loading next stage...");

        StartStage();
    }

    public void SetScrolling(bool enabled)
    {
        scrollingEnabled = enabled;
        backgroundSpawner?.SetScrolling(enabled);
    }

    public bool IsScrolling => scrollingEnabled;
    public StageState CurrentState => currentState;

}
