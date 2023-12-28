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
    //{ get; set;}
    public int playerMana;
    //{ get; private set; }
    public Queue<string> unitList { get; private set; }
    [HideInInspector]
    public Dictionary<string, int> enemyPrices;
    public int bearPrice = 500;
    public int bettlePrice = 100;
    public int opossumPrice = 200;
    public int dinoPrice = 150;
    public int slimerPrice = 250;
    public int defenderHealth;
    public string enemyNickname = "";
    public string myNickName = "";
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
    }

    // Make sure there is only one instance
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
            { "Bear", bearPrice },
            { "Bettle", bettlePrice },
            { "Opossum", opossumPrice },
            { "Dino", dinoPrice },
            { "Slimer", slimerPrice }
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

    // set round time during play set up

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
