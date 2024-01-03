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
        speed = unitPrefab.GetComponent<MultiplayerEnemy>().speed;
        maxHealth = unitPrefab.GetComponent<MultiplayerEnemy>().maxHealth;
        damage = unitPrefab.GetComponent<MultiplayerEnemy>().damage;
        moneyReward = unitPrefab.GetComponent<MultiplayerEnemy>().moneyReward;
    }
}
