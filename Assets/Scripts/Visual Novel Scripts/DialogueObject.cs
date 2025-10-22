using UnityEngine;
using System.Collections.Generic;

public enum VNEventType
{
    None,
    FadeAndTeleport,
    SurprisedRival,
    CustomEvent
}

[CreateAssetMenu(fileName = "NewDialogue", menuName = "VisualNovel/Dialogue")]
public class DialogueObject : ScriptableObject
{
    public List<DialogueLine> lines;
    
    [Header("Next Dialogue Options")]
    public bool continueToNextDialogue;
    public DialogueObject nextDialogue;
    
    [Header("Event Options")]
    public VNEventType eventType = VNEventType.None;
}
