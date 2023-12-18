using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RedGreenToggle : MonoBehaviour
{
    public void ChangeRedToGreenAndBack()
    {
        if (gameObject.GetComponent<Toggle>().isOn)
        {
            gameObject.GetComponent<Toggle>().transform.Find("Label").GetComponent<Text>().color = Color.green;
        }
        else
        {
            gameObject.GetComponent<Toggle>().transform.Find("Label").GetComponent<Text>().color = Color.red;
        }
    }
}
