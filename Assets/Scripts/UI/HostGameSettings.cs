using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HostGameSettings : MonoBehaviour
{
    public void SetAttackOrDefend()
    {
        if (gameObject.GetComponent<Toggle>().isOn)
        {
            transform.Find("Label").GetComponent<Text>().text = "I am defending first!";
        } else
        {
            transform.Find("Label").GetComponent<Text>().text = "I am attacking first!";
        }
    }
}
