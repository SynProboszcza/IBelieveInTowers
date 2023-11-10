using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class ShootAtEnemy : MonoBehaviour
{
    public GameObject Bullet;
    public GameObject fireTarget;
    public float speed = 0.1f;
    public float damage = 13;
    [Tooltip("OneHitKill")]
    public bool ohk = false;
    [Tooltip("Relative instanitate position (x)")]
    public float relativeInsPosX = 2f;
    [Tooltip("Relative instanitate position (y)")]
    public float relativeInsPosY = 0f;
    public Vector2 firePosition;
    [ReadOnly(true)]
    public Vector2 fireTargetPosition;
    public float angle;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        firePosition = transform.parent.position;
        if(fireTarget != null)
        {
            fireTargetPosition = fireTarget.transform.position;
        } else
        {
            //no wsm to nie zmieniam
        }
        angle = Vector2.SignedAngle(firePosition, fireTargetPosition);
        Transform center = gameObject.transform;
        if (Input.GetKeyDown(KeyCode.K))
        {
            Instantiate(Bullet, new Vector3(center.position.x + relativeInsPosX, center.position.y + relativeInsPosY), Quaternion.AngleAxis(angle, Vector3.forward));
            Bullet.GetComponent<Bullet>().SetSpeed(speed);
            Bullet.GetComponent<Bullet>().SetDamage(damage);
            Bullet.GetComponent<Bullet>().Setohk(ohk);

        }
    }
}
