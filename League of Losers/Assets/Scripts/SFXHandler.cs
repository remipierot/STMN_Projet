using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SFXHandler : MonoBehaviour {
    private static SFXHandler instance;

    #region Instance Audio Library
    public AudioSource menuButtonSound,
                       aimEmote,
                       fireBowmanEmote,
                       fireSpecialBowmanEmote,
                       fireKnightEmote,
                       fireSpecialKnightEmote,
                       respawnEmote,
                       deathEmote,
                       fallingDeathEmote,
                       dashEmote,
                       jumpEmote,
                       doubleJumpEmote,
                       hitGroundEmote,
                       killPlayerEmote,
                       takeDamageEmote;

    public bool sfxEnabled = true;
    public float sfxVolume = 1;
    #endregion

    #region Static Audio Library
    public static AudioSource MenuButtonSound { get { return instance.menuButtonSound; } }
    public static AudioSource AimEmote { get { return instance.aimEmote; } }
    public static AudioSource FireBowmanEmote { get { return instance.fireBowmanEmote; } }
    public static AudioSource FireSpecialBowmanEmote { get { return instance.fireSpecialBowmanEmote; } }
    public static AudioSource FireKnightEmote { get { return instance.fireKnightEmote; } }
    public static AudioSource FireSpecialKnightEmote { get { return instance.fireSpecialKnightEmote; } }
    public static AudioSource RespawnEmote { get { return instance.respawnEmote; } }
    public static AudioSource DeathEmote { get { return instance.deathEmote; } }
    public static AudioSource FallingDeathEmote { get { return instance.fallingDeathEmote; } }
    public static AudioSource DashEmote { get { return instance.dashEmote; } }
    public static AudioSource JumpEmote { get { return instance.jumpEmote; } }
    public static AudioSource DoubleJumpEmote { get { return instance.doubleJumpEmote; } }
    public static AudioSource HitGroundEmote { get { return instance.hitGroundEmote; } }
    public static AudioSource KillPlayerEmote { get { return instance.killPlayerEmote; } }
    public static AudioSource TakeDamageEmote { get { return instance.takeDamageEmote; } }

    public static bool SFXEnabled { get { return instance.sfxEnabled; } set { instance.sfxEnabled = value; } }
    public static float SFXVolume { get { return instance.sfxVolume; } set { instance.sfxVolume = value; } }
    #endregion

    #region Unity Callbacks
    void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    #endregion

    #region Audio Playing Methods
    public void PlayMenuButtonWrapper()
    {
        PlayMenuButton();
    }

    public static void PlayMenuButton()
    {
        if (instance != null && MenuButtonSound != null)
        {
            MenuButtonSound.Play();
        }
    }

    public static void PlayAimEmote()
    {
        if (instance != null && AimEmote != null && SFXEnabled)
        {
            AimEmote.Play();
        }
    }

    public static void PlayFireBowmanEmote()
    {
        if (instance != null && AimEmote != null && FireBowmanEmote != null && SFXEnabled)
        {
            AimEmote.Stop();
            FireBowmanEmote.Play();
        }
    }

    public static void PlayFireSpecialBowmanEmote()
    {
        if(instance != null && AimEmote != null && FireSpecialBowmanEmote != null && SFXEnabled)
        {
            AimEmote.Stop();
            FireSpecialBowmanEmote.Play();
        }
    }

    public static void PlayFireKnightEmote()
    {
        if (instance != null && AimEmote != null && FireKnightEmote != null && SFXEnabled)
        {
            FireKnightEmote.Play();
        }
    }

    public static void PlayFireSpecialKnightEmote()
    {
        if (instance != null && AimEmote != null && FireSpecialKnightEmote != null && SFXEnabled)
        {
            FireSpecialKnightEmote.Play();
        }
    }

    public static void PlayRespawnEmote()
    {
        if(instance != null && RespawnEmote != null && SFXEnabled)
        {
            RespawnEmote.Play();
        }
    }

    public static void PlayDeathEmote()
    {
        if (instance != null && DeathEmote != null && SFXEnabled)
        {
            DeathEmote.Play();
        }
    }

    public static void PlayFallingDeathEmote()
    {
        if (instance != null && FallingDeathEmote != null && SFXEnabled)
        {
            FallingDeathEmote.Play();
        }
    }

    public static void PlayDashEmote()
    {
        if (instance != null && DashEmote != null && SFXEnabled)
        {
            DashEmote.Play();
        }
    }

    public static void PlayJumpEmote()
    {
        if (instance != null && JumpEmote != null && SFXEnabled)
        {
            JumpEmote.Play();
        }
    }

    public static void PlayDoubleJumpEmote()
    {
        if (instance != null && DoubleJumpEmote != null && SFXEnabled)
        {
            DoubleJumpEmote.Play();
        }
    }

    public static void PlayHitGroundEmote()
    {
        if (instance != null && HitGroundEmote != null && SFXEnabled)
        {
            HitGroundEmote.Play();
        }
    }

    public static void PlayKillPlayerEmote()
    {
        if (instance != null && KillPlayerEmote != null && SFXEnabled)
        {
            KillPlayerEmote.Play();
        }
    }

    public static void PlayTakeDamageEmote()
    {
        if (instance != null && TakeDamageEmote != null && SFXEnabled)
        {
            TakeDamageEmote.Play();
        }
    }
    #endregion

    #region Volume Control
    public void AutoUpdateSFXVolume()
    {
        GameObject SFXSliderGO = GameObject.Find("Sound_Slider");
        if(SFXSliderGO != null)
        {
            Slider SFXSlider = SFXSliderGO.GetComponent<Slider>();
            if(SFXSlider != null)
            {
                SetSFXVolume(SFXSlider.value);
            }
        }
    }

    public void SetSFXVolumeWrapper(float volume)
    {
        SetSFXVolume(volume);
    }

    public static void SetSFXVolume(float volume)
    {
        if (instance != null)
        {
            SFXVolume = volume;

            if (AimEmote != null)
            {
                AimEmote.volume = Mathf.Clamp01(volume);
            }
            if (FireBowmanEmote != null)
            {
                FireBowmanEmote.volume = Mathf.Clamp01(volume);
            }
            if (FireSpecialBowmanEmote != null)
            {
                FireSpecialBowmanEmote.volume = Mathf.Clamp01(volume);
            }
            if (FireKnightEmote != null)
            {
                FireKnightEmote.volume = Mathf.Clamp01(volume);
            }
            if (FireSpecialKnightEmote != null)
            {
                FireSpecialKnightEmote.volume = Mathf.Clamp01(volume);
            }
            if (RespawnEmote != null)
            {
                RespawnEmote.volume = Mathf.Clamp01(volume);
            }
            if (DeathEmote != null)
            {
                DeathEmote.volume = Mathf.Clamp01(volume);
            }
            if (FallingDeathEmote != null)
            {
                FallingDeathEmote.volume = Mathf.Clamp01(volume);
            }
            if (DashEmote != null)
            {
                DashEmote.volume = Mathf.Clamp01(volume);
            }
            if (JumpEmote != null)
            {
                JumpEmote.volume = Mathf.Clamp01(volume);
            }
            if (DoubleJumpEmote != null)
            {
                DoubleJumpEmote.volume = Mathf.Clamp01(volume);
            }
            if (HitGroundEmote != null)
            {
                HitGroundEmote.volume = Mathf.Clamp01(volume);
            }
            if (KillPlayerEmote != null)
            {
                KillPlayerEmote.volume = Mathf.Clamp01(volume);
            }
            if (TakeDamageEmote != null)
            {
                TakeDamageEmote.volume = Mathf.Clamp01(volume);
            }
        }
    }

    public void ToggleSFXWrapper()
    {
        ToggleSFX();
    }

    public static void ToggleSFX()
    {
        SFXEnabled = !SFXEnabled;
    }
    #endregion
}
