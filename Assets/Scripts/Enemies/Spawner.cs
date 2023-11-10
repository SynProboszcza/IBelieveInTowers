using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject enemy;
    public Transform[] Waypoints;
    public float spawnRate = 1.0f;
    public float enemySpeed = 2f;
    public float enemyHealth = 200f;
    public float timeSinceLastRespawn = 0;
    public int enemyDamage = 0;
    [ReadOnly(true)]
    public int spawnCountMax = 3;
    private int spawnCount = 0;
    public bool isSpawnAllowed = true;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isSpawnAllowed //for disabling spawning for debugging
            && enemy != null 
            && Waypoints != null 
            && (Time.time > spawnRate + timeSinceLastRespawn)
            && spawnCount < spawnCountMax)
        {
            Spawn();
            timeSinceLastRespawn = Time.time;
        }
    }

    private void Spawn()
    {
        GameObject foe = Instantiate(enemy);
        foe.GetComponent<Enemy>().setDamage(enemyDamage);
        foe.GetComponent<Enemy>().setSpeed(enemySpeed);
        foe.GetComponent<Enemy>().setWaypoints(Waypoints);
        foe.GetComponent<Enemy>().setHealth(enemyHealth);
        spawnCount++;
    }
}
