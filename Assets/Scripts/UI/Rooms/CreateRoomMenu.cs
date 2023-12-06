using Photon.Pun;
using Photon.Pun.Demo.Cockpit;
using Photon.Realtime;
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
    private Button refreshListButton;
    [SerializeField]
    private GameObject roomPrefab;
    [SerializeField]
    private GameObject roomList;
    [SerializeField]
    private TMP_Text showRoomsFound;
    public List<RoomInfo> openRoomsFromMaster = new List<RoomInfo>();
    public List<RoomInfo> openRoomsFromMasterCache = new List<RoomInfo>();
    public List<GameObject> displayedRoomsCache = new List<GameObject>();
    public GameObject showConnection;
    public string backupNickNamePrefix = "defaultNickname";
    public string gameVersion = "0.1";
    private bool restartConnectionFlag = false;


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
                //ShowCachedRooms();
                RestartConnection(); // Doing this to get OnRoomListUpdate callback
            }
        }
        // We're not ready, so we need to set up and connect
        else
        {
            SetUpConnection();
        }
    }

    // Called when player clicks Host Game button
    // If connectedAndReady check/set nickname and room name
    // and create room with these; also max players is set 2 2
    // If room is created Photon joins it automatically
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

    private void SetUpConnection()
    {
        PhotonNetwork.GameVersion = gameVersion;
        gameObject.GetComponent<Button>().interactable = false;
        print("Connecting to server...");
        PhotonNetwork.ConnectUsingSettings();
    }

    public void RefreshListOfRooms()
    {
        refreshListButton.GetComponent<Button>().interactable = false;
        ClearVisibleList();
        RestartConnection();
    }

    public void ClearVisibleList()
    {
        for(int i = 0; i < roomList.transform.childCount; i++)
        {
            Destroy(roomList.transform.GetChild(i).gameObject);
        }
    }

    public void RestartConnection()
    {
        restartConnectionFlag = true;
        PhotonNetwork.Disconnect();
    }

    private void ShowRooms(List<RoomInfo> _list)
    {
        foreach (RoomInfo _room in _list)
        {
            GameObject _roomPrefab = (GameObject)Instantiate(this.roomPrefab, roomList.transform);
            _roomPrefab.SetActive(false);
            _roomPrefab.transform.Find("RoomName").GetComponent<TMP_Text>().text = _room.Name;
            _roomPrefab.SetActive(true);
            displayedRoomsCache.Add(_roomPrefab);
            openRoomsFromMasterCache.Add(_room);
        }
    }

    public override void OnConnectedToMaster()
    {
        showConnection.GetComponent<TMP_Text>().text = "Connected to master";
        gameObject.GetComponent<Button>().interactable = true;
        refreshListButton.GetComponent<Button>().interactable = true;
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
    
    private void HideUnavailableRooms()
    {
        for (int k = 0; k < Mathf.Abs(displayedRoomsCache.Count - openRoomsFromMaster.Count); k++)
        {
            foreach (GameObject _room in displayedRoomsCache)
            {
                if (!openRoomsFromMaster.Contains(
                    new Room(_room.transform.Find("RoomName").GetComponent<TMP_Text>().text,
                    new RoomOptions())))
                {
                    displayedRoomsCache.Remove(_room);
                    Destroy(_room);
                    print("Removed and destroyed: " + _room);
                    break;
                }
            }
        }
        //displayedRoomsCache.Clear();
    }

    public override void OnRoomListUpdate(List<RoomInfo> _roomList)
    {
        print("Got update List:");
        openRoomsFromMaster.Clear();
        foreach (RoomInfo _room in _roomList)
        {
            print("Got room: " + _room.Name);
            if (_room.IsVisible
               && _room.IsOpen
               && _room.PlayerCount != 0)
            {
                print("Added room: " + _room.Name);
                openRoomsFromMaster.Add(_room);
            }
        }

        HideUnavailableRooms();

        // Here we have 3 options: 
        //  Less than one - nothing
        //  One - special case
        //  More than one - general case
        // We update list only when rooms.count >= 1
        if (openRoomsFromMaster.Count < 1)
        {
            showRoomsFound.GetComponent<TMP_Text>().text = "No new rooms found!";
        }
        else if (openRoomsFromMaster.Count >= 1)
        {
            if (openRoomsFromMaster.Count == 1)
            {
                showRoomsFound.GetComponent<TMP_Text>().text = "Found one new room.";
            }
            else
            {
                showRoomsFound.GetComponent<TMP_Text>().text = "Found " + openRoomsFromMaster.Count.ToString() + " new rooms.";
            }

            ShowRooms(openRoomsFromMaster);
        }
        base.OnRoomListUpdate(_roomList);
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
        if (cause == DisconnectCause.DisconnectByClientLogic && restartConnectionFlag)
        {
            restartConnectionFlag = false;
            SetUpConnection();
        }
        base.OnDisconnected(cause);
    }

}
