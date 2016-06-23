using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Script gérant l'attaque (rangée ou CaC) d'un joueur.
/// </summary>
public class PlayerAttackScript : MonoBehaviour {
    
    public float knockbackStrength = 4;             //Intensité du knockback
    public BoxCollider2D detectArea;                //Zone de détection d'ennemis (corps à corps)
    public Rigidbody2D m_Projectile;                //Projectile lancé par le personnage lors de l'attaque
    public bool rangedAttack = true;                //Indique que le joueur utilise une attaque à distance. Autrement, indique une attaque corps à corps.
    public int rangedAttackReleaseTimeMs = 300;     //Nombre de ms avant qu'un projectile puisse être lancé (correspondant à l'animation toAim)
    public int specialAttackCooldownMs = 15000;     //Nombre de ms de cooldown entre deux attaques spéciales
    public float meleeAttackDelaySec = .3f;         //Nombre de sec lors du clic d'attaque avant d'effectuer des dégâts aux autres joueurs
    public float meleeAttackCooldownSec = .3f;      //Nombre de sec (en plus de meleeAttackDelaySec) avant de pouvoir enchaîner une attaque
    
    public GameObject dummyProjectile;              //Prefab de projectile à afficher durant l'aim si le joueur contrôlé n'est pas celui-ci
    public GameObject dummySpecialProjectile;       //Pareil.
    public GameObject meleeAttackEffect;            //Effet visuel à faire pop lors d'une attaque corps à corps
    public GameObject meleeAttackSpecialEffect;     //Effet visuel à faire pop lors d'une attaque spéciale corps à corps
    public GameObject meleeAttackSpecialEffectEnd;  //Effet visuel à faire pop lors de la fin d'une attaque spéciale corps à corps
    private GameObject meleeAttackEffectInstance;   //Instance de l'effet visuel précédent
    
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
    private float AttackCooldownTimer = -20000;     // timer de cooldown d'attaque spéciale
    private Vector2 mouseStartPosition;             // position de départ de la souris lors d'un tir de flèche
    private Vector2 mouseEndPosition;               // position de départ de la souris lors d'un tir de flèche
    private GameObject projectileInstance;          // instance du projectile envoyé
    private float m_AttackInitializationTime;       // moment de commencement de l'animation d'attaque à distance
    
    private List<GameObject> playersInAttackRange = new List<GameObject>();
    
    public PlayerGUI m_GUI;
    
    /// <summary>
    /// Initialisation, récupère les différents composants.
    /// </summary>
    void Awake() {
        m_Body = GetComponent<Rigidbody2D>();
        m_PhotonView = GetComponent<PhotonView>();
        m_ControlScript = GetComponent<PlayerControllerScript>();
        m_BodyBone = getChildByName(transform, "boneBODY");
        m_HandBone = getChildByName(transform, "boneHANDF");
        //Cursor.lockState = CursorLockMode.Confined;
    }
    
    /// <summary>
    /// Récupère un objet enfant en fonction de son nom.
    /// </summary>
    /// <param name="obj">le parent</param>
    /// <param name="name">le nom de l'enfant</param>
    /// <returns></returns>
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
	
