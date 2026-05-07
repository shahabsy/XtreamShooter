using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using System;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    [Header("Game Over Panel")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverMessageText;
    public Button restartButton;
    public Button quitButton;

    [Header("Mission UI")]
    public TextMeshProUGUI missionNameText;
    public TextMeshProUGUI timerText;
    private float currentTime = 0f;

    private bool isTiming = false;

    [Header("Boss UI")]
    public GameObject bossHealthBarPanel;
    public Image bossHealthBarFill;
    public Text bossNameText;
    public float autoHideDelay = 2f;

    private Coroutine hideCoroutine;

    [Header("Player UI")]
    public Slider healthSlider;
    public Slider shieldSlider;
    public Slider energySlider;

    private Action onRestartCallback;
    private Action onQuitCallback;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if(gameOverPanel != null) gameOverPanel.SetActive(false);
        if (restartButton != null) restartButton.onClick.AddListener(OnRestartClicked);
        if (quitButton != null) quitButton.onClick.AddListener(OnQuitClicked);
    }
    public void ShowGameOverDialog(string message, Action onRestart, Action onQuit)
    {
        if (gameOverPanel == null) return;
        onRestartCallback = onRestart;
        onQuitCallback = onQuit;
        if (gameOverMessageText != null) gameOverMessageText.text = message;
        gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }
    private void OnRestartClicked()
    {
        Time.timeScale = 1f;
        onRestartCallback?.Invoke();
        gameOverPanel.SetActive(false);
    }
    private void OnQuitClicked()
    {
        Time.timeScale = 1f;
        onQuitCallback?.Invoke();
        gameOverPanel.SetActive(false);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(bossHealthBarPanel != null) bossHealthBarPanel.SetActive(false);
    }
    private void Update()
    {
        if(isTiming)
        {
            currentTime += Time.deltaTime;
            UpdateTimerDisplay();
        }
    }

    public void StartTimer()
    {
        currentTime = 0f;
        isTiming = true;
        UpdateTimerDisplay();
        Debug.Log("Timer started.");
    }
    public void StopTimer()
    {
        isTiming = false;
        Debug.Log($"Timer stopped. Final time: {FormatTime(currentTime)}");
    }

    public void ResetTimer()
    {
        currentTime = 0f;
        UpdateTimerDisplay();
        if (isTiming) return;
    }
    private void UpdateTimerDisplay()
    {
        if (timerText != null) timerText.text = FormatTime(currentTime);
    }
    private string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        int milliseconds = Mathf.FloorToInt((time * 100f) % 100f);
        return $"{minutes:00}:{seconds:00}:{milliseconds:00}";
    }

    public void SetMissionName(string name)
    {
        if(missionNameText != null) missionNameText.text = name;
    }

    public void ShowBossUI(string bossName)
    {
        if (bossHealthBarPanel == null) return;

        if(hideCoroutine != null) StopCoroutine(hideCoroutine);
        bossHealthBarPanel.SetActive(true);

        if(bossNameText != null)
            bossNameText.text = bossName;

        SetBossHealth(1f);// need to update this when boss takes damage, currently it will be set to full health when shown
    }

    public void SetBossHealth(float normalizedHealth)
    {
        if (bossHealthBarFill != null)
            bossHealthBarFill.fillAmount = Mathf.Clamp01(normalizedHealth);
    }

    public void HideBossUIAfterDelay(float delay = -1f)
    {
        if (delay < 0) delay = autoHideDelay;
        if(hideCoroutine != null) StopCoroutine(hideCoroutine);
        hideCoroutine = StartCoroutine(HideAfterDelay(delay));
    }

    private IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (bossHealthBarPanel != null)
            bossHealthBarPanel.SetActive(false);
    }

    public void UpdatePlayerHealth(float current, float max)
    {
        if (healthSlider != null) healthSlider.value = current / max;
    }

    public void UpdatePlayerShield(float current, float max)
    {
        if(shieldSlider != null) shieldSlider.value = current / max;
    }

    public void UpdatePlayerEnergy(float current, float max)
    {
        if (energySlider != null) energySlider.value = current / max;
    }
}
