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

    void Start()
    {
        originalPosition = transform.position;
    }

    void Update()
    {
        if(distanceToLive < distanceFromShot)
        {
            // We can add death effects here
            Destroy(gameObject);
        } else
        {
            distanceFromShot = Vector3.Distance(originalPosition, gameObject.transform.position);
        }
        transform.Translate(new Vector3(speed*0.001f, 0f));
    }

    private void OnTriggerEnter2D(Collider2D collision)
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
            // We don't want bullets colliding with each other
            // because this generates bugs
        } else if (collision.gameObject.GetComponent<MultiplayerEnemy>() != null)
        {
            collision.gameObject.GetComponent<MultiplayerEnemy>().TakeDamage(damage);

        } else 
        {
            // Do nothing because it can destroy other objects
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

    public void SetDistanceToLive(float distanceToLive)
    {
        this.distanceToLive = distanceToLive;
    }
}
