using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class SpawnerMultiplayer : MonoBehaviour
{
    public GameObject enemiesCollection;
    [Tooltip("Content GO of list")]
    public GameObject listOfEnemies;
    public GameObject spawnCircle;
    public string[] enemies;
    public Transform[] waypoints;
    private Vector3 spawnPosition;
    [Tooltip("Time inbetween spawns in seconds")]
    public float spawnRate = 3.0f;
    [HideInInspector]
    public float enemySpeed = 2f;
    [HideInInspector]
    public float enemyHealth = 200f;
    private float timeSinceLastRespawn = 0;
    [HideInInspector]
    public int enemyDamage = 0;
    [SerializeField]
    public Queue<GameObject> queueToSpawn = new Queue<GameObject>();

    // Leaving for potential future gamemodes,
    // for now set high as to not interfere with basic gameplay
    //[HideInInspector]
    public int spawnCountMax = 50000;

    [HideInInspector]
    [Range(0, 4)]
    public int whichEnemyToSpawnIndex = 2;
    private int spawnCount = 0;
    //public int waveAmount = 5;
    [HideInInspector]
    public int moneyRewardPerEnemy = 0;
    //public bool simpleMode = true;
    //[HideInInspector]
    public bool isSpawnAllowed = true;
    //[HideInInspector]
    public bool isSpawningConstantly = false;
    private bool _showWhyNotSpawning = true;
    private bool _showFirstSpawnDelay = true;
    public bool isCurrentlySpawning = false;
    private float timeStartToShowInitialDelay = 0f;

    private void Start()
    {
        // If enemiesCollection GO is not set, make one myself
        if (enemiesCollection == null)
        {
            enemiesCollection = new GameObject("EnemiesCollection");
        }
        // Set spawnPosition for every spawned enemy
        spawnPosition = new Vector3(waypoints[0].position.x, waypoints[0].position.y, 0);
        // Check if enemy names are set, if not use this fallback
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
        // "Steal" enemies from CSM to listOfEnemies parent GO
        while (CrossSceneManager.instance.transform.Find("EnemiesFromPreMainGame").transform.childCount > 0
            && CrossSceneManager.instance.transform.Find("EnemiesFromPreMainGame").transform.GetChild(0) != null)
        {
            //print("Stealing enemy from CSM: " + CrossSceneManager.instance.transform.Find("EnemiesFromPreMainGame").transform.GetChild(0).name +
            //    "\nhis scale: " + CrossSceneManager.instance.transform.Find("EnemiesFromPreMainGame").transform.GetChild(0).localScale);
            CrossSceneManager.instance.transform.Find("EnemiesFromPreMainGame").transform.GetChild(0).SetParent(listOfEnemies.transform);
        }
        // Enqueue them to spawn and fix their scale
        // Scale can be chagned by SetParent
        for (int i = 0; i < listOfEnemies.transform.childCount; i++)
        {
            if (listOfEnemies.transform.GetChild(i).localScale != new Vector3(1, 1, 1))
            {
                listOfEnemies.transform.GetChild(i).localScale = new Vector3(1, 1, 1);
                //print("Fixed scale of: " + listOfEnemies.transform.GetChild(0).name);
            }
            //print("Stole enemy from CSM: " + listOfEnemies.transform.GetChild(0).name +
            //    "\nhis scale: " + listOfEnemies.transform.GetChild(0).localScale);
            //
            //print("trying to enqueue: " + listOfEnemies.transform.GetChild(i).gameObject.name +
            //    "\nhis scale: " + listOfEnemies.transform.GetChild(i).gameObject.transform.localScale);
            queueToSpawn.Enqueue(listOfEnemies.transform.GetChild(i).gameObject);
            //print("queueToSpawn.Count: " + queueToSpawn.Count +
            //    "\nhis scale: " + listOfEnemies.transform.GetChild(i).gameObject.transform.localScale);
        }
        //print("After Start():" + ShowWholeQueue());

        // Wait 3 seconds before spawning and show it on the spawnCircle
        // Make it red and the restore color before
        // Value should be read from CrossSceneManager - easier to access
        StartCoroutine(WaitForNSeconds(CrossSceneManager.instance.delayFirstSpawn));
        isSpawnAllowed = false;
        // Make spawnCircle invisible regardless of default value set in Editor
        // After initial spawn delay
        spawnCircle.GetComponent<Image>().fillAmount = 0;
        timeStartToShowInitialDelay = Time.time;
    }

    private string ShowWholeQueue()
    {
        //print("Count: " + queueToSpawn.Count);
        string result = "Whole queue(" + queueToSpawn.Count + "): \n";
        foreach (GameObject enemy in queueToSpawn)
        {
            result += enemy.name +
                //"\t timeAdded: " + enemy.GetComponent<UnitStatistics>().spawnTimeWhenAddedToList + // its not used
                "\t spawnTime: " + enemy.GetComponent<UnitStatistics>().spawnTime + "\n";
            //Debug.LogWarning(enemy, enemy);
        }
        return result;
    }

    public void AddEnemyToSpawnQueue(GameObject PFP)
    {
        //PFP.GetComponent<UnitStatistics>().spawnTimeWhenAddedToList = Time.time;
        PFP.GetComponent<ReturnUnit>().canReturnUnit = false;
        queueToSpawn.Enqueue(PFP);
        //print("Added: " + ShowWholeQueue());
        //SpawnFirstEnemyAfterNSeconds(queueToSpawn.Peek().gameObject.GetComponent<UnitStatistics>().spawnTime);

        // ------------------------------------
        //float timeToSpawn = queueToSpawn.Peek().gameObject.GetComponent<UnitStatistics>().spawnTime;
        //float timeFoundUnit = queueToSpawn.Peek().gameObject.GetComponent<UnitStatistics>().spawnTimeWhenAddedToList;
        //print("Current timeToSpawn: " + timeToSpawn + " timeFoundUnit: " + timeFoundUnit);

        //SpawnThisUnit(PFP);
    }

    private void Update()
    {
        if (_showFirstSpawnDelay)
        {
            // To modify initial spawn delay change the value in CSM
            // This Cor. uses it directly
            // This Cor. also sets _showFirstSpawnDelay to false at the end
            StartCoroutine(ShowInitialSpawnDelay());
        }
        if (queueToSpawn.Count > 0 && !isCurrentlySpawning && isSpawnAllowed)
        {
            //print("found to spawn: " + queueToSpawn.Peek().gameObject);
            SpawnFirstEnemyAfterNSeconds(queueToSpawn.Peek().gameObject.GetComponent<UnitStatistics>().spawnTime);
        } 
        else
        {
            //print("cant spawn: " + "\n" +
            //    "queueToSpawn.Count: " + queueToSpawn.Count + "\n" +
            //    "isCurrentlySpawning: " + isCurrentlySpawning + "\n" +
            //    "isSpawnAllowed: " + isSpawnAllowed + "");
        }

        /*
        if (queueToSpawn.Count > 0)
        {
            if (Time.time > queueToSpawn.Peek().gameObject.GetComponent<UnitStatistics>().spawnTimeWhenAddedToList + queueToSpawn.Peek().gameObject.GetComponent<UnitStatistics>().spawnTime)
            {
                SpawnThisUnit(queueToSpawn.Dequeue());
                print("Sent to spawn: " + ShowWholeQueue());
            }
            else
            {
                float _fillAmount = Mathf.InverseLerp(queueToSpawn.Peek().gameObject.GetComponent<UnitStatistics>().spawnTimeWhenAddedToList, queueToSpawn.Peek().gameObject.GetComponent<UnitStatistics>().spawnTimeWhenAddedToList + queueToSpawn.Peek().gameObject.GetComponent<UnitStatistics>().spawnTime, Time.time);
                //print("fill amount: " + _fillAmount + "\nTime.time: " + Time.time + "\ntimeToSpawn: " + timeToSpawn + "\ntimeFoundUnit: " + timeFoundUnit + "");
                spawnCircle.GetComponent<Image>().fillAmount = _fillAmount;
                // SpawnThisUnit disables it
            }
        }
        */
        /*
        if (listOfEnemies.transform.childCount > 0)
        {
            // _unitPrefab is PFP => PFP has unit stats => unit stats has unit prefab => has Enemy
            GameObject _unitPrefab = listOfEnemies.transform.GetChild(0).gameObject;
            float timeToSpawn = _unitPrefab.GetComponent<UnitStatistics>().spawnTime;
            float timeAtWhichAddedToList = _unitPrefab.GetComponent<UnitStatistics>().spawnTimeWhenAddedToList;
            if (Time.time > timeToSpawn + timeAtWhichAddedToList)
            {
                SpawnThisUnit(_unitPrefab);
            } else
            {
                float _fillAmount = Mathf.InverseLerp(timeAtWhichAddedToList, timeAtWhichAddedToList + timeToSpawn, Time.time);
                print("fill amount: " + _fillAmount + "\nTime.time: " + Time.time + "\ntimeToSpawn: " + timeToSpawn + "");
                spawnCircle.GetComponent<Image>().fillAmount = _fillAmount;
                // SpawnThisUnit disables it
            }
        }
        */
    }

    private void SpawnFirstEnemyAfterNSeconds(float seconds)
    {
        isCurrentlySpawning = true;
        StartCoroutine(SpawnFirstEnemyAfterNSecondsCoroutine(seconds));
    }

    System.Collections.IEnumerator SpawnFirstEnemyAfterNSecondsCoroutine(float seconds)
    {
        float timeStart = Time.time;
        while (Time.time < timeStart + seconds)
        {
            float fillAmount = Mathf.InverseLerp(timeStart, timeStart + seconds, Time.time);
            spawnCircle.GetComponent<Image>().fillAmount = fillAmount;
            yield return null;
        }
        //yield return new WaitForSeconds(seconds);
        SpawnThisUnit(queueToSpawn.Dequeue());
        //print("Spawned: " + ShowWholeQueue());
        isCurrentlySpawning = false;
    }

    System.Collections.IEnumerator ShowInitialSpawnDelay()
    {
        Color colorBefore = spawnCircle.GetComponent<Image>().color;
        spawnCircle.GetComponent<Image>().color = Color.red;
        while (Time.time < timeStartToShowInitialDelay + CrossSceneManager.instance.delayFirstSpawn)
        {
            float fillAmount = Mathf.InverseLerp(timeStartToShowInitialDelay, timeStartToShowInitialDelay + CrossSceneManager.instance.delayFirstSpawn, Time.time);
            spawnCircle.GetComponent<Image>().fillAmount = fillAmount;
            //print("fill: " + fillAmount +
            //    "\ntimeStartToShowInitialDelay: " + timeStartToShowInitialDelay +
            //    "\nCrossSceneManager.instance.delayFirstSpawn: " + CrossSceneManager.instance.delayFirstSpawn +
            //    "\nTime.time: " + Time.time);
            yield return null;
        }
        spawnCircle.GetComponent<Image>().color = colorBefore;
        spawnCircle.GetComponent<Image>().fillAmount = 0f;
        _showFirstSpawnDelay = false;
    }


    System.Collections.IEnumerator WaitForNSeconds(int seconds)
    {
        yield return new WaitForSeconds(seconds);
        isSpawnAllowed = true;
        CrossSceneManager.instance.spawnDelayPassed = true;
    }

    public void SpawnThisUnit(GameObject unitReference)
    {
        //print("got this: " + unitReference);
        if (
        (isSpawnAllowed // Potentially usefull
        //&& enemies != null
        && waypoints != null
        //&& (Time.time > spawnRate + timeSinceLastRespawn)
        && spawnCount < spawnCountMax)
        )
        //|| isSpawningConstantly // Forcing to spawn all the time to playtest
        //&& (Time.time > spawnRate + timeSinceLastRespawn))
        {
            //print("passed checks");
            //object[] customData = { waypoints };
            string unitName = "Enemies/" + unitReference.GetComponent<UnitStatistics>().unitPrefab.name;
            GameObject _unit = PhotonNetwork.Instantiate(unitName, spawnPosition, Quaternion.identity);
            _unit.GetComponent<MultiplayerEnemy>().SetDamage(_unit.GetComponent<MultiplayerEnemy>().damage);
            _unit.GetComponent<MultiplayerEnemy>().SetSpeed(_unit.GetComponent<MultiplayerEnemy>().speed);
            _unit.GetComponent<MultiplayerEnemy>().SetWaypoints(waypoints);
            _unit.GetComponent<MultiplayerEnemy>().SetHealth(_unit.GetComponent<MultiplayerEnemy>().maxHealth);
            _unit.GetComponent<MultiplayerEnemy>().SetMoneyReward(_unit.GetComponent<MultiplayerEnemy>().moneyReward);
            //_unit.transform.SetParent(enemiesCollection.transform); // _unit will set its parent itself with SetParent component
            // _unit.GetComponent<SetParent>().SetParentOfThisGO(enemiesCollection); // this will be local only
            spawnCircle.GetComponent<Image>().fillAmount = 0f;
            //Debug.LogError(unitReference, unitReference);
            Destroy(listOfEnemies.transform.GetChild(0).gameObject);
            timeSinceLastRespawn = Time.time;
            spawnCount++;
        }
        else
        {
            if (!(Time.time > spawnRate + timeSinceLastRespawn))
            {
                print("Waiting to spawn...");
            }
            else if (!isSpawnAllowed)
            {
                if (_showWhyNotSpawning)
                {
                    print("isSpawnAllowed is blocked, probably for first " + CrossSceneManager.instance.delayFirstSpawn + " seconds.");
                }
                _showWhyNotSpawning = false; // Flag to send this msg only once
            }
            else
            {
                print(
                    "Something is blocking spawn, click to see details:\n" +
                    "isSpawnAllowed: " + isSpawnAllowed + "\n" +
                    "enemies: " + enemies + "\n" +
                    "waypoints: " + waypoints + "\n" +
                    "(Time.time > spawnRate + timeSinceLastRespawn): " + (Time.time > spawnRate + timeSinceLastRespawn) + "\n" +
                    "spawncount: " + spawnCount + "\n" +
                    "spawncountmax: " + spawnCountMax + "\n" +
                    "isspawningconstantly: " + isSpawningConstantly + "\n"
                    );
            }
        }

    }

    public void SpawnBearDebug() // Yeah its not only bears
    {
        // Debug.LogError("this should not work nor be visible");
        string unitName = "Enemies/EnemySlimer";
        GameObject _unit = PhotonNetwork.Instantiate(unitName, spawnPosition, Quaternion.identity);
        _unit.GetComponent<MultiplayerEnemy>().SetDamage(1);
        _unit.GetComponent<MultiplayerEnemy>().SetExplosiveDamage(50);
        _unit.GetComponent<MultiplayerEnemy>().SetSpeed(5f);
        _unit.GetComponent<MultiplayerEnemy>().SetWaypoints(waypoints);
        _unit.GetComponent<MultiplayerEnemy>().SetMaxHealth(1000);
        _unit.GetComponent<MultiplayerEnemy>().SetMoneyReward(1234);
        _unit.GetComponent<MultiplayerEnemy>().explosionRadius = 1f;

    }
}
