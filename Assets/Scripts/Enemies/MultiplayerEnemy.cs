using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MultiplayerEnemy : MonoBehaviour, IPunObservable
{
    public GameObject mainGame;
    [HideInInspector]
    public Transform[] waypoints;
    public float speed = 2f;
    //[HideInInspector]
    public float currentHealth;
    public float maxHealth;
    public int damage = 0;
    [HideInInspector]
    public int waypointIndex = 0;
    public int moneyReward = 50;
    public int costToSpawn = 555;

    void Start()
    {
        currentHealth = maxHealth;
        mainGame = GameObject.FindWithTag("SingleTagForMainGameLoop");
        if (this.GetComponent<PhotonView>().IsMine)
        {
            transform.position = waypoints[waypointIndex].position;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (SceneManager.GetActiveScene().name.Equals("MainMenu"))
        {
            Move();
            if (currentHealth <= 0)
            {
                Die();
            }
        }
        else
        {
            if (this.GetComponent<PhotonView>() != null && this.GetComponent<PhotonView>().IsMine)
            {
                Move();
                if (currentHealth <= 0)
                {
                    Die();
                }
            }
            else
            {
                // do nothing, its controlled by other player
            }
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
        }
        else
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
        //mainGame.GetComponent<MainGameLoop>().AddPlayerMoney(moneyReward);
        // if (mainGame != null) // Had to add this check for main menu enemies
        // {
        //     mainGame.GetComponent<MainGameLoop>().AddPlayerMoney(moneyReward);
        // }
        CrossSceneManager.instance.AddMoney(moneyReward);
        PhotonNetwork.Destroy(gameObject);
    }

    public void SetCostToSpawn(int amount)
    {
        this.costToSpawn = amount;
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

    public int GetCostToSpawn()
    {
        return this.costToSpawn;
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

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsReading)
        {
            transform.position = (Vector3)stream.ReceiveNext();
            currentHealth = (float)stream.ReceiveNext();
        }
        else if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(currentHealth);
        }
    }
}
