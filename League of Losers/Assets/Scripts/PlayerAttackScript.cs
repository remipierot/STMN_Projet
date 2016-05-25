﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerAttackScript : MonoBehaviour {
    
    public float knockbackStrength = 4;             //Intensité du knockback
    public BoxCollider2D detectArea;                //Zone de détection d'ennemis (corps à corps)
    public Rigidbody2D m_Projectile;                //Projectile lancé par le personnage lors de l'attaque
    public bool rangedAttack = true;                //Indique que le joueur utilise une attaque à distance. Autrement, indique une attaque corps à corps.
    public int rangedAttackReleaseTimeMs = 300;     //Nombre de ms avant qu'un projectile puisse être lancé (correspondant à l'animation toAim)
    
    private Rigidbody2D m_Body;                     //Rigidbody2D de l'objet, utile pour le saut
    private PhotonView m_PhotonView;    		    //Objet lié au Network
    private PlayerControllerScript m_ControlScript; //Script de contrôle du joueur
    private Transform m_BodyBone;                   //Bone de la partie supérieure du perso, lors de la visée
    private Transform m_HandBone;                   //Bone de la main, auquel est attaché la flèche
    
    private bool attacking = false;                 // vrai si on est en train d'attaquer / de viser
    private bool wantAttack = false;                // vrai si le joueur est dans l'incapacité de viser (chute) mais veut viser
    private bool wantRelease = false;               // vrai si le joueur veut envoyer son projectile, mais l'animation n'est pas encore finie
    private bool isSpecialAttack = false;           // vrai pour une compétence spéciale (ex: flèche explosive)
    private float aimingAngle = 0;                  // angle visé lors de l'attaque
    private float MsBeforeNextAngleUpdate = 150;    // temps avant la prochaine synchronisation d'angle de visée
    private float AngleUpdateTimer = -1;            // timer de synchronisation d'angle de visée
    private Vector2 mouseStartPosition;             // position de départ de la souris lors d'un tir de flèche
    private GameObject projectileInstance;          // instance du projectile envoyé
    private float m_AttackInitializationTime;       // moment de commencement de l'animation d'attaque à distance
    
    private List<GameObject> playersInAttackRange = new List<GameObject>();
    
    void Awake() {
        m_Body = GetComponent<Rigidbody2D>();
        m_PhotonView = GetComponent<PhotonView>();
        m_ControlScript = GetComponent<PlayerControllerScript>();
        m_BodyBone = getChildByName(transform, "boneBODY");
        m_HandBone = getChildByName(transform, "boneHANDF");
    }
    
    Transform getChildByName(Transform obj, string name)
    {
        if (obj.name == name)
            return obj;
        
	    foreach (Transform child in obj)
        {
            Transform res = getChildByName(child, name);
            if (res != null)
                return res;
        }
        
        return null;
    }

	void Start () {
	
	}
	
	void Update () {
        if (!m_PhotonView.isMine)
            return;
        
        if (!rangedAttack)
        {
            if (Input.GetButtonDown("Attack"))
            {
                if (!attacking)
                {
                    attacking = true;
                    foreach (var player in playersInAttackRange)
                    {
                        // nécessite d'être amélioré
                        Rigidbody2D otherBody = player.GetComponent<Rigidbody2D>();
                        ((PhotonView)(player.GetComponent<PhotonView>())).RPC("PhTakeDamage", PhotonTargets.All, m_Body.transform.position.x < otherBody.transform.position.x);
                    }
                }
            }
            else
                attacking = false;
        }
        else
        {
            if (Input.GetButtonDown("Attack") && !attacking)
            {
                wantAttack = true;
                isSpecialAttack = false;
            }
            if (Input.GetButtonDown("Skill") && !attacking)
            {
                wantAttack = true;
                isSpecialAttack = true;
            }
            if (m_ControlScript.CanAttack() && wantAttack)
            {
                wantAttack = false;
                wantRelease = false;
                attacking = true;
                if (m_ControlScript.GetCurrentFacing())
                    mouseStartPosition = new Vector2(Input.mousePosition.x-40, Input.mousePosition.y);
                else
                    mouseStartPosition = new Vector2(Input.mousePosition.x+40, Input.mousePosition.y);
                // change la position du perso
                m_ControlScript.ChangeState(PlayerControllerScript.STATE_AIMING);
                m_PhotonView.RPC("PhChangeState", PhotonTargets.Others, PlayerControllerScript.STATE_AIMING);
                m_PhotonView.RPC("PhSetAttacking", PhotonTargets.Others, true);
                
                if (isSpecialAttack)
                {
                    // ...
                    // POULEEEEEEEEEEEEET !
                    projectileInstance = PhotonNetwork.Instantiate("ArcherArrowSpecial", m_HandBone.position+new Vector3(0,0,-1), m_HandBone.rotation * Quaternion.Euler(new Vector3(0, 0, -20)), 0);
                }
                else
                    projectileInstance = PhotonNetwork.Instantiate("ArcherArrow", m_HandBone.position+new Vector3(0,0,-1), m_HandBone.rotation * Quaternion.Euler(new Vector3(0, 0, -20)), 0);
                
                projectileInstance.GetComponent<Arrow>().setOwner(this.gameObject.GetComponent<PlayerControllerScript>().owner);
                projectileInstance.transform.SetParent(m_HandBone, true);
                m_AttackInitializationTime = Time.realtimeSinceStartup * 1000;
            }
            if (Input.GetButtonUp("Attack") || Input.GetButtonUp("Skill"))
                wantRelease = true;
            if (wantRelease && attacking && (Time.realtimeSinceStartup * 1000 - m_AttackInitializationTime) >= rangedAttackReleaseTimeMs)
            {
                wantRelease = false;
                Vector2 direction = (Vector2)(Input.mousePosition) - mouseStartPosition;
                if (direction.magnitude < 40)
                    // pas de direction de tir
                    direction = new Vector2(1,0);
                else
                    direction.Normalize();
                float angle = Vector2.Angle(new Vector2(1,0), direction);
                if (direction.y < 0)
                    angle *= -1;
                Quaternion directionQuat = Quaternion.Euler(new Vector3(0, 0, angle));
                projectileInstance.GetComponent<Arrow>().Launch();
                Rigidbody2D rb2d = projectileInstance.GetComponent<Rigidbody2D>();
                if (isSpecialAttack)
                    rb2d.velocity = direction * 7; // moins de vélocité, afin d'admirer ce superbe poulet
                else
                    rb2d.velocity = direction * 15;
                projectileInstance.transform.parent = null;
                attacking = false;
                AngleUpdateTimer = Time.realtimeSinceStartup * 1000;
                m_ControlScript.ChangeState(PlayerControllerScript.STATE_IDLE);
                m_PhotonView.RPC("PhChangeState", PhotonTargets.Others, PlayerControllerScript.STATE_IDLE);
                m_PhotonView.RPC("PhSetAttacking", PhotonTargets.Others, false);
            }
        }
	}
    
	void LateUpdate () {
        if (!m_PhotonView.isMine)
        {
            SetAimDir();
            return;
        }
        else if (attacking)
        {
            Vector2 direction = (Vector2)(Input.mousePosition) - mouseStartPosition;
            direction.Normalize();
            float angle = Vector2.Angle(new Vector2(1,0), direction);
            if (m_ControlScript.GetCurrentFacing() && direction.x < 0)
            {
                m_ControlScript.ChangeDirection(false);
                m_PhotonView.RPC("PhChangeDirection", PhotonTargets.Others, false);
            }
            if (!m_ControlScript.GetCurrentFacing() && direction.x > 0)
            {
                m_ControlScript.ChangeDirection(true);
                m_PhotonView.RPC("PhChangeDirection", PhotonTargets.Others, true);
            }
            if (direction.y < 0)
                angle *= -1;
            aimingAngle = angle+90;
            SetAimDir();
            if ((Time.realtimeSinceStartup * 1000 - AngleUpdateTimer) > MsBeforeNextAngleUpdate)
            {
                AngleUpdateTimer = Time.realtimeSinceStartup * 1000;
                m_PhotonView.RPC("PhSetAimDir", PhotonTargets.Others, aimingAngle);
            }
        }
    }
    
    [PunRPC]
    void PhSetAimDir(float angle)
    {
        aimingAngle = angle;
        SetAimDir();
    }
    
    [PunRPC]
    void PhSetAttacking(bool attck)
    {
        attacking = attck;
        if (attacking)
        {
            if (isSpecialAttack)
                projectileInstance = PhotonNetwork.Instantiate("ArcherArrowSpecial", m_HandBone.position, m_HandBone.rotation * Quaternion.Euler(new Vector3(0, 0, -20)), 0);
            else
                projectileInstance = PhotonNetwork.Instantiate("ArcherArrow", m_HandBone.position, m_HandBone.rotation * Quaternion.Euler(new Vector3(0, 0, -20)), 0);
            projectileInstance.transform.SetParent(m_HandBone, true);
            projectileInstance.GetComponent<Arrow>().setOwner(this.gameObject.GetComponent<PlayerControllerScript>().owner);
        }
        else
        {
            projectileInstance.transform.parent = null;
        }
    }
    
    void SetAimDir()
    {
        Quaternion directionQuat = Quaternion.Euler(new Vector3(0, 0, aimingAngle));
        if (aimingAngle > 180 || aimingAngle < 0)
        {
            directionQuat *= (Quaternion.Euler(new Vector3(0, 180, 0)));
            m_BodyBone.rotation = directionQuat;
        }
        else
            m_BodyBone.rotation = directionQuat;
    }
    
    public bool IsAiming()
    {
        return attacking;
    }
    
    public void ExitAiming()
    {
        wantAttack = attacking;
        attacking = false;
        m_PhotonView.RPC("PhSetAttacking", PhotonTargets.Others, false);
        Destroy(projectileInstance);
    }
    
    void OnTriggerEnter2D(Collider2D coll) {
        if (coll.gameObject.tag == "Player")
        {
            if (playersInAttackRange.Contains(coll.gameObject))
                return;
            playersInAttackRange.Add(coll.gameObject);
        }
    }
    void OnTriggerExit2D(Collider2D coll) {
        if (coll.gameObject.tag == "Player")
            playersInAttackRange.Remove(coll.gameObject);
    }
}
