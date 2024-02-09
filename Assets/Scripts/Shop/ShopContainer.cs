using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopContainer : MonoBehaviour
{
    private SpriteRenderer sr;
    public GameObject mainGame;
    [Tooltip("Distance from shop center to mouse position in worldspace units")]
    public float distanceToCloseShop = 4;
    private Vector2 mouseWorldPos;

    // Start is called before the first frame update
    void Start()
    {
        gameObject.transform.GetChild(0).gameObject.SetActive(false);
        sr = GetComponent<SpriteRenderer>();
        sr.enabled = false;
        mainGame = GameObject.FindWithTag("SingleTagForMainGameLoop");
        mainGame.GetComponent<MainGameLoop>().isShopOpen = false;
    }

    // Update is called once per frame
    void Update()
    {
        // Close the shop if mouse is specified units away
        // or when other shop is open
        mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if ((
            Vector2.Distance(mouseWorldPos, gameObject.transform.GetChild(0).transform.position) > distanceToCloseShop) 
            && gameObject.transform.GetChild(0).gameObject.activeSelf
            )
        {
            CloseShop();
        }
    }

    private void OpenShop()
    {
        Debug.Log("Opening shop interface");
        gameObject.transform.GetChild(0).gameObject.SetActive(true);
        mainGame.GetComponent<MainGameLoop>().isShopOpen = true;
        //Destroy(gameObject);
    }

    public void CloseShop()
    {
        sr.enabled = false;
        gameObject.transform.GetChild(0).gameObject.SetActive(false);
        mainGame.GetComponent<MainGameLoop>().isShopOpen = false;
        //Debug.Log("mouse:" + mouseWorldPos);
        //Debug.Log("shop:" + gameObject.transform.GetChild(0).transform.position);
        //Debug.Log("distance:" + Vector2.Distance(Input.mousePosition, gameObject.transform.GetChild(0).transform.position));

    }

    public void CloseNode()
    {
        CloseShop();
        Destroy(gameObject);
    }

    // Turn background texture on when mouse drags over
    private void OnMouseOver()
    {
        sr.enabled = true;
    }

    // Turn backround texture off when mouse drags off
    private void OnMouseExit()
    {
        sr.enabled = false;
    }

    private void OnMouseDown()
    {
        if (!mainGame.GetComponent<MainGameLoop>().isShopOpen)
        {
            OpenShop();
        }
    }
}
