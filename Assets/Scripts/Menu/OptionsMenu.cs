using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    // Topper order should be decided based on location in UI from left-right.
    [Header("Toppers")]
    [SerializeField] private GameObject activeTopperHeader;
    [SerializeField] private GameObject inactiveTopperHeader;
    [SerializeField] private List<GameObject> toppers;
    [SerializeField] private int activeTopper;
    [SerializeField] private Color activeColor;
    [SerializeField] private Color inactiveColor;

    [Header("Menus")]
    [SerializeField] private List<GameObject> menus;
    [SerializeField] private GameObject optionsUI;

    [Header("Video Options")]
    [SerializeField] private TextMeshProUGUI fullscreenValue;
    [SerializeField] private TextMeshProUGUI windowedValue;

    [Header("Audio Options")]
    [SerializeField] private Slider masterVolumeValue;
    [SerializeField] private Slider musicVolumeValue;
    [SerializeField] private Slider sfxVolumeValue;
    [SerializeField] private Slider voiceVolumeValue;

    [Header("Saving")]
    private List<RevertedAction> revertedActions = new List<RevertedAction>();

    void Start()
    {
        ResetActiveToppers();
        LoadOptions();
    }

    public void ActivateOptions()
    {
        optionsUI.SetActive(true);
    }

    public void DeactivateOptions()
    {
        optionsUI.SetActive(false);
    }

    /// <summary>
    /// When adding new options:
    ///     - Add to LoadOptions and add "Changed" function.
    ///     - Create sliders/options in UI and event correct "Changed" events.
    ///     - Add properties, keys, events, etc. to OptionsManager
    ///     - Add to LoadSettings and SaveSettings in OptionsManager.
    ///     - Add connections in the corresponding class. ex. GameManager, AudioManager, etc.
    /// </summary>
    private void LoadOptions()
    {
        // Video Settings
        fullscreenValue.text = (OptionsManager.instance.Fullscreen) ? "On" : "Off";
        windowedValue.text = (OptionsManager.instance.WindowedFullscreen) ? "On" : "Off";

        // Audio Settings
        masterVolumeValue.value = OptionsManager.instance.MasterVolumePercentage;
        musicVolumeValue.value = OptionsManager.instance.MusicVolumePercentage;
        sfxVolumeValue.value = OptionsManager.instance.SFXVolumePercentage;
        voiceVolumeValue.value = OptionsManager.instance.VoiceVolumePercentage;
    }

    public void OptionsCancelled()
    {
        foreach (RevertedAction action in revertedActions)
        {
            action.actions();
        }
    }

    public void OptionsSaved()
    {
        revertedActions.Clear();
    }

    void ResetActiveToppers()
    {
        foreach (var topper in toppers)
        {
            topper.transform.SetParent(inactiveTopperHeader.transform);
            topper.GetComponent<Image>().color = inactiveColor;
        }

        toppers[activeTopper].transform.SetParent(activeTopperHeader.transform);
        toppers[activeTopper].GetComponent<Image>().color = activeColor;

        for (int i = 0; i < menus.Count; i++)
        {
            menus[i].SetActive(false);
        }
        menus[activeTopper].SetActive(true);
    }

    public void ChangeActiveTopper(int indexOfTopper)
    {
        toppers[activeTopper].transform.SetParent(inactiveTopperHeader.transform);
        toppers[activeTopper].GetComponent<Image>().color = inactiveColor;

        toppers[indexOfTopper].transform.SetParent(activeTopperHeader.transform);
        toppers[indexOfTopper].GetComponent<Image>().color = activeColor;

        menus[activeTopper].SetActive(false);
        menus[indexOfTopper].SetActive(true);

        activeTopper = indexOfTopper;
    }

    public void FullscreenClicked()
    {
        bool oldFullscreen = OptionsManager.instance.Fullscreen;
        RecordAction(() =>
        {
            OptionsManager.instance.Fullscreen = oldFullscreen;
            fullscreenValue.text = (OptionsManager.instance.Fullscreen) ? "On" : "Off";
        }, "Fullscreen");

        OptionsManager.instance.Fullscreen = !OptionsManager.instance.Fullscreen;
        fullscreenValue.text = (OptionsManager.instance.Fullscreen) ? "On" : "Off";
    }

    public void WindowedClicked()
    {
        bool oldWindowedFullscreen = OptionsManager.instance.WindowedFullscreen;
        RecordAction(() => 
        {
            OptionsManager.instance.WindowedFullscreen = oldWindowedFullscreen;
            windowedValue.text = (OptionsManager.instance.WindowedFullscreen) ? "On" : "Off";
        }, "WindowedFullscreen");

        OptionsManager.instance.WindowedFullscreen = !OptionsManager.instance.WindowedFullscreen;
        windowedValue.text = (OptionsManager.instance.WindowedFullscreen) ? "On" : "Off";
    }

    public void MasterVolumeChanged(float value)
    {
        float oldMasterVolume = OptionsManager.instance.MasterVolumePercentage;
        RecordAction(() =>
        {
            OptionsManager.instance.MasterVolumePercentage = oldMasterVolume;
            masterVolumeValue.value = oldMasterVolume;
        }, "MasterVolume");

        OptionsManager.instance.MasterVolumePercentage = masterVolumeValue.value;
    }

    public void MusicVolumeChanged(float value)
    {
        float oldMusicVolume = OptionsManager.instance.MusicVolumePercentage;
        RecordAction(() =>
        {
            OptionsManager.instance.MusicVolumePercentage = oldMusicVolume;
            musicVolumeValue.value = oldMusicVolume;
        }, "MusicVolume");

        OptionsManager.instance.MusicVolumePercentage = musicVolumeValue.value;
    }

    public void SFXVolumeChanged(float value)
    {
        float oldSFXVolume = OptionsManager.instance.SFXVolumePercentage;
        RecordAction(() =>
        {
            OptionsManager.instance.SFXVolumePercentage = oldSFXVolume;
            sfxVolumeValue.value = oldSFXVolume;
        }, "SFXVolume");

        OptionsManager.instance.SFXVolumePercentage = sfxVolumeValue.value;
    }

    public void VoiceVolumeChanged(float value)
    {
        float oldVoiceVolume = OptionsManager.instance.VoiceVolumePercentage;
        RecordAction(() =>
        {
            OptionsManager.instance.VoiceVolumePercentage = oldVoiceVolume;
            voiceVolumeValue.value = oldVoiceVolume;
        }, "VoiceVolume");

        OptionsManager.instance.VoiceVolumePercentage = voiceVolumeValue.value;
    }

    public void RecordAction(Action actions, string key)
    {
        if (revertedActions.Exists(action => action.key == key))
            return;

        revertedActions.Add(new RevertedAction(actions, key));
    }
}

public class RevertedAction
{
    public Action actions;
    public string key;

    public RevertedAction(Action actions, string key)
    {
        this.actions = actions;
        this.key = key;
    }
}
