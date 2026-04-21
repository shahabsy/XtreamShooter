using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

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
