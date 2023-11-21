using Mono.Cecil;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Shoot : MonoBehaviour
{
    public GameObject Bullet;
    public int damage;
    public float speed = 0.01f;
    [Tooltip("OneHitKill")]
    public bool ohk = false;
    [Tooltip("Relative instanitate position (x)")]
    public float relativeInsPosX = 2f;
    [Tooltip("Relative instanitate position (y)")]
    public float relativeInsPosY = 0f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Transform center = gameObject.transform;
        if(Input.GetKeyDown(KeyCode.K)) { 
            Instantiate(Bullet, new Vector3(center.position.x + relativeInsPosX, center.position.y + relativeInsPosY), Quaternion.identity);
            Bullet.GetComponent<Bullet>().SetSpeed(speed);
            Bullet.GetComponent<Bullet>().SetDamage(damage);
            Bullet.GetComponent<Bullet>().Setohk(ohk);

        }

    }
}