    /// <summary>
    /// Appelé à chaque trame. Gère les différentes étapes de l'entrée, et agis sur les personnages en conséquence.
    /// </summary>
	void Update () {
        // update exécuté uniquement par le possesseur du perso
        if (!m_PhotonView.isMine)
            return;
        
        if (!rangedAttack)
        {
            // gère l'attaque au corps à corps
            if (Input.GetButtonDown("Attack") && m_ControlScript.CanAttack())
            {
                if (!attacking)
                {
                    attacking = true;
                    m_PhotonView.RPC("PlayAttackAnimation", PhotonTargets.Others);
                    StartCoroutine(PlayAttackAnimation());
                    m_PhotonView.RPC("PhPlayerSpeaks", PhotonTargets.All, "fire");
                    StartCoroutine(DelayAttack());
                }
            }
            if (Input.GetButtonDown("Skill") && m_ControlScript.CanAttack())
            {
                if ((Time.realtimeSinceStartup * 1000 - AttackCooldownTimer) < specialAttackCooldownMs)
                {
                    m_ControlScript.PhPlayerSpeaks("specialrecharge");
                    return;
                }
                attacking = true;
                isSpecialAttack = true;
                m_PhotonView.RPC("PhPlayAttackSpecialAnimation", PhotonTargets.Others, true);
                PlayAttackSpecialAnimation(true);
                foreach (GameObject player in playersInAttackRange)
                {
                    Rigidbody2D otherBody = player.GetComponent<Rigidbody2D>();
                    ((PhotonView)(player.GetComponent<PhotonView>())).RPC("PhTakeDamage", PhotonTargets.All, m_ControlScript.GetCurrentFacing(), m_ControlScript.owner);
                }
            }
            if (attacking && isSpecialAttack && (Input.GetButtonUp("Attack") || Input.GetButtonUp("Skill")))
            {
                AttackCooldownTimer = Time.realtimeSinceStartup * 1000;
                m_GUI.AnimateSpecial(specialAttackCooldownMs/1000f);
                isSpecialAttack = false;
                m_PhotonView.RPC("PhPlayAttackSpecialAnimation", PhotonTargets.Others, false);
                PlayAttackSpecialAnimation(false);
            }
        }
        else
        {
            if (Input.GetButtonDown("Attack") && !attacking && !wantAttack)
            {
                // le joueur veut attaquer, on délaie l'animation en attendant d'être sûr d'être au sol
                wantAttack = true;
                wantRelease = false;
                isSpecialAttack = false;
            }
            if (Input.GetButtonDown("Skill") && !attacking && !wantAttack)
            {
                if ((Time.realtimeSinceStartup * 1000 - AttackCooldownTimer) >= specialAttackCooldownMs)
                {
                    // le joueur veut attaquer, on délaie l'animation en attendant d'être sûr d'être au sol
                    wantAttack = true;
                    isSpecialAttack = true;
                    wantRelease = false;
                }
                else
                {
                    //m_PhotonView.RPC("PhPlayerSpeaks", PhotonTargets.All, "specialrecharge");
                    m_ControlScript.PhPlayerSpeaks("specialrecharge");
                }
            }
            if (m_ControlScript.CanAttack() && wantAttack && !attacking)
            {
                // initialisation de l'attaque à distance
                wantAttack = false;
                attacking = true;
                if (m_ControlScript.GetCurrentFacing())
                    mouseStartPosition = new Vector2(Input.mousePosition.x-40, Input.mousePosition.y);
                else
                    mouseStartPosition = new Vector2(Input.mousePosition.x+40, Input.mousePosition.y);
                // change la position du perso
                m_ControlScript.ChangeState(PlayerControllerScript.STATE_AIMING);
                m_PhotonView.RPC("PhChangeState", PhotonTargets.Others, PlayerControllerScript.STATE_AIMING);
                m_PhotonView.RPC("PhSetAttacking", PhotonTargets.Others, true, isSpecialAttack);
                
                if (isSpecialAttack)
                {
                    // ...
                    // POULEEEEEEEEEEEEET !
                    projectileInstance = (GameObject) (PhotonNetwork.Instantiate("ArcherArrowSpecial", m_HandBone.position+new Vector3(0,0,-1), m_HandBone.rotation * Quaternion.Euler(new Vector3(0, 0, -20)), 0) as GameObject);
                }
                else
                    // projectile normal
                    projectileInstance = PhotonNetwork.Instantiate("ArcherArrow", m_HandBone.position+new Vector3(0,0,-1), m_HandBone.rotation * Quaternion.Euler(new Vector3(0, 0, -20)), 0);
                
                // définit le parent du projectile, et positionne l'objet
                projectileInstance.GetComponent<Arrow>().setOwner(m_ControlScript.owner);
                projectileInstance.transform.SetParent(m_HandBone, true);
                m_AttackInitializationTime = Time.realtimeSinceStartup * 1000;
                m_PhotonView.RPC("PhPlayerSpeaks", PhotonTargets.All, "aim");
            }
            if (Input.GetButtonUp("Attack") || Input.GetButtonUp("Skill"))
            {
                if (attacking)
                {
                    // le joueur veut lancer son attaque
                    wantRelease = true;
                    wantAttack = false;
                    mouseEndPosition = (Vector2)(Input.mousePosition);
                }
                else
                {
                    wantAttack = false;
                }
            }
            
            if (attacking && wantRelease && (Time.realtimeSinceStartup * 1000 - m_AttackInitializationTime) >= rangedAttackReleaseTimeMs)
            {
                // lancement de l'attaque, après s'être assuré que l'animation initiale est terminée
                wantRelease = false;
                wantAttack = false;
                attacking = false;
                Vector2 direction = GetDirection();
                float angle = Vector2.Angle(new Vector2(1,0), direction);
                if (direction.y < 0)
                    angle *= -1;
                Quaternion directionQuat = Quaternion.Euler(new Vector3(0, 0, angle));
                // détache le projectile du parent
                projectileInstance.transform.parent = null;
                projectileInstance.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
                projectileInstance.GetComponent<Arrow>().Launch();
                Rigidbody2D rb2d = projectileInstance.GetComponent<Rigidbody2D>();
                // met en mouvement l'objet
                if (isSpecialAttack)
                    rb2d.velocity = direction * 7; // moins de vélocité, afin d'admirer ce superbe poulet
                else
                    rb2d.velocity = direction * 15;
                AngleUpdateTimer = Time.realtimeSinceStartup * 1000;
                if (isSpecialAttack)
                    AttackCooldownTimer = Time.realtimeSinceStartup * 1000;
                // retour à une stance normale
                m_ControlScript.ChangeState(PlayerControllerScript.STATE_IDLE);
                m_PhotonView.RPC("PhChangeState", PhotonTargets.Others, PlayerControllerScript.STATE_IDLE);
                m_PhotonView.RPC("PhSetAttacking", PhotonTargets.Others, false, false);
                // fait parler le personnage
                if (isSpecialAttack)
                    m_PhotonView.RPC("PhPlayerSpeaks", PhotonTargets.All, "firespecial");
                else
                    m_PhotonView.RPC("PhPlayerSpeaks", PhotonTargets.All, "fire");
                if (isSpecialAttack)
                    m_GUI.AnimateSpecial(specialAttackCooldownMs/1000f);
            }
        }
	}
    
