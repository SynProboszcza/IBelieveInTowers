using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainTurret : MonoBehaviour
{
    // Base references to modify and access
    // All public GameObjects fields needs to be set in Inspector
    // ----------------------------------------------------------------------
    public Sprite[] baseSprites;
    public Sprite[] muzzleEffectsSprites;
    public GameObject[] bullets;
    public GameObject muzzleEffects;
    public GameObject gun;
    public GameObject shootSpawnPoint;
    public GameObject shopNodePrefab;
    private GameObject bulletsCollection;
    private GameObject target;
    private GameObject bulletInstance;
    // Upgrade slider
    // ----------------------------------------------------------------------
    [Range(0, 3)]
    public int upgradeLevel = 0;
    // Base bullet attributes
    // ----------------------------------------------------------------------
    public float bulletSpeed = 100f; 
    public float bulletDamage = 13;
    [Tooltip("Amount of time in seconds inbetween shots")] 
    public float fireRate = 1;
    [Tooltip("Distance after which bullets disappear")]
    public float bulletRange = 5f;
    // Fire rate  
    // ----------------------------------------------------------------------
    private float timeSinceLastShot;
    // Distance to disable muzzle effects
    // ----------------------------------------------------------------------
    [Tooltip("Distance after which muzzle effects disappear")]
    public float distanceToShutoffMuzzleEffects = 0.5f;
    // Upgrade cost
    // ----------------------------------------------------------------------
    public int upgradeCost = 100;
    // Turret health
    // ----------------------------------------------------------------------
    public float turretHealth = 200f;
    private float turretMaxHealth;
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
    // OneHitKill mechanic, potential upgrade
    // -----------------------------------------------------------------------
    [Tooltip("OneHitKill")]
    public bool ohk = false;
    // Shooting helpers
    // -----------------------------------------------------------------------
    public bool isEnemyClose = false;
    public bool shootThisFrame = false;
    // Settings for a shotgun mode
    // -----------------------------------------------------------------------
    public bool isShotgun = false;
    public float shotgunSpreadInDegrees = 15f;
    [Tooltip("Amount of pellets added to the left and to the right")]
    public int shotgunPelletsToTheSides = 1;
    // Current target location
    // -----------------------------------------------------------------------
    public Vector2 targetPosition;
    // SpriteRenderers of gameObjects we want to update sprites of
    // -----------------------------------------------------------------------
    public SpriteRenderer srBase;
    //public SpriteRenderer srGun; // For now gun does not need changing with upgrades
    public SpriteRenderer srMuzzleEffects;

    void Start()
    {
        srBase = GetComponent<SpriteRenderer>();
        //srGun = transform.GetChild(0).GetComponent<SpriteRenderer>(); // For now gun does not need changing with upgrades
        srMuzzleEffects = transform.Find("Gun").transform.Find("Muzzle").transform.Find("MuzzleEffects").GetComponent<SpriteRenderer>();
        transform.Find("UpgradeCollider").GetComponent<UpgradeTurret>().SetUpgradeCost(upgradeCost);
        muzzleEffects.SetActive(false);
        if (bulletsCollection == null) {
            bulletsCollection = new GameObject("BulletsCollection");
        }
        turretMaxHealth = turretHealth;
    }

    void Update()
    {
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
        if (isEnemyClose && target != null)
        {
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
                        bulletInstance = ShootAtTarget(bullets[upgradeLevel], shootSpawnPoint.transform.position, _rotation, bulletSpeed, bulletDamage, bulletRange, ohk);
                        
                        // Bullet centered
                        bulletInstance = ShootAtTarget(bullets[upgradeLevel], shootSpawnPoint.transform.position, rotation, bulletSpeed, bulletDamage, bulletRange, ohk);
                        
                        // Bullet to the other side
                        _rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 180f + shotgunSpreadInDegrees * i);
                        bulletInstance = ShootAtTarget(bullets[upgradeLevel], shootSpawnPoint.transform.position, _rotation, bulletSpeed, bulletDamage, bulletRange, ohk);
                        
                        timeSinceLastShot = Time.time;
                    }
                } else
                {
                    // This is main shooting
                    bulletInstance = ShootAtTarget(bullets[upgradeLevel], shootSpawnPoint.transform.position, rotation, bulletSpeed, bulletDamage, bulletRange, ohk);
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
        if (turretHealth <= 0) {
            DieAndLeaveShopNode();
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

    private void DieAndLeaveShopNode() {
        Instantiate(shopNodePrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
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
            target = collision.gameObject;
            isEnemyClose = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        isEnemyClose = false;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            target = collision.gameObject;
            isEnemyClose = true;
        }
    }

    private GameObject ShootAtTarget(GameObject bullet, Vector3 position, Quaternion rotation, float baseBulletSpeed, float baseBulletDamage, float baseBulletRange, bool oneHitKill)
    {
        GameObject _bullet = (GameObject)Instantiate(bullet, position, rotation);
        _bullet.GetComponent<Bullet>().SetSpeed(baseBulletSpeed * speedMultipliers[upgradeLevel]);
        _bullet.GetComponent<Bullet>().SetDamage(baseBulletDamage * damageMultipliers[upgradeLevel]);
        _bullet.GetComponent<Bullet>().SetDistanceToLive(baseBulletRange * bulletRangeMultipliers[upgradeLevel]);
        _bullet.GetComponent<Bullet>().Setohk(oneHitKill);
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

    public bool LevelUp()
    {
        if(upgradeLevel < 3)
        {
            // Money cost is handled by UpgradeTurret.cs
            // Upgrades to bullet speed, damage and bulletRange are 
            // handled by arrays of multipliers

            // Potentially upgrade sell cost

            upgradeLevel++;
            return true;
        } else
        {
            return false;
        }
    }
}
