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
    [SerializeField]
    private TMP_InputField _nickName;
    public GameObject showConnection;
    public string backupNickNamePrefix = "defaultNickname";
    public string gameVersion = "0.1";


    void Start()
    {
        print("Connecting to server...");
        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.ConnectUsingSettings();
    }

    public void OnClickCreateRoom()
    {
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            Debug.Log("Not Connected, aborting creating room");
            showConnection.GetComponent<TMP_Text>().text = "Can't connect or not ready yet!";
            return;
        }
        RoomOptions options = new RoomOptions();
        options.MaxPlayers = 2;
        
        // Checking for room name, if not exists set up default
        if (_roomName.text == "")
        {
            _roomName.text = "DefaultRoomName";
        }

        // Checking for nickname, if not exists set up default
        // There is minimum length for nickname specified by Dawid,
        // not demanded by Photon
        if(_nickName.text.ToString().Length > 3)
        {
            PhotonNetwork.NickName = _nickName.text.ToString();
        } else
        {
            backupNickNamePrefix += Random.Range(0, 55555).ToString();
            PhotonNetwork.NickName = backupNickNamePrefix;
        }
        PhotonNetwork.CreateRoom(_roomName.text, options, TypedLobby.Default);
    }

    public override void OnConnectedToMaster()
    {
        showConnection.GetComponent<TMP_Text>().text = "Connected to master";
        base.OnConnectedToMaster();
    }

    public override void OnCreatedRoom()
    {
        showConnection.GetComponent<TMP_Text>().text = "Joining room: " + _roomName.text + " as " + PhotonNetwork.LocalPlayer.NickName + "...";
        PhotonNetwork.JoinLobby();
        base.OnCreatedRoom();
    }

    public override void OnJoinedLobby()
    {
        // TODO change scene here
        print("Joined Lobby" + PhotonNetwork.CurrentLobby.ToString());
        base.OnJoinedLobby();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        showConnection.GetComponent<TMP_Text>().text = "Failed to create room!";
        base.OnCreateRoomFailed(returnCode, message);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        print("Disconnected because: " + cause.ToString());
        base.OnDisconnected(cause);
    }

}
