using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PreMainGame : MonoBehaviour
{
    public Toggle readyToggle;
    void Start()
    {
        
    }

    void Update()
    {
        // Check for readiness
        if(readyToggle.isOn)
        {
            readyToggle.transform.Find("Label").GetComponent<Text>().color = Color.green;
        } else
        {
            readyToggle.transform.Find("Label").GetComponent<Text>().color = Color.red;

        }
        
    }
}
