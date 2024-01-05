using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MultiShopContainer : MonoBehaviour
{
    private SpriteRenderer sr;
    public GameObject mainGame;
    [Tooltip("Distance from shop center to mouse position in worldspace units")]
    public float distanceToCloseShop = 4;
    private Vector2 mouseWorldPos;

    void Start()
    {
        gameObject.transform.GetChild(0).gameObject.SetActive(false);
        sr = GetComponent<SpriteRenderer>();
        sr.enabled = false;
        mainGame = GameObject.FindWithTag("SingleTagForMainGameLoop"); // Multiplayer maingame is also tagged this
        mainGame.GetComponent<MultiplayerMainGameLoop>().isShopOpen = false;

    }

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
        //Debug.Log("Opening MULTIshop interface");
        gameObject.transform.GetChild(0).gameObject.SetActive(true);
        mainGame.GetComponent<MultiplayerMainGameLoop>().isShopOpen = true;
    }

    public void CloseShop()
    {
        sr.enabled = false;
        gameObject.transform.GetChild(0).gameObject.SetActive(false);
        gameObject.transform.Find("Background").Find("Canvas").Find("Price").GetComponent<TMP_Text>().text = "Select a turret!";
        mainGame.GetComponent<MultiplayerMainGameLoop>().isShopOpen = false;
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
        if (!mainGame.GetComponent<MultiplayerMainGameLoop>().isShopOpen)
        {
            OpenShop();
        }
    }
}
