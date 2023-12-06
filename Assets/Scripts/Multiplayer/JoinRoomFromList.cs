using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class JoinRoomFromList : MonoBehaviour
{
    public void OnMouseDown()
    {
        PhotonNetwork.JoinRoom(gameObject.transform.Find("RoomName").GetComponent<TMP_Text>().text);
    }
}
