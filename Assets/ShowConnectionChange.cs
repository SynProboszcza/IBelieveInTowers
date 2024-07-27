using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowConnectionChange : MonoBehaviour
{
    public Sprite connectingIcon;
    public Sprite connectedIcon;
    public Sprite connectionError;

    public void ShowConnecting()
    {
        gameObject.GetComponent<Image>().sprite = connectingIcon;
    }

    public void ShowConnected()
    {
        gameObject.GetComponent<Image>().sprite = connectedIcon;
    }

    public void ShowConnectionError()
    {
        gameObject.GetComponent<Image>().sprite = connectionError;
    }
}
