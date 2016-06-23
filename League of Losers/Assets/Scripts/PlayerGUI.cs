using UnityEngine;
using System.Collections;

/// <summary>
/// Classe gérant l'interface utilisateur associée à un personnage.
/// </summary>
public class PlayerGUI : MonoBehaviour {

    public PlayerControllerScript playerController;
    public Renderer curseu;
    public Renderer vie1;
    public Renderer vie2;
    public Renderer vie3;
    public Transform dashRechargeTimer;
    public Transform specialRechargeTimer;
    
    private float dashFullRechargeBar;
    private float specialFullRechargeBar;
    private float dashAnimationLength;
    private float specialAnimationLength;
    
    /// <summary>
    /// Initialisation
    /// </summary>
    void Start () {
        if (!playerController.m_PhotonView.isMine)
            curseu.enabled = false;
        dashFullRechargeBar = dashRechargeTimer.localScale.x;
        specialFullRechargeBar = specialRechargeTimer.localScale.x;
    }
	
    /// <summary>
    /// Appelé à chaque nouvelle trame.
    /// Met à jour les coeurs affichés et les barres de rechargement.
    /// </summary>
	void Update () {
        if (vie1.enabled && playerController.m_Lives < 1)
            vie1.enabled = false;
        else if (!vie1.enabled && playerController.m_Lives >= 1)
            vie1.enabled = true;
        
        if (vie2.enabled && playerController.m_Lives < 2)
            vie2.enabled = false;
        else if (!vie2.enabled && playerController.m_Lives >= 2)
            vie2.enabled = true;
        
        if (vie3.enabled && playerController.m_Lives < 3)
            vie3.enabled = false;
        else if (!vie3.enabled && playerController.m_Lives >= 3)
            vie3.enabled = true;
        
        if (playerController.GetCurrentFacing())
            transform.localScale = new Vector3(1, 1, 1);
        else
            transform.localScale = new Vector3(-1, 1, 1);
        
        
        float dashScale = dashRechargeTimer.localScale.x - (Time.deltaTime / dashAnimationLength * dashFullRechargeBar);
        if (dashScale < 0)
            dashRechargeTimer.localScale = new Vector3(0, dashRechargeTimer.localScale.y, dashRechargeTimer.localScale.z);
        else
            dashRechargeTimer.localScale = new Vector3(dashScale, dashRechargeTimer.localScale.y, dashRechargeTimer.localScale.z);
        
        float specialScale = specialRechargeTimer.localScale.x - (Time.deltaTime / specialAnimationLength * specialFullRechargeBar);
        if (specialScale < 0)
            specialRechargeTimer.localScale = new Vector3(0, specialRechargeTimer.localScale.y, specialRechargeTimer.localScale.z);
        else
            specialRechargeTimer.localScale = new Vector3(specialScale, specialRechargeTimer.localScale.y, specialRechargeTimer.localScale.z);
    }
    
    /// <summary>
    /// Anime la barre de cooldown de dash.
    /// </summary>
    /// <param name="seconds">le nombre de secondes avant de faire disparaître la barre</param>
    public void AnimateDash(float seconds)
    {
        dashRechargeTimer.localScale = new Vector3(dashFullRechargeBar, dashRechargeTimer.localScale.y, dashRechargeTimer.localScale.z);
        dashAnimationLength = seconds;
    }
    
    /// <summary>
    /// Anime la barre de cooldown d'attaque spéciale.
    /// </summary>
    /// <param name="seconds">le nombre de secondes avant de faire disparaître la barre</param>
    public void AnimateSpecial(float seconds)
    {
        specialRechargeTimer.localScale = new Vector3(specialFullRechargeBar, specialRechargeTimer.localScale.y, specialRechargeTimer.localScale.z);
        specialAnimationLength = seconds;
    }
}
