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
    [Tooltip("GameObject to be set active when no rooms are available")]
    private GameObject showNoRoomsFound;
    private Dictionary<string, bool> _playerPreferences = new Dictionary<string, bool>();
    public List<RoomInfo> openRoomsFromMaster = new List<RoomInfo>();
    public List<RoomInfo> openRoomsFromMasterCache = new List<RoomInfo>();
    public List<GameObject> displayedRoomsCache = new List<GameObject>();
    public GameObject showConnection;
    public string backupNickNamePrefix = "defaultNickname";
    public string gameVersion = "0.1";
    private bool restartConnectionFlag = false;

    /// <summary>
    /// Starts with
    /// <see cref="CrossSceneManager.FullReset()">CSM full reset</see>,
    /// then checks if player set its nickname before, and populates the nickname field.
    /// Then connects with PUN if not already connected.
    /// </summary>
    void Start()
    {
        CrossSceneManager.instance.FullReset();

        // Check if player set its nickname before, and populate nickname field
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
            showConnection.GetComponent<ShowConnectionChange>().ShowConnected();

            // Checking if connected to a particular room
            if (PhotonNetwork.CurrentRoom != null)
            {
                gameObject.GetComponent<Button>().interactable = false;
                // Code theoretically should not get here, but in case 
                // TODO: Change scene to PrePlay or Play
            }
            else
            {
                RestartConnection(); // Doing this to get OnRoomListUpdate callback
            }
        }
        // We're not ready, so we need to set up and connect
        else
        {
            showConnection.GetComponent<ShowConnectionChange>().ShowConnectionError();
            SetUpConnection();
        }
    }

    /// <summary>
    /// Called when player clicks Create Game button. Tries to create a room using 
    /// user-filled data. Corrects what it deems to be entered wrongfully.
    /// Sets used nickname in PlayerPrefs. If successfull, we get a 
    /// <see cref="OnJoinedRoom()">OnJoinedRoom</see>
    /// callback.
    /// </summary>
    public void OnClickCreateRoom()
    {
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            Debug.Log("Not Connected, aborting creating room");
            //showConnection.GetComponent<TMP_Text>().text = "Can't connect or not ready yet!";
            showConnection.GetComponent<ShowConnectionChange>().ShowConnectionError();
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

    /// <summary>
    /// Tries to join room specified. Sets nickname if not set. Called from script inside room prefab.
    /// </summary>
    /// <param name="roomName"></param>
    public static void JoinRoomFromList(string roomName)
    {
        // i know i know
        GameObject.Find("HostGame").GetComponent<Button>().interactable = false;
        // Searching with tag because its static method, called by room prefab
        TMP_InputField _nickName = GameObject.FindWithTag("NickName").GetComponent<TMP_InputField>();
        if (_nickName.text.ToString().Length <= 3)
        {
            _nickName.text = "IDidntSetMyNickName";
        }
        PhotonNetwork.NickName = _nickName.text;
        PlayerPrefs.SetString("LocalNickName", _nickName.text.ToString());
        PhotonNetwork.JoinRoom(roomName);
        // its only static because it needs to bee seen in global context, from room prefab
    }

    /// <summary>
    /// Called by user. Clears shown rooms, cached rooms and restarts connection, to get a OnRoomListUpdate callback.
    /// </summary>
    public void RefreshListOfRooms()
    {
        refreshListButton.GetComponent<Button>().interactable = false;
        ClearVisibleAndCachedRoomList();
        RestartConnection();
    }

    /// <summary>
    /// Clears room list - visible and cached.
    /// </summary>
    public void ClearVisibleAndCachedRoomList()
    {
        for (int i = 0; i < roomList.transform.childCount; i++)
        {
            Destroy(roomList.transform.GetChild(i).gameObject);
        }
        displayedRoomsCache.Clear();
    }

    /// <summary>
    /// Sets the restart connection flag and disconnects from Photon.
    /// Flag is set as to reconnect only once, as Photon uses 
    /// <see cref="OnDisconnected(DisconnectCause)">OnDisconnected</see>
    /// callback. If this flag is set to true then it will try to connect again.
    /// </summary>
    public void RestartConnection()
    {
        restartConnectionFlag = true;
        PhotonNetwork.Disconnect();
    }

    /// <summary>
    /// Shows ready to join rooms and stores them in 
    /// <see cref="displayedRoomsCache">cache</see>. Disables showNoRoomsFound.
    /// TODO: idk what openroomsfrommastercache is supposed to do
    /// </summary>
    /// <param name="_list"></param>
    private void ShowRooms(List<RoomInfo> _list)
    {
        showNoRoomsFound.SetActive(false);
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
            _roomPrefab.transform.Find("MatchTime").transform.Find("Minutes").GetComponent<TMP_Text>().text = minutes.ToString();
            _roomPrefab.transform.Find("MatchTime").transform.Find("Seconds").GetComponent<TMP_Text>().text = string.Format("{0:00}", seconds);
            _roomPrefab.SetActive(true);
            // Keeping active and shown rooms in cache 
            displayedRoomsCache.Add(_roomPrefab);
            //openRoomsFromMasterCache.Add(_room);
        }
    }

    /// <summary>
    /// Tries to connect with Photon server. Callbacks OnConnectedToMaster.
    /// </summary>
    private void SetUpConnection()
    {
        PhotonNetwork.GameVersion = gameVersion;
        gameObject.GetComponent<Button>().interactable = false;
        //print("Connecting to server...");
        showConnection.GetComponent<ShowConnectionChange>().ShowConnecting();
        PhotonNetwork.ConnectUsingSettings();
    }

    /// <summary>
    /// Currently unused, introduced bugs. Instead of hiding we dont show 
    /// rooms that are not good to join.
    /// </summary>
    private void HideUnavailableRooms()
    {
        for (int k = 0; k < Mathf.Abs(displayedRoomsCache.Count - openRoomsFromMaster.Count); k++)
        {
            print(k);
            foreach (GameObject _room in displayedRoomsCache)
            {
                if (!openRoomsFromMaster.Contains(
                    new Room(_room.transform.Find("RoomName").GetComponent<TMP_Text>().text,
                    new RoomOptions())))
                {
                    print("Removed and destroyed a room named: " + _room.transform.Find("RoomName").GetComponent<TMP_Text>().text);
                    displayedRoomsCache.Remove(_room);
                    Destroy(_room);
                    break;
                }
            }
        }
        //displayedRoomsCache.Clear();
    }

    public override void OnConnectedToMaster()
    {
        showConnection.GetComponent<ShowConnectionChange>().ShowConnected();
        gameObject.GetComponent<Button>().interactable = true;
        refreshListButton.GetComponent<Button>().interactable = true;
        //CrossSceneManager.instance.ResetAfterPlaying();
        PhotonNetwork.JoinLobby(TypedLobby.Default);
        base.OnConnectedToMaster();
    }

    public override void OnCreatedRoom()
    {
        //showConnection.GetComponent<TMP_Text>().text = "Created room: " + _roomName.text;
        base.OnCreatedRoom();
    }

    public override void OnJoinedRoom()
    {
        gameObject.GetComponent<Button>().interactable = false;
        // Show big text "Joining room" or smth
        SceneManager.LoadScene("InBetweenScene");
    }

    /// <summary>
    /// Called for any updates from master server. 
    /// Checks if rooms we got from server are available and calls
    /// <see cref="ShowRooms(List{RoomInfo})">ShowRooms</see> to display them. Also handles removing
    /// unavailable rooms, like full rooms. 
    /// When no rooms are available, turnes on "no rooms available" message.
    /// </summary>
    /// <param name="_roomList">List of RoomInfo object from master server</param>
    public override void OnRoomListUpdate(List<RoomInfo> _roomList)
    {
        print("Got update List:");
        openRoomsFromMaster.Clear(); 
        foreach (RoomInfo _room in _roomList)
        {
            print("Processing room: " + _room.ToStringFull() + ":::");
            if (_room.IsVisible
               && _room.IsOpen
               && _room.PlayerCount != 0
               && _room.PlayerCount != _room.MaxPlayers)
            {
                print("Adding room: " + _room.Name);
                openRoomsFromMaster.Add(_room);
            } else
            {
                print("NOT adding: " + _room.ToStringFull() + ".\n");
                // _room determined to be defective
                // check if it is not listed already
                foreach(GameObject room in displayedRoomsCache)
                {
                    // Prefab has a child go named RoomName,
                    // TMP_Text.text is then processed, bc its full gameobject name, like <roomname>\nby: <nickname>
                    // and then compared to defective roomname from _room
                    // You cant put \n in roomnames, and if you find a way you break this by putting "\nby: " in the name
                    if (room.transform.Find("RoomName").GetComponent<TMP_Text>().text.Split("\nby: ")[0].Equals(_room.Name))
                    {
                        print("Destroying: " + _room.Name);
                        displayedRoomsCache.Remove(room);
                        Destroy(room);
                        break;
                    }
                }
            }
        } 
        // Check to see if there are still any rooms
        if(displayedRoomsCache.Count < 1)
        {
            showNoRoomsFound.SetActive(true);
        } else
        {
            showNoRoomsFound.SetActive(false);
        }

        if (openRoomsFromMaster.Count >= 1)
        {
            ShowRooms(openRoomsFromMaster);
        }
    }

    public override void OnJoinedLobby()
    {
        //print("Joined Lobby: " + PhotonNetwork.CurrentLobby.ToString());
        base.OnJoinedLobby();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        //showConnection.GetComponent<TMP_Text>().text = "Failed to create room!";
        showConnection.GetComponent<ShowConnectionChange>().ShowConnectionError();
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
        showConnection.GetComponent<ShowConnectionChange>().ShowConnectionError();
        base.OnDisconnected(cause);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        print("Failed joining room! mess: " + message);
        showConnection.GetComponent<ShowConnectionChange>().ShowConnectionError();
        base.OnJoinRoomFailed(returnCode, message);
    }

}
