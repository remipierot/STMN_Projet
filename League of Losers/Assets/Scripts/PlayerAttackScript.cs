using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerAttackScript : MonoBehaviour {
    
    public float knockbackStrength = 4;             //Intensité du knockback
    public BoxCollider2D detectArea;                //Zone de détection d'ennemis (corps à corps)
    public Rigidbody2D m_Projectile;                //Projectile lancé par le personnage lors de l'attaque
    public bool rangedAttack = true;                //Indique que le joueur utilise une attaque à distance. Autrement, indique une attaque corps à corps.
    public int rangedAttackReleaseTimeMs = 300;     //Nombre de ms avant qu'un projectile puisse être lancé (correspondant à l'animation toAim)
    public int specialAttackCooldownMs = 15000;     //Nombre de ms de cooldown entre deux attaques spéciales
    
    public GameObject dummyProjectile;              //Prefab de projectile à afficher durant l'aim si le joueur contrôlé n'est pas celui-ci
    public GameObject dummySpecialProjectile;            //Pareil.
    
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
    
    void Awake() {
        m_Body = GetComponent<Rigidbody2D>();
        m_PhotonView = GetComponent<PhotonView>();
        m_ControlScript = GetComponent<PlayerControllerScript>();
        m_BodyBone = getChildByName(transform, "boneBODY");
        m_HandBone = getChildByName(transform, "boneHANDF");
        Cursor.lockState = CursorLockMode.Confined;
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
        // update exécuté uniquement par le possesseur du perso
        if (!m_PhotonView.isMine)
            return;
        
        if (!rangedAttack)
        {
            // gère l'attaque au corps à corps
            if (Input.GetButtonDown("Attack"))
            {
                if (!attacking)
                {
                    attacking = true;
                    foreach (var player in playersInAttackRange)
                    {
                        // nécessite d'être amélioré
                        Rigidbody2D otherBody = player.GetComponent<Rigidbody2D>();
                        m_PhotonView.RPC("PhTakeDamage", PhotonTargets.All, m_Body.transform.position.x < otherBody.transform.position.x);
                    }
                }
            }
            else
                attacking = false;
        }
        else
        {
            if (Input.GetButtonDown("Attack") && !attacking && !wantAttack)
            {
                // le joueur veut attaquer, on délaie l'animation en attendant d'être sûr d'être au sol
                wantAttack = true;
                isSpecialAttack = false;
            }
            if (Input.GetButtonDown("Skill") && !attacking && !wantAttack)
            {
                if ((Time.realtimeSinceStartup * 1000 - AttackCooldownTimer) >= specialAttackCooldownMs)
                {
                    // le joueur veut attaquer, on délaie l'animation en attendant d'être sûr d'être au sol
                    wantAttack = true;
                    isSpecialAttack = true;
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
                    projectileInstance = PhotonNetwork.Instantiate("ArcherArrowSpecial", m_HandBone.position+new Vector3(0,0,-1), m_HandBone.rotation * Quaternion.Euler(new Vector3(0, 0, -20)), 0);
                    AttackCooldownTimer = Time.realtimeSinceStartup * 1000;
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
                // le joueur veut lancer son attaque
                wantRelease = true;
                wantAttack = false;
                mouseEndPosition = (Vector2)(Input.mousePosition);
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
                projectileInstance.GetComponent<Arrow>().Launch();
                Rigidbody2D rb2d = projectileInstance.GetComponent<Rigidbody2D>();
                // met en mouvement l'objet
                if (isSpecialAttack)
                    rb2d.velocity = direction * 7; // moins de vélocité, afin d'admirer ce superbe poulet
                else
                    rb2d.velocity = direction * 15;
                // détache le projectile du parent
                projectileInstance.transform.parent = null;
                AngleUpdateTimer = Time.realtimeSinceStartup * 1000;
                // retour à une stance normale
                m_ControlScript.ChangeState(PlayerControllerScript.STATE_IDLE);
                m_PhotonView.RPC("PhChangeState", PhotonTargets.Others, PlayerControllerScript.STATE_IDLE);
                m_PhotonView.RPC("PhSetAttacking", PhotonTargets.Others, false, false);
                // fait parler le personnage
                if (isSpecialAttack)
                    m_PhotonView.RPC("PhPlayerSpeaks", PhotonTargets.All, "firespecial");
                else
                    m_PhotonView.RPC("PhPlayerSpeaks", PhotonTargets.All, "fire");
            }
        }
	}
    
    Vector2 GetDirection()
    {
        Vector2 direction = ((Vector2) Input.mousePosition) - mouseStartPosition;
        float vert = Input.GetAxis("Vertical");
        float horiz = Input.GetAxis("Horizontal");
        if (direction.magnitude < 40)
            // pas de direction de tir
            direction = new Vector2(1,0);
        if (vert!=0 || horiz!=0)
            direction = new Vector2(horiz, vert);
        direction.Normalize();
        return direction;
    }
    
	void LateUpdate () {
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
    
    [PunRPC]
    void PhSetAimDir(float angle)
    {
        aimingAngle = angle;
        SetAimDir();
    }
    
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
            projectileInstance.transform.parent = null;
            wantRelease = false;
            wantAttack = false;
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
        attacking = false;
        wantRelease = false;
        wantAttack = false;
        m_PhotonView.RPC("PhSetAttacking", PhotonTargets.Others, false);
        if (projectileInstance != null)
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
