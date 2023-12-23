using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    private GameObject enemiesCollection;
    public GameObject[] enemies;
    public Transform[] waypoints;
    private Vector3 spawnPosition;
    [Tooltip("Time inbetween spawns")]
    public float spawnRate = 1.0f;
    [HideInInspector]
    public float enemySpeed = 2f;
    [HideInInspector]
    public float enemyHealth = 200f;
    private float timeSinceLastRespawn = 0;
    [HideInInspector]
    public int enemyDamage = 0;
    [HideInInspector]
    public int spawnCountMax = 3;
    [HideInInspector]
    [Range(0, 4)]
    public int whichEnemyToSpawnIndex = 2;
    private int spawnCount = 0;
    //public int waveAmount = 5;
    [HideInInspector]
    public int moneyRewardPerEnemy = 0;
    //public bool simpleMode = true;
    public bool isSpawnAllowed = true;
    public bool isSpawningConstantly = false;

    private void Start()
    {
        enemiesCollection = new GameObject("EnemiesCollection");
        spawnPosition = new Vector3(waypoints[0].position.x, waypoints[0].position.y, 0);
    }

    public void SpawnThisUnit(string enemyName, int moneyReward, float damageMultiplier=1, float speedMultiplier=1, float healthMultiplier=1)
    {
        if (
        (isSpawnAllowed // Disabling spawning for debugging
        && enemies[whichEnemyToSpawnIndex] != null
        && waypoints != null
        && (Time.time > spawnRate + timeSinceLastRespawn)
        && spawnCount < spawnCountMax)
        || isSpawningConstantly // Forcing to spawn all the time to playtest
        && (Time.time > spawnRate + timeSinceLastRespawn))
        {
            GameObject _enemy = PhotonNetwork.Instantiate(enemyName, spawnPosition, Quaternion.identity);
            _enemy.GetComponent<Enemy>().SetDamage(_enemy.GetComponent<Enemy>().damage);
            _enemy.GetComponent<Enemy>().SetSpeed(_enemy.GetComponent<Enemy>().speed);
            _enemy.GetComponent<Enemy>().SetWaypoints(waypoints);
            _enemy.GetComponent<Enemy>().SetHealth(_enemy.GetComponent<Enemy>().maxHealth);
            _enemy.GetComponent<Enemy>().SetMoneyReward(_enemy.GetComponent<Enemy>().moneyReward);
            _enemy.transform.SetParent(enemiesCollection.transform);
            timeSinceLastRespawn = Time.time;
            spawnCount++;
        }

    }

}
