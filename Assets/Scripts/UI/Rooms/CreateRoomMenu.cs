using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
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
    private Slider matchTimeSlider;
    [SerializeField]
    private Button refreshListButton;
    [SerializeField]
    private GameObject roomPrefab;
    [SerializeField]
    private GameObject roomList;
    [SerializeField]
    private TMP_Text showRoomsFound;
    private Dictionary<string, bool> _playerPreferences = new Dictionary<string, bool>();
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
        CrossSceneManager.instance.FullReset();
        if (PlayerPrefs.GetString("LocalNickName").Length >= 3)
        {
            _nickName.text = PlayerPrefs.GetString("LocalNickName");
            CrossSceneManager.instance.myNickName = _nickName.text;
        }
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
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            Debug.Log("Not Connected, aborting creating room");
            showConnection.GetComponent<TMP_Text>().text = "Can't connect or not ready yet!";
            return;
        }
        // -----------------------------------------------------------
        // Checking for room name, if not exists set up default
        // -----------------------------------------------------------
        if (_roomName.text == "")
        {
            _roomName.text = "DefaultRoomName";
        }
        // -----------------------------------------------------------
        // Checking for nickname, if not exists set up default
        // There is minimum length for nickname specified by Dawid,
        // not demanded by Photon
        // -----------------------------------------------------------
        if (_nickName.text.ToString().Length >= 3)
        {
            PhotonNetwork.NickName = _nickName.text.ToString();
        }
        else
        {
            backupNickNamePrefix += UnityEngine.Random.Range(0, 99999).ToString();
            _nickName.text = backupNickNamePrefix;
            PhotonNetwork.NickName = backupNickNamePrefix;
        }

        // -----------------------------------------------------------
        // Checking for user selected settings 
        // and store them to set up room with them
        // private _playerPreferences Dictionary<string, bool>
        // -----------------------------------------------------------
        foreach (Toggle toggle in settingToggles)
        {
            _playerPreferences.Add(toggle.gameObject.name, toggle.isOn);
        }
        // -----------------------------------------------------------
        // Show prefs to console
        // -----------------------------------------------------------
        string _playerPrefsToShowDebug = "Player preferences: to set\n";
        foreach (KeyValuePair<string, bool> kvp in _playerPreferences)
        {
            if (kvp.Key.Equals("DefendOrAttackIntention"))
            {
                _playerPrefsToShowDebug += string.Format("{0}:\t\t{1}\n", "DefOrAttInt", kvp.Value); // sugar
            }
            else if (kvp.Key.Equals("InvincibleTurrets"))
            {
                _playerPrefsToShowDebug += string.Format("{0}:\t\t{1}\n", "InvincTurrets", kvp.Value); // sugar
            }
            else
            {
                _playerPrefsToShowDebug += string.Format("{0}:\t\t{1}\n", kvp.Key, kvp.Value);
            }
        }
        // Add time in seconds to debugging info
        _playerPrefsToShowDebug += "MatchTime[s]:\t\t" + matchTimeSlider.GetComponent<UpdateMatchDuration>().secondsMatchShouldBe.ToString() + "\n";
        print(_playerPrefsToShowDebug);

        // -----------------------------------------------------------
        // Set default custom room properties:
        //  prepare keys to fill in nicknames
        //  set player time to live to 5 seconds
        // HACK: first hashtable;; custom room properties
        // not really a hack, its just a way to find it quicker inside VS
        // -----------------------------------------------------------
        ExitGames.Client.Photon.Hashtable _customProperties = new ExitGames.Client.Photon.Hashtable();
        _customProperties.Add("roomCreatorNickname", _nickName.text);
        _customProperties.Add("roomJoinedNickname", "Waiting ...");
        _customProperties.Add("isMasterDefending", _playerPreferences["DefendOrAttackIntention"]); // this should change every round
        _customProperties.Add("UnlimitedMoney", _playerPreferences["UnlimitedMoney"]);
        _customProperties.Add("UnlimitedMana", _playerPreferences["UnlimitedMana"]);
        _customProperties.Add("InvincibleTurrets", _playerPreferences["InvincibleTurrets"]);
        _customProperties.Add("SpecialRules", _playerPreferences["SpecialRules"]);
        _customProperties.Add("MatchTime", matchTimeSlider.GetComponent<UpdateMatchDuration>().secondsMatchShouldBe);
        _customProperties.Add("DidMasterWon", ""); // match history : "fft", "tft", "tt", "ff" etc.
        _customProperties.Add("isMasterReady", false);
        _customProperties.Add("isJoinedReady", false);
        RoomOptions options = new RoomOptions();
        options.MaxPlayers = 2;
        options.PlayerTtl = 5000;
        options.CustomRoomPropertiesForLobby = new string[] {
            "roomCreatorNickname",
            "isMasterDefending",
            "UnlimitedMoney",
            "UnlimitedMana",
            "InvincibleTurrets",
            "SpecialRules",
            "MatchTime"
        };
        options.CustomRoomProperties = _customProperties;
        // -----------------------------------------------------------
        PlayerPrefs.SetString("LocalNickName", _nickName.text.ToString());
        CrossSceneManager.instance.myNickName = _nickName.text;
        PhotonNetwork.CreateRoom(_roomName.text, options, TypedLobby.Default);
        _playerPreferences.Clear();
    }

    public static void JoinRoomFromList(string roomName)
    {
        // Searching with tag because its static method, called by room prefab
        TMP_InputField _nickName = GameObject.FindWithTag("NickName").GetComponent<TMP_InputField>();
        if (_nickName.text.ToString().Length <= 3)
        {
            _nickName.text = "IDidntSetMyNickName";
        }
        PhotonNetwork.NickName = _nickName.text;
        PlayerPrefs.SetString("LocalNickName", _nickName.text.ToString());
        PhotonNetwork.JoinRoom(roomName);
    }

    public void RefreshListOfRooms()
    {
        refreshListButton.GetComponent<Button>().interactable = false;
        ClearVisibleAndCachedRoomList();
        RestartConnection();
    }

    public void ClearVisibleAndCachedRoomList()
    {
        for (int i = 0; i < roomList.transform.childCount; i++)
        {
            Destroy(roomList.transform.GetChild(i).gameObject);
        }
        displayedRoomsCache.Clear();
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
            // We want to display ready room, so we disable it -> set it up -> enable it
            _roomPrefab.SetActive(false);
            _roomPrefab.transform.Find("RoomName").GetComponent<TMP_Text>().text = _room.Name + "\nby: " + _room.CustomProperties["roomCreatorNickname"];
            _roomPrefab.GetComponent<JoinRoomFromList>().rawRoomName = _room.Name;
            // Setting toggles to display custom room settings
            _roomPrefab.transform.Find("RoomProps").transform.Find("Defender").GetComponent<Toggle>().isOn = (bool)_room.CustomProperties["isMasterDefending"];
            _roomPrefab.transform.Find("RoomProps").transform.Find("UnlimitedMoney").GetComponent<Toggle>().isOn = (bool)_room.CustomProperties["UnlimitedMoney"];
            _roomPrefab.transform.Find("RoomProps").transform.Find("UnlimitedMana").GetComponent<Toggle>().isOn = (bool)_room.CustomProperties["UnlimitedMana"];
            _roomPrefab.transform.Find("RoomProps").transform.Find("InvincibleTurrets").GetComponent<Toggle>().isOn = (bool)_room.CustomProperties["InvincibleTurrets"];
            _roomPrefab.transform.Find("RoomProps").transform.Find("SpecialRules").GetComponent<Toggle>().isOn = (bool)_room.CustomProperties["SpecialRules"];
            int _time = (int)_room.CustomProperties["MatchTime"];
            int seconds = Mathf.FloorToInt(_time % 60);
            int minutes = Mathf.FloorToInt(_time / 60);
            _roomPrefab.transform.Find("RoomProps").transform.Find("MatchTime").transform.Find("Minutes").GetComponent<TMP_Text>().text = minutes.ToString();
            _roomPrefab.transform.Find("RoomProps").transform.Find("MatchTime").transform.Find("Seconds").GetComponent<TMP_Text>().text = string.Format("{0:00}", seconds);
            _roomPrefab.SetActive(true);
            // Keeping active and shown rooms in cache 
            displayedRoomsCache.Add(_roomPrefab);
            openRoomsFromMasterCache.Add(_room);
        }
    }

    private void SetUpConnection()
    {
        PhotonNetwork.GameVersion = gameVersion;
        gameObject.GetComponent<Button>().interactable = false;
        //print("Connecting to server...");
        PhotonNetwork.ConnectUsingSettings();
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

    public override void OnConnectedToMaster()
    {
        showConnection.GetComponent<TMP_Text>().text = "Connected to master";
        gameObject.GetComponent<Button>().interactable = true;
        refreshListButton.GetComponent<Button>().interactable = true;
        //CrossSceneManager.instance.ResetAfterPlaying();
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
        showConnection.GetComponent<TMP_Text>().text = "Joined room: " + PhotonNetwork.CurrentRoom.Name;
        gameObject.GetComponent<Button>().interactable = false;
        // Show big text "Found player" or smth
        // -----------------------------------------------------------
        // && IS NOT LOGICAL AND
        // && RETURNS FALSE WHEN BOTH ARGUMENTS ARE FALSE!!!!
        if (PhotonNetwork.IsMasterClient == (bool)PhotonNetwork.CurrentRoom.CustomProperties["isMasterDefending"])
        {
            SceneManager.LoadScene("InBetweenScene");
        }
        else
        {
            SceneManager.LoadScene("InBetweenScene");
        }
    }

    public override void OnRoomListUpdate(List<RoomInfo> _roomList)
    {
        //print("Got update List:");
        openRoomsFromMaster.Clear();
        foreach (RoomInfo _room in _roomList)
        {
            //print("Got room: " + _room.ToStringFull() + ":::");
            if (_room.IsVisible
               && _room.IsOpen
               && _room.PlayerCount != 0
               && _room.PlayerCount != _room.MaxPlayers)
            {
                //print("Added room: " + _room.Name);
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
        //print("Joined Lobby: " + PhotonNetwork.CurrentLobby.ToString());
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

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        print("Failed joining room! mess: " + message);
        base.OnJoinRoomFailed(returnCode, message);
    }

}
