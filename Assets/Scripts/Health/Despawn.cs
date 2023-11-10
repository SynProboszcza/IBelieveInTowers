using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Despawn : MonoBehaviour
{
    public TMP_Text playerHealth;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        int damage = collision.gameObject.GetComponent<Enemy>().getDamage();
        playerHealth.GetComponent<Health>().takeDamage(damage);
        Destroy(collision.gameObject);
    }
}
