using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlWithArrows : MonoBehaviour
{
    public float speed = 1;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            gameObject.transform.Translate(new Vector2(0, speed));
        } else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            gameObject.transform.Translate(new Vector2(0, -speed));
        } else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            gameObject.transform.Translate(new Vector2(-speed, 0));
        } else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            gameObject.transform.Translate(new Vector2(speed, 0));
        }
    }
}
