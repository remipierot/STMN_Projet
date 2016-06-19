using UnityEngine;
using System.Collections;

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
    
    // Use this for initialization
    void Start () {
        if (!playerController.m_PhotonView.isMine)
            curseu.enabled = false;
        dashFullRechargeBar = dashRechargeTimer.localScale.x;
        specialFullRechargeBar = specialRechargeTimer.localScale.x;
    }
	
	// Update is called once per frame
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
    
    public void AnimateDash(float seconds)
    {
        dashRechargeTimer.localScale = new Vector3(dashFullRechargeBar, dashRechargeTimer.localScale.y, dashRechargeTimer.localScale.z);
        dashAnimationLength = seconds;
    }
    
    public void AnimateSpecial(float seconds)
    {
        specialRechargeTimer.localScale = new Vector3(specialFullRechargeBar, specialRechargeTimer.localScale.y, specialRechargeTimer.localScale.z);
        specialAnimationLength = seconds;
    }
}
