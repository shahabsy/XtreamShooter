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

        float autoDuration = action.floatValue > 0 ? action.floatValue : 3f;
        StartCoroutine(AutoAdvance(autoDuration));

        skipButton.onClick.RemoveAllListeners();
        skipButton.onClick.AddListener(CloseDialog);
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