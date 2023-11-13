using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretOffer : MonoBehaviour
{
    public GameObject turretPrefab;
    public GameObject mainGame;
    public int turretCost = 50;
    // Start is called before the first frame update
    void Start()
    {
        mainGame = GameObject.FindWithTag("SingleTagForMainGameLoop");
        // Set turret base sprite
        gameObject.GetComponent<SpriteRenderer>().sprite = turretPrefab.GetComponent<SpriteRenderer>().sprite;
        // Set turret weapon sprite
        gameObject.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = turretPrefab.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite;
    }

    // Update is called once per frame
    void Update()
    {
        
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
            //show some error; money is tight
            Debug.Log("no moneeeeeyyyyyyy");
            CloseShop();
        }
    }

    public void CloseShop()
    {
        transform.parent.transform.parent.GetComponent<ShopContainer>().CloseShop();
    }
    
    public void CloseNode()
    {
        transform.parent.transform.parent.GetComponent<ShopContainer>().CloseNode();
    }
}
