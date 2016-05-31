using UnityEngine;
using System.Collections;

public class CameraFollowGameobject : MonoBehaviour {
    
    public static GameObject target;
    private BoxCollider2D bounds;
    public float yOffset=.5f;
    public float catchupSpeed=10f;
    private float originalX;
    private float originalY;

    // Use this for initialization
    void Start () {
        originalX = transform.position.x;
        originalY = transform.position.y;
        bounds = GetComponent<BoxCollider2D>();
    }
    
    // Update is called once per frame
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
