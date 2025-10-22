using UnityEngine;

[System.Serializable]
public class DialogueChoice
{
    public string choiceText;
    public DialogueObject nextDialogue; // The dialogue branch this choice leads to
}