using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitStatistics : MonoBehaviour
{
    // Set this in inspector for every PFP
    // Spawner takes these values and sets them to unit prefab <Enemy>
    public float speed = 2f;
    public int maxHealth = 200;
    public int damage = 13;
    public int moneyReward = 150;
    public GameObject unitPrefab;
}
