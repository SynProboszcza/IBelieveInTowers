using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateRoomMenu : MonoBehaviourPunCallbacks
{

    [SerializeField]
    private TMP_InputField _roomName;
    public GameObject showConnection;


    public void OnClickCreateRoom()
    {
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            Debug.Log("Not Connected, aborting creating room");

            showConnection.GetComponent<TMP_Text>().text = "Connection is null or not ready yet!";
            return;
        }
        RoomOptions options = new RoomOptions();
        options.MaxPlayers = 2;
        print(":::" + _roomName.text + ":::");
        if (_roomName.text == "")
        {
            _roomName.text = "DefaultRoomName";
        }
        print(":::" + _roomName.text + ":::");
        PhotonNetwork.CreateRoom(_roomName.text, options, TypedLobby.Default);
    }

    public override void OnCreatedRoom()
    {
        // TODO: change scene to appropriate room

        showConnection.GetComponent<TMP_Text>().text = "Room created with name: "+ _roomName.text +"\n Joining...";
        PhotonNetwork.JoinLobby();
        base.OnCreatedRoom();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        showConnection.GetComponent<TMP_Text>().text = "Failed to create room!";
        base.OnCreateRoomFailed(returnCode, message);
    }

    public override void OnConnectedToMaster()
    {
        showConnection.GetComponent<TMP_Text>().text = "Connected to master!";

        base.OnConnectedToMaster();
    }
}
