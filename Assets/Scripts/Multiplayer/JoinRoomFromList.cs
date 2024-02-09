using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class JoinRoomFromList : MonoBehaviour
{
    [HideInInspector]
    public string rawRoomName;
    // Raw room name is set when instantiating this prefab
    // so we don't need to worry if it is null
    public void ClickInterceptToJoinRoomFromList()
    {
        string _roomName = gameObject.transform.Find("RoomName").GetComponent<TMP_Text>().text;
        print("Connecting with room: " + _roomName);
        // Leaving custom print for now, bc its nice for debugging
        CreateRoomMenu.JoinRoomFromList(rawRoomName);
    }
}
