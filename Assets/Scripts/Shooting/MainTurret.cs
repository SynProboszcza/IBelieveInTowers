using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainTurret : MonoBehaviour, IPunObservable
{
    // Base bullet attributes
    // ----------------------------------------------------------------------
    [Header("Base attributes")]
    public float bulletSpeed = 100f; 
    public float bulletDamage = 13;
    [Tooltip("Amount of time in seconds inbetween shots")] 
    public float fireRate = 1;
    [Tooltip("Distance after which bullets disappear")]
    public float bulletRange = 5f;
    [Tooltip("Distance in which turrets can see")]
    public float turretRange = 2f;
    [Tooltip("Money awarded for destroying the turret")]
    public int moneyReward = 300;
    [Tooltip("Amount of money that's needed for every upgrade")]
    public int upgradeCost = 100;
    // Calculated bullet attributes
    // ----------------------------------------------------------------------
    [Header("Calculated attributes")]
    [Tooltip("NOT USED IN-GAME. Its base * current upgrade level")] 
    public float calculatedBulletSpeed; 
    [Tooltip("NOT USED IN-GAME. Its base * current upgrade level")] 
    public float calculatedBulletDamage;
    [Tooltip("NOT USED IN-GAME. Its base * current upgrade level")] 
    public float calculatedFireRate;
    [Tooltip("NOT USED IN-GAME. Its base * current upgrade level")] 
    public float calculatedBulletRange;
    [Tooltip("NOT USED IN-GAME. Its base * current upgrade level")] 
    public float calculatedTurretRange;
    [Tooltip("NOT USED IN-GAME. Its base * current upgrade level")] 
    public float calculatedMoneyReward;
    // Turret health
    // ----------------------------------------------------------------------
    [Header("Turret health attributes")]
    public float turretHealth = 200f;
    public float turretMaxHealth = 200f;
    [Tooltip("Time in-between self health-removal (default=1)")] 
    public float turretSelfDamageTime = 1f; // turret time Damage over Time
    [Tooltip("Amount of health removed per specified time (default=2)")] 
    public float turretSelfDamage = 2f; // turret time Damage over Time
    private Coroutine healthLossOp;
    private bool isTurretInvincible = false;
    private bool isDestroyRPCSent = false;
    // Multipliers of upgrades
    // ----------------------------------------------------------------------
    [Tooltip("Array of bullet damage bonuses for upgrading")]
    public float[] damageMultipliers = new float[] {1.0f, 1.2f, 1.4f, 1.6f};
    [Tooltip("Array of bullet speed bonuses for upgrading")]
    public float[] speedMultipliers = new float[] { 1.0f, 1.2f, 1.4f, 1.6f };
    [Tooltip("Array of fire rate bonuses for upgrading")]
    public float[] fireRateMultipliers = new float[] { 1.0f, 0.9f, 0.8f, 0.7f };
    [Tooltip("Array of bullet range bonuses for upgrading")]
    public float[] bulletRangeMultipliers = new float[] { 1.0f, 1.2f, 1.4f, 1.6f };
    [Tooltip("Array of turret seeing range bonuses for upgrading")]
    public float[] turretRangeMultipliers = new float[] { 1.0f, 1.2f, 1.4f, 1.6f };
    [Tooltip("Array of turret seeing range bonuses for upgrading")]
    public float[] turretmoneyRewardMultipliers = new float[] { 1.0f, 1.5f, 2f, 3f };
    // Shooting helpers
    // -----------------------------------------------------------------------
    [Header("Shooting helpers")]
    [Tooltip("Only applicable if isExsplosive is enabled")]
    public bool isExplosive = false;
    public float timeToShowExplosion = 0.2f;
    public bool isEnemyClose = false;
    public bool shootThisFrame = false;
    public List<GameObject> closeEnemies;
    // Settings for a shotgun mode
    // -----------------------------------------------------------------------
    [Header("Shotgun mode")]
    public bool isShotgun = false;
    public float shotgunSpreadInDegrees = 15f;
    [Tooltip("Amount of pellets added to the left and to the right")]
    public int shotgunPelletsToTheSides = 1;
    [Tooltip("Randomised speed modification to pellets - added to base pellet speed from -value to +value")]
    public float shotgunRandomSpeed = 15f;
    // Fire rate helper
    // ----------------------------------------------------------------------
    private float timeSinceLastShot;
    // ----------------------------------------------------------------------
    // ----------------------------------------------------------------------
    // Base references to modify and access
    // Every public GameObject field needs to be set in Inspector
    // ----------------------------------------------------------------------
    // ----------------------------------------------------------------------
    public Sprite[] baseSprites;
    public Sprite[] muzzleEffectsSprites;
    public GameObject[] bullets;
    public GameObject muzzleEffects;
    public GameObject gun;
    public GameObject shootSpawnPoint;
    public GameObject shopNodePrefab;
    public GameObject multishopNodePrefab;
    public GameObject mainGame;
    private GameObject bulletsCollection;
    private GameObject target;
    private GameObject bulletInstance;
    // Upgrade level
    // ----------------------------------------------------------------------
    public int upgradeLevel = 0;
    // Distance to disable muzzle effects
    // ----------------------------------------------------------------------
    [Tooltip("Distance after which muzzle effects disappear")]
    public float distanceToShutoffMuzzleEffects = 0.5f;
    // OneHitKill mechanic, potential upgrade
    // -----------------------------------------------------------------------
    [Tooltip("OneHitKill")]
    public bool ohk = false;
    // Current target location
    // -----------------------------------------------------------------------
    public Vector2 targetPosition;
    // SpriteRenderers of gameObjects we want to update sprites of
    // -----------------------------------------------------------------------
    public SpriteRenderer srBase;
    //public SpriteRenderer srGun; // For now gun does not need changing with upgrades
    public SpriteRenderer srMuzzleEffects;
    public string niceName = "nice name not set in Editor";

    void Start()
    {
        srBase = GetComponent<SpriteRenderer>();
        if (mainGame == null)
        {
            mainGame = GameObject.FindGameObjectWithTag("SingleTagForMainGameLoop");
        }
        //srGun = transform.GetChild(0).GetComponent<SpriteRenderer>(); // For now gun does not need changing with upgrades
        srMuzzleEffects = transform.Find("Gun").transform.Find("Muzzle").transform.Find("MuzzleEffects").GetComponent<SpriteRenderer>();
        //transform.Find("UpgradeCollider").GetComponent<MultiUpgradeTurret>().SetUpgradeCost(upgradeCost);
        muzzleEffects.SetActive(false);
        if (bulletsCollection == null) {
            bulletsCollection = new GameObject("BulletsCollection");
            bulletsCollection.transform.SetParent(transform);
        }
        if (CrossSceneManager.instance != null && CrossSceneManager.instance.invincibleTurrets)
        {
            turretMaxHealth = 50000;
            isTurretInvincible = true;
            // Update also refreshes this
        } else
        {
            turretHealth = turretMaxHealth;
        }
        if (shotgunRandomSpeed > (bulletSpeed - 10))
        {
            Debug.LogWarning("Shotgun random speed value exceeded maximal allowed value(" + bulletSpeed + " - 10). Setting default(" + (bulletSpeed / 2) + ") as fallback");
            shotgunRandomSpeed = bulletSpeed / 2;
        }
        UpdateAndShowTurretRange();
        StartTakingConstantDamage();
    }

    void Update()
    {
        // -----------------------------------------------------------------------------
        // Update calculated values to show in inspector
        // -----------------------------------------------------------------------------
        calculatedBulletSpeed  = bulletSpeed  * speedMultipliers[upgradeLevel];
        calculatedBulletDamage = bulletDamage * damageMultipliers[upgradeLevel];
        calculatedFireRate     = fireRate     * fireRateMultipliers[upgradeLevel];
        calculatedBulletRange  = bulletRange  * bulletRangeMultipliers[upgradeLevel];
        calculatedTurretRange  = turretRange  * turretRangeMultipliers[upgradeLevel];
        calculatedMoneyReward  = moneyReward  * turretmoneyRewardMultipliers[upgradeLevel];
        // -----------------------------------------------------------------------------
        // Refresh max health if invincible turrets are enabled
        // -----------------------------------------------------------------------------
        if (isTurretInvincible)
        {
            turretHealth = turretMaxHealth;
        }
        // -----------------------------------------------------------------------------
        // Check for existing bullets, and if they're far enough to disable muzzle effects
        // -----------------------------------------------------------------------------
        if (bulletInstance == null)
        {
            muzzleEffects.SetActive(false);
        } else if (bulletInstance != null)
        {
            float distance = Vector3.Distance(bulletInstance.transform.position, shootSpawnPoint.transform.position);
            if(distance > distanceToShutoffMuzzleEffects)
            {
                muzzleEffects.SetActive(false);
            }
        }
        // -----------------------------------------------------------------------------
        // Handle shooting:
        //  calculate angle, instantiate and configure correct bullets,
        //  rotate the gun pointing at enemy, handle fire rate,
        //  handle shotgun shooting.
        // -----------------------------------------------------------------------------
        if (isEnemyClose)
        {
            // -----------------------------------------------------------------------------
            // Target selection
            // -----------------------------------------------------------------------------
            float distance = Mathf.Infinity;
            for (int i = 0; i < closeEnemies.Count; i++)
            {
                float newDistance = Vector3.Distance(transform.position, closeEnemies[i].transform.position);
                if (newDistance < distance)
                {
                    distance = newDistance;
                    target = closeEnemies[i];
                }
            }
            targetPosition = target.transform.position;
            Vector2 direction = new Vector2(shootSpawnPoint.transform.position.x, shootSpawnPoint.transform.position.y) - targetPosition;
            direction.Normalize();
            Quaternion rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 180f);
            
            gun.transform.rotation = rotation;

            // -----------------------------------------------------------------
            // Fire rate handling has a backdoor to debug from Editor
            // (shootThisFrame can be checked/unchecked to see what happens)
            // -----------------------------------------------------------------
            if (
                (Time.time > (fireRate * fireRateMultipliers[upgradeLevel]) + timeSinceLastShot)
                || shootThisFrame
               )
            {
                if (isShotgun)
                {
                    // We're going from i = 1 because we multiply shotgunSpread by it,
                    // so zero would be the same rotation as shooting straight
                    for (int i = 1; i <= shotgunPelletsToTheSides; i++)
                    {
                        // Bullet to the side
                        Quaternion _rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 180f - shotgunSpreadInDegrees * i);
                        float _speedMod = Random.value * shotgunRandomSpeed - Random.value * shotgunRandomSpeed; // Creates a range from -value to + value
                        bulletInstance = ShootAtTarget(bullets[upgradeLevel], shootSpawnPoint.transform.position,
                            _rotation, bulletSpeed + _speedMod, bulletDamage, bulletRange, ohk, isExplosive, timeToShowExplosion);
                        // Bullet centered
                        _speedMod = Random.value * shotgunRandomSpeed - Random.value * shotgunRandomSpeed;
                        bulletInstance = ShootAtTarget(bullets[upgradeLevel], shootSpawnPoint.transform.position,
                            rotation, bulletSpeed + _speedMod, bulletDamage, bulletRange, ohk, isExplosive, timeToShowExplosion);
                        // Bullet to the other side
                        _speedMod = Random.value * shotgunRandomSpeed - Random.value * shotgunRandomSpeed;
                        _rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 180f + shotgunSpreadInDegrees * i);
                        bulletInstance = ShootAtTarget(bullets[upgradeLevel], shootSpawnPoint.transform.position,
                            _rotation, bulletSpeed + _speedMod, bulletDamage, bulletRange, ohk, isExplosive, timeToShowExplosion);
                        
                        timeSinceLastShot = Time.time;
                    }
                } else
                {
                    // This is main shooting
                    bulletInstance = ShootAtTarget(bullets[upgradeLevel], shootSpawnPoint.transform.position,
                        rotation, bulletSpeed, bulletDamage, bulletRange, ohk, isExplosive, timeToShowExplosion);
                    timeSinceLastShot = Time.time;
                }
            } 
            
        }
        // -----------------------------------------------------------------------------
        // We need to update base sprite every frame, so it does not lag behind shooting,
        // because muzzle effects upgrades are processed with every shot,
        // not with every frame.
        // -----------------------------------------------------------------------------
        UpdateBaseSprite();

        // Check for death of turret
        if (turretHealth <= 0 && !isDestroyRPCSent) {
            DieAndLeaveShopNode();
            isDestroyRPCSent = true;
        }
    }

    private void StartTakingConstantDamage()
    {
        healthLossOp = StartCoroutine(TakeConstantDamage());
        //print("started DOT");
    }

    IEnumerator TakeConstantDamage()
    {
        //print("started to take constant damage");
        for (;;)
        {
            yield return new WaitForSeconds(turretSelfDamageTime);
            TakeDamage(turretSelfDamage);
            //print("ticked damage");
        }
    }

    public float GetHealth()
    {
        return this.turretHealth;
    }

    public float GetMaxHealth()
    {
        return this.turretMaxHealth;
    }

    public void TakeDamage(float _damage)
    {
        if (CrossSceneManager.instance.invincibleTurrets)
        {
            return;
        }
        this.turretHealth -= _damage;
    }

    private void DieAndLeaveShopNode() {
        // TryGetComponent is needed as this script is also used in singleplayer
        if (gameObject.TryGetComponent<PhotonView>(out _) && gameObject.GetComponent<PhotonView>().IsMine) 
        {
            // If multiplayer
            GameObject go = Instantiate(multishopNodePrefab, new Vector3(transform.position.x, transform.position.y, MainGameLoop.shopNodesZOffset), Quaternion.identity);
            go.transform.SetParent(mainGame.GetComponent<MultiplayerMainGameLoop>().shopNodesCollection.transform);
            int _finalMoneyReward = Mathf.FloorToInt(moneyReward * turretmoneyRewardMultipliers[upgradeLevel]);
            mainGame.GetComponent<PhotonView>().RPC("AddResourcesShowAtSpecifiedPoint", RpcTarget.All,
                false, true, _finalMoneyReward, new Vector2(transform.position.x, transform.position.y));
            // bool forDefender, bool isMoney, int amount, Vector2 fromWhere
            PhotonNetwork.Destroy(gameObject);
        } else if(gameObject.TryGetComponent<PhotonView>(out _) && !gameObject.GetComponent<PhotonView>().IsMine)
        {
            // If online but turret is not mine
            //GameObject go = Instantiate(multishopNodePrefab, new Vector3(transform.position.x, transform.position.y, MainGameLoop.shopNodesZOffset), Quaternion.identity);
            //go.transform.SetParent(mainGame.GetComponent<MultiplayerMainGameLoop>().shopNodesCollection.transform);
            //int _finalMoneyReward = Mathf.FloorToInt(moneyReward * turretmoneyRewardMultipliers[upgradeLevel]);
            //mainGame.GetComponent<PhotonView>().RPC("AddResourcesShowAtSpecifiedPoint", RpcTarget.All, false, true, _finalMoneyReward, new Vector2(transform.position.x, transform.position.y));
            // bool forDefender, bool isMoney, int amount, Vector2 fromWhere
            //PhotonNetwork.Destroy(gameObject);
            // Sending RPC to owner so he can PhotonNetwork.Destroy it
            gameObject.GetComponent<PhotonView>().RPC("DestroyMe", RpcTarget.Others);

        }
        else if(!gameObject.TryGetComponent<PhotonView>(out _))
        {
            // If singleplayer
            Instantiate(shopNodePrefab, new Vector3(transform.position.x, transform.position.y, MainGameLoop.shopNodesZOffset), Quaternion.identity);
            Destroy(gameObject);
        }
    }

    private void UpdateAndShowTurretRange()
    {
        if (transform.Find("Range") != null) // nullcheck for main menu
        {
            gameObject.GetComponent<CircleCollider2D>().radius = turretRange * turretRangeMultipliers[upgradeLevel];
            transform.Find("Range").GetComponent<SetAndShowTurretRange>().UpdateRange();
        }
    }

    private void DestroyBullet(GameObject bullet)
    {
        //maybe add effects
        Destroy(bullet);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Enemy"))
        {
            closeEnemies.Add(collision.gameObject);
            target = collision.gameObject;
            isEnemyClose = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (closeEnemies.Contains(collision.gameObject))
        {
            closeEnemies.Remove(collision.gameObject);
            isEnemyClose = false;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            if (!closeEnemies.Contains(collision.gameObject))
            {
                closeEnemies.Add(collision.gameObject);
            }
            //target = collision.gameObject;
            isEnemyClose = true;
        }
    }

    private GameObject ShootAtTarget(GameObject bullet, Vector3 position, Quaternion rotation,
        float baseBulletSpeed, float baseBulletDamage, float baseBulletRange, bool oneHitKill, bool isExplosive, float timeToShowExplosion)
    {
        GameObject _bullet = (GameObject)Instantiate(bullet, position, rotation);
        //GameObject _bullet = BulletPool.instance.GetBulletFromPool("smgBullets", 4);
        //_bullet.transform.position = position;
        //_bullet.transform.rotation = rotation;
        _bullet.GetComponent<Bullet>().SetSpeed(baseBulletSpeed * speedMultipliers[upgradeLevel]);
        _bullet.GetComponent<Bullet>().SetDamage(baseBulletDamage * damageMultipliers[upgradeLevel]);
        _bullet.GetComponent<Bullet>().SetDistanceToLive(baseBulletRange * bulletRangeMultipliers[upgradeLevel]);
        _bullet.GetComponent<Bullet>().Setohk(oneHitKill);
        _bullet.GetComponent<Bullet>().SetIsExplosive(isExplosive);
        _bullet.GetComponent<Bullet>().SetTimeToShowExplosion(timeToShowExplosion);
        _bullet.transform.SetParent(bulletsCollection.transform);
        UpdateMuzzleEffects();
        muzzleEffects.SetActive(true);
        return _bullet;
    }

    public void UpdateMuzzleEffects()
    {
        // Exposed so other objects can update it
        // We dont need to include it in Update method, just every shot
        srMuzzleEffects.sprite = muzzleEffectsSprites[upgradeLevel];
    }

    public void UpdateBaseSprite()
    {
        srBase.sprite = baseSprites[upgradeLevel];
    }

    public int GetUpgradeLevel()
    {
        return upgradeLevel;
    }

    [PunRPC]
    public void DestroyMe()
    {
        GameObject go = Instantiate(multishopNodePrefab, new Vector3(transform.position.x, transform.position.y, MainGameLoop.shopNodesZOffset), Quaternion.identity);
        go.transform.SetParent(mainGame.GetComponent<MultiplayerMainGameLoop>().shopNodesCollection.transform);
        int _finalMoneyReward = Mathf.FloorToInt(moneyReward * turretmoneyRewardMultipliers[upgradeLevel]);
        mainGame.GetComponent<PhotonView>().RPC("AddResourcesShowAtSpecifiedPoint", RpcTarget.All,
            false, true, _finalMoneyReward, new Vector2(transform.position.x, transform.position.y));
        // bool forDefender, bool isMoney, int amount, Vector2 fromWhere
        PhotonNetwork.Destroy(gameObject);
    }

    [PunRPC]
    public bool LevelUp()
    {
        if (upgradeLevel < 3)
        {
            if (CrossSceneManager.instance.amIDefender 
                && CrossSceneManager.instance.CanPlayerAffordWithMoney(upgradeCost))
            {
                // Money cost is handled by CrossSceneManager.cs
                // Upgrades to bullet speed, damage and bulletRange are 
                // handled by arrays of multipliers
                // check if it has enough money and substract it
                CrossSceneManager.instance.PayWithMoney(upgradeCost);
            }
            upgradeLevel++;
            turretMaxHealth *= 1.1f;
            turretHealth *= 1.1f;
            float _toAdd = (turretMaxHealth - turretHealth)/2;
            turretHealth += _toAdd;
            UpdateAndShowTurretRange();
            return true;
        }
        return false;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsReading)
        {
            turretHealth = (float)stream.ReceiveNext();
            upgradeLevel = (int)stream.ReceiveNext();
        }
        else if (stream.IsWriting)
        {
            stream.SendNext(turretHealth);
            stream.SendNext(upgradeLevel);
        }
    }

}
