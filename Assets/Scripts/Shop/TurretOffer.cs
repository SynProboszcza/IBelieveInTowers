using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretOffer : MonoBehaviour
{
    public GameObject turretPrefab;
    private GameObject mainGame;
    public int turretCost = 50;
    public string fireballSpritePath; 

    // Start is called before the first frame update
    void Start()
    {
        mainGame = GameObject.FindWithTag("SingleTagForMainGameLoop");
        // Set turret base sprite
        gameObject.GetComponent<SpriteRenderer>().sprite = turretPrefab.GetComponent<SpriteRenderer>().sprite;
        // Set turret weapon sprite
        gameObject.transform.Find("WeaponSprite").GetComponent<SpriteRenderer>().sprite = turretPrefab.transform.Find("Gun").GetComponent<SpriteRenderer>().sprite;

        fireballSpritePath = "Flames/Fireball/ffireball_0001";
        Sprite fireballSprite = Resources.Load<Sprite>(fireballSpritePath);

        if (fireballSprite != null)
        {
            gameObject.transform.Find("WeaponSprite").GetComponent<SpriteRenderer>().sprite = fireballSprite;
        }
        else
        {
            Debug.LogError("Failed to load the fireball sprite from Resources folder. Check the file path.");
        }

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

    public void BuyFireBall(Sprite fireballSprite, int cost)
    {
        if (mainGame.GetComponent<MainGameLoop>().CanBuyTurret(cost))
        {
            GameObject fireball = new GameObject("Fireball"); // Create an empty GameObject
            fireball.transform.position = transform.position; // Set the position
            fireball.AddComponent<SpriteRenderer>().sprite = fireballSprite;      

            mainGame.GetComponent<MainGameLoop>().CanBuyTurret(cost); 
            CloseNode();
        }
        else
        {
            Debug.Log("Not enough money to buy the fireball spell");
            CloseShop();
        }
    }

}
