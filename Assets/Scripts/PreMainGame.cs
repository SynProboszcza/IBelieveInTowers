using ExitGames.Client.Photon;
using Photon.Pun;
using TMPro;
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
    public bool readyState = false;

    void Start()
    {
        amIMaster = PhotonNetwork.IsMasterClient;
        CrossSceneManager.instance.amIMaster = amIMaster;
        readyState = false;

        // Expose nicknames
        // -----------------------------------------------------------
        if (amIMaster)
        {
            print("i am master");
            Hashtable _customProperties = new Hashtable();
            _customProperties.Add("roomCreatorNickname", PhotonNetwork.NickName);
            PhotonNetwork.CurrentRoom.SetCustomProperties(_customProperties);
        }
        else
        {
            print("i am joined");
            Hashtable _customProperties = new Hashtable();
            _customProperties.Add("roomJoinedNickname", PhotonNetwork.NickName);
            PhotonNetwork.CurrentRoom.SetCustomProperties(_customProperties);
        }


        RefreshTextfields(PhotonNetwork.CurrentLobby.Type.ToString(), PhotonNetwork.CurrentRoom.Name.ToString(), PhotonNetwork.CloudRegion, PhotonNetwork.NickName, "Waiting for opponnent...");
    }

    public void RefreshTextfields(string _lobbyName, string _roomName, string _regionName, string _nickName, string _enemyNickName)
    {
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
        //print("amount:"+ propertiesThatChanged.Count + "props:" + propertiesThatChanged.ToString());



        // Nicknames checking
        // -------------------------------------------------------------
        if (amIMaster)
        {
            // I am master and creator, so joined is my enemy

            if (propertiesThatChanged.ContainsKey("roomJoinedNickname"))
            {
                RefreshTextfields(PhotonNetwork.CurrentLobby.Type.ToString(), PhotonNetwork.CurrentRoom.Name.ToString(), PhotonNetwork.CloudRegion, PhotonNetwork.NickName, PhotonNetwork.CurrentRoom.CustomProperties["roomJoinedNickname"].ToString());
            }
            // ReadyToggle checking
            // -------------------------------------------------------------
            if (propertiesThatChanged.ContainsKey("isJoinedReady"))
            {
                ShowEnemyReadyState((bool)propertiesThatChanged["isJoinedReady"]);
            }


        }
        else
        {
            // I am joining and not master, so creator is my enemy
            // We don't check if we're joined, because if we're not master we have to be
            // So PN.currRoom.CustProps["roomCreatorNickname"] is enemy nick
            RefreshTextfields(PhotonNetwork.CurrentLobby.Type.ToString(), PhotonNetwork.CurrentRoom.Name.ToString(), PhotonNetwork.CloudRegion, PhotonNetwork.NickName, PhotonNetwork.CurrentRoom.CustomProperties["roomCreatorNickname"].ToString());

            // ReadyToggle checking
            // -------------------------------------------------------------
            if (propertiesThatChanged.ContainsKey("isMasterReady"))
            {
                ShowEnemyReadyState((bool)propertiesThatChanged["isMasterReady"]);
            }
        }

        base.OnRoomPropertiesUpdate(propertiesThatChanged);
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
