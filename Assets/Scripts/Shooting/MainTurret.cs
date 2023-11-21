using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainTurret : MonoBehaviour
{
    public Sprite[] baseSprites;
    public Sprite[] muzzleEffectsSprites;
    public GameObject[] bullets;
    public GameObject muzzleEffects;
    public GameObject gun;
    public GameObject shootSpawnPoint;
    public GameObject target;
    private GameObject bulletInstance;
    [Range(0, 3)]
    public int upgradeLevel = 0;
    //public int layerIndex = 0;
    public float speed = 0.1f;
    public float damage = 13;
    //private float angle = 0;
    [Tooltip("Amount of time in seconds inbetween shots")] 
    public float fireRate = 1;
    private float timeSinceLastShot;
    [Tooltip("Distance after which muzzle effects disappear")]
    public float distanceToShutoffMuzzleEffects = 0.5f;
    [Tooltip("Distance after which bullets disappear")]
    public float bulletRange = 5f;
    [Tooltip("OneHitKill")]
    public bool ohk = false;
    public bool isEnemyClose = false;
    public bool shootThisFrame = false;
    public bool isShotgun = false;
    public float shotgunSpreadInDegrees = 15f;
    [Tooltip("Amount of pellets added to the left and to the right")]
    public int shotgunPelletsToTheSides = 1;
    public Vector2 targetPosition;
    public SpriteRenderer srBase;
    public SpriteRenderer srGun; // For now gun does not need changing with upgrades
    public SpriteRenderer srMuzzleEffects;
    // Start is called before the first frame update
    void Start()
    {
        srBase = GetComponent<SpriteRenderer>();
        srGun = transform.GetChild(0).GetComponent<SpriteRenderer>(); // For now gun does not need changing with upgrades
        //srMuzzleEffects = transform.GetChild(0).transform.GetChild(0).transform.GetChild(0).GetComponent<SpriteRenderer>();
        srMuzzleEffects = transform.Find("Gun").transform.Find("Muzzle").transform.Find("MuzzleEffects").GetComponent<SpriteRenderer>();
        muzzleEffects.SetActive(false);
    }

    // Update is called once per frame
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

            if ((Time.time > fireRate + timeSinceLastShot) || shootThisFrame)
            {
                if (isShotgun)
                {
                    // We're going from i = 1 because we multiply shotgunSpread by it,
                    // so zero would be the same rotation as shooting straight
                    for (int i = 1; i <= shotgunPelletsToTheSides; i++)
                    {
                        // Bullet to the left
                        Quaternion _rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 180f - shotgunSpreadInDegrees * i);
                        bulletInstance = ShootAtTarget(bullets[upgradeLevel], shootSpawnPoint.transform.position, _rotation, speed, damage, bulletRange, ohk);
                        
                        // Bullet centered
                        bulletInstance = ShootAtTarget(bullets[upgradeLevel], shootSpawnPoint.transform.position, rotation, speed, damage, bulletRange, ohk);
                        
                        // Bullet to the right
                        _rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 180f + shotgunSpreadInDegrees * i);
                        bulletInstance = ShootAtTarget(bullets[upgradeLevel], shootSpawnPoint.transform.position, _rotation, speed, damage, bulletRange, ohk);
                        
                        timeSinceLastShot = Time.time;
                    }
                } else
                {
                    // This is main shooting
                    bulletInstance = ShootAtTarget(bullets[upgradeLevel], shootSpawnPoint.transform.position, rotation, speed, damage, bulletRange, ohk);
                    timeSinceLastShot = Time.time;
                }
            } 
            
        }
        // -----------------------------------------------------------------------------
        // We need to update it every frame, so it does not lag behind shooting.
        // -----------------------------------------------------------------------------
        UpdateBaseSprite();
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

    private GameObject ShootAtTarget(GameObject bullet, Vector3 position, Quaternion rotation, float bulletSpeed, float bulletDamage, float bulletRange, bool oneHitKill)
    {
        GameObject _bullet = (GameObject)Instantiate(bullet, position, rotation);
        _bullet.GetComponent<Bullet>().SetSpeed(bulletSpeed);
        _bullet.GetComponent<Bullet>().SetDamage(bulletDamage);
        _bullet.GetComponent<Bullet>().SetDistanceToLive(bulletRange);
        _bullet.GetComponent<Bullet>().Setohk(oneHitKill);
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

    public bool LevelUp()
    {
        if(upgradeLevel < 3)
        {
            // Update sprites
       
            // Money cost is handled by UpgradeTurret.cs
            // Upgrade speed
            // Upgrade damage
            // Upgrade bulletRange
            upgradeLevel++;
            return true;
        } else
        {
            return false;
        }
    }
}
