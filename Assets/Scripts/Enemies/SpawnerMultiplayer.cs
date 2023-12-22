using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class SpawnerMultiplayer : MonoBehaviour
{
    public GameObject enemiesCollection;
    public GameObject listOfEnemies;
    public string[] enemies;
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
    
    // Leaving for potential future gamemodes,
    // for now set high as to not interfere with basic gameplay
    [HideInInspector]
    public int spawnCountMax = 50000; 

    [HideInInspector]
    [Range(0, 4)]
    public int whichEnemyToSpawnIndex = 2;
    private int spawnCount = 0;
    //public int waveAmount = 5;
    [HideInInspector]
    public int moneyRewardPerEnemy = 0;
    //public bool simpleMode = true;
    [HideInInspector]
    public bool isSpawnAllowed = true;
    [HideInInspector]
    public bool isSpawningConstantly = false;

    private void Start()
    {
        if(enemiesCollection == null)
        {
            enemiesCollection = new GameObject("EnemiesCollection");
        }
        spawnPosition = new Vector3(waypoints[0].position.x, waypoints[0].position.y, 0);
        if (enemies.Length < 5)
        {
            print("Got less enemies than 5, setting up default");
            enemies = new string[] {
                "EnemyBear",
                "EnemyBettle",
                "EnemyDino",
                "EnemyOpossum",
                "EnemySlimer"
            };
        }
    }

    private void Update() 
    {
        if (listOfEnemies.transform.childCount > 0)
        {
            // _unitPrefab is PFP => PFP has unit stats => unit stats has unit prefab => has Enemy
            GameObject _unitPrefab = listOfEnemies.transform.GetChild(0).gameObject;
            print(_unitPrefab);
            // int _moneyReward = _unitPrefab.GetComponent<UnitStatistics>().moneyReward;
            // string _unitToSpawnString = listOfEnemies.transform.GetChild(0).name.ToString();
            // _unitToSpawnString = _unitToSpawnString.Replace("(Clone)", string.Empty);
            // _unitToSpawnString = "PFPs/" + _unitToSpawnString;
            // print(_unitToSpawnString);
            SpawnThisUnit(_unitPrefab);
        }
    }

    public void SpawnThisUnit(GameObject unitReference, float damageMultiplier = 1, float speedMultiplier = 1, float healthMultiplier = 1)
    {
        if (
        (isSpawnAllowed // Potentially usefull
        && enemies != null
        && waypoints != null
        && (Time.time > spawnRate + timeSinceLastRespawn)
        && spawnCount < spawnCountMax)
        || isSpawningConstantly // Forcing to spawn all the time to playtest
        && (Time.time > spawnRate + timeSinceLastRespawn))
        {
            GameObject _unit = PhotonNetwork.Instantiate(unitReference.GetComponent<UnitStatistics>().unitPrefab.name, spawnPosition, Quaternion.identity);
            _unit.GetComponent<Enemy>().SetDamage(_unit.GetComponent<UnitStatistics>().damage);
            _unit.GetComponent<Enemy>().SetSpeed(_unit.GetComponent<UnitStatistics>().speed);
            _unit.GetComponent<Enemy>().SetWaypoints(waypoints);
            _unit.GetComponent<Enemy>().SetHealth(_unit.GetComponent<UnitStatistics>().maxHealth);
            _unit.GetComponent<Enemy>().SetMoneyReward(_unit.GetComponent<UnitStatistics>().moneyReward);
            //_unit.transform.SetParent(enemiesCollection.transform); // _unit will set its parent itself with SetParent component
            // _unit.GetComponent<SetParent>().SetParentOfThisGO(enemiesCollection); // this will be local only
            Destroy(unitReference);
            timeSinceLastRespawn = Time.time;
            spawnCount++;
        }

    }

}
