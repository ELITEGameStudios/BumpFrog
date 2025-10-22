using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class CinematicSystem : MonoBehaviour
{
    [SerializeField] private CinemaScene[] entries;
    [SerializeField] private CinemaScene[] winEntries, loseEntries, nedEntries;
    [SerializeField] private Image bg;
    [SerializeField] private DialogueStringScript script;
    [SerializeField] private Color bgColor;
    [SerializeField] private float timer, bgFadeTime, pauseSeconds = 2, videoTime = 50;
    [SerializeField] public static CinematicSystem instance { get; private set; }
    [SerializeField] public bool inProgress;


    public void Awake()
    {
        if (instance == null) { instance = this; }
        else if (instance != this) { Destroy(this); }
    }


    public void BeginSequence(){
        StartCoroutine(IntroCinematicCoroutine());
    }

    public void BeginWinSequence(){
        StartCoroutine(EndGameCoroutine(true));
    }

    public void BeginLoseSequence(){
        StartCoroutine(EndGameCoroutine(false));
    }
    
    public void BeginNedSequence(){
        StartCoroutine(NedCinematicCoroutine());
    }


    IEnumerator IntroCinematicCoroutine()
    {

        inProgress = true;


        timer = 1;
        // while (timer < bgFadeTime)
        // {
        //     bg.color = Color.Lerp(Color.clear, bgColor, timer / bgFadeTime);
        //     timer += Time.unscaledDeltaTime;
        //     yield return null;
        // }
        
        bg.color = bgColor;

        yield return new WaitForSeconds(pauseSeconds);

        foreach (CinemaScene entry in entries)
        {

            yield return StartCoroutine(FadeInPanelCoroutine(entry));
            while (!Input.GetKeyDown(KeyCode.D)) { yield return null; } //while D is not pressed, don't continue slides
            yield return StartCoroutine(FadeOutPanelCoroutine(entry));
        }

        script.BeginDialogueTree(Dialogue.introTrees[0]);
        while (script.inTree) { yield return null; }

        while (timer > 0)
        {
            bg.color = Color.Lerp(Color.clear, bgColor, timer / bgFadeTime);
            timer -= Time.unscaledDeltaTime;
            yield return null;
        }

        bg.color = Color.clear;

        script.BeginDialogueTree(Dialogue.introTrees[1]);
        while (script.inTree) { yield return null; }

        inProgress = false;
        GameManager.instance.gameState = GameManager.GameState.PRERALLY;

    }

    IEnumerator NedCinematicCoroutine() {

        inProgress = true;
        script.BeginDialogueTree(Dialogue.outroTrees[2]);
        while (script.inTree) { yield return null; }

        timer = 0;

        while (timer < bgFadeTime) {
            bg.color = Color.Lerp(Color.clear, bgColor, timer / bgFadeTime);
            timer += Time.unscaledDeltaTime;
            yield return null;
        }
        bg.color = bgColor;

        yield return new WaitForSeconds(pauseSeconds);

        foreach (CinemaScene entry in nedEntries)
        {

            yield return StartCoroutine(FadeInPanelCoroutine(entry));
            while (!Input.GetKeyDown(KeyCode.D)) { yield return null; } //While D is down, progress comic
            yield return StartCoroutine(FadeOutPanelCoroutine(entry));
        }

        inProgress = false;
        SceneManager.LoadScene(0);
        // GameManager.instance.gameState = GameManager.GameState.PRERALLY;

    }
    
    IEnumerator EndGameCoroutine(bool won){

        inProgress = true;
        CinemaScene[] targetEntries = won ? winEntries : loseEntries;

        timer = 0;

        while (timer < bgFadeTime){
            bg.color = Color.Lerp(Color.clear, bgColor, timer / bgFadeTime);
            timer += Time.unscaledDeltaTime;
            yield return null;
        }
        bg.color = bgColor;

        // yield return new WaitForSeconds(pauseSeconds);
        
        script.BeginDialogueTree(Dialogue.outroTrees[won?0:1]);
        while (script.inTree) { yield return null; }

        foreach (CinemaScene entry in targetEntries)
        {
            yield return StartCoroutine(FadeInPanelCoroutine(entry));
            while (!Input.GetKeyDown(KeyCode.D)) { yield return null; } //D Key
            yield return StartCoroutine(FadeOutPanelCoroutine(entry));
        }

        inProgress = false;
        SceneManager.LoadScene(0);
        // GameManager.instance.gameState = GameManager.GameState.PRERALLY;

    }

    IEnumerator FadeInPanelCoroutine(CinemaScene entry)
    {

        timer = 0;
        while (timer < entry.fade)
        {
            // text.color = Color.Lerp(Color.clear, entry.color, timer / entry.fade);
            foreach (Graphic graphic in entry.graphics) { graphic.color = Color.Lerp(Color.clear, Color.white, timer / entry.fade); }

            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        foreach (Graphic graphic in entry.graphics) { graphic.color = Color.white; }
    }
    
    IEnumerator FadeOutPanelCoroutine(CinemaScene entry){

        while (timer > 0)
        {
            // text.color = Color.Lerp( Color.clear, entry.color, timer / entry.fade);
            foreach (Graphic graphic in entry.graphics) { graphic.color = Color.Lerp(Color.clear, entry.color, timer / entry.fade); }
            
            timer -= Time.unscaledDeltaTime;
            yield return null;
        }
        
        foreach (Graphic graphic in entry.graphics) { graphic.color = Color.clear; }
    }
}


[System.Serializable]
public class CinemaScene{
    public float fade;
    public Graphic[] graphics;
    public Color color;
    // public DialogueTree tree;
}
