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
    private float overallAudioPercentage = 1.0f;

    public float OverallAudioPercentage
    {
        get => overallAudioPercentage;
        set
        {
            overallAudioPercentage = value;
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
            Destroy(this);
        }

        LoadOptions();
    }

    private void LoadOptions()
    {
        // Video Settings
        Fullscreen = PlayerPrefs.GetInt(FULLSCREEN_KEY, 1) == 1;
        WindowedFullscreen = PlayerPrefs.GetInt(WINDOWED_FULLSCREEN_KEY, 0) == 1; ;

        // Audio Settings
        OverallAudioPercentage = 1.0f;
    }

    public void SaveSettings()
    {
        // Video Settings
        PlayerPrefs.SetInt(FULLSCREEN_KEY, Fullscreen ? 1 : 0);
        PlayerPrefs.SetInt(WINDOWED_FULLSCREEN_KEY, WindowedFullscreen ? 1 : 0);
    }
}
