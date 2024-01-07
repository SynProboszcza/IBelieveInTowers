using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
// using UnityEngine.SocialPlatforms.GameCenter;

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
    public int currentMatchMaxTime = 180;
    public int delayFirstSpawn = 3;
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
    private Vector2 mouseWorldPos;
    public GameObject showPriceCostPrefab;
    public bool spawnDelayPassed = false;
    public List<string> mapMiddleNames;
    [Tooltip("ONLY change if you added FULLY FUNCTIONAL map that is correctly named (Map<next number>Multiplayer)")]
    public int amountOfMaps = 4;

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

    public void RandomizeMapSelection()
    {
        List<int> possible = Enumerable.Range(1, amountOfMaps).ToList();
        List<int> listNumbers = new List<int>();
        for (int i = 0; i < 3; i++)
        {
            int index = Random.Range(0, possible.Count);
            listNumbers.Add(possible[index]);
            possible.RemoveAt(index);
        }
        foreach (int i in listNumbers)
        {
            mapMiddleNames.Add(i.ToString());
        }
        // ----------------------
        // HACK: debugging maps
        // ----------------------
        mapMiddleNames.Clear();
        mapMiddleNames.Add("3");

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
        print("CSM Reset");
        playerMoney = 2000;
        playerMana = 500;
        defenderHealth = 275;
        currentMatchMaxTime = 180;
        delayFirstSpawn = 3;
        enemyNickname = "";
        myNickName = "";
        mapMiddleNames.Clear();
        // All not-set bools are implicitly false
        amIMaster = false;
        amIDefender = false;
        hasDefenderDied = false;
        isMoneyInfinite = false;
        isManaInfinite = false;
        invincibleTurrets = false;
    }

    private void ShowMoneyChange(int cost, bool isPaying) 
    {
        // 2 params to use current mouse position
        mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        GameObject go = Instantiate(showPriceCostPrefab, new Vector3(mouseWorldPos.x+1, mouseWorldPos.y, 0), Quaternion.identity);
        if (isPaying)
        {
            go.transform.Find("Price").GetComponent<TMP_Text>().text = "-" + cost.ToString() + " G";
        } else
        {
            go.transform.Find("Price").GetComponent<TMP_Text>().text = "+" + cost.ToString() + " G";
        }
    }

    private void ShowMoneyChange(int cost, bool isPaying, Vector2 position) 
    {
        // 3 params to use position thats passed
        GameObject go = Instantiate(showPriceCostPrefab, new Vector3(position.x, position.y, 0), Quaternion.identity);
        if (isPaying)
        {
            go.transform.Find("Price").GetComponent<TMP_Text>().text = "-" + cost.ToString() + " G";
        } else
        {
            go.transform.Find("Price").GetComponent<TMP_Text>().text = "+" + cost.ToString() + " G";
        }
    }

    public bool PayWithMoney(int cost)
    {
        if (isMoneyInfinite)
        {
            ShowMoneyChange(cost, true);
            return true;
        }
        if (playerMoney >= cost)
        {
            ShowMoneyChange(cost, true);
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

    public void AddMana(int amount, Vector2 fromWhere)
    {
        // Show adding mana
        playerMana += amount;
    }

    public void AddMoney(int amount)
    {
        ShowMoneyChange(amount, false);
        playerMoney += amount;
    }

    public void AddMoney(int amount, Vector2 fromWhere)
    {
        ShowMoneyChange(amount, false, fromWhere);
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
