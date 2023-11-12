using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class MainGameLoop : MonoBehaviour
{
    public GameObject nodePrefab;
    public List<GameObject> nodes = new List<GameObject>();

    // playArenaCorners is an object that has 4 properly named child gameObjects
    // their transform.position 's are corners of play arena
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
    public float shopNodesZOffset = -2;
    public bool isDebugging = false;
    public bool isShopOpen = false;
    // Start is called before the first frame update
    void Start()
    {
        GetPlayArenaCorners();
        GenerateAndConfigureNodeMesh(topLeft, bottomRight);

        //configure them maybe?
    }

    // Update is called once per frame
    void Update()
    {
        //update player health
        string playerHealthTextTemplate = "Zdrowie: " + playerHealth;
        playerHealthTextField.text = playerHealthTextTemplate;
        //update their money
        string playerMoneyTextTemplate = "Gold: " + playerMoney;
        playerMoneyTextField.text = playerMoneyTextTemplate;

        //synchronize with host/client
        //maybe cheats?
        //wait for escape menu
        //maybe listen for quitting? or add button for it

    }

    public void TakePlayerDamage(float damage)
    {
        playerHealth -= damage;
    }

    public void AddPlayerMoney(int money)
    {
        playerMoney += money;
    }

    private void GenerateAndConfigureNodeMesh(Vector2 topleft, Vector2 bottomright)
    {
        nodesInstantiated = 0;
        if (nodePrefab != null) {
            Vector2 distance = (new Vector2(Mathf.Abs(topleft.x), Mathf.Abs(topleft.y))) - new Vector2(Mathf.Abs(bottomright.x), Mathf.Abs(bottomright.y));
            distance = new Vector2(Mathf.Abs(distance.x), Mathf.Abs(distance.y));
            for (int x = 0; x <= distance.x; x++)
            {
                for (int y = 0; y <= distance.y; y++)
                {
                    //it would be better to check if node is on
                    //forbidden tile, then skip that tile
                    GameObject singleNode = Instantiate(nodePrefab, new Vector3(topleft.x + x, bottomright.y + y, shopNodesZOffset), Quaternion.identity);
                    //configure node right here and right now
                    //reduce number of nodes - we cant place them on:
                    //paths, obstacles, spawners, despawners,
                    //other turrets need to be configured to not let any more
                    //or they need to be upgradable
                    nodesInstantiated++;
                    nodes.Add(singleNode);
                }
            }
            //remove nodes from obstacles
            foreach (GameObject node in nodes)
            {
                foreach (Transform obstacle in obstacles)
                {
                    //if(node.transform.position == obstacle.transform.position)
                    if(Mathf.Approximately(node.transform.position.x, obstacle.transform.position.x) && Mathf.Approximately(node.transform.position.y, obstacle.transform.position.y))
                    {
                        Destroy(node.gameObject);
                        nodesDestroyed++;
                    }
                }
            }
            //remove nodes from path

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
        } 
        else
        {
            Debug.Log("nie mamy nodes");
        }
        Debug.Log("function ended, instantiated: " + nodesInstantiated + ", destroyed: " + nodesDestroyed);
    }

    private void GetPlayArenaCorners()
    {
        //topLeft = playArenaCorners.GetChild(0).transform.position;
        //topRight = playArenaCorners.GetChild(1).transform.position;
        //bottomLeft = playArenaCorners.GetChild(2).transform.position;
        //bottomRight = playArenaCorners.GetChild(3).transform.position;
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
