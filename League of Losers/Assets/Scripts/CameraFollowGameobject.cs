using UnityEngine;
using System.Collections;

/// <summary>
/// Force la caméra associée à suivre le déplacement d'un objet dans une zone spécifiée.
/// </summary>
public class CameraFollowGameobject : MonoBehaviour {
    
    public static GameObject target;
    private BoxCollider2D bounds;
    public float yOffset=.5f;
    public float catchupSpeed=10f;
    private float originalX;
    private float originalY;

    /// <summary>
    /// Use this for initialization
    /// </summary>
    void Start () {
        originalX = transform.position.x;
        originalY = transform.position.y;
        bounds = GetComponent<BoxCollider2D>();
    }
    
    /// <summary>
    /// Update is called once per frame
    /// </summary>
    void Update () {
        if (target != null)
        {
            float targetX = target.transform.position.x;
            float targetY = target.transform.position.y + yOffset;
            float curX = transform.position.x;
            float curY = transform.position.y;
            
            if (bounds != null)
            {
                float maxNegX = originalX - bounds.size.x/2 + bounds.offset.x;
                float maxNegY = originalY - bounds.size.y/2 + bounds.offset.y;
                float maxX = originalX + bounds.size.x/2 + bounds.offset.x;
                float maxY = originalY + bounds.size.y/2 + bounds.offset.y;
                targetX = (targetX < maxNegX) ? maxNegX : targetX;
                targetX = (targetX > maxX) ? maxX : targetX;
                targetY = (targetY < maxNegY) ? maxNegY : targetY;
                targetY = (targetY > maxY) ? maxY : targetY;
            }
            
            float diffX = targetX - curX;
            float diffY = targetY - curY;
            
            transform.position = new Vector3(curX + diffX/catchupSpeed, curY + diffY/catchupSpeed, transform.position.z);
        }
    }
}