    /// <summary>
    /// Retourne la direction visée par le joueur (souris ou joystick).
    /// </summary>
    /// <returns>un Vector2 de direction.</returns>
    Vector2 GetDirection()
    {
        Vector2 direction = ((Vector2) Input.mousePosition) - mouseStartPosition;
        float vert = Input.GetAxis("Vertical");
        float horiz = Input.GetAxis("Horizontal");
        if (direction.magnitude < 40)
            // pas de direction de tir
            direction = new Vector2(1,0);
        if (vert!=0 || horiz!=0 && Input.GetJoystickNames().Length > 0)
            direction = new Vector2(horiz, vert);
        direction.Normalize();
        return direction;
    }
    
    /// <summary>
    /// Appelé après MAJ du système d'animation. Utilisé pour orienter le tir de l'archer.
    /// </summary>
	void LateUpdate () {
        if (!rangedAttack)
            return;
        
        // appelé après le rendu du système d'animation, mais avant le rendu de la scène
        if (!m_PhotonView.isMine && attacking)
        {
            // les autres joueurs se contentent de mettre à jour la direction de tir
            SetAimDir();
            return;
        }
        else if (attacking)
        {
            // change la direction du perso en fonction de la direction indiquée par la souris
            Vector2 direction = GetDirection();
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
    
    /// <summary>
    /// Définit le nouvel angle de tir d'un joueur sur le réseau.
    /// </summary>
    /// <param name="angle"></param>
    [PunRPC]
    void PhSetAimDir(float angle)
    {
        aimingAngle = angle;
        SetAimDir();
    }
    
    /// <summary>
    /// Synchronise l'état d'attaque sur le réseau.
    /// </summary>
    /// <param name="attck">le joueur est en train d'attaquer</param>
    /// <param name="special">l'attaque est une compétence spéciale</param>
    [PunRPC]
    void PhSetAttacking(bool attck, bool special)
    {
        attacking = attck;
        if (attacking)
        {
            /*
            if (isSpecialAttack)
                projectileInstance = PhotonNetwork.Instantiate("ArcherArrowSpecial", m_HandBone.position, m_HandBone.rotation * Quaternion.Euler(new Vector3(0, 0, -20)), 0);
            else
                projectileInstance = PhotonNetwork.Instantiate("ArcherArrow", m_HandBone.position, m_HandBone.rotation * Quaternion.Euler(new Vector3(0, 0, -20)), 0);
            //*/
            if (m_PhotonView.isMine)
            {
                projectileInstance.GetComponent<Arrow>().setOwner(m_ControlScript.owner);
                projectileInstance.transform.SetParent(m_HandBone, true);
            }
            else if (projectileInstance == null)
            {
                if (special)
                    projectileInstance = (GameObject)Instantiate(dummySpecialProjectile, m_HandBone.position+new Vector3(0,0,-1), m_HandBone.rotation * Quaternion.Euler(new Vector3(0, 0, -20)));
                else
                    projectileInstance = (GameObject)Instantiate(dummyProjectile, m_HandBone.position+new Vector3(0,0,-1), m_HandBone.rotation * Quaternion.Euler(new Vector3(0, 0, -20)));
                
                projectileInstance.transform.SetParent(m_HandBone, true);
            }
            wantRelease = false;
            wantAttack = false;
        }
        else
        {
            if (m_PhotonView.isMine)
                projectileInstance.transform.parent = null;
            else
                Destroy(projectileInstance);
            wantRelease = false;
            wantAttack = false;
        }
    }
    
    /// <summary>
    /// Définit l'angle de tir du personnage. Effectue une rotation de la partie haute du personnage en train de viser.
    /// </summary>
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
    
    /// <summary>
    /// Retourne vrai si le personnage est actuellement en train de viser (ou de charger pour le chevalier).
    /// </summary>
    /// <returns></returns>
    public bool IsAiming()
    {
        return attacking;
    }
    
    /// <summary>
    /// Force le joueur à quitter l'état de visée (en cas de prise de dégâts, etc).
    /// </summary>
    public void ExitAiming()
    {
        if (rangedAttack)
        {
            attacking = false;
            wantRelease = false;
            wantAttack = false;
            m_PhotonView.RPC("PhSetAttacking", PhotonTargets.Others, false);
            if (projectileInstance != null)
                Destroy(projectileInstance);
        }
    }
    
    /// <summary>
    /// Superposition de la zone d'attaque avec un autre élément.
    /// Les éléments joueurs actuellement en contact avec la zone d'attaque sont stockés pour leur infliger des dégâts en CaC.
    /// </summary>
    /// <param name="coll"></param>
    void OnTriggerEnter2D(Collider2D coll) {
        if (coll.gameObject.tag == "Player")
        {
            if (coll.gameObject.GetComponent<PlayerControllerScript>().owner == m_ControlScript.owner)
                return;
            if (playersInAttackRange.Contains(coll.gameObject))
                return;
            playersInAttackRange.Add(coll.gameObject);
        }
    }
    /// <summary>
    /// Sortie d'un élément de la zone d'attaque.
    /// Les éléments joueurs actuellement en contact avec la zone d'attaque sont stockés pour leur infliger des dégâts en CaC.
    /// Dans le cas de la charge du chevalier, c'est à la sortie de la zone d'attaque que les dégâts sont infligés (plus réaliste que lors de l'entrée).
    /// </summary>
    /// <param name="coll"></param>
    void OnTriggerExit2D(Collider2D coll) {
        if (coll.gameObject.tag == "Player")
        {
            playersInAttackRange.Remove(coll.gameObject);
        
            if (attacking && isSpecialAttack)
                ((PhotonView)(coll.gameObject.GetComponent<PhotonView>())).RPC("PhTakeDamage", PhotonTargets.All, m_ControlScript.GetCurrentFacing(), m_ControlScript.owner);
        }
    }
    
    /// <summary>
    /// Attends un court temps avant d'appliquer des dégâts CaC, afin de synchroniser les dégâts avec l'animation.
    /// </summary>
    /// <returns></returns>
    IEnumerator DelayAttack()
    {
        yield return new WaitForSeconds(.15f);
        foreach (GameObject player in playersInAttackRange)
        {
            Rigidbody2D otherBody = player.GetComponent<Rigidbody2D>();
            ((PhotonView)(player.GetComponent<PhotonView>())).RPC("PhTakeDamage", PhotonTargets.All, m_Body.transform.position.x < otherBody.transform.position.x, m_ControlScript.owner);
        }
        Destroy(meleeAttackEffectInstance);
        yield return new WaitForSeconds(meleeAttackCooldownSec);
        attacking = false;
        if (m_ControlScript.GetCurrentState() == PlayerControllerScript.STATE_AIMING)
            m_ControlScript.ChangeState(PlayerControllerScript.STATE_IDLE);
    }
    
    /// <summary>
    /// Joue l'animation d'attaque CaC sur le réseau.
    /// </summary>
    [PunRPC]
    public void PhPlayAttackAnimation()
    {
        PlayAttackAnimation();
    }
    
    /// <summary>
    /// Initialise l'animation d'attaque CaC.
    /// </summary>
    /// <returns></returns>
    IEnumerator PlayAttackAnimation()
    {
        m_ControlScript.ChangeState(PlayerControllerScript.STATE_AIMING);
        yield return new WaitForSeconds(.08f);
        meleeAttackEffectInstance = (GameObject) Instantiate(meleeAttackEffect, new Vector3(
            transform.position.x+0.272f * (m_ControlScript.GetCurrentFacing() ? 1:-1),
            transform.position.y+0.332f,
            transform.position.z),
            m_ControlScript.GetCurrentFacing() ?
                Quaternion.identity :
                Quaternion.Euler(new Vector3(0, 180, 0)));
    }
    
    /// <summary>
    /// Joue l'animation d'attaque spéciale sur le réseau.
    /// </summary>
    /// <param name="playing">vrai si l'animation doit être jouée, faux si elle est interrompue</param>
    [PunRPC]
    public void PhPlayAttackSpecialAnimation(bool playing)
    {
        PlayAttackSpecialAnimation(playing);
    }
    
    /// <summary>
    /// Joue l'animation d'attaque spéciale (localement).
    /// </summary>
    /// <param name="playing">vrai si l'animation doit être jouée, faux si elle est interrompue</param>
    public void PlayAttackSpecialAnimation(bool playing)
    {
        if (playing)
        {
            m_ControlScript.ChangeState(PlayerControllerScript.STATE_CHARGE);
            meleeAttackEffectInstance = (GameObject) Instantiate(meleeAttackSpecialEffect, new Vector3(
            transform.position.x+.07f * (m_ControlScript.GetCurrentFacing() ? 1:-1),
            transform.position.y+.24f,
            transform.position.z),
            m_ControlScript.GetCurrentFacing() ?
                Quaternion.identity :
                Quaternion.Euler(new Vector3(0, 180, 0)));
            meleeAttackEffectInstance.transform.SetParent(transform, true);
        }
        else
        {
            if (m_ControlScript.GetCurrentState() == PlayerControllerScript.STATE_CHARGE)
                m_ControlScript.ChangeState(PlayerControllerScript.STATE_AIMING);
            Destroy(meleeAttackEffectInstance);
            StartCoroutine(EndAttackAnimation());
        }
    }
    /// <summary>
    /// Finalise l'animation d'attaque spéciale CaC, infligeant des dégâts et faisant revenir le joueur à un état normal.
    /// </summary>
    /// <returns></returns>
    IEnumerator EndAttackAnimation()
    {
        yield return new WaitForSeconds(.05f);
        meleeAttackEffectInstance = (GameObject) Instantiate(meleeAttackSpecialEffectEnd, new Vector3(transform.position.x+.06f, transform.position.y+.24f, transform.position.z), m_ControlScript.GetCurrentFacing() ?
                Quaternion.identity :
                Quaternion.Euler(new Vector3(0, 180, 0)));
        meleeAttackEffectInstance.transform.SetParent(transform, true);
        
        foreach (GameObject player in playersInAttackRange)
        {
            Rigidbody2D otherBody = player.GetComponent<Rigidbody2D>();
            ((PhotonView)(player.GetComponent<PhotonView>())).RPC("PhTakeDamage", PhotonTargets.All, m_Body.transform.position.x < otherBody.transform.position.x, m_ControlScript.owner);
        }
        
        yield return new WaitForSeconds(.7f);
        
        foreach (GameObject player in playersInAttackRange)
        {
            Rigidbody2D otherBody = player.GetComponent<Rigidbody2D>();
            ((PhotonView)(player.GetComponent<PhotonView>())).RPC("PhTakeDamage", PhotonTargets.All, m_Body.transform.position.x < otherBody.transform.position.x, m_ControlScript.owner);
        }
        
        Destroy(meleeAttackEffectInstance);
        attacking = false;
        
        if (m_ControlScript.GetCurrentState() == PlayerControllerScript.STATE_AIMING)
            m_ControlScript.ChangeState(PlayerControllerScript.STATE_IDLE);
    }
}
