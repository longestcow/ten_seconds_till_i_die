using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.SceneManagement; 

public class SFXManager : MonoBehaviour
{
    public static SFXManager instance;

    public AudioSource sfxObject;
    public AudioClip[] music; // 0 menu, 1 main, 2 pause, 3 boss
    public AudioClip[] sfx; // 0 jump, 1 hit, 2 death, 3 pickup, 4 shoot, 5 timer
    public float sfxVol = 0.36f, musicVol = 0.1f, sensitivity = 1f;
    public AudioSource currentMusic = null;
    public int currentMusicIndex = 1, prevMusicIndex = 0;
    [HideInInspector] public int fov = 70;

    public void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(this.gameObject);
        if (instance == null)
            instance = this;
        
        
    }


    public void playSFX(int id, Transform spawnpoint, float volume, float pitch = 1f)
    {
        AudioSource source = Instantiate(sfxObject, spawnpoint.position, Quaternion.identity);
        source.clip = sfx[id];
        source.volume = sfxVol / 1.7f;
        source.pitch = pitch;
        source.Play();
        Destroy(source.gameObject, source.clip.length);
    }

    public void playRandomSFX(AudioClip[] clips, Transform spawnpoint)
    {
        int rand = Random.Range(0, clips.Length);
        AudioSource source = Instantiate(sfxObject, spawnpoint.position, Quaternion.identity);
        source.clip = clips[rand];
        source.volume = sfxVol / 1.7f;
        source.Play();
        Destroy(source.gameObject, source.clip.length);
    }

    public void changeMusic(int id, Transform spawnpoint)
    {
        float time = 0;
        if (currentMusic != null)
        {
            if ((currentMusicIndex == 0 && id == 1) || (currentMusicIndex == 1 && id == 0))
                time = currentMusic.time;

            Destroy(currentMusic);
            currentMusic = null;
        }
        prevMusicIndex = currentMusicIndex;
        currentMusic = Instantiate(sfxObject, spawnpoint.position, Quaternion.identity);
        currentMusic.clip = music[id];
        currentMusic.volume = musicVol;
        currentMusic.loop = true;
        currentMusic.Play();
        currentMusic.time = time;
        currentMusicIndex = id;

    }


    public void fadeOut()
    {
        StartCoroutine(FadeOutCor());
    }

    IEnumerator FadeOutCor()
    {
        float startVolume = currentMusic.volume;
        float targetVolume = 0f;

        float timer = 0f;
        while (timer < 0.8f)
        {
            timer += Time.deltaTime;
            currentMusic.volume = Mathf.Lerp(startVolume, targetVolume, timer / 0.8f);
            yield return null; // Wait for the next frame
        }
        currentMusic.volume = targetVolume; // Ensure volume is exactly 0 at the end
        currentMusic.Stop(); // Stop the audio after fading out completely
    }

    public void fadeIn()
    {
        StartCoroutine(FadeInCor());
    }

    IEnumerator FadeInCor()
    {
        float startVolume = 0f;
        float targetVolume = musicVol;
        currentMusic.volume = startVolume;
        currentMusic.Play();

        float timer = 0f;
        while (timer < 0.8f)
        {
            timer += Time.deltaTime;
            currentMusic.volume = Mathf.Lerp(startVolume, targetVolume, timer / 0.8f);
            yield return null; // Wait for the next frame
        }
        currentMusic.volume = targetVolume;
    }


}
