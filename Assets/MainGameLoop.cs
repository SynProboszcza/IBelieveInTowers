using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MainGameLoop : MonoBehaviour
{
    public GameObject nodePrefab;

    // playArenaCorners is an object that has 4 properly named child gameObjects
    // their transform.position 's are corners of play arena
    public Transform playArenaCorners;
    public Transform[] waypoints;
    public Transform[] obstacles;
    public Vector2 topLeft;
    public Vector2 topRight;
    public Vector2 bottomLeft;
    public Vector2 bottomRight;
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
        //update their money
        //synchronize with host/client
        //maybe cheats?
        //wait for escape menu
        //maybe listen for quitting? or add button for it

    }

    private void GenerateAndConfigureNodeMesh(Vector2 topleft, Vector2 bottomright)
    {
        if (nodePrefab != null) { 
            Vector2 distance = topleft.Abs() - bottomright.Abs();
            distance = distance.Abs();
            for (int x = 0; x <= distance.x; x++)
            {
                for (int y = 0; y <= distance.y; y++)
                {
                    //it would be better to check if node is on
                    //forbidden tile, then skip that tile
                    //if ()
                    //{
                    //
                    //}
                    GameObject node = Instantiate(nodePrefab, new Vector3(topleft.x + x, bottomright.y + y, -1f), Quaternion.identity);
                    //configure node right here and right now
                    //reduce number of nodes - we cant place them on:
                    //paths, obstacles, spawners, despawners,
                    //other turrets need to be configured to not let any more
                    //or they need to be upgradable
                }
            }
        } 
        else
        {
            Debug.Log("nie mamy nodes");
        }
        Debug.Log("function ended");
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
                topLeft = child.transform.position;
            }
            else if (child.name == "topRight")
            {
                topRight = child.transform.position;
            }
            else if (child.name == "bottomLeft")
            {
                bottomLeft = child.transform.position;
            }
            else if (child.name == "bottomRight")
            {
                bottomRight = child.transform.position;
            }
        }
    }
}
