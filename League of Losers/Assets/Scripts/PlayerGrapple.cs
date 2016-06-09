﻿using UnityEngine;
using System.Collections;

public class PlayerGrapple : MonoBehaviour {

    public int ShootingStrength = 25,
               ShootingAngle = 45,
               GrappleProjectionStrength = 500,
               GrappleDurationInMilliseconds = 500;
    public Rigidbody2D Body, 
                       ParentBody;
    public BoxCollider2D Collider;
    public Renderer Renderer;
    public Transform PopSpot;
    public bool IsHooked = false;
    public LineRenderer Rope;
    public PhotonView m_PhotonView;
    
    private float m_StartTime = 0;

    public Vector3 LeftShoot { get; private set; }
    public Vector3 RightShoot { get; private set; }
    public Vector3 LeftProjection { get; private set; }
    public Vector3 RightProjection { get; private set; }

    public bool IsGrappling
    {
        get
        {
            return !Body.isKinematic;
        }
        private set
        {
            if (!value)
            {
                Body.velocity = Vector3.zero;
                transform.position = PopSpot.position;
                transform.rotation = PopSpot.rotation;
            }
            else
                m_StartTime = Time.realtimeSinceStartup;

            Body.isKinematic = !value;
            Renderer.enabled = value;
            Collider.enabled = value;
        }
    }

    void Start()
    {
        LeftShoot = Quaternion.Euler(0, 0, -ShootingAngle) * Vector3.left;
        RightShoot = Quaternion.Euler(0, 0, ShootingAngle) * Vector3.right;
        LeftProjection = Quaternion.Euler(0, 0, -ShootingAngle - 15) * Vector3.left;
        RightProjection = Quaternion.Euler(0, 0, ShootingAngle + 15) * Vector3.right;
        Renderer.enabled = false;
        Collider.enabled = false;
    }

    void Update()
    {
        if (Input.GetButtonDown("Grapple"))
            IsGrappling = true;

        if (IsGrappling)
        {
            Body.velocity = (transform.parent.right.x > 0) ? RightShoot * ShootingStrength : LeftShoot * ShootingStrength;

            if (1000 * (Time.realtimeSinceStartup - m_StartTime) > GrappleDurationInMilliseconds)
                IsGrappling = false;
        }

        if (IsHooked)
            IsHooked = false;

        DrawRope();
        m_PhotonView.RPC("PhDrawRope", PhotonTargets.Others);
    }

    void OnCollisionEnter2D(Collision2D CollidedObject)
    {
        if (IsGrappling && CollidedObject.rigidbody != ParentBody)
        {
            m_StartTime = 0;
            IsGrappling = false;
            IsHooked = true;
        }
    }

    void DrawRope()
    {
        Vector3 start = Vector3.zero,
                end = Vector3.zero;

        start.x = PopSpot.position.x;
        start.y = PopSpot.transform.position.y;
        start.z = -5;
        end.x = transform.position.x;
        end.y = transform.position.y;
        end.z = -5;

        Rope.SetPositions(new Vector3[] { start, end });
    }

    [PunRPC]
    void PhDrawRope()
    {
        DrawRope();
    }
}
