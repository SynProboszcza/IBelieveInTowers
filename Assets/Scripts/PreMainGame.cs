using ExitGames.Client.Photon;
using Photon.Pun;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PreMainGame : MonoBehaviourPunCallbacks, IPunObservable
{
    public Toggle readyToggle;
    public GameObject preListOfEnemies;
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
    private bool RPCToAllowChangeSceneSent = false;

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
        readyState = false;
        amIMaster = PhotonNetwork.IsMasterClient;
        amIDefender = (bool)PhotonNetwork.CurrentRoom.CustomProperties["isMasterDefending"] == amIMaster;
        // This shows amIMaster and amIDefender in nice way, just minified
        // this line can be commented out, it only prints to the console
        if (amIMaster) { if (amIDefender) { print("i am defender master");} else { print("i am attacker master");}} else { if (amIDefender) { print("i am defender joined");} else {print("i am attacker joined");}}
        // -----------------------------------------------------------
        CrossSceneManager.instance.amIMaster = amIMaster;
        CrossSceneManager.instance.amIDefender = amIDefender;
        //print("am i defender?:" + amIDefender);
        // Expose nicknames
        // -----------------------------------------------------------
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
        // Checking for special cases like unlimited money, mana and inv. turrets
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
        if (PhotonNetwork.CurrentRoom.CustomProperties["MatchTime"] != null)
        {
            CrossSceneManager.instance.currentMatchMaxTime = (int)PhotonNetwork.CurrentRoom.CustomProperties["MatchTime"];
        }

        RefreshTextfields(PhotonNetwork.CurrentLobby.Type.ToString(), PhotonNetwork.CurrentRoom.Name.ToString(), PhotonNetwork.CloudRegion, PhotonNetwork.NickName, "Waiting for opponnent...");
    }

    void Update()
    {       // floating point math smh
        // if ((mapLoadProgress >= 0.9f && _enemyLoadProgress >= 0.9f)
        //     && !RPCToAllowChangeSceneSent) 
        // {
        //     gameObject.GetComponent<PhotonView>().RPC("AllowToChangeScene", RpcTarget.All);
        //     RPCToAllowChangeSceneSent = true; // this flag is here so RPC is sent only once
        // }

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
        // Debug section
        // -------------------------------------------------------------
        //print("some properties changed!");
        print("amount:"+ propertiesThatChanged.Count + "props:" + propertiesThatChanged.ToString());



        // Nicknames checking / ready checking
        // -------------------------------------------------------------
        if (amIMaster)
        {
            // I am master and creator, so joined is my enemy

            if (propertiesThatChanged.ContainsKey("roomJoinedNickname"))
            {
                RefreshTextfields(PhotonNetwork.CurrentLobby.Type.ToString(), PhotonNetwork.CurrentRoom.Name.ToString(), PhotonNetwork.CloudRegion, PhotonNetwork.NickName, PhotonNetwork.CurrentRoom.CustomProperties["roomJoinedNickname"].ToString());
            }
            // Updating enemy ready state
            // -------------------------------------------------------------
            if (propertiesThatChanged.ContainsKey("isJoinedReady"))
            {
                ShowEnemyReadyState((bool)propertiesThatChanged["isJoinedReady"]);
            }
            // Only Master checks if both are ready
            // -------------------------------------------------------------
            if (readyState && (bool)PhotonNetwork.CurrentRoom.CustomProperties["isJoinedReady"])
            {
                //textfieldEnemyReadyState.text = "Both players ready!";
                print("Sending RPC to change scene!");
                gameObject.GetComponent<PhotonView>().RPC("SetUpPlayArena", RpcTarget.All);
            }
        }
        else
        {
            // I am joining and not master, so creator is my enemy
            // We don't check if we're joined, because if we're not master we have to be
            // So PN.currRoom.CustProps["roomCreatorNickname"] is enemy nick
            RefreshTextfields(PhotonNetwork.CurrentLobby.Type.ToString(), PhotonNetwork.CurrentRoom.Name.ToString(), PhotonNetwork.CloudRegion, PhotonNetwork.NickName, PhotonNetwork.CurrentRoom.CustomProperties["roomCreatorNickname"].ToString());
            // Updating enemy ready state
            // -------------------------------------------------------------
            if (propertiesThatChanged.ContainsKey("isMasterReady"))
            {
                ShowEnemyReadyState((bool)propertiesThatChanged["isMasterReady"]);
            }
        }
        base.OnRoomPropertiesUpdate(propertiesThatChanged);
    }

    // SetUpPlayArena() starts loading, cant change yet
    // AllowToChangeScene() allows it
    // 

    [PunRPC]
    public void SetUpPlayArena()
    {
        readyToggle.interactable = false;
        leaveRoom.interactable = false;
        textfieldEnemyReadyState.text = "Both players ready!";
        // Transfer selected units from PreMainGame -> CSM -> MultiplayerMainGameLoop
        if (preListOfEnemies != null)
        {
            while (preListOfEnemies.transform.childCount > 0 && preListOfEnemies.transform.GetChild(0) != null)
            {
                Transform child = preListOfEnemies.transform.GetChild(0).transform;
                Transform parent = CrossSceneManager.instance.gameObject.transform.Find("EnemiesFromPreMainGame");
                child.localScale = new Vector3(0.5f, 0.5f, 0.5f); // List in multimaingame is scaled down by 0.5
                child.SetParent(parent);
            }
        }
        StartCoroutine(LoadYourAsyncScene());
        // I might be wrong, but here map should be already loaded
        // 
        // When joining room, timer is stuck at 0:30
        // When somebody joins, timer starts running down
        // When both are ready:
        //  set timer to 0:05(sync it); start scene loading; maybe display some text
        // Then at 0:00 allow to change scene
        // Master should control time flow

        //ShowConnectedDecorationAndChangeSceneAfterNSeconds(5);

    }

    [PunRPC]
    public void AllowToChangeScene()
    {
        print("local load progress:" + mapLoadProgress + "\nremote load progress:" + _enemyLoadProgress);
        asyncLoad.allowSceneActivation = true;
    }

    private void ShowConnectedDecorationAndChangeSceneAfterNSeconds(int seconds)
    {
        // Here show to players that we are both ready and going into
        // playing scene
        print("Going to different scene after " + seconds + " seconds!");
        //StartCoroutine(LoadYourAsyncScene());
        StartCoroutine(ChangeSceneAfterNSeconds(seconds));
    }

    System.Collections.IEnumerator ChangeSceneAfterNSeconds(int seconds)
    {
        yield return new WaitForSeconds(seconds);
        gameObject.GetComponent<PhotonView>().RPC("AllowToChangeScene", RpcTarget.All);
    }

    System.Collections.IEnumerator LoadYourAsyncScene()
    {
        // The Application loads the Scene in the background as the current Scene runs.
        // This is particularly good for creating loading screens.
        // You could also load the Scene by using sceneBuildIndex. In this case Scene2 has
        // a sceneBuildIndex of 1 as shown in Build Settings.
        print("scene loading");
        this.asyncLoad = SceneManager.LoadSceneAsync("Map1Multiplayer");
        asyncLoad.allowSceneActivation = false;

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            //print("local load progress: " + this.asyncLoad.progress);
            this.mapLoadProgress = asyncLoad.progress;
            // if(this.asyncLoad.progress >= 0.9f)
            // {
            //     // map is ready
            //     //print("scene loaded");
            //     //gameObject.GetComponent<PhotonView>().RPC("AllowToChangeScene", RpcTarget.All);
            // }
            yield return null;
        }
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
            stream.SendNext(this.mapLoadProgress);
        } else
        {
            _enemyLoadProgress = (float)stream.ReceiveNext();
            //print("ENEMY load progress: " + _enemyLoadProgress);
        }
        //print("local load progress:" + mapLoadProgress + "\nremote load progress:" + _enemyLoadProgress);

    }
}
