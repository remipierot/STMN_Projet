using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MusicHandler : MonoBehaviour {
    private static MusicHandler instance;

    #region Instance Audio Library
    public AudioSource menuMusic,
                       arenaMusic;

    public bool musicEnabled = true;
    public float musicVolume = 1;
    #endregion

    #region Static Audio Library
    public static AudioSource MenuMusic { get { return instance.menuMusic; } }
    public static AudioSource ArenaMusic { get { return instance.arenaMusic; } }

    public static bool MusicEnabled
    {
        get { return instance.musicEnabled; }
        set
        {
            instance.musicEnabled = value;
            if(value == false)
            {
                MenuMusic.Stop();
                ArenaMusic.Stop();
            }
        }
    }
    public static float MusicVolume { get { return instance.musicVolume; } set { instance.musicVolume = value; } }
    #endregion

    #region Unity Callbacks
    void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    void Update()
    {
        UnityEngine.SceneManagement.Scene currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (currentScene.name.Equals("MainMenu") && !MenuMusic.isPlaying && MusicEnabled)
        {
            ArenaMusic.Stop();
            PlayMenuMusic();
        }
        else if(currentScene.name.Contains("Arene") && !ArenaMusic.isPlaying && MusicEnabled)
        {
            MenuMusic.Stop();
            PlayArenaMusic();
        }
    }
    #endregion

    #region Audio Playing Methods
    public static void PlayMenuMusic()
    {
        if (instance != null && MenuMusic != null)
        {
            MenuMusic.Play();
        }
    }

    public static void PlayArenaMusic()
    {
        if (instance != null && ArenaMusic != null)
        {
            ArenaMusic.Play();
        }
    }
    #endregion

    #region Volume Control
    public void AutoUpdateMusicVolume()
    {
        GameObject MusicSliderGO = GameObject.Find("Music_Slider");
        if (MusicSliderGO != null)
        {
            Slider MusicSlider = MusicSliderGO.GetComponent<Slider>();
            if (MusicSlider != null)
            {
                SetMusicVolume(MusicSlider.value);
            }
        }
    }

    public void SetMusicVolumeWrapper(float volume)
    {
        SetMusicVolume(volume);
    }

    public static void SetMusicVolume(float volume)
    {
        if (instance != null)
        {
            MusicVolume = volume;

            if (MenuMusic != null)
            {
                MenuMusic.volume = Mathf.Clamp01(volume);
            }
            if (ArenaMusic != null)
            {
                ArenaMusic.volume = Mathf.Clamp01(volume);
            }
        }
    }

    public void ToggleMusicWrapper()
    {
        ToggleMusic();
    }

    public static void ToggleMusic()
    {
        MusicEnabled = !MusicEnabled;
    }
    #endregion
}
