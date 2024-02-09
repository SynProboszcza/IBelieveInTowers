using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootAtMouse : MonoBehaviour
{
    [SerializeField]
    public GameObject Bullet;
    [SerializeField]
    public GameObject Target;
    [SerializeField]
    public Transform bulletSpawnPoint;
    public float speed = 0.1f;
    public float damage = 13;
    public float angle = 0;
    [Tooltip("Relative instanitate position (x)")]
    public float relativeInsPosX = 2f;
    [Tooltip("Relative instanitate position (y)")]
    public float relativeInsPosY = 0f;
    [Tooltip("Shots per 60 frames")]
    public int fireRate = 1;
    //private int frames = 0;
    [Tooltip("OneHitKill")]
    public bool ohk = false;
    public bool isEnemyClose = false;
    public Vector2 AttackerPosition;
    public Vector2 MousePosition;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        AttackerPosition = gameObject.transform.position;
        if (Input.GetMouseButtonDown(0))
        {
            Instantiate(Bullet, new Vector3(AttackerPosition.x + relativeInsPosX, AttackerPosition.y + relativeInsPosY), Quaternion.AngleAxis(angle, Vector3.forward));
            Bullet.GetComponent<Bullet>().SetSpeed(speed);
            Bullet.GetComponent<Bullet>().SetDamage(damage);
            Bullet.GetComponent<Bullet>().Setohk(ohk);
        }
    }
}
