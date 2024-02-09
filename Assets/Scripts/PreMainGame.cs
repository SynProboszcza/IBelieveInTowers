using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PreMainGame : MonoBehaviourPunCallbacks, IPunObservable
{
    public Toggle readyToggle;
    [Tooltip("Content - GO from Canvas.ListEnemyFirstToDeploy.Viewport.Content")]
    public GameObject preListOfEnemies;
    [Tooltip("Enemies - GO that has 5 children shops")]
    public GameObject enemiesShopParent;
    public TMP_Text textfieldLobby;
    public TMP_Text textfieldRoom;
    public TMP_Text textfieldRegion;
    public TMP_Text textfieldMyNickName;
    public TMP_Text textfieldEnemyNickName;
    public TMP_Text textfieldEnemyReadyState;
    public TMP_Text textfieldTimerToClickReady;
    public Button leaveRoom;
    public Canvas mainCanvasReference;
    [HideInInspector]
    public bool amIMaster;
    [HideInInspector]
    public bool amIDefender;
    [HideInInspector]
    public bool readyState = false;
    [HideInInspector]
    private AsyncOperation asyncLoad;
    [SerializeField]
    private float mapLoadProgress = 0f;
    [SerializeField]
    private float _enemyLoadProgress = 0f;
    //private bool RPCToAllowChangeSceneSent = false;
    private bool isTimerRunning = false;
    private bool areBothReady = false;
    public float currentTime = 30.0f;
    [Tooltip("Essentially it is time for both clients to load a map. Only really set by Master, because it is synced across clients")]
    public float timeToShowThatBothAreReady = 2f;

    private void Awake()
    {
        if (GameObject.Find("SIMPLEConnect") != null && GameObject.Find("SIMPLEConnect").activeSelf)
        {
            print("Disabling PreMainGame for SIMPLEConnect to be enabled when connection is established");
            gameObject.SetActive(false);
        }

    }

    void Start()
    {
        print("Starting PreMainGame");
        CrossSceneManager.instance.SoftReset();
        // -----------------------------------------------------------------------
        // Set ready state to false and update basic flags (also in CSM)
        // -----------------------------------------------------------------------
        readyState = false;
        amIMaster = PhotonNetwork.IsMasterClient;
        amIDefender = (bool)PhotonNetwork.CurrentRoom.CustomProperties["isMasterDefending"] == amIMaster;
        // This shows amIMaster and amIDefender in nice way, just minified
        // this line can be commented out, it only prints to the console
        if (amIMaster) { if (amIDefender) { print("i am defender master");} else { print("i am attacker master");}} else { if (amIDefender) { print("i am defender joined");} else {print("i am attacker joined");}}
        CrossSceneManager.instance.amIMaster = amIMaster;
        CrossSceneManager.instance.amIDefender = amIDefender;
        //print("am i defender?:" + amIDefender);
        // -----------------------------------------------------------------------
        // Expose and update nicknames
        // -----------------------------------------------------------------------
        if (amIMaster)
        {
            //print("i am master");
            Hashtable _customProperties = new Hashtable();
            _customProperties.Add("roomCreatorNickname", PhotonNetwork.NickName);
            PhotonNetwork.CurrentRoom.SetCustomProperties(_customProperties);
        }
        else
        {
            //print("i am joined");
            Hashtable _customProperties = new Hashtable();
            _customProperties.Add("roomJoinedNickname", PhotonNetwork.NickName);
            PhotonNetwork.CurrentRoom.SetCustomProperties(_customProperties);
        }
        // -----------------------------------------------------------
        // Set gamemode flags and round time
        // -----------------------------------------------------------
        if ((bool)PhotonNetwork.CurrentRoom.CustomProperties["UnlimitedMoney"])
        {
            CrossSceneManager.instance.isMoneyInfinite = true;
        }
        if ((bool)PhotonNetwork.CurrentRoom.CustomProperties["UnlimitedMana"])
        {
            CrossSceneManager.instance.isManaInfinite = true;
        }
        if ((bool)PhotonNetwork.CurrentRoom.CustomProperties["InvincibleTurrets"])
        {
            CrossSceneManager.instance.invincibleTurrets = true;
        }
        if ((int)PhotonNetwork.CurrentRoom.CustomProperties["MatchTime"] != 0)
        {
            CrossSceneManager.instance.currentMatchMaxTime = (int)PhotonNetwork.CurrentRoom.CustomProperties["MatchTime"];
            //print("Set time from custom room props: " + CrossSceneManager.instance.currentMatchMaxTime);
        }
        // -----------------------------------------------------------------------
        // Activate proper parts of the scene defender/attacker
        // -----------------------------------------------------------------------
        // list is another gameobject than the "Content" alone
        GameObject.Find("Canvas").transform.Find("FirstEnemyListToDeploy").gameObject.SetActive(false); 
        enemiesShopParent.SetActive(false);
        if (!amIDefender) // if i am attacker
        {
            GameObject.Find("Canvas").transform.Find("FirstEnemyListToDeploy").gameObject.SetActive(true);
            enemiesShopParent.SetActive(true);
        }



        RefreshTextfields(PhotonNetwork.CurrentLobby.Type.ToString(), PhotonNetwork.CurrentRoom.Name.ToString(), PhotonNetwork.CloudRegion, PhotonNetwork.NickName, "Waiting for opponnent...");
    }

    void Update()
    {
        // -----------------------------------------------------------------------
        // Timer logic (copied from MultiplayerMainGameLoop)
        // -----------------------------------------------------------------------
        if (isTimerRunning)
        {
            if (currentTime > 0)
            {
                currentTime -= Time.deltaTime;
                if (amIDefender)
                {
                    RefreshTimer(textfieldTimerToClickReady, currentTime);
                }
                else
                {
                    RefreshTimer(textfieldTimerToClickReady, currentTime);
                }
            }
            else
            {
                // Time has passed!
                currentTime = -1f; // RefreshTimer adds 1, so the result is 0:00
                if (amIDefender)
                {
                    RefreshTimer(textfieldTimerToClickReady, currentTime);
                }
                else
                {
                    RefreshTimer(textfieldTimerToClickReady, currentTime);
                }
                if (areBothReady)
                {
                    if (amIMaster)
                    {
                        print("Sending RPC allow bc time has ended");
                        gameObject.GetComponent<PhotonView>().RPC("AllowToChangeScene", RpcTarget.All);
                    }
                    //if joined do nothing - expect RPC from master to allow to change scene
                }
                else
                {
                    GameNotStarted();
                }
                isTimerRunning = false;
            }
        }
    }

    public void GameNotStarted()
    {
        print("yea, not ready during 30seconds");
        textfieldTimerToClickReady.color = Color.red;
        textfieldTimerToClickReady.text = "You did not click ready, leaving room...";
        PhotonNetwork.LeaveRoom();
        // Change scene after 3 seconds
        StartCoroutine(ChangeSceneToThisAfterNSeconds(3, "HostGame"));
    }

    public void RefreshTimer(TMP_Text timer, float time)
    {
        time += 1; // this is so it does not show 0 for whole last second
        int seconds = Mathf.FloorToInt(time % 60);
        int minutes = Mathf.FloorToInt(time / 60);
        timer.text = string.Format("{0:0}:{1:00}", minutes, seconds);
    }

    public void RefreshTextfields(string _lobbyName, string _roomName, string _regionName, string _nickName, string _enemyNickName)
    {
        if (amIDefender)
        {
            _nickName += " - as defender";
        } else
        {
            _nickName += " - as attacker";
        }
        textfieldLobby.GetComponent<TMP_Text>().text = "Lobby: " + _lobbyName;
        textfieldRoom.GetComponent<TMP_Text>().text = "Room: " + _roomName;
        textfieldRegion.GetComponent<TMP_Text>().text = "Region: " + _regionName;
        textfieldMyNickName.GetComponent<TMP_Text>().text = "Me: " + _nickName;
        textfieldEnemyNickName.GetComponent<TMP_Text>().text = "Enemy: " + _enemyNickName;
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        // -----------------------------------------------------------------------
        // Debug section
        // -----------------------------------------------------------------------
        //print("some properties changed!");
        //print("amount:"+ propertiesThatChanged.Count + "props:" + propertiesThatChanged.ToString());



        // -----------------------------------------------------------------------
        // Nicknames / ready checking and showing
        // If master detects both ready, sends SetUpPlayArena(map)
        // -----------------------------------------------------------------------
        if (amIMaster)
        {
            // I am master and creator, so joined is my enemy

            if (propertiesThatChanged.ContainsKey("roomJoinedNickname"))
            {
                RefreshTextfields(PhotonNetwork.CurrentLobby.Type.ToString(), PhotonNetwork.CurrentRoom.Name.ToString(), PhotonNetwork.CloudRegion, PhotonNetwork.NickName, PhotonNetwork.CurrentRoom.CustomProperties["roomJoinedNickname"].ToString());
                BothJoined(PhotonNetwork.CurrentRoom.CustomProperties["roomJoinedNickname"].ToString());
            }
            // Updating enemy ready state
            // -------------------------------------------------------------
            if (propertiesThatChanged.ContainsKey("isJoinedReady"))
            {
                ShowEnemyReadyState((bool)propertiesThatChanged["isJoinedReady"]);
            }
            // Only Master checks if both are ready, then chooses a map
            // -------------------------------------------------------------
            if (readyState && (bool)PhotonNetwork.CurrentRoom.CustomProperties["isJoinedReady"])
            {
                /*
                // Map choosing consists of selecting random int from 1 to amount of maps
                // Then it is passed in RPC to joined
                // All maps should be named like Map1Multiplayer, Map2Multiplayer, Map3Multiplayer etc.
                // It is pre-ready to set the middle int to something more, SetUpPlayArena glues it together like this:
                // "Map" + _middleName + "Multiplayer";
                // so if map is named MapSuperMultiplayer you can pass "Super" and it'll work
                // -------------------------------------------------------------
                */
                if (CrossSceneManager.instance.mapMiddleNames.Count < 3)
                {
                    CrossSceneManager.instance.RandomizeMapSelection();
                }
                string _middleName = CrossSceneManager.instance.mapMiddleNames[CrossSceneManager.instance.didMasterWin.Count];
                print("Both ready: Sending RPC to change scene! Sent by master?: " + amIMaster);
                gameObject.GetComponent<PhotonView>().RPC("SetUpPlayArena", RpcTarget.All, _middleName);
                // -------------------------------------------------------------
                // Occasionally, OnPropertiesUpdate was called more than once
                // with the same data, and RPC was sent twice
                // which blocked map loading. 
                // This sets ready state back to false after sending RPC,
                // so its not called multiple times
                readyState = false; 
            }
        }
        else
        {
            // I am joining and not master, so creator is my enemy
            // We don't check if we're joined, because if we're not master we have to be
            // So PN.currRoom.CustProps["roomCreatorNickname"] is enemy nick
            RefreshTextfields(PhotonNetwork.CurrentLobby.Type.ToString(), PhotonNetwork.CurrentRoom.Name.ToString(), PhotonNetwork.CloudRegion, PhotonNetwork.NickName, PhotonNetwork.CurrentRoom.CustomProperties["roomCreatorNickname"].ToString());
            BothJoined(PhotonNetwork.CurrentRoom.CustomProperties["roomCreatorNickname"].ToString());
            // Updating enemy ready state
            // -------------------------------------------------------------
            if (propertiesThatChanged.ContainsKey("isMasterReady"))
            {
                ShowEnemyReadyState((bool)propertiesThatChanged["isMasterReady"]);
            }
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        // Disable everything clickable and leave room and change scene
        isTimerRunning = false;
        readyToggle.interactable = false;
        leaveRoom.interactable = false;
        readyToggle.isOn = false;
        textfieldTimerToClickReady.color = Color.magenta;
        textfieldEnemyReadyState.text = "Enemy left the room! Going back in a few seconds...";
        if (enemiesShopParent != null)
        {
            for (int i = 0; i < enemiesShopParent.transform.childCount; i++)
            {
                Transform child = enemiesShopParent.transform.GetChild(i).transform;
                child.GetComponent<Button>().interactable = false;
            }
        }
        PhotonNetwork.LeaveRoom();
        StartCoroutine(GoBackAfterEnemyPlayerLeaves(3));
        base.OnPlayerLeftRoom(otherPlayer);
    }

    private void BothJoined(string enemyNick)
    {
        isTimerRunning = true;
        textfieldTimerToClickReady.color = Color.blue;
        CrossSceneManager.instance.enemyNickname = enemyNick;
    }

    [PunRPC]
    public void SetUpPlayArena(string mapNameMiddlePart)
    {
        /* Called when both are ready
        // 
        // When joining room, timer is stuck at 0:30
        // When somebody joins, timer starts running down
        // When both are ready:
        //  set timer to 0:05(sync it); start scene loading; maybe display some text
        // Then at 0:00 allow to change scene
        // Master should control time flow
        */
        areBothReady = true;
        readyToggle.interactable = false;
        leaveRoom.interactable = false;
        textfieldEnemyReadyState.text = "Both players ready!";
        currentTime = timeToShowThatBothAreReady;
        textfieldTimerToClickReady.color = Color.green;
        // Disable EnemyShop being clickable
        if (enemiesShopParent != null)
        {
            for (int i = 0; i < enemiesShopParent.transform.childCount; i++)
            {
                Transform child = enemiesShopParent.transform.GetChild(i).transform;
                if (child.TryGetComponent<Button>(out _))
                {
                    child.GetComponent<Button>().interactable = false;
                } else
                {
                    Debug.LogError("child does not have button component!?", child);
                }
            }
        }
        // Transfer selected units from PreMainGame -> CSM -> MultiplayerMainGameLoop
        if (preListOfEnemies != null)
        {
            while (preListOfEnemies.transform.childCount > 0 && preListOfEnemies.transform.GetChild(0) != null)
            {
                Transform child = preListOfEnemies.transform.GetChild(0).transform;
                Transform parent = CrossSceneManager.instance.gameObject.transform.Find("EnemiesFromPreMainGame");
                child.localScale = new Vector3(0.5f, 0.5f, 0.5f); // List in multimaingame is scaled down by 0.5
                //print("scale: " + child.localScale);
                child.SetParent(parent);
            }
        }
        // Randomization is handled by OnRoomPropertiesUpdate() and passed only by Master client
        string _mapNameToLoadOnBothClients = "Map" + mapNameMiddlePart + "Multiplayer";
        StartCoroutine(LoadYourAsyncScene(_mapNameToLoadOnBothClients));
    }

    [PunRPC]
    public void AllowToChangeScene()
    {
        //print("local load progress:" + mapLoadProgress + "\nremote load progress:" + _enemyLoadProgress);
        if (asyncLoad != null)
        {
            asyncLoad.allowSceneActivation = true;
        } else
        {
            print("AsyncLoad is null whaat;;; i got permission to change scene but no scene is loading");
        }
    }

    private void ShowConnectedDecorationAndChangeSceneAfterNSeconds(int seconds)
    {
        // Here show to players that we are both ready and going into
        // playing scene
        print("Going to different scene after " + seconds + " seconds!");
        //StartCoroutine(LoadYourAsyncScene());
        StartCoroutine(ChangeSceneAfterNSeconds(seconds));
    }

    System.Collections.IEnumerator GoBackAfterEnemyPlayerLeaves(int seconds)
    {
        yield return new WaitForSeconds(seconds);
        SceneManager.LoadScene("HostGame");
    }

    System.Collections.IEnumerator ChangeSceneToThisAfterNSeconds(int seconds, string sceneName)
    {
        yield return new WaitForSeconds(seconds);
        SceneManager.LoadScene(sceneName);
    }

    System.Collections.IEnumerator ChangeSceneAfterNSeconds(int seconds)
    {
        yield return new WaitForSeconds(seconds);
        gameObject.GetComponent<PhotonView>().RPC("AllowToChangeScene", RpcTarget.All);
    }

    System.Collections.IEnumerator LoadYourAsyncScene(string mapName)
    {
        print("Loading scene: " + mapName);
        this.asyncLoad = SceneManager.LoadSceneAsync(mapName);
        asyncLoad.allowSceneActivation = false;
        while (!asyncLoad.isDone)
        {
            this.mapLoadProgress = asyncLoad.progress;
            yield return null;
        }
        // Here map should be already loaded and not activated 


    }

    public void ChangeReadyState()
    {
        readyState = !readyState;
        Hashtable _readyStateHashtable = new Hashtable();

        if (amIMaster)
        {
            _readyStateHashtable.Add("isMasterReady", readyState);
        } else
        {
            _readyStateHashtable.Add("isJoinedReady", readyState);
        }

        PhotonNetwork.CurrentRoom.SetCustomProperties(_readyStateHashtable);
    }

    public void ShowEnemyReadyState(bool _isEnemyReady)
    {
        if (_isEnemyReady)
        {
            textfieldEnemyReadyState.text = "Enemy is ready!";
        } else
        {
            textfieldEnemyReadyState.text = "Enemy is not ready.";
        }
    }

    public void ShowConnectionTestingNetworking()
    {
        PhotonNetwork.Instantiate("test", new Vector3(700, 500, 0), Quaternion.identity, 0);
    }

    public void ShowTextToConsole()
    {
        print("i am working from" + gameObject.name);
    }

    // Streaming level load progress to one another so both can check if the second one is loaded
    // as to achieve "synchronization" during timers
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(mapLoadProgress);
            stream.SendNext(currentTime);
        } else
        {
            _enemyLoadProgress = (float)stream.ReceiveNext();
            currentTime = (float)stream.ReceiveNext();
            //print("ENEMY load progress: " + _enemyLoadProgress);
        }
        //print("local load progress:" + mapLoadProgress + "\nremote load progress:" + _enemyLoadProgress);

    }
}
