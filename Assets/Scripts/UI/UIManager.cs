using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
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

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
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
}
