using UnityEngine;
using System.Collections;

public class MusicHandler : MonoBehaviour {
    private static MusicHandler instance;

    #region Instance Audio Library
    public AudioSource menuMusic,
                       arenaMusic;
    #endregion

    #region Static Audio Library
    public static AudioSource MenuMusic { get { return instance.menuMusic; } }
    public static AudioSource ArenaMusic { get { return instance.arenaMusic; } }
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
        if (currentScene.name.Equals("MainMenu") && !MenuMusic.isPlaying)
        {
            ArenaMusic.Stop();
            PlayMenuMusic();
        }
        else if(currentScene.name.Contains("Arene") && !ArenaMusic.isPlaying)
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
}
