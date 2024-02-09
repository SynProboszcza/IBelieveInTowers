using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class SpawnerSingleplayer : MonoBehaviour
{
    private GameObject enemiesCollection;
    public GameObject[] enemies;
    public Transform[] waypoints;
    [Tooltip("Time inbetween spawns")]
    public float spawnRate = 1.0f;
    public float enemySpeed = 2f;
    public float enemyHealth = 200f;
    private float timeSinceLastRespawn = 0;
    public int enemyDamage = 0;
    public int spawnCountMax = 3;
    [Range(0, 4)]
    public int whichEnemyToSpawnIndex = 2;
    private int spawnCount = 0;
    //public int waveAmount = 5;
    public int moneyRewardPerEnemy = 0;
    //public bool simpleMode = true;
    public bool isSpawnAllowed = true;
    public bool isSpawningConstantly = false;

    private void Start()
    {
        enemiesCollection = new GameObject("EnemiesCollection");
    }

    // Update is called once per frame
    void Update()
    {
        if (
            (isSpawnAllowed // Disabling spawning for debugging
            && enemies[whichEnemyToSpawnIndex] != null
            && waypoints != null
            && (Time.time > spawnRate + timeSinceLastRespawn)
            && spawnCount < spawnCountMax)
            || isSpawningConstantly // Forcing to spawn all the time to playtest
            && (Time.time > spawnRate + timeSinceLastRespawn)
            )
        {
            Spawn();
        }
    }

    private void Spawn()
    {
        GameObject _enemy = Instantiate(enemies[whichEnemyToSpawnIndex]);
        _enemy.GetComponent<Enemy>().SetDamage(enemyDamage);
        _enemy.GetComponent<Enemy>().SetSpeed(enemySpeed);
        _enemy.GetComponent<Enemy>().SetWaypoints(waypoints);
        _enemy.GetComponent<Enemy>().SetHealth(enemyHealth);
        _enemy.GetComponent<Enemy>().SetMoneyReward(moneyRewardPerEnemy);
        _enemy.transform.SetParent(enemiesCollection.transform);
        timeSinceLastRespawn = Time.time;
        spawnCount++;
    }
}
