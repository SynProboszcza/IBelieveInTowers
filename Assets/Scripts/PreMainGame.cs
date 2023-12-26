using ExitGames.Client.Photon;
using Photon.Pun;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PreMainGame : MonoBehaviourPunCallbacks
{
    public Toggle readyToggle;
    public TMP_Text textfieldLobby;
    public TMP_Text textfieldRoom;
    public TMP_Text textfieldRegion;
    public TMP_Text textfieldMyNickName;
    public TMP_Text textfieldEnemyNickName;
    public TMP_Text textfieldEnemyReadyState;
    public Canvas mainCanvasReference;
    [HideInInspector]
    public bool amIMaster;
    [HideInInspector]
    public bool amIDefender;
    [HideInInspector]
    public bool readyState = false;

    void Start()
    {
        readyState = false;
        amIMaster = PhotonNetwork.IsMasterClient;
        amIDefender = (bool)PhotonNetwork.CurrentRoom.CustomProperties["isMasterDefending"] == amIMaster;
        // This shows amimaster and amidefender in nice way, just minified
        // this line can be commented out to optimize
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


        RefreshTextfields(PhotonNetwork.CurrentLobby.Type.ToString(), PhotonNetwork.CurrentRoom.Name.ToString(), PhotonNetwork.CloudRegion, PhotonNetwork.NickName, "Waiting for opponnent...");
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



        // Nicknames checking
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

            if (readyState && (bool)PhotonNetwork.CurrentRoom.CustomProperties["isJoinedReady"])
            {
                textfieldEnemyReadyState.text = "BOTH ARE READY";
                // SetUpPlayArena();
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

    [PunRPC]
    public void SetUpPlayArena()
    {
        // Wait 5 seconds, send info that both are ready
        // Change scene using photonnetwork
        readyToggle.interactable = false;
        textfieldEnemyReadyState.text = "BOTH ARE READY";
        ShowConnectedDecorationAndChangeSceneAfterNSeconds(5);

    }

    private void ShowConnectedDecorationAndChangeSceneAfterNSeconds(int seconds)
    {
        // Here show to players that we are both ready and going into
        // playing scene
        print("Going to different scene after " + seconds + " seconds!");
        StartCoroutine(ChangeSceneAfterNSeconds(seconds));
    }

    System.Collections.IEnumerator ChangeSceneAfterNSeconds(int seconds)
    {
        yield return new WaitForSeconds(seconds);
        // Change scene using photonnetwork to playing scene
        // Select random map
        PhotonNetwork.LoadLevel("Map1Multiplayer");

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
}
