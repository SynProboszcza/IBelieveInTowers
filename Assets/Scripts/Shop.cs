using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shop : MonoBehaviour
{
    private SpriteRenderer sr;

    // Start is called before the first frame update
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {

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
        //Destroy(gameObject);
    }
}
