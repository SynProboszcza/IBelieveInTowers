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
    public int layerIndex = 0;
    public float speed = 0.1f;
    public float damage = 13;
    public float angle = 0;
    [Tooltip("Amount of time in seconds inbetween shots")] 
    public float fireRate = 1;
    public float timeSinceLastShot;
    [Tooltip("Distance after which muzzle effects disappear")]
    public float distanceToShutoffMuzzleEffects = 0.5f;
    [Tooltip("OneHitKill")]
    public bool ohk = false;
    public bool isEnemyClose = false;
    public bool ShootThisFrame = false;
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
                bulletInstance =  ShootAtTarget(Bullet, shootSpawnPoint.transform.position, rotation, speed, damage, ohk);
                timeSinceLastShot = Time.time;
            } 
            
        }
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
        round.GetComponent<Bullet>().Setohk(oneHitKill);
        muzzleEffects.SetActive(true);
        return round;
    }
}
