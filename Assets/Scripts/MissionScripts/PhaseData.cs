using UnityEngine;

[CreateAssetMenu(fileName = "PhaseData_", menuName = "Game/PhaseData")]
public class PhaseData : ScriptableObject
{
    public string phaseId;
    public StageAction[] actions;
    public bool autoComplete = true;
    public string completeTrigger;
}
