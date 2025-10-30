using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Events;

public class VNDialogueManager : MonoBehaviour
{
    [Header("UI Refereneces")]
    public TMP_Text nameText;
    public TMP_Text dialogueText;
    public Button nextButton;
    
    [Header("Player Emoticon Positions")]
    public Image playerPos1;
    public Image playerPos2;
    public Image playerPos3;
    public Image playerPos4;

    [Header("Rival Emoticon Positions")]
    public Image rivalPos1;
    public Image rivalPos2;
    public Image rivalPos3;
    public Image rivalPos4;
    
    [Header("Audio")]
    public AudioSource audioSource;

    [Header("Data")]
    public DialogueObject dialogueObjectWin;
    public DialogueObject dialogueObjectLose;
    [Header("Game State")]
    public bool playerWon = true;
    
    [Header("Choices UI")]
    public GameObject choicePanel;
    public Button choiceButtonPrefab;
    
    private DialogueObject dialogueObject; 
    private int currentLineIndex = 0;
    private List<Button> spawnedChoices = new();
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // ✅ Choose starting dialogue based on outcome
        dialogueObject = playerWon ? dialogueObjectWin : dialogueObjectLose;
        Time.timeScale = 1;

        if (nextButton != null)
            nextButton.onClick.AddListener(DisplayNextLine);

        DisplayNextLine();
    }

    void DisplayNextLine()
    {
        ClearChoices();
        
        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();

        if (currentLineIndex < dialogueObject.lines.Count)
        {
            var line = dialogueObject.lines[currentLineIndex];
            nameText.text = line.characterName;
            dialogueText.text = line.dialogueText;

            // clear all emoticons first
            ClearAllEmoticons();

            // player
            Image playerSlot = GetPlayerSlot(line.playerPosition);
            if (playerSlot != null)
            {
                if (line.playerEmoticon != null)
                {
                    playerSlot.enabled = true;
                    playerSlot.sprite = line.playerEmoticon;
                }
            }

            // rival
            Image rivalSlot = GetRivalSlot(line.rivalPosition);
            if (rivalSlot != null)
            {
                if (line.rivalEmoticon != null)
                {
                    rivalSlot.enabled = true;
                    rivalSlot.sprite = line.rivalEmoticon;
                }
            }

            // play audio
            if (audioSource != null && line.soundEffect != null)
            {
                audioSource.clip = line.soundEffect;
                audioSource.Play();
            }

            // if this line has choices, stop and show them
            if (line.hasChoices && line.choices != null && line.choices.Length > 0)
            {
                ShowChoices(line.choices);
                return; // stop here, don’t go to next line yet
            }
            
            currentLineIndex++;
        }
        else
        {
            EndDialogue();
        }
    }
    
    void ShowChoices(DialogueChoice[] choices)
    {
        nextButton.gameObject.SetActive(false);

        foreach (var choice in choices)
        {
            var button = Instantiate(choiceButtonPrefab, choicePanel.transform);
            button.gameObject.SetActive(true);
            button.GetComponentInChildren<TMP_Text>().text = choice.choiceText;
            button.onClick.AddListener(() =>
            {
                SelectChoice(choice);
            });
            spawnedChoices.Add(button);
        }

        choicePanel.SetActive(true);
    }

    void ClearChoices()
    {
        foreach (var button in spawnedChoices)
            Destroy(button.gameObject);
        spawnedChoices.Clear();

        choicePanel.SetActive(false);
        nextButton.gameObject.SetActive(true);
    }

    void SelectChoice(DialogueChoice choice)
    {
        ClearChoices();

        if (choice.nextDialogue != null)
        {
            dialogueObject = choice.nextDialogue;
            currentLineIndex = 0;
            DisplayNextLine();
        }
        else
        {
            EndDialogue();
        }
    }
    
    void ClearAllEmoticons()
    {
        foreach (var img in new[] { playerPos1, playerPos2, playerPos3, playerPos4, rivalPos1, rivalPos2, rivalPos3, rivalPos4 })
        {
            if (img == null) continue;
            img.sprite = null;
            img.enabled = false;
        }
    }
    
    Image GetPlayerSlot(EmoticonPosition pos)
    {
        return pos switch
        {
            EmoticonPosition.Pos1 => playerPos1,
            EmoticonPosition.Pos2 => playerPos2,
            EmoticonPosition.Pos3 => playerPos3,
            EmoticonPosition.Pos4 => playerPos4,
            _ => null
        };
    }
    
    Image GetRivalSlot(EmoticonPosition pos)
    {
        return pos switch
        {
            EmoticonPosition.Pos1 => rivalPos1,
            EmoticonPosition.Pos2 => rivalPos2,
            EmoticonPosition.Pos3 => rivalPos3,
            EmoticonPosition.Pos4 => rivalPos4,
            _ => null
        };
    }

    void EndDialogue()
    {
        nameText.text = "";
        dialogueText.text = "";
        ClearAllEmoticons();

        if (audioSource != null)
            audioSource.Stop();

        if (nextButton != null)
            nextButton.interactable = false;

        // Trigger post-dialogue events if needed
        if (dialogueObject != null)
        {
            VNEventManager eventManager = FindFirstObjectByType<VNEventManager>();
            if (eventManager != null)
            {
                switch (dialogueObject.eventType)
                {
                    case VNEventType.FadeAndTeleport:
                        eventManager.FadeOutAndChangeCamera(dialogueObject.targetCameraID);
                        break;
                    case VNEventType.RivalCaught:
                        eventManager.RivalCaught();
                        break;
                    case VNEventType.Fade:
                        eventManager.FadeToBlack();
                        break;
                }
            }
        }
        
        // Continue chain if another dialogue is linked
        if (dialogueObject.continueToNextDialogue && dialogueObject.nextDialogue != null)
        {
            dialogueObject = dialogueObject.nextDialogue;
            currentLineIndex = 0;
            nextButton.interactable = true;
            DisplayNextLine();
            return;
        }
    }
}
