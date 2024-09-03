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
    {
        get { return gameVersion; }
        private set { this.gameVersion = Application.version; }
    }
    public GameInfo gameInfo;
    [Header("Current playing values")]
    public int defenderHealth; // ++
    public int playerMoney; // ++
    public int playerMana; // ++
    [Tooltip("Real-time float time is tracked by MainGame")]
    public int currentMatchMaxTime = 180;
    [Tooltip("Additional gold for attacker; dmg dealt * this = add. gold")]
    public int goldForDamageMultiplier = 10; // ++
    public int delayFirstSpawn = 3; // ++
    public string enemyNickname = ""; 
    public string myNickName = "";
    public Queue<string> unitList { get; private set; }
    [HideInInspector]
    public Dictionary<string, int> enemyPrices;
    [Tooltip("List that stores results - amount of <bool>s is the amount of matches played (shold be max 3)")]
    public List<bool> didMasterWin = new List<bool>();
    [HideInInspector]
    public List<GameObject> enemyListFromPreMainGame;
    // Default prices fallback just in case, they are to be
    // set from prefabs in Resources
    //[HideInInspector]
    public int bearPrice = 554; // ++ 
    //[HideInInspector]
    public int bettlePrice = 554; // ++
    //[HideInInspector]
    public int opossumPrice = 554; // ++
    //[HideInInspector]
    public int dinoPrice = 554; // ++ 
    //[HideInInspector]
    public int slimerPrice = 554;// ++ 
    // default... are for reset, actual are just names without default prefix
    // ----------------------------------------------------------------------------
    [Header("Default values to set")]
    public int defaultDefenderHealth = 300; // ++
    public int defaultMoneyAmount = 2000; // ++
    public int defaultManaAmount = 500; // ++
    public int defaultDelayFirstSpawn = 3; // ++
    public int defaultCurrentMatchMaxTime = 180; // ++
    // ----------------------------------------------------------------------------
    [Header("Informational flags")]
    public bool amIMaster;
    public bool amIDefender;
    public bool hasDefenderDied = false;
    public bool isMoneyInfinite = false;
    public bool isManaInfinite = false;
    public bool invincibleTurrets = false;
    public bool isMatchOver = false;
    public bool spawnDelayPassed = false;
    [Tooltip("For now these are integers as <string>, like \"1\", \"3\" etc. It can be more complex in future if need be")]
    public List<string> mapMiddleNames;
    [Tooltip("ONLY change if you added FULLY FUNCTIONAL map that is correctly named (Map<next number>Multiplayer)")]
    public int amountOfMaps = 5; // ++
    public string roundWon = "You won the round, nice!"; // ++
    public string roundLost = "You lost the round, prepare for next one"; // ++
    public string matchWon = "You won the match, congratulations!"; // ++
    public string matchLost = "You lost the match, keep trying :)"; // ++
    public GameObject showPriceCostPrefab;
    public GameObject showHealthChangePrefab;
    public GameObject bearPrefab;
    public GameObject bettlePrefab;
    public GameObject opossumPrefab;
    public GameObject dinoPrefab;
    public GameObject slimerPrefab;
    private Vector2 mouseWorldPos;


    void Start()
    {
        DontDestroyOnLoad(gameObject);
        playerMoney = defaultMoneyAmount;
        playerMana = defaultManaAmount;
        defenderHealth = defaultDefenderHealth;
        myNickName = PlayerPrefs.GetString("LocalNickName");
        GameObject parentForEnemies = Instantiate(new GameObject("EnemiesFromPreMainGame"));
        parentForEnemies.name = "EnemiesFromPreMainGame"; // Default instantiation adds "(Clone)" to the name
        parentForEnemies.transform.parent = transform;
    }

    // Make sure there is only one instance and set prices
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
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

    /// <summary>
    /// Calls <see cref="MatchHistoryToString()">MatchHistoryToString()</see> and changes returned value to pretty string, ready to display to the user
    /// </summary>
    /// <returns>
    /// Depending on match history:
    /// [..., ...]
    /// [W, ...] 
    /// [L, ...] 
    /// [L, W, ...]
    /// [W, L, ...] 
    /// [L, L]
    /// [W, W]
    /// </returns>
    public string PlayerFriendlyMatchHistory()
    {
        string matchHist = MatchHistoryToString();
        string result = "";
        if (matchHist.Length == 0)
        {
            result = "[..., ...]";
        }
        else if (matchHist.Length == 1)
        {
            if (amIMaster)
            {
                if (matchHist[0].Equals('t'))
                {
                    result = "[W, ...]"; // won first match as master
                }
                else if (matchHist[0].Equals('f'))
                {
                    result = "[L, ...]"; // lost first match as master
                }
            }
            else
            {
                if (matchHist[0].Equals('t'))
                {
                    result = "[L, ...]"; // lost first match as joined
                }
                else if (matchHist[0].Equals('f'))
                {
                    result = "[W, ...]"; // won first match as joined
                }
            }
        }
        else if (matchHist.Length == 2)
        {
            // [W, L, ...] [L, W, ...] [W, W] [L, L]
            if (amIMaster)
            {
                if (matchHist[0].Equals(matchHist[1]))
                {
                    // WW / LL
                    // "["
                    // "[tt"
                    // "[tt]"
                    // "[WW]"
                    // "[W, W]"
                    result = "[";
                    result += matchHist;
                    result += "]";
                    result = result.Replace('t', 'W');
                    result = result.Replace('f', 'L');
                    result = result.Insert(2, ", ");

                } 
                else
                {
                    // LW / WL
                    // "["
                    // "[tf"
                    // "[tf]"
                    // "[WL]"
                    // "[WL, ...]"
                    // "[W, L, ...]"
                    result = "[";
                    result += matchHist;
                    result += "]";
                    result = result.Replace('t', 'W');
                    result = result.Replace('f', 'L');
                    result = result.Insert(3, ", ...");
                    result = result.Insert(2, ", ");
                }
            }
            else
            {
                if (matchHist[0].Equals(matchHist[1]))
                {
                    // WW / LL
                    // "["
                    // "[tt"
                    // "[tt]"
                    // "[WW]"
                    // "[W, W]"
                    result = "[";
                    result += matchHist;
                    result += "]";
                    result = result.Replace('t', 'L');
                    result = result.Replace('f', 'W');
                    result = result.Insert(2, ", ");

                }
                else
                {
                    // LW / WL
                    // "["
                    // "[tf"
                    // "[tf]"
                    // "[WL]"
                    // "[WL, ...]"
                    // "[W, L, ...]"
                    result = "[";
                    result += matchHist;
                    result += "]";
                    result = result.Replace('t', 'L');
                    result = result.Replace('f', 'W');
                    result = result.Insert(3, ", ...");
                    result = result.Insert(2, ", ");
                }
            }
        }
        else if (matchHist.Length == 3)
        {
            if (amIMaster)
            {
                // "["
                // "[tft"
                // "[tft]"
                // "[WLW]"
                // "[W, L, W]"
                result = "[";
                result += matchHist;
                result += "]";
                result = result.Replace('t', 'W');
                result = result.Replace('f', 'L');
                result = result.Insert(3, ", ");
                result = result.Insert(2, ", ");
            }
            else
            {
                // "["
                // "[tft"
                // "[tft]"
                // "[LWL]"
                // "[L, W, L]"
                result = "[";
                result += matchHist;
                result += "]";
                result = result.Replace('t', 'L');
                result = result.Replace('f', 'W');
                result = result.Insert(3, ", ");
                result = result.Insert(2, ", ");
            }
        }
        else
        {
            result = "???";
            print("aaa");
        }
        print("Player Friendly match history: " + result + " from: \"" + matchHist + "\"");
        return result;
    }

    /// <summary>
    /// Takes stored list of bools of who won, and returns it in string format
    /// </summary>
    /// <returns>
    /// Ordered string in "XXX" format, where X is either "t" or "f". "t" means master won that round.
    /// </returns>
    public string MatchHistoryToString()
    {
        string tmp = "";
        for (int i = 0; i < didMasterWin.Count; i++)
        {
            if (didMasterWin[i])
            {
                tmp += "t";
            }
            else
            {
                tmp += "f";
            }
        }
        return tmp;
    }

    /// <summary>
    /// Takes match history in string format and sets it as the correct one. 
    /// See <see cref="MatchHistoryToString()">MatchHistoryToString()</see>.
    /// Used to synchronize win history between master and joined.
    /// </summary>
    /// <param name="hist">String in "XXX" format, where X is either "t" or "f". "t" means master won that round.</param>
    public void MatchHistoryFromString(string hist)
    {
        string debugString = "CSM:Received match history from string: " + hist + "\n";
        didMasterWin.Clear();
        debugString += "CSM:Cleared local history: " + MatchHistoryToString() + "\n";
        for (int i = 0; i < hist.Length; i++)
        {
            if (hist[i].Equals('t'))
            {
                didMasterWin.Add(true);
            }
            else if (hist[i].Equals('f'))
            {
                didMasterWin.Add(false);
            }
            else
            {
                Debug.LogError("Got bad data to read match history!!! from provided string: " + hist);
            }
        }
        debugString += "CSM:Now set history is: " + MatchHistoryToString();
        print(debugString);
    }

    /// <summary>
    /// Randomizes without repeating which maps are going to be played. Modifies 
    /// <see cref="mapMiddleNames">mapMiddleNames</see>
    /// list. Implementation has debugging tools commented out.
    /// </summary>
    public void RandomizeMapSelection()
    {
        List<int> possible = Enumerable.Range(1, amountOfMaps).ToList();
        List<int> listNumbers = new List<int>();
        for (int i = 0; i < 3; i++)
        {
            int index = UnityEngine.Random.Range(0, possible.Count);
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
        // mapMiddleNames.Clear();
        // mapMiddleNames.Add("5");
        // mapMiddleNames.Add("5");
        // mapMiddleNames.Add("5");

    }

    /// <summary>
    /// Processess any damage that is to be dealt to defender and 
    /// adds money to the attacker for every dmg point dealt.
    /// If defender health reaches zero, sets the 
    /// <see cref="hasDefenderDied">hasDefenderDied</see>
    /// flag and sets defender health to zero, as to prettify looks.
    /// </summary>
    /// <param name="amount">Amount of damage to deal</param>
    public void TakeDefenderDamageAndCheckIfDied(int amount)
    {
        if (hasDefenderDied)
        {
            return;
        }
        //print("Damaging defender: before hit: " + defenderHealth);
        defenderHealth -= amount;
        //print("Damaging defender: after hit: " + defenderHealth);
        if (!amIDefender)
        {
            AddMoney(amount * goldForDamageMultiplier);
        }
        if (defenderHealth <= 0)
        {
            print("Defnder diededed: " + defenderHealth);
            hasDefenderDied = true;
            defenderHealth = 0;
        }
    }

    /// <summary>
    /// Resets money, mana, defender health, did spawn delay passed, has defender died.
    /// Designed to be called in-between rounds.
    /// </summary>
    public void SoftReset()
    {
        print("CSM Soft Reset: \nmoney mana defHealth spawnDelayPassed hasDefenderDied");
        playerMoney = defaultMoneyAmount;
        playerMana = defaultManaAmount;
        defenderHealth = defaultDefenderHealth;
        //currentMatchMaxTime = defaultCurrentMatchMaxTime;
        //delayFirstSpawn = defaultDelayFirstSpawn;
        spawnDelayPassed = false;
        hasDefenderDied = false;
        //amIDefender = false;
    }

    /// <summary>
    /// Resets money, mana, defender health, match time, delay first spawn, 
    /// enemy nickname, local nickname, selected list of maps, 
    /// history of matches, and all possible bools.
    /// Designed to be called in-between matches.
    /// </summary>
    public void FullReset()
    {
        print("CSM Full Reset");
        playerMoney = defaultMoneyAmount;
        playerMana = defaultManaAmount;
        defenderHealth = defaultDefenderHealth;
        currentMatchMaxTime = defaultCurrentMatchMaxTime;
        delayFirstSpawn = defaultDelayFirstSpawn;
        enemyNickname = "";
        myNickName = "";
        mapMiddleNames.Clear();
        didMasterWin.Clear();
        // All not-set bools are implicitly false
        amIMaster = false;
        amIDefender = false;
        hasDefenderDied = false;
        isMoneyInfinite = false;
        isManaInfinite = false;
        invincibleTurrets = false;
        isMatchOver = false;
        spawnDelayPassed = false;
    }

    /// <summary>
    /// Shows short animation of getting or loosing money at current mouse location. 
    /// This method does not add or remove money - use
    /// <see cref="PayWithMoney(int)">PayWithMoney()</see> or 
    /// <see cref="AddMoney(int)">AddMoney()</see>.
    /// Is overloaded with 
    /// <see cref="ShowMoneyChange(int, bool, Vector2)">3-param version</see>.
    /// </summary>
    /// <param name="cost">Amount to show</param>
    /// <param name="isPaying">True shows "+" sign, false shows "-" sign</param>
    private void ShowMoneyChange(int cost, bool isPaying)
    {
        mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        GameObject go = Instantiate(showPriceCostPrefab, new Vector3(mouseWorldPos.x + 1, mouseWorldPos.y, 0), Quaternion.identity);
        if (isPaying)
        {
            go.transform.Find("Price").GetComponent<TMP_Text>().text = "-" + cost.ToString() + " G";
        }
        else
        {
            go.transform.Find("Price").GetComponent<TMP_Text>().text = "+" + cost.ToString() + " G";
        }
    }

    /// <summary>
    /// Shows short animation of getting or loosing money at specified location on the screen.
    /// This method does not add or remove money - use
    /// <see cref="PayWithMoney(int)">PayWithMoney()</see>.
    /// Is overloaded with 
    /// <see cref="ShowMoneyChange(int, bool)">2-param version</see>.
    /// </summary>
    /// <param name="cost">Amount to show</param>
    /// <param name="isPaying">True shows "+" sign, false shows "-" sign</param>
    /// <param name="position">Position at the screen</param>
    private void ShowMoneyChange(int cost, bool isPaying, Vector2 position)
    {
        GameObject go = Instantiate(showPriceCostPrefab, new Vector3(position.x, position.y, 0), Quaternion.identity);
        if (isPaying)
        {
            go.transform.Find("Price").GetComponent<TMP_Text>().text = "-" + cost.ToString() + " G";
        }
        else
        {
            go.transform.Find("Price").GetComponent<TMP_Text>().text = "+" + cost.ToString() + " G";
        }
    }

    /*
    public void ShowHealthChange(float amount, bool isDamaging)
    {
        mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        GameObject go = Instantiate(showHealthChangePrefab, new Vector3(mouseWorldPos.x + 1, mouseWorldPos.y, 0), Quaternion.identity);
        if (isDamaging)
        {
            go.transform.Find("Health").GetComponent<TMP_Text>().text = "-" + amount.ToString();
        }
        else
        {
            go.transform.Find("Health").GetComponent<TMP_Text>().color = Color.green;
            go.transform.Find("Health").GetComponent<ShowPrice>().durationSeconds = 2f;
            go.transform.Find("Health").GetComponent<TMP_Text>().text = "+" + amount.ToString();
        }
    }

    public void ShowHealthChange(float amount, bool isDamaging, Vector2 position)
    {
        GameObject go = Instantiate(showHealthChangePrefab, new Vector3(position.x + 1, position.y, 0), Quaternion.identity);
        if (isDamaging)
        {
            go.transform.Find("Health").GetComponent<TMP_Text>().text = "-" + amount.ToString();
        }
        else
        {
            go.transform.Find("Health").GetComponent<TMP_Text>().color = Color.green;
            go.transform.Find("Health").GetComponent<ShowPrice>().durationSeconds = 2f;
            go.transform.Find("Health").GetComponent<TMP_Text>().text = "+" + amount.ToString();
        }
    }
    */

    public void ShowHealthChange(float amount, bool isDamaging, Vector2 position, Color? color, float duration = 1, int fontSize = 36)
    {
        GameObject go = Instantiate(showHealthChangePrefab, new Vector3(position.x, position.y, 0), Quaternion.identity);
        if (isDamaging)
        {
            if (!color.HasValue)
            {
                color = Color.red;
            }
            go.transform.Find("Health").GetComponent<ShowPrice>().durationSeconds = duration;
            go.transform.Find("Health").GetComponent<TMP_Text>().color = (Color)color;
            go.transform.Find("Health").GetComponent<TMP_Text>().fontSize = fontSize;
            go.transform.Find("Health").GetComponent<TMP_Text>().text = "-" + amount.ToString();
        }
        else
        {
            if (!color.HasValue)
            {
                color = Color.green;
            }
            go.transform.Find("Health").GetComponent<ShowPrice>().durationSeconds = duration;
            go.transform.Find("Health").GetComponent<TMP_Text>().color = (Color)color;
            go.transform.Find("Health").GetComponent<TMP_Text>().fontSize = fontSize;
            go.transform.Find("Health").GetComponent<TMP_Text>().text = "+" + amount.ToString();
        }

    }

    /// <summary>
    /// Tries to pay, and if succesfull, calls
    /// <see cref="ShowMoneyChange(int, bool)">ShowMoneyChange(int, bool)</see>.
    /// Infinite money will always return true.
    /// </summary>
    /// <param name="cost"></param>
    /// <returns>true if bought, false otherwise</returns>
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

    /// <summary>
    /// Adds the amount of money specified, and shows it at current mouse location. 
    /// Is overloaded with 
    /// <see cref="AddMoney(int, Vector2)">2-param version</see>.
    /// </summary>
    /// <param name="amount">Amount of money to add</param>
    public void AddMoney(int amount)
    {
        ShowMoneyChange(amount, false);
        playerMoney += amount;
    }

    /// <summary>
    /// Adds the amount of money specified, and shows it at specified location.
    /// Is overloaded with 
    /// <see cref="AddMoney(int, Vector2)">2-param version</see>.
    /// </summary>
    /// <param name="amount">Amount of money to add</param>
    /// <param name="fromWhere">Location to show the animation</param>
    public void AddMoney(int amount, Vector2 fromWhere)
    {
        ShowMoneyChange(amount, false, fromWhere);
        playerMoney += amount;
    }

    /// <summary>
    /// Checks if player can afford cost specified.
    /// </summary>
    /// <param name="cost">cost to check</param>
    /// <returns>true if can afford or money is infinite; false otherwise</returns>
    public bool CanPlayerAffordWithMoney(int cost)
    {
        if (isMoneyInfinite)
        {
            return true;
        }
        else if (playerMoney >= cost)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool CanPlayerAffordWithMana(int cost)
    {
        if (isManaInfinite)
        {
            return true;
        }
        else if (playerMana >= cost)
        {
            return true;
        }
        else
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
