using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngineInternal;

public class GoInSquares : MonoBehaviour
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
        //horizontal = Input.GetAxisRaw("Horizontal");
        //vertical = Input.GetAxisRaw("Vertical");

        
        if (frames < 30*timeCoefficient) {
            //horizontal += movespeed;
            rigidbody2.velocity = new Vector2(1, 0);
        } else if (frames > (30 * timeCoefficient) && frames < (60 * timeCoefficient))
        {
            //vertical += movespeed;
            rigidbody2.velocity = new Vector2(0, 1);
        } else if (frames > (60 * timeCoefficient) && frames < (90 * timeCoefficient))
        {
            //horizontal -= movespeed * backFeedBack;
            rigidbody2.velocity = new Vector2(-1, 0);
        } else if(frames > (90 * timeCoefficient) && frames < (120 * timeCoefficient))
        {
            //vertical -= movespeed * backFeedBack;
            rigidbody2.velocity = new Vector2(0, -1);
        } else if (frames > (120 * timeCoefficient)) 
        {
            frames = 0;
        }

        if(frames == (30*timeCoefficient) || frames == (60*timeCoefficient) || frames == (90*timeCoefficient) || frames == (120 * timeCoefficient) || frames == 0)
        {
            horizontal = 0;
            vertical = 0;
        }

        frames++;
    }

    private void FixedUpdate()
    {
        //rigidbody2.velocity = new Vector2(horizontal * movespeed, vertical * movespeed);
    }
}
