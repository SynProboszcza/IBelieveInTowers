using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Multi as in Multiplayer, not multiple offer per one
// This is one script for all turrets and potentially spells
public class MultiTurretOffer : MonoBehaviour
{
    public GameObject turretPrefab;
    private GameObject multiMainGame;
    public int turretCost = 50;
    void Start()
    {
        multiMainGame = GameObject.FindWithTag("SingleTagForMainGameLoop");

        // Set turret base sprite
        gameObject.GetComponent<SpriteRenderer>().sprite = turretPrefab.GetComponent<SpriteRenderer>().sprite;
        // Set turret weapon sprite
        gameObject.transform.Find("WeaponSprite").GetComponent<SpriteRenderer>().sprite = turretPrefab.transform.Find("Gun").GetComponent<SpriteRenderer>().sprite;
    }

    private void OnMouseDown()
    {
        BuyTurret(turretPrefab, turretCost);
    }

    public void BuyTurret(GameObject turret, int cost)
    {
        if (multiMainGame.GetComponent<MultiplayerMainGameLoop>().PayWithGold(cost))
        {
            string _turretName = "Turrets/" + turret.name;
            PhotonNetwork.Instantiate(_turretName, new Vector3(transform.parent.transform.parent.position.x, transform.parent.transform.parent.position.y, 0), Quaternion.identity);
            CloseNode();
        }
        else
        {
            Debug.Log("Not enough money to buy a turret");
            CloseShop();
        }
    }

    private void CloseShop()
    {
        transform.parent.transform.parent.GetComponent<MultiShopContainer>().CloseShop();
    }

    private void CloseNode()
    {
        transform.parent.transform.parent.GetComponent<MultiShopContainer>().CloseNode();
    }
}
