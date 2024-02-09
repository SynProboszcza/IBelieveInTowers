using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.UI.Image;

public class MainGameLoop : MonoBehaviour
{
    public GameObject nodePrefab;
    public List<GameObject> nodes = new List<GameObject>();
    private GameObject shopNodesCollection;

    // playArenaCorners is an object that has 4 properly named child gameObjects
    // Their transform.position 's are corners of play arena
    public Transform playArenaCorners;
    public Transform[] waypoints;
    public Transform[] obstacles;
    public TMP_Text playerHealthTextField;
    public TMP_Text playerMoneyTextField;
    public Vector2 topLeft;
    public Vector2 topRight;
    public Vector2 bottomLeft;
    public Vector2 bottomRight;
    public int nodesInstantiated = 0;
    public int nodesDestroyed = 0;
    public int playerMoney = 0;
    public float playerHealth;
    [Tooltip("Specifies an offset on a Z axis. 0 is default map, camera is -15, positive will be hidden. This is mainly for colliders to work properly")]
    public static float shopNodesZOffset = -2;
    public bool isDebugging = false;
    public bool isShopOpen = false;
    public bool isAllowedInstantiating = true;

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

    // Start is called before the first frame update
    void Start()
    {
        if (isAllowedInstantiating)
        {
            shopNodesCollection = new GameObject("ShopNodesCollection");
            GetPlayArenaCorners();
            GenerateAndConfigureNodeMesh(topLeft, bottomRight);
        }

        // Configure them maybe?
    }

    // Update is called once per frame
    void Update()
    {
        playerMoney = CrossSceneManager.instance.playerMoney;
        playerHealth = CrossSceneManager.instance.defenderHealth;
        //update player health
        string playerHealthTextTemplate = "Health: " + playerHealth;
        playerHealthTextField.text = playerHealthTextTemplate;
        //update their money
        string playerMoneyTextTemplate = "Gold: " + playerMoney;
        playerMoneyTextField.text = playerMoneyTextTemplate;

        //synchronize with host/client
        //maybe cheats?
        //wait for escape menu
        //maybe listen for quitting? or add button for it

    }

    public bool CanBuyTurret(int cost)
    {
        if(playerMoney >= cost)
        {
            playerMoney -= cost;
            return true;
        } else
        {
            return false;
        }
    }

    public bool CanPlayerBearCost(int cost)
    {
        if (playerMoney >= cost)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool PayWithPlayerMoney(int cost)
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

    public void TakePlayerDamage(float damage)
    {
        playerHealth -= damage;
    }

    public void AddPlayerMoney(int money)
    {
        playerMoney += money;
    }

    private void DestroyNode(GameObject node)
    {
        Destroy(node);
        nodesDestroyed++;
    }

    private void GenerateAndConfigureNodeMesh(Vector2 topleft, Vector2 bottomright)
    {
        nodesInstantiated = 0;
        if (nodePrefab != null) {
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
            foreach (GameObject node in nodes)
            {
                foreach (Transform obstacle in obstacles)
                {
                    //if(node.transform.position == obstacle.transform.position)
                    if(Mathf.Approximately(node.transform.position.x, obstacle.transform.position.x) && Mathf.Approximately(node.transform.position.y, obstacle.transform.position.y))
                    {
                        DestroyNode(node);
                    }
                }
            }


            // ---------------------------------------------------------------------------------------------
            // ---------------------------------------------------------------------------------------------
            // My old attempts; TODO remove it 
            // ---------------------------------------------------------------------------------------------
            // ---------------------------------------------------------------------------------------------

            // It would be easier to change array to List
            // Add obstacles on path
            //for (int i = 0; i < waypoints.Length - 1; i++)
            //{
            //    if (waypoints[i].position.y == waypoints[i + 1].position.y)
            //    {
            //        // y = y, go horizontal
            //        for (float y = waypoints[i].position.y + 1; y <= waypoints[i /+ /1].position.y; y++)
            //        {
            //           Transform _obstacle = Instantiate(obstacles[0], new Vector3//(waypoints[i].position.x, y, 0), Quaternion.identity);
            //            //_obstacle.transform.parent = obstacles.;
            //        }
            //    }
            //}

            //remove nodes from path
            //for every pair
            //find out if it is vertical or horizontal
            //go ++ and destroy in approxx position
            //for(int i = 0; i <= waypoints.Length - 2; i++)
            //{
            //    if (Mathf.Approximately(waypoints[i].position.x, waypoints[i//+1].position.x))
            //    {
            //        // x = x, means its on the same vertical axis
            //        // so we iterate for ys
            //        for(float y = waypoints[i].position.y; y <= waypoints[i//+1].position.y ; y++)
            //        {
            //            //do stuff
            //            foreach(GameObject node in nodes)
            //            {
            //                if (Mathf.Approximately(waypoints[i].position.x, waypoints/[i/+1].position.x)
            //                    && Mathf.Approximately(waypoints[i].position.y, /waypoints/[i + 1].position.y)
            //                    )
            //                {
            //                    DestroyNode(node);
            //                }
            //            }
            //        }
            //    } else if (Mathf.Approximately(waypoints[i].position.y, waypoints[i /+ /1].position.y))
            //    {
            //        // y = y, means its on the same horizontal axis
            //        // so we iterate for xs
            //        for (float x = waypoints[i].position.x; x <= waypoints[i /+ /1].position.x; x++)
            //        {
            //            //do stuff
            //            foreach (GameObject node in nodes)
            //            {
            //                if (Mathf.Approximately(waypoints[i].position.x, waypoints/[i /+ 1].position.x)
            //                    && Mathf.Approximately(waypoints[i].position.y, /waypoints/[i + 1].position.y)
            //                    )
            //                {
            //                    DestroyNode(node);
            //                }
            //            }
            //        }
            //    }
            //}


            //for (int i = 0; i < waypoints.Length - 2; i++)
            //{
            //    if (waypoints[i].transform.position.x == waypoints[i + 1].transform.position.x)
            //    {
            //        //delete line from this waypoint to next
            //        foreach(GameObject node in nodes)
            //        {
            //            if (waypoints[i + 1].transform.position.y < node.transform.position.y && node.transform.position.y < waypoints[i].transform.position.y)
            //            {
            //                Destroy (node.gameObject);
            //                nodesDestroyed++;
            //                Debug.Log("Destroyed x");
            //            }
            //        }
            //    } else if (waypoints[i].transform.position.y == waypoints[i + 1].transform.position.y)
            //    {
            //        //delete line from this waypoint to next
            //        foreach (GameObject node in nodes)
            //        {
            //            if (waypoints[i + 1].transform.position.x < node.transform.position.x && node.transform.position.x < waypoints[i].transform.position.x)
            //            {
            //                Destroy(node.gameObject);
            //                nodesDestroyed++;
            //                Debug.Log("Destroyed y");
            //            }
            //        }
            //    }
            //    else
            //    {
            //        Debug.Log("Could not find straight line inbetween waypoints, maybe position isnt int?");
            //    }
            //}
            // ---------------------------------------------------------------------------------------------
            // ---------------------------------------------------------------------------------------------
            // ---------------------------------------------------------------------------------------------
            // ---------------------------------------------------------------------------------------------

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
                if (isDebugging) { Debug.Log("got topleft"); }
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
                if (isDebugging) { Debug.Log("got botright"); }
                bottomRight = child.transform.position;
            }
        }
    }
}
