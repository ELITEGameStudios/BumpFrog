using UnityEngine.Audio;
using UnityEngine;
using UnityEngine.UI;
using System;

public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;
    
    public static AudioManager instance;
    
    [SerializeField] Slider volumeSlider;
    
    void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;       
        }
        
        DontDestroyOnLoad(gameObject);
        
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }
    }

    void Start()
    {
        Play("Main Theme");
    }

    public void Play(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }

        if (PauseMenu.GameIsPaused)
        {
            s.source.pitch *= 0.5f;
        }
        
        s.source.Play();
    }

    public void SetVolume()
    {
        if (volumeSlider == null) return;
        AudioListener.volume = volumeSlider.value;
    }
}
