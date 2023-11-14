using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootAtClosestEnemy : MonoBehaviour
{
    public GameObject Bullet;
    public GameObject shootSpawnPoint;
    public GameObject Gun;
    public GameObject muzzleEffects;
    public GameObject Target;
    private GameObject bulletInstance;
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
    public bool ShootThisFrame = false;
    public bool isShotgun = false;
    public float shotgunSpreadInDegrees = 15f;
    [Tooltip("Amount of pellets added to the left and to the right")]
    public int shotgunPelletsToTheSides = 1;
    public Vector2 TargetPosition;
    // Start is called before the first frame update
    void Start()
    {
        muzzleEffects.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        // If there is no bullet (it hit sth)
        // disable muzzle effects
        if (bulletInstance == null)
        {
            muzzleEffects.SetActive(false);
        } else if (bulletInstance != null)
        {
            // If bullet exists, check its distance
            // and remove it when it gets away
            float distance = Vector3.Distance(bulletInstance.transform.position, shootSpawnPoint.transform.position);
            if(distance > distanceToShutoffMuzzleEffects)
            {
                muzzleEffects.SetActive(false);
            }
            //if(distance > bulletRange) 
            //{ 
            //    DestroyBullet(bulletInstance);
            //}
        }

        if (isEnemyClose && Target != null)
        {
            TargetPosition = Target.transform.position;
            Vector2 direction = new Vector2(shootSpawnPoint.transform.position.x, shootSpawnPoint.transform.position.y) - TargetPosition;
            direction.Normalize();
            Quaternion rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 180f);
            
            Gun.transform.rotation = rotation;

            if ((Time.time > fireRate + timeSinceLastShot) || ShootThisFrame)
            {
                if (isShotgun)
                {
                    // We're going from i = 1 because we multiply shotgunSpread by it,
                    // so zero would not suffice
                    for (int i = 1; i <= shotgunPelletsToTheSides; i++)
                    {
                        // Bullet to the left
                        Quaternion _rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 180f - shotgunSpreadInDegrees * i);
                        bulletInstance = ShootAtTarget(Bullet, shootSpawnPoint.transform.position, _rotation, speed, damage, ohk);
                        
                        // Bullet centered
                        bulletInstance = ShootAtTarget(Bullet, shootSpawnPoint.transform.position, rotation, speed, damage, ohk);
                        
                        // Bullet to the right
                        _rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 180f + shotgunSpreadInDegrees * i);
                        bulletInstance = ShootAtTarget(Bullet, shootSpawnPoint.transform.position, _rotation, speed, damage, ohk);
                        
                        timeSinceLastShot = Time.time;
                    }

                    // We're adding two bullets 15 degrees to left and to right
                    //Quaternion rotationToLeft = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 180f - 15f);
                    //Quaternion rotationToRight = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 180f + 15f);
                    //bulletInstance = ShootAtTarget(Bullet, shootSpawnPoint.transform.position, rotationToRight, speed, damage, ohk);

                } else
                {
                    bulletInstance = ShootAtTarget(Bullet, shootSpawnPoint.transform.position, rotation, speed, damage, ohk);
                    timeSinceLastShot = Time.time;
                }
            } 
            
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
            Target = collision.gameObject;
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
            Target = collision.gameObject;
            isEnemyClose = true;
        }
    }

    private GameObject ShootAtTarget(GameObject bullet, Vector3 position, Quaternion rotation, float bulletSpeed, float bulletDamage, bool oneHitKill)
    {
        GameObject round = (GameObject)Instantiate(bullet, position, rotation);
        round.GetComponent<Bullet>().SetSpeed(bulletSpeed);
        round.GetComponent<Bullet>().SetDamage(bulletDamage);
        round.GetComponent<Bullet>().SetDistanceToLive(bulletRange);
        round.GetComponent<Bullet>().Setohk(oneHitKill);
        muzzleEffects.SetActive(true);
        return round;
    }
}
