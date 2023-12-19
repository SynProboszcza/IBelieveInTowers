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
    public Canvas mainCanvasReference;
    [HideInInspector]
    public bool amIMaster;

    private void Start()
    {
        amIMaster = PhotonNetwork.IsMasterClient;
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
        // as of rn properties will change every time someone joins a room
        // property changed will be players name being added
        print("some properties changed!");
        print(propertiesThatChanged);
        if (amIMaster)
        {
            // i am master and creator, so joined is my enemy
            RefreshTextfields(PhotonNetwork.CurrentLobby.Type.ToString(), PhotonNetwork.CurrentRoom.Name.ToString(), PhotonNetwork.CloudRegion, PhotonNetwork.NickName, PhotonNetwork.CurrentRoom.CustomProperties["roomJoinedNickname"].ToString());
        }
        else
        {
            // i am joining and not master, so created is my enemy
            RefreshTextfields(PhotonNetwork.CurrentLobby.Type.ToString(), PhotonNetwork.CurrentRoom.Name.ToString(), PhotonNetwork.CloudRegion, PhotonNetwork.NickName, PhotonNetwork.CurrentRoom.CustomProperties["roomCreatorNickname"].ToString());

        }
        base.OnRoomPropertiesUpdate(propertiesThatChanged);
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
