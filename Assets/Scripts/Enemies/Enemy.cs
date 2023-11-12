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
    public int moneyReward = 50;
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

    public void SetSpeed(float speed)
    {
        this.speed = speed;
    }

    public void SetDamage(int damage)
    {
        this.damage = damage;
    }

    public void SetWaypoints(Transform[] waypoints)
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

    public void SetHealth(float health)
    {
        gameObject.GetComponent<Health>().setHealth(health);
    }

    public int GetDamage()
    {
        return this.damage;
    }

    public void SetMoneyReward(int moneyReward)
    {
        this.moneyReward = moneyReward;
    }
    
    public int GetMoneyReward()
    {
        return this.moneyReward;
    }

}
