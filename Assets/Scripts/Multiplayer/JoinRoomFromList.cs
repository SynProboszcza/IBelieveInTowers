using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class JoinRoomFromList : MonoBehaviour
{
    public void ClickIntercept()
    {
        string _roomName = gameObject.transform.Find("RoomName").GetComponent<TMP_Text>().text;
        print("Connecting with room: " + _roomName);
        CreateRoomMenu.JoinRoomFromList(_roomName);
    }
}
