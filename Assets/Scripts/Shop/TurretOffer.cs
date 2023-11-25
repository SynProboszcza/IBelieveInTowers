using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretOffer : MonoBehaviour
{
    public GameObject turretPrefab;
    private GameObject mainGame;
    public int turretCost = 50;
    // Start is called before the first frame update
    void Start()
    {
        mainGame = GameObject.FindWithTag("SingleTagForMainGameLoop");
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
        if (mainGame.GetComponent<MainGameLoop>().CanBuyTurret(cost))
        {
            Instantiate(turret, new Vector3(transform.parent.transform.parent.position.x, transform.parent.transform.parent.position.y, 0), Quaternion.identity);
            CloseNode();
        } else
        {
            Debug.Log("Not enough money to buy a turret");
            CloseShop();
        }
    }

    private void CloseShop()
    {
        transform.parent.transform.parent.GetComponent<ShopContainer>().CloseShop();
    }
    
    private void CloseNode()
    {
        transform.parent.transform.parent.GetComponent<ShopContainer>().CloseNode();
    }
}
