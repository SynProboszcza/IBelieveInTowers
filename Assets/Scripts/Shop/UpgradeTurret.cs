using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeTurret : MonoBehaviour
{
    private GameObject mainGame;
    public int upgradeCost = 100;
    // Start is called before the first frame update
    void Start()
    {
        mainGame = GameObject.FindWithTag("SingleTagForMainGameLoop");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMouseDown()
    {
        // If player can afford
        // Try to pay
        // Check if upgraded
        if (mainGame.GetComponent<MainGameLoop>().CanPlayerBearCost(upgradeCost)) 
        {
            if (mainGame.GetComponent<MainGameLoop>().PayWithPlayerMoney(upgradeCost))
            {
                if(transform.parent.gameObject.GetComponent<MainTurret>().LevelUp())
                {
                    Debug.Log("Upgraded");
                } else
                {
                    Debug.Log("NOT Upgraded");
                }
            } else
            {
                Debug.Log("We checked and player could afford, and then not");
            }
        }
    }
}
