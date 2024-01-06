using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitStatistics : MonoBehaviour
{
    // PFP is only a representation, takes arguments from the enemy
    // it represents
    public GameObject unitPrefab;
    [HideInInspector]
    public float speed = 2f;
    [HideInInspector]
    public float maxHealth = 200;
    [HideInInspector]
    public int damage = 13;
    [HideInInspector]
    public int moneyReward = 150;

    private void Start()
    {
        speed = unitPrefab.GetComponent<Enemy>().speed;
        maxHealth = unitPrefab.GetComponent<Enemy>().maxHealth;
        damage = unitPrefab.GetComponent<Enemy>().damage;
        moneyReward = unitPrefab.GetComponent<Enemy>().moneyReward;
    }

    private void Die()
    {
        
        Destroy(gameObject);
    }

    public void TakeDamage(float damageAmount)
    {
       
        maxHealth -= damageAmount;

       
        if (maxHealth <= 0)
        {
            Die(); 
        }
    }
}
