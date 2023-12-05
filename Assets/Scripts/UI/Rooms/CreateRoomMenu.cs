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
    [SerializeField]
    private Button debugButton;
    public GameObject showConnection;
    public string backupNickNamePrefix = "defaultNickname";
    public string gameVersion = "0.1";


    // Check for connection and connect if not
    void Start()
    {
        // We need to check for readiness, because user can go back to main menu
        // and this will be called twice, and we cant connect twice because connection
        // persists between scene changes
        if (PhotonNetwork.IsConnectedAndReady)
        {
            gameObject.GetComponent<Button>().interactable = true;
            showConnection.GetComponent<TMP_Text>().text = "Connected to ";

            // Checking if connected to a particular room
            if (PhotonNetwork.CurrentRoom != null)
            {
                showConnection.GetComponent<TMP_Text>().text += "room: " + PhotonNetwork.CurrentRoom.Name;
                gameObject.GetComponent<Button>().interactable = false;
                // Code theoretically should not get here, but in case 
                // Change scene to PrePlay or Play
            }
            else
            {
                showConnection.GetComponent<TMP_Text>().text += "master";
            }
        }
        // We're not ready, so we need to set up and connect
        else
        {
            PhotonNetwork.GameVersion = gameVersion;
            gameObject.GetComponent<Button>().interactable = false;
            print("Connecting to server...");
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    // Called when player clicks Host Game button
    // If connectedAndReady check/set nickname and room name
    // and create room with these; also max players is set 2 2
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
        if(_nickName.text.ToString().Length >= 3)
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
        PhotonNetwork.JoinLobby(TypedLobby.Default);
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
        gameObject.GetComponent<Button>().interactable = false;
        base.OnJoinedRoom();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        print("List updated");
        print(roomList[0].Name);
        base.OnRoomListUpdate(roomList);
    }

    public override void OnJoinedLobby()
    {
        print("Joined Lobby: " + PhotonNetwork.CurrentLobby.ToString());
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
