using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoLeftRight : MonoBehaviour
{
    Rigidbody2D rigidbody2;
    float horizontal;
    float vertical;
    public float movespeed = 1f;
    private int frames = 0;
    [Tooltip("This times 30 frames")]
    public float timeCoefficient = 1f;
    [Tooltip("This times movement down and left")]
    public float backFeedBack = 1;
    // Start is called before the first frame update
    void Start()
    {
        rigidbody2 = GetComponent<Rigidbody2D>();
        horizontal = transform.localScale.x;
        vertical = transform.localScale.y;
    }

    // Update is called once per frame
    void Update()
    {
        if (frames < 30 * timeCoefficient)
        {
            //horizontal += movespeed;
            rigidbody2.velocity = new Vector2(-1, 0);
        }
        else if (frames > (30 * timeCoefficient) && frames < (60 * timeCoefficient))
        {
            //vertical += movespeed;
            rigidbody2.velocity = new Vector2(1, 0);
        }
        else if (frames > (60 * timeCoefficient))
        {
            frames = 0;
        }

        frames++;

    }
}
