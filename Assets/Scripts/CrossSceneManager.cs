using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.SocialPlatforms.GameCenter;

// Publicly accesible Singleton
// Add fields that need to be kept inbetween scenes
public class CrossSceneManager : MonoBehaviour
{
    public static CrossSceneManager instance;

    [SerializeField]
    public string gameVersion
    {  get { return gameVersion;}
        private set { this.gameVersion = Application.version; }}
    [SerializeField]
    public int playerMoney;
    public int playerMana;
    public Queue<string> unitList { get; private set; }
    [HideInInspector]
    public Dictionary<string, int> enemyPrices;
    //[HideInInspector]
    public List<GameObject> enemyListFromPreMainGame;
    // Default prices fallback just in case, they are to be
    // set from prefabs in Resources
    [HideInInspector]
    public int bearPrice = 554;
    [HideInInspector]
    public int bettlePrice = 554;
    [HideInInspector]
    public int opossumPrice = 554;
    [HideInInspector]
    public int dinoPrice = 554;
    [HideInInspector]
    public int slimerPrice = 554;
    public int defenderHealth;
    public int currentMatchMaxTime;
    public string enemyNickname = "";
    public string myNickName = "";
    public GameObject bearPrefab;
    public GameObject bettlePrefab;
    public GameObject opossumPrefab;
    public GameObject dinoPrefab;
    public GameObject slimerPrefab;
    public bool amIMaster;
    public bool amIDefender;
    public bool hasDefenderDied = false;
    public bool isMoneyInfinite = false;
    public bool isManaInfinite = false;
    public bool invincibleTurrets = false;

    void Start()
    {
        DontDestroyOnLoad(gameObject);
        playerMoney = 2000;
        playerMana = 500;
        defenderHealth = 275;
        myNickName = PlayerPrefs.GetString("LocalNickName");
        GameObject parentForEnemies =  Instantiate(new GameObject("EnemiesFromPreMainGame"));
        parentForEnemies.name = "EnemiesFromPreMainGame"; // Default instantiation adds "(Clone)" to the name
        parentForEnemies.transform.parent = transform;
    }

    // Make sure there is only one instance and set prices
    private void Awake()
    {
        if(instance != null && instance != this)
        {
            Destroy(this);
        } else
        {
            instance = this;
        }
        // We got a race condition and this needs to be set fast
        // BuyUnit is accessing this dict and it accessed it before it was initialized
        // so i moved it to Awake instead of Start
        enemyPrices = new Dictionary<string, int>
        {
            { "Bear", bearPrefab.GetComponent<MultiplayerEnemy>().costToSpawn },
            { "Bettle", bettlePrefab.GetComponent<MultiplayerEnemy>().costToSpawn },
            { "Opossum", opossumPrefab.GetComponent<MultiplayerEnemy>().costToSpawn },
            { "Dino", dinoPrefab.GetComponent<MultiplayerEnemy>().costToSpawn },
            { "Slimer", slimerPrefab.GetComponent<MultiplayerEnemy>().costToSpawn }
        };

    }

    public void TakeDefenderDamageAndCheckIfDied(int amount)
    {
        defenderHealth -= amount;
        if (defenderHealth <= 0)
        {
            hasDefenderDied = true;
        }
    }

    public void ResetAfterPlaying()
    {
        // reset everything
    }

    public bool PayWithMoney(int cost)
    {
        if (isMoneyInfinite)
        {
            return true;
        }
        if (playerMoney >= cost)
        {
            playerMoney -= cost;
            return true;
        }
        return false;
    }

    public bool PayWithMana(int cost)
    {
        if (isManaInfinite)
        {
            return true;
        }
        if (playerMana >= cost)
        {
            playerMana -= cost;
            return true;
        }
        return false;
    }

    public void AddMana(int amount)
    {
        playerMana += amount;
    }

    public void AddMoney(int amount)
    {
        playerMoney += amount;
    }

    public bool CanPlayerAffordWithMoney(int cost)
    {
        if (isMoneyInfinite)
        {
            return true;
        } else if (playerMoney >= cost)
        {
            return true;
        } else
        {
            return false;
        }
    }

    public bool CanPlayerAffordWithMana(int cost)
    {
        if (isManaInfinite)
        {
            return true;
        } else if (playerMana >= cost)
        {
            return true;
        } else
        {
            return false;
        }
    }

    public void AddUnitToList(string name)
    {
        unitList.Enqueue(name);
    }

    public string PopUnitFromList()
    {
        return unitList.Dequeue();
    }
}
