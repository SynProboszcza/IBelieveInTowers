using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Transform[] waypoints;
    public GameObject playerHealth;
    public float speed = 2f;
    public int damage = 0;
    public int waypointIndex = 0;
    // Start is called before the first frame update
    void Start()
    {
        transform.position = waypoints[waypointIndex].position;
    }

    // Update is called once per frame
    void Update()
    {
        Move();
    }

    public void setSpeed(float speed)
    {
        this.speed = speed;
    }

    public void setDamage(int damage)
    {
        this.damage = damage;
    }

    public void setWaypoints(Transform[] waypoints)
    {
        this.waypoints = waypoints;
    }

    private void Move()
    {
        if (waypointIndex <= waypoints.Length - 1)
        {
            transform.position = Vector2.MoveTowards(transform.position,
               waypoints[waypointIndex].transform.position,
               speed * Time.deltaTime);

            if (transform.position == waypoints[waypointIndex].transform.position)
            {
                waypointIndex += 1;
            }
        } else
        {
            //this should never execute, leaving just as fallback
            //destroy is handled by Despawner, that also deals dmg
            Destroy(gameObject);
        }
    }

    public void setHealth(float health)
    {
        gameObject.GetComponent<Health>().setHealth(health);
    }

    public int getDamage()
    {
        return this.damage;
    }

}
