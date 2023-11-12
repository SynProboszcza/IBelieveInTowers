using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed;
    public float damage;
    public float distanceToLive;
    public float distanceFromShot;
    public Vector2 originalPosition;
    [Tooltip("One Hit Kill - deletes whatever it collides with")]
    public bool ohk = false;
    // Start is called before the first frame update
    void Start()
    {
        originalPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if(distanceToLive < distanceFromShot)
        {
            Destroy(gameObject);
        } else
        {
            distanceFromShot = Vector3.Distance(originalPosition, gameObject.transform.position);
        }
        transform.Translate(new Vector3(speed*0.001f, 0f));
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (ohk)
        {
            Destroy(collision.gameObject);
            Destroy(gameObject);
        } else if(collision.gameObject.GetComponent<Enemy>() != null)
        {
            collision.gameObject.GetComponent<Enemy>().TakeDamage(damage);
            Destroy(gameObject);
        } else if (collision.gameObject.tag == "Bullet")
        {
            //do nothing
        } else
        {
            //do nothing because it can destroy other objects
            //Destroy(collision.gameObject);
        }
    }

    public void SetSpeed(float speed)
    {
        this.speed = speed;
    }

    public void SetDamage(float damage)
    {
        this.damage = damage;
    }

    public void Setohk(bool ohk)
    {
        this.ohk = ohk;
    }
}
