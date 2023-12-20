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
    public int playerMoney
    { get; private set;}
    public int playerMana
    { get; private set; }
    [HideInInspector]
    public Dictionary<string, int> enemyPrices;
    public int bearPrice = 500;
    public int bettlePrice = 100;
    public int opossumPrice = 200;
    public int dinoPrice = 150;
    public int slimerPrice = 250;

    void Start()
    {
        DontDestroyOnLoad(gameObject);
        playerMoney = 2000;
        playerMana = 500;
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

    public bool PayWithMoney(int cost)
    {
        if (playerMoney >= cost)
        {
            playerMoney -= cost;
            return true;
        }
        return false;
    }

    public bool PayWithMana(int cost)
    {
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
}
