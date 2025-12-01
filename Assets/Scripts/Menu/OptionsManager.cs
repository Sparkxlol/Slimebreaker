using System;
using UnityEngine;

/// <summary>
/// Might move this to multiple Managers, ex. AudioManager, VideoManager 
/// Might move to a GameManager rather than options canvas.
/// </summary>
public class OptionsManager : MonoBehaviour
{
    public static OptionsManager instance;

    [Header("Video")]
    private const string FULLSCREEN_KEY = "FullscreenOption";
    private const string WINDOWED_FULLSCREEN_KEY = "WindowedOption";

    private bool fullscreen = true;
    private bool windowedFullscreen = false;

    public bool Fullscreen
    {
        get => fullscreen;
        set
        {
            Screen.fullScreen = value;
            fullscreen = value;
        }
    }

    public bool WindowedFullscreen
    {
        get => windowedFullscreen;
        set
        {
            Screen.fullScreen = value;
            windowedFullscreen = value;
        }
    }

    [Header("Audio")]
    private const string MASTER_VOLUME_KEY = "MasterVolumeOption";
    private const string MUSIC_VOLUME_KEY = "MusicVolumeOption";
    private const string SFX_VOLUME_KEY = "SFXVolumeOption";
    private const string VOICE_VOLUME_KEY = "VoiceVolumeOption";

    private float masterVolumePercentage = 1.0f;
    private float musicVolumePercentage = 1.0f;
    private float sfxVolumePercentage = 1.0f;
    private float voiceVolumePercentage = 1.0f;

    public static event Action MasterVolumeChanged;
    public static event Action MusicVolumeChanged;
    public static event Action SFXVolumeChanged;
    public static event Action VoiceVolumeChanged;

    public float MasterVolumePercentage
    {
        get => masterVolumePercentage;
        set
        {
            masterVolumePercentage = value;
            MasterVolumeChanged?.Invoke();
        }
    }

    public float MusicVolumePercentage
    {
        get => musicVolumePercentage;
        set
        {
            musicVolumePercentage = value;
            MusicVolumeChanged?.Invoke();
        }
    }

    public float SFXVolumePercentage
    {
        get => sfxVolumePercentage;
        set
        {
            sfxVolumePercentage = value;
            SFXVolumeChanged?.Invoke();
        }
    }

    public float VoiceVolumePercentage
    {
        get => voiceVolumePercentage;
        set
        {
            voiceVolumePercentage = value;
            VoiceVolumeChanged?.Invoke();
        }
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }

        LoadOptions();
    }

    private void LoadOptions()
    {
        // Video Settings
        Fullscreen = PlayerPrefs.GetInt(FULLSCREEN_KEY, 1) == 1;
        WindowedFullscreen = PlayerPrefs.GetInt(WINDOWED_FULLSCREEN_KEY, 0) == 1; ;

        // Audio Settings
        MasterVolumePercentage = PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, 1);
        MusicVolumePercentage = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 1);
        SFXVolumePercentage = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1);
        VoiceVolumePercentage = PlayerPrefs.GetFloat(VOICE_VOLUME_KEY, 1);
    }

    public void SaveSettings()
    {
        // Video Settings
        PlayerPrefs.SetInt(FULLSCREEN_KEY, Fullscreen ? 1 : 0);
        PlayerPrefs.SetInt(WINDOWED_FULLSCREEN_KEY, WindowedFullscreen ? 1 : 0);

        // Audio Settings
        PlayerPrefs.SetFloat(MASTER_VOLUME_KEY, MasterVolumePercentage);
        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, MusicVolumePercentage);
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, SFXVolumePercentage);
        PlayerPrefs.SetFloat(VOICE_VOLUME_KEY, VoiceVolumePercentage);

        PlayerPrefs.Save();
    }
}
