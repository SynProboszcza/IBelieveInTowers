using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UpdateRoomList : MonoBehaviourPunCallbacks
{

    void Start()
    {
        // We need to check for readiness, because user can go back to main menu
        // and this will be called twice, and we cant connect twice because connection
        // persists between scene changes
        if (PhotonNetwork.IsConnectedAndReady)
        {

        }
        // We're not ready, so we need to set up and connect
        else
        {
            //PhotonNetwork.GameVersion = gameVersion;
            //gameObject.GetComponent<Button>().interactable = false;
            print("Connecting to server...");
            PhotonNetwork.ConnectUsingSettings();

        }
    }


    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        print("ROOMS UPDATED");
        print(roomList);
        base.OnRoomListUpdate(roomList);
    }
}
