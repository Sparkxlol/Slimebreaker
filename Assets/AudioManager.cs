using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Soundtracks")]
    [SerializeField] private List<AudioClip> levelSoundtracks;
    [SerializeField] private int level = 0;

    private AudioSource activeSoundtrack;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void Start()
    {
        activeSoundtrack = GetComponent<AudioSource>();
        activeSoundtrack.volume = OptionsManager.instance.MusicVolumePercentage;

        activeSoundtrack.clip = levelSoundtracks[level];
        activeSoundtrack.Play();
    }

    private void Update()
    {
        // Event would be better here, bit advanced for current standings...
        activeSoundtrack.volume = OptionsManager.instance.MusicVolumePercentage;
    }
}
