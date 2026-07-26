using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.SceneManagement; 


public class UIManager : MonoBehaviour
{
    Button[] allButtons;
    public GameObject mainUI, optionsUI;
    public TextMeshProUGUI musText, sfxText, fovText, sensText;
    public Slider musSlider, sfxSlider, fovSlider, sensSlider;
    void Start()
    {
        SFXManager.instance.changeMusic(0, transform);
        allButtons = Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Button button in allButtons){
            EventTrigger trigger = button.gameObject.AddComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerEnter;
            entry.callback.AddListener((data) => { OnButtonHovered(); });
            trigger.triggers.Add(entry);
        }
        musicVolChange(SFXManager.instance.musicVol);
        sfxVolChange(SFXManager.instance.sfxVol);
        fovChange(SFXManager.instance.fov);
        sensChange(SFXManager.instance.sensitivity);
    }

    void OnButtonHovered()
    {
        SFXManager.instance.playSFX(8, transform, 1f);
    }

    public void playb()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Playground");
    }
    public void optionsb()
    {
        mainUI.SetActive(false);
        optionsUI.SetActive(true);
    }
    public void backb()
    {
        mainUI.SetActive(true);
        optionsUI.SetActive(false);
    }
    public void exitb()
    {
        Application.Quit();
    }

    public void musicVolChange(System.Single value)
    {
        SFXManager.instance.musicVol = value;
        musSlider.value=value;
        if (SFXManager.instance.currentMusic != null)
            SFXManager.instance.currentMusic.volume = value;
        musText.text = ((int)((value/0.4f)*100))+"%";

    }

    public void sfxVolChange(System.Single value)
    {
       SFXManager.instance.sfxVol = value;
       sfxSlider.value=value;
       sfxText.text = ((int)((value/0.4f)*100))+"%";
    }
    public void fovChange(System.Single value)
    {
        fovSlider.value=(int)value;
        SFXManager.instance.fov = (int)value;
        fovText.text = (int)value+"";
    }
    public void sensChange(System.Single value)
    {
        sensSlider.value=value;
        SFXManager.instance.sensitivity = value;
        sensText.text = Mathf.Round(value*100f)+"%";
    }

 
}
