using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shop : MonoBehaviour
{
    private SpriteRenderer sr;
    [Tooltip("Distance to close shop in worldspace units")]
    public float distanceToCloseShop = 4;

    // Start is called before the first frame update
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        //check current mouse position to know when 
        //close the shop - if mouse is 2 units away - close
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (
            (Vector2.Distance(mouseWorldPos, gameObject.transform.GetChild(0).transform.position) > distanceToCloseShop) 
            && gameObject.transform.GetChild(0).gameObject.activeSelf)
        {
            sr.enabled = false;
            gameObject.transform.GetChild(0).gameObject.SetActive(false);
            Debug.Log("mouse:" + mouseWorldPos);
            Debug.Log("shop:" + gameObject.transform.GetChild(0).transform.position);
            Debug.Log("distance:" + Vector2.Distance(Input.mousePosition, gameObject.transform.GetChild(0).transform.position));
        }
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
        Debug.Log("Opening non-existent shop interface");
        gameObject.transform.GetChild(0).gameObject.SetActive(true);
        //Destroy(gameObject);
    }
}
