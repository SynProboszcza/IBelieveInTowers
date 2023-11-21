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
        Debug.Log("no tak, kliknelo sie");
        if (mainGame.GetComponent<MainGameLoop>().CanBuyTurretOrUpgrade(upgradeCost)) 
        {
            transform.parent.gameObject.GetComponent<MainTurret>().LevelUp();
        }
    }
}
