using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowPrice : MonoBehaviour
{
    public float durationSeconds = 2f;
    public float forceY = 2f;
    public Vector2 leftToRight = new Vector2(-1,1);
    public Vector2 topToBottom = new Vector2(0.5f,1.5f);


    private void Start()
    {
        Vector2 force = new Vector2(Random.Range(leftToRight.x, leftToRight.y), Random.Range(topToBottom.x * forceY, topToBottom.y * forceY));
        gameObject.GetComponent<Rigidbody2D>().AddForce(force);
        Destroy(gameObject, durationSeconds);
        //Destroy(transform.parent, durationSeconds+1);
        // Destruction of parent is handled by a different script attached to it
    }

}
