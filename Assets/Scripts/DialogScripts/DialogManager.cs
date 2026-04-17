using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DialogManager : MonoBehaviour
{
    public GameObject dialogPanel;
    public Text dialogText;
    public Text characterNameText;
    public Image portraitImage;
    public Button skipButton;
    public Button continueButton;

    private StageAction currentAction;
    private System.Action onComplete;

    public void ShowDialog(StageAction action, System.Action callback)
    {
        currentAction = action;
        onComplete = callback;

        dialogText.text = action.text;
        characterNameText.text = action.characterName;
        if (!string.IsNullOrEmpty(action.characterPortrait))
        {
            portraitImage.sprite = Resources.Load<Sprite>(action.characterPortrait);
        }
        dialogPanel.SetActive(true);

        // Clear previous listners
        if (continueButton != null) continueButton.onClick.RemoveAllListeners();
        if(skipButton != null ) skipButton.onClick.RemoveAllListeners();

        if (action.skippable)
        {
            continueButton.gameObject.SetActive(true);
            skipButton.gameObject.SetActive(false);
            continueButton.onClick.AddListener(CloseDialog);
        }
        else
        {
            continueButton.gameObject.SetActive(false);
            skipButton.gameObject.SetActive(true);
            float duration = action.floatValue > 0 ? action.floatValue : 3f;
            StartCoroutine(AutoAdvance(duration));
        }
        
        if (skipButton != null)
        {
            skipButton.onClick.AddListener(CloseDialog);
        }
    }

    private IEnumerator AutoAdvance(float duration)
    {
        yield return new WaitForSeconds(duration);
        CloseDialog();
    }

    private void CloseDialog()
    {
        dialogPanel.SetActive(false);
        onComplete?.Invoke();
    }
}