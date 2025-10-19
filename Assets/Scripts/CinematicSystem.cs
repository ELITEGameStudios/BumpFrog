using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class CinematicSystem : MonoBehaviour
{
    [SerializeField] private CinemaScene[] entries;
    [SerializeField] private Image bg, whiteBg;
    [SerializeField] private RawImage vidtex;
    [SerializeField] private VideoPlayer v_player;
    [SerializeField] private Color bgColor;
    [SerializeField] private TMP_Text text;
    [SerializeField] private float timer, bgFadeTime, pauseSeconds = 2, videoTime = 50;

    // Update is called once per frame
    public void BeginSequence(){
        StartCoroutine(CinematicCoroutine());
    }

    IEnumerator CinematicCoroutine(){


        timer = 0;

        while (timer < bgFadeTime){
            bg.color = Color.Lerp(Color.clear, bgColor, timer / bgFadeTime);
            timer += Time.deltaTime;
            yield return null;
        }
        bg.color = bgColor;

        yield return new WaitForSeconds(pauseSeconds);

        foreach (CinemaScene entry in entries)
        {

            text.text = entry.phrase;
            timer = 0;
            while (timer < entry.fade)
            {
                text.color = Color.Lerp(Color.clear, entry.color, timer / entry.fade);
                foreach (Graphic graphic in entry.graphics) { graphic.color = Color.Lerp(Color.clear, entry.color, timer / entry.fade); }
                
                
                timer += Time.deltaTime;
                yield return null;
            }

            text.color = entry.color;
            yield return new WaitForSeconds(entry.hold);

            while (timer > 0)
            {
                text.color = Color.Lerp( Color.clear, entry.color, timer / entry.fade);
                foreach (Graphic graphic in entry.graphics) { graphic.color = Color.Lerp(Color.clear, entry.color, timer / entry.fade); }
                
                timer -= Time.deltaTime;
                yield return null;
            }
            text.color = Color.clear;
        }
        
        // Application.Quit();
        // load game
    }
}


[System.Serializable]
public class CinemaScene{
    public float fade;
    public Graphic[] graphics;
    public float hold;
    public string phrase;
    public Color color;
}
