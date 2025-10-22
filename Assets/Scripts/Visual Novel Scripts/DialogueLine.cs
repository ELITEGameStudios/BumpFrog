using UnityEngine;

public enum EmoticonPosition
{
    Pos1 = 1,
    Pos2 = 2,
    Pos3 = 3,
    Pos4 = 4
}

[System.Serializable]
public class DialogueLine
{
    public string characterName;
    public string dialogueText;
    public AudioClip soundEffect;
    
    [Header("Emoticons")]
    public Sprite playerEmoticon;
    public EmoticonPosition playerPosition = EmoticonPosition.Pos1;
    public Sprite rivalEmoticon;
    public EmoticonPosition rivalPosition = EmoticonPosition.Pos2;
    
    [Header("Branching Choices")]
    public bool hasChoices;
    public DialogueChoice[] choices;
}
