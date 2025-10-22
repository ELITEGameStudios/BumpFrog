using UnityEngine;
using System.Collections.Generic;

public enum VNEventType
{
    None,
    FadeAndTeleport,
    RivalCaught,
    Fade,
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

    [Tooltip("If FadeAndTeleport, select which camera ID to switch to (matches VNEventManager camera list).")]
    public int targetCameraID; // <-- new int ID instead of Camera
}

