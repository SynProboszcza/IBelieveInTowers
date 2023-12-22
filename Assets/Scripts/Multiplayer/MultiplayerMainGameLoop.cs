using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.UI.Image;

public class MultiplayerMainGameLoop : MonoBehaviourPunCallbacks
{
    // Public instance so we can enable the correct one
    // Every scene(map) has two parts for simplicity
    public GameObject attackerPart;
    public GameObject defenderPart;

    public GameObject nodePrefab;
    [HideInInspector]
    public List<GameObject> nodes = new List<GameObject>();
    private GameObject shopNodesCollection;

    // playArenaCorners is an object that has 4 properly named child gameObjects
    // Their transform.position 's are corners of play arena
    public Transform playArenaCorners;
    public Transform[] waypoints;
    public Transform[] obstacles;
    public TMP_Text enemyHealthTextField;
    public TMP_Text playerMoneyTextField;
    public TMP_Text playerManaTextField;
    private Vector2 topLeft;
    private Vector2 topRight;
    private Vector2 bottomLeft;
    private Vector2 bottomRight;
    [HideInInspector]
    public int nodesInstantiated = 0;
    [HideInInspector]
    public int nodesDestroyed = 0;
    public int playerMoney = 0;
    public int playerMana = 0;
    public int defenderHealth;
    [Tooltip("Specifies an offset on a Z axis. 0 is default map, camera is -15, positive will be hidden. This is mainly for colliders to work properly")]
    public static float shopNodesZOffset = -2;
    public bool isDebugging = false;
    [HideInInspector]
    public bool isShopOpen = false;
    public bool isAllowedInstantiating = true;
    [HideInInspector]
    public bool amIMaster;
    [HideInInspector]
    public bool amIDefender;

    void Awake()
    {
        // Doing this because difference in framerate between Editor and 
        // an application causes differences in projectile speed
        //
        // This should be copied inside MainMenuLoop and MainGameLoop
        // (i think(?)(and maybe every scene?))
        QualitySettings.vSyncCount = 0;  // VSync must be disabled
        Application.targetFrameRate = 60;
    }

    void Start()
    {
        // Check if defending, then activate proper part
        amIMaster = CrossSceneManager.instance.amIMaster;
        amIDefender = CrossSceneManager.instance.amIDefender;
        playerMoney = CrossSceneManager.instance.playerMoney;
        playerMana = CrossSceneManager.instance.playerMana;
        defenderPart.gameObject.SetActive(false);
        attackerPart.gameObject.SetActive(false);
        if (amIDefender)
        {
            defenderPart.gameObject.SetActive(true);
        } else
        {
            attackerPart.gameObject.SetActive(true);
            defenderHealth = CrossSceneManager.instance.defenderHealth;
        }

        if (isAllowedInstantiating && amIDefender)
        {
            shopNodesCollection = new GameObject("ShopNodesCollection");
            GetPlayArenaCorners();
            GenerateAndConfigureNodeMesh(topLeft, bottomRight);
            // TODO: ShopNode needs to show spells
        } else 
        {
            // We're attacking so we need to instantiate spell mesh
        }
        // Configure them maybe?
        
    }

    void Update()
    {
        string enemyHealthTextTemplate = "Health: " + CrossSceneManager.instance.defenderHealth;
        enemyHealthTextField.text = enemyHealthTextTemplate;
        string playerMoneyTextTemplate = "Gold: " + CrossSceneManager.instance.playerMoney;
        playerMoneyTextField.text = playerMoneyTextTemplate;
        string playerManaTextTemplate = "Mana: " + CrossSceneManager.instance.playerMana;
        playerManaTextField.text = playerManaTextTemplate;

        // Check if defending when ded
        //  checking is done when dealing damage

        //synchronize with host/client
        //maybe cheats?
        //wait for escape menu
        //maybe listen for quitting? or add button for it

    }

    public void DefenderDied()
    {
        print("DEFENDER DIEDED XD");
    }

    public bool PayWithGold(int cost)
    {
        if (playerMoney >= cost)
        {
            playerMoney -= cost;
            return true;
        }
        else
        {
            return false;
        }
    }
    
    public bool PayWithMana(int cost)
    {
        if (playerMana >= cost)
        {
            playerMana -= cost;
            return true;
        }
        else
        {
            return false;
        }
    }

    public void TakeDefenderDamage(int damage)
    {
        defenderHealth -= damage;
        if (defenderHealth <= 0)
        {
            DefenderDied();
        }
    }

    public void AddPlayerMoney(int money)
    {
        playerMoney += money;
    }

    public void AddPlayerMana(int mana)
    {
        playerMana += mana;
    }

    // ----------------------------------------------------
    private void DestroyNode(GameObject node)
    {
        Destroy(node);
        nodesDestroyed++;
    }

    private void GenerateAndConfigureNodeMesh(Vector2 topleft, Vector2 bottomright)
    {
        nodesInstantiated = 0;
        if (nodePrefab != null)
        {
            Vector2 distance = (new Vector2(Mathf.Abs(topleft.x), Mathf.Abs(topleft.y))) - new Vector2(Mathf.Abs(bottomright.x), Mathf.Abs(bottomright.y));
            distance = new Vector2(Mathf.Abs(distance.x), Mathf.Abs(distance.y));
            for (float x = topleft.x; x <= distance.x + topleft.x; x++)
            {
                for (float y = bottomright.y; y <= bottomright.y + distance.y; y++)
                {
                    //it would be better to check if node is on
                    //forbidden tile, then skip that tile
                    GameObject singleNode = Instantiate(nodePrefab, new Vector3(x, y, shopNodesZOffset), Quaternion.identity);
                    singleNode.transform.SetParent(shopNodesCollection.transform);
                    //configure node right here and right now
                    //reduce number of nodes - we cant place them on:
                    //paths, obstacles, spawners, despawners,
                    //other turrets need to be configured to not let any more
                    //or they need to be upgradable
                    nodesInstantiated++;
                    nodes.Add(singleNode);
                }
            }


            // ---------------------------------------------------------------------------------------------
            // Remove nodes from obstacles
            // ---------------------------------------------------------------------------------------------
            //print("trying to remove nodes");
            //print(nodes);
            //print(obstacles[0].transform.position);
            foreach (GameObject node in nodes)
            {
                foreach (Transform obstacle in obstacles)
                {
                    //if(node.transform.position == obstacle.transform.position)
                    if (Mathf.Approximately(node.transform.position.x, obstacle.transform.position.x) 
                        && Mathf.Approximately(node.transform.position.y, obstacle.transform.position.y))
                    {
                        DestroyNode(node);
                    }
                }
            }
        }
        else
        {
            Debug.Log("Node prefabs were not set");
        }
        Debug.Log("shopNodes instantiated: " + nodesInstantiated + ", destroyed: " + nodesDestroyed + ",  left: " + (nodesInstantiated - nodesDestroyed));
    }

    private void GetPlayArenaCorners()
    {
        foreach (Transform child in playArenaCorners)
        {
            if (child.name == "topLeft")
            {
                if (isDebugging) { Debug.Log("got topleft:" + child.transform.position); }
                topLeft = child.transform.position;
            }
            else if (child.name == "topRight")
            {
                if (isDebugging) { Debug.Log("got topright"); }
                topRight = child.transform.position;
            }
            else if (child.name == "bottomLeft")
            {
                if (isDebugging) { Debug.Log("got botleft"); }
                bottomLeft = child.transform.position;
            }
            else if (child.name == "bottomRight")
            {
                if (isDebugging) { Debug.Log("got botright" + child.transform.position); }
                bottomRight = child.transform.position;
            }
        }
    }
}
