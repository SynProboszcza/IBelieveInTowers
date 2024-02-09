using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Enemy : MonoBehaviour
{
    public GameObject mainGame;
    //[HideInInspector]
    public Transform[] waypoints;
    public float speed = 2f;
    [HideInInspector]
    public float currentHealth;
    public float maxHealth;
    public int damage = 0;
    [HideInInspector]
    public int waypointIndex = 0;
    public int moneyReward = 50;
    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;
        mainGame = GameObject.FindWithTag("SingleTagForMainGameLoop");
        transform.position = waypoints[waypointIndex].position;
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Move()
    {
        if (waypointIndex <= waypoints.Length - 1)
        {
            transform.position = Vector2.MoveTowards(transform.position,
               waypoints[waypointIndex].transform.position,
               speed * Time.deltaTime);

            //if (transform.position == waypoints[waypointIndex].transform.position)
            if (Mathf.Approximately(transform.position.x, waypoints[waypointIndex].transform.position.x) 
                && Mathf.Approximately(transform.position.y, waypoints[waypointIndex].transform.position.y)
                )
            {
                waypointIndex += 1;
            }
        } else
        {
            // This should never execute, leaving just as fallback
            // destroy is handled by Despawner, that also deals dmg
            Die();
        }
    }

    public void Die()
    {
        // Maybe add some effects to death, idk particles or
        // animated text of how much money it gave

        // Check for main menu anim., if mainGame GO does not have
        // MainGameLoop component dont try to add money
        if (mainGame != null && mainGame.TryGetComponent<MainGameLoop>(out _))
        {
            mainGame.GetComponent<MainGameLoop>().AddPlayerMoney(moneyReward);
        }
        Destroy(gameObject);
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

    public void SetHealth(float health)
    {
        this.currentHealth = health;
    }

    public void SetMoneyReward(int moneyReward)
    {
        this.moneyReward = moneyReward;
    }

    public float GetSpeed()
    {
        return this.speed;
    }

    public int GetDamage()
    {
        return this.damage;
    }

    public Transform[] GetWaypoints()
    {
        return this.waypoints;
    }

    public float GetHealth()
    {
        return this.currentHealth;
    }

    public float GetMaxHealth()
    {
        return this.maxHealth;
    }

    public int GetMoneyReward()
    {
        return this.moneyReward;
    }

    public void TakeDamage(float damage)
    {
        this.currentHealth -= damage;   
    }

}
