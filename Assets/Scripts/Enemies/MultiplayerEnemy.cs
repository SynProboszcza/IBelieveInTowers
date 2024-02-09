using Photon.Pun;
using UnityEngine;

public class MultiplayerEnemy : MonoBehaviour, IPunObservable
{
    public GameObject mainGame;
    [HideInInspector]
    public Transform[] waypoints;
    public float spawnTime = 3f;
    [HideInInspector]
    public float spawnTimeWhenAddedToList;
    public float speed = 2f;
    [Tooltip("Remember speed at which this unit was spawned with")]
    public float defaultSpeed;
    //[HideInInspector]
    public float currentHealth;
    public float maxHealth;
    public int damage = 0;
    [HideInInspector]
    public int waypointIndex = 0;
    public int moneyReward = 50;
    public int costToSpawn = 555;
    [Tooltip("Additional flip; use it if enemy sprites look to right; default enemy is looking left")]
    public bool spriteFlip = false;
    [Header("Exploding - leave empty except for slimer")]
    public bool canAttackTurrets = false;
    public bool isTurretClose = false;
    [HideInInspector]
    public Transform targetPosition;
    public GameObject explosionEffect;
    public float explosionRadius = 2f;
    public float explosionDamage = 150f;
    public float timeToShowExplosion = 0.25f;
    public float timeToDelayExplosionAnimation = 0.1f;
    public float timeBetweenExplosions = 2f;
    public float timeSinceLastShot = 0f;


    void Start()
    {
        currentHealth = maxHealth;
        defaultSpeed = speed;
        mainGame = GameObject.FindWithTag("SingleTagForMainGameLoop");
        if (waypoints == null || waypoints.Length == 0)
        {
            waypoints = mainGame.GetComponent<MultiplayerMainGameLoop>().waypoints;
        }
        if (this.GetComponent<PhotonView>().IsMine)
        {
            transform.position = waypoints[waypointIndex].position;
        }
        // it needs to be created in prefab, not in code
        if (canAttackTurrets)
        {
            GameObject ec = transform.Find("ExplosionCollider").gameObject;
            ec.GetComponent<CircleCollider2D>().radius = explosionRadius / 2;
        }
        gameObject.GetComponent<PhotonView>().OwnershipTransfer = OwnershipOption.Takeover;
        gameObject.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.MasterClient);

    }

    void Update()
    {
        Move();
        if (canAttackTurrets && isTurretClose)
        {
            ExplodeTimed();
        }
        if (gameObject.GetComponent<PhotonView>().IsMine)
        {
            if (currentHealth <= 0)
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
            if (waypoints.Length > waypointIndex) // So it does not result in outOfArrayIndex sth
            {
                // - ..... +
                if (waypoints[waypointIndex].transform.position.x < transform.position.x)
                {
                    // looking left
                    if (spriteFlip)
                    {
                        //print(gameObject.name + " is looking left now" + " my x: " + transform.position.x + " waypoint x:" + waypoints[waypointIndex].transform.position.x);
                        gameObject.GetComponent<SpriteRenderer>().flipX = true;
                    }
                    else
                    {
                        //print(gameObject.name + " is looking left now" + " my x: " + transform.position.x + " waypoint x:" + waypoints[waypointIndex].transform.position.x);
                        gameObject.GetComponent<SpriteRenderer>().flipX = false;
                    }
                }
                else if (waypoints[waypointIndex].transform.position.x > transform.position.x)
                {
                    // looking right
                    if (spriteFlip)
                    {
                        //print(gameObject.name + " is looking right now" + " my x: " + transform.position.x + " waypoint x:" + waypoints[waypointIndex].transform.position.x);
                        gameObject.GetComponent<SpriteRenderer>().flipX = false;
                    }
                    else
                    {
                        //print(gameObject.name + " is looking right now" + " my x: " + transform.position.x + " waypoint x:" + waypoints[waypointIndex].transform.position.x);
                        gameObject.GetComponent<SpriteRenderer>().flipX = true;
                    }
                }
                else
                {
                    // going up / down
                }
            }

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
            // --------------------------------------------------------------------------------- forDefender, isMoney,      amount,      fromWhere
            mainGame.GetComponent<PhotonView>().RPC("AddResourcesShowAtSpecifiedPoint", RpcTarget.All,  true,    true, moneyReward, new Vector2(transform.position.x, transform.position.y));
            // Defender gets full enemy reward, attacker gets 25% back of it
            // This is done to make gameplay more dynamic
            mainGame.GetComponent<PhotonView>().RPC("AddResourcesShowAtSpecifiedPoint", RpcTarget.All, false,    true, Mathf.FloorToInt(moneyReward * 0.25f), new Vector2(transform.position.x, transform.position.y));
            PhotonNetwork.Destroy(gameObject);
        }
        else
        {
            // Do nothing as this is not my object
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

    public void SetExplosiveDamage(int damage)
    {
        explosionDamage = damage;
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

    public void SetMaxHealth(float amount)
    {
        maxHealth = amount;
    }

    public int GetMoneyReward()
    {
        return this.moneyReward;
    }

    public void TakeDamage(float damage)
    {
        this.currentHealth -= damage;
    }

    public void ExplodeTimed()
    {
        //print("try to boom");
        if ((Time.time > (timeBetweenExplosions + timeSinceLastShot)))
        {
            Explode();
            timeSinceLastShot = Time.time;
        } //else
        //{
        //    print("Time.time: " + Time.time +
        //        "\ntimeBetweenExplosions: " + timeBetweenExplosions +
        //        "\ntimeSinceLastShot: " + timeSinceLastShot +
        //        "\nWhole thing: " + (Time.time > timeBetweenExplosions + timeSinceLastShot));
        //}
    }

    public void Explode()
    {
        // we need to check not if it collides with turret collider but with turret edges, like upgrade collider
        if (canAttackTurrets)
        {
            Collider2D[] affected = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
            foreach (Collider2D c in affected)
            {
                if (c.GetComponent<MultiUpgradeTurret>() != null)
                {
                    //var closestPoint = c.gameObject.GetComponent<BoxCollider2D>().ClosestPoint(transform.position);
                    //var distance = Vector3.Distance(closestPoint, transform.position);
                    //float damagePercent = Mathf.InverseLerp(explosionRadius, 0, distance);
                    //c.transform.parent.GetComponent<MainTurret>().TakeDamage(damage * damagePercent);
                    c.transform.parent.GetComponent<MainTurret>().TakeDamage(explosionDamage);
                }
            }
            // Show boom

            //print("boomed");
            GameObject boom = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            // Default sprite is of r=1, so we scale it with radious
            boom.transform.localScale = new Vector3(explosionRadius * 2, explosionRadius * 2, explosionRadius * 2);
            boom.GetComponent<ExplosionAnimation>().timeToDestruction = timeToShowExplosion;
            boom.GetComponent<ExplosionAnimation>().timeToDelayAnimation = timeToDelayExplosionAnimation;
            Destroy(boom, timeToShowExplosion);
            //Destroy(gameObject);
        }
        else
        {
            print("am not explodyy boy");
        }
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out MultiplayerEnemy enemy))
        {
            if (enemy.GetSpeed() <= this.speed)
            {
                speed = enemy.GetSpeed();
            }
        }
    }

    public void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out MultiplayerEnemy enemy))
        {
            if (enemy.GetSpeed() < this.speed)
            {
                speed = enemy.GetSpeed();
            }
        }
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<MultiUpgradeTurret>() != null)
        {
            isTurretClose = false;
        }
        if (collision.GetComponent<MultiplayerEnemy>() != null)
        {
            speed = defaultSpeed;
        }
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
