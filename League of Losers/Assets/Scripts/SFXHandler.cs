using UnityEngine;
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
        if (instance != null && AimEmote != null)
        {
            AimEmote.Play();
        }
    }

    public static void PlayFireBowmanEmote()
    {
        if (instance != null && AimEmote != null && FireBowmanEmote != null)
        {
            AimEmote.Stop();
            FireBowmanEmote.Play();
        }
    }

    public static void PlayFireSpecialBowmanEmote()
    {
        if(instance != null && AimEmote != null && FireSpecialBowmanEmote != null)
        {
            AimEmote.Stop();
            FireSpecialBowmanEmote.Play();
        }
    }

    public static void PlayFireKnightEmote()
    {
        if (instance != null && AimEmote != null && FireKnightEmote != null)
        {
            FireKnightEmote.Play();
        }
    }

    public static void PlayFireSpecialKnightEmote()
    {
        if (instance != null && AimEmote != null && FireSpecialKnightEmote != null)
        {
            FireSpecialKnightEmote.Play();
        }
    }

    public static void PlayRespawnEmote()
    {
        if(instance != null && RespawnEmote != null)
        {
            RespawnEmote.Play();
        }
    }

    public static void PlayDeathEmote()
    {
        if (instance != null && DeathEmote != null)
        {
            DeathEmote.Play();
        }
    }

    public static void PlayFallingDeathEmote()
    {
        if (instance != null && FallingDeathEmote != null)
        {
            FallingDeathEmote.Play();
        }
    }

    public static void PlayDashEmote()
    {
        if (instance != null && DashEmote != null)
        {
            DashEmote.Play();
        }
    }

    public static void PlayJumpEmote()
    {
        if (instance != null && JumpEmote != null)
        {
            JumpEmote.Play();
        }
    }

    public static void PlayDoubleJumpEmote()
    {
        if (instance != null && DoubleJumpEmote != null)
        {
            DoubleJumpEmote.Play();
        }
    }

    public static void PlayHitGroundEmote()
    {
        if (instance != null && HitGroundEmote != null)
        {
            HitGroundEmote.Play();
        }
    }

    public static void PlayKillPlayerEmote()
    {
        if (instance != null && KillPlayerEmote != null)
        {
            KillPlayerEmote.Play();
        }
    }

    public static void PlayTakeDamageEmote()
    {
        if (instance != null && TakeDamageEmote != null)
        {
            TakeDamageEmote.Play();
        }
    }
    #endregion
}
