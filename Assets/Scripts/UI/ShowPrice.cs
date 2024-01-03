using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowPrice : MonoBehaviour
{
    public float durationSeconds = 2f;
    public float speed = 2f;

    private void Update()
    {
        for(float i = 0; i < durationSeconds; i += Time.deltaTime)
        {
            transform.position = Vector2.MoveTowards(transform.position, new Vector2(transform.position.x, transform.position.y+0.2f), speed * Time.deltaTime);
        }
        Destroy(gameObject);
    }

}
