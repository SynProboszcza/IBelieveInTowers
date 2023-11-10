using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UpdatePlayerHealth : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        string health = "Zdrowie: " + gameObject.GetComponent<Health>().getHealth();
        gameObject.GetComponent<TMP_Text>().SetText(health);
    }
}
