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
    [Header("Shooting parts - leave empty except for slimer")]
    public bool canAttackTurrets = false;
    public GameObject bullet;
    public Transform targetPosition;
    public int bulletAmount = 10;


    void Start()
    {
        currentHealth = maxHealth;
        mainGame = GameObject.FindWithTag("SingleTagForMainGameLoop");
        if (waypoints == null || waypoints.Length == 0)
        {
            waypoints = mainGame.GetComponent<MultiplayerMainGameLoop>().waypoints;
        }
        if (this.GetComponent<PhotonView>().IsMine)
        {
            transform.position = waypoints[waypointIndex].position;
        }
    }

    void Update()
    {
        Move();
        if (gameObject.GetComponent<PhotonView>().IsMine)
        {
            if(currentHealth <= 0) 
            {
                Die();
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
            //print("zero waypoints");
            // This should never execute, leaving just as fallback
            // destroy is handled by Despawner, that also deals dmg
            Die();
        }
    }

    public void Die()
    {
        // Maybe add some effects to death, idk particles or
        // animated text of how much money it gave
        if (gameObject.GetComponent<PhotonView>().IsMine)
        {
            // isMine == true means im attacker
            // gameObject.GetComponent<PhotonView>().RPC("AddResources", RpcTarget.All, true, true, 50);
            // CrossSceneManager.instance.AddMoney(moneyReward, transform.position);
            PhotonNetwork.Destroy(gameObject);
        } else
        {
            // -------------------------------------------------------------  forDefender, isMoney, amount
            gameObject.GetComponent<PhotonView>().RPC("AddResources", RpcTarget.All, true, true,    moneyReward);
        }
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
    
    //public void ShootAround(Transform targetTurretPosition)
    //{
    //    Quaternion rotation = Quaternion.Euler(0, 0, Mathf.Atan2(targetTurretPosition.position.y, targetTurretPosition.position.x) * Mathf.Rad2Deg + 180f);
    //    Instantiate(bullet, transform.position, rotation);
    //}

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.TryGetComponent<MultiplayerEnemy>(out MultiplayerEnemy enemy))
        {
            if(enemy.GetSpeed() <= this.speed)
            {
                this.speed = enemy.GetSpeed();
            }
        }
        //ShootAround(collision.transform);
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

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] instantiationData = info.photonView.InstantiationData;
        waypoints = (Transform[])instantiationData[0];
        print("got sth");
    }
}
