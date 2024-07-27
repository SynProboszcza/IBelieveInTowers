using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowSettingsToCreateGameScript : MonoBehaviour
{
    public GameObject hostGameButton;
    public GameObject createRoomGroup;

    public void SwitchVisibility()
    {
        if(hostGameButton.activeSelf)
        {
            hostGameButton.SetActive(false);
            createRoomGroup.SetActive(false);
        } else
        {
            hostGameButton.SetActive(true);
            createRoomGroup.SetActive(true);
        }
    }
}
