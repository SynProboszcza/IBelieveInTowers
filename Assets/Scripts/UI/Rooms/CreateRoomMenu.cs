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
    [SerializeField]
    private Toggle[] settingToggles;
    public GameObject showConnection;
    public string backupNickNamePrefix = "defaultNickname";
    public string gameVersion = "0.1";


    void Start()
    {
        gameObject.GetComponent<Button>().interactable = false;
        print("Connecting to server...");
        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.ConnectUsingSettings();
    }

    // Called when player clicks Host Game button
    public void OnClickCreateRoom()
    {
        //
        //
        // Make toggles not clickable
        //
        //
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
            _nickName.text = backupNickNamePrefix;
            PhotonNetwork.NickName = backupNickNamePrefix;
        }

        PhotonNetwork.CreateRoom(_roomName.text, options, TypedLobby.Default);

    }

    public override void OnConnectedToMaster()
    {
        showConnection.GetComponent<TMP_Text>().text = "Connected to master";
        gameObject.GetComponent<Button>().interactable = true;
        base.OnConnectedToMaster();
    }

    public override void OnCreatedRoom()
    {
        showConnection.GetComponent<TMP_Text>().text = "Created room: " + _roomName.text;
        base.OnCreatedRoom();
    }

    public override void OnJoinedRoom()
    {
        // TODO change scene here
        showConnection.GetComponent<TMP_Text>().text = "Joined room: " + PhotonNetwork.CurrentRoom.Name;
        base.OnJoinedRoom();
    }

    public override void OnJoinedLobby()
    {
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
