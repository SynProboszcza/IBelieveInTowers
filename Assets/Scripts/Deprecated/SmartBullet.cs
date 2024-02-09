using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmartBullet : MonoBehaviour
{
    public float speed = 0.01f;
    public float damage = 7;
    [Tooltip("One Hit Kill")]
    public bool ohk = false;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(ohk) { 
            Destroy(collision.gameObject);
            Destroy(gameObject);
        } else
        {
            collision.gameObject.GetComponent<Health>().takeDamage(damage);
        }
    }
}
