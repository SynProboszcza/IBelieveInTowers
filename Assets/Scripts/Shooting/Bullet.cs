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
    public bool isExplosive;
    public float explosionRadious = 2f;
    public float timeToShowExplosion = 0.2f;
    public GameObject explosionEffect;

    void Start()
    {
        originalPosition = transform.position;
    }

    void Update()
    {
        if(distanceToLive < distanceFromShot)
        {
            // We can add death effects here
            //gameObject.SetActive(false);
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
            if (isExplosive)
            {
                Collider2D[] affected = Physics2D.OverlapCircleAll(transform.position, explosionRadious);
                foreach (Collider2D c in affected)
                {
                    if (c.TryGetComponent<MultiplayerEnemy>(out _))
                    {
                        var closestPoint = c.ClosestPoint(transform.position);
                        var distance = Vector3.Distance(closestPoint, transform.position);
                        float damagePercent = Mathf.InverseLerp(explosionRadious, 0, distance);
                        c.GetComponent<MultiplayerEnemy>().TakeDamage(damage * damagePercent);
                    }
                }
                // Show boom
             
                GameObject boom = Instantiate(explosionEffect, transform.position, Quaternion.identity);
                // Default sprite is of r=1, so we scale it with radious
                boom.transform.localScale = new Vector3(explosionRadious * 2, explosionRadious * 2, explosionRadious * 2);
                Destroy(boom, timeToShowExplosion);
                Destroy(gameObject);
                //gameObject.SetActive(false);
            }
            else
            {
                collision.gameObject.GetComponent<MultiplayerEnemy>().TakeDamage(damage);
                Destroy(gameObject);
                //gameObject.SetActive(false);
            }

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

    public void SetIsExplosive(bool isExplosive)
    {
        this.isExplosive = isExplosive;
    }

    public void SetTimeToShowExplosion(float time)
    {
        this.timeToShowExplosion = time;
    }

    public void SetDistanceToLive(float distanceToLive)
    {
        this.distanceToLive = distanceToLive;
    }
}
