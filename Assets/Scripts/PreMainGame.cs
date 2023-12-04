using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PreMainGame : MonoBehaviourPunCallbacks
{
    public Toggle readyToggle;
    // public TMP_Text textfieldLobby;
    // public TMP_Text textfieldRoom;
    // public TMP_Text textfieldRegion;
    // public TMP_Text textfieldMyNickName;
    // public TMP_Text textfieldEnemyNickName;
    void Start()
    {
        
    }

    void Update()
    {
        // Check for readiness
        if(readyToggle.isOn)
        {
            readyToggle.transform.Find("Label").GetComponent<Text>().color = Color.green;
        } else
        {
            readyToggle.transform.Find("Label").GetComponent<Text>().color = Color.red;
        }
        //RefreshTextfields(true);
        //print(PhotonNetwork.IsConnectedAndReady);
    }

    // PhotonNetwork is not set(?) 
    // public void RefreshTextfields(bool qwe)
    // {
    //     if (qwe)
    //     {
    //         //textfieldLobby.GetComponent<TMP_Text>().text = "Lobby: " + PhotonNetwork.CurrentLobby.Name.ToString();
    //         //textfieldRoom.GetComponent<TMP_Text>().text = "Room: " + PhotonNetwork.CurrentRoom.Name.ToString();
    //         //textfieldRegion.GetComponent<TMP_Text>().text = "Region: " + PhotonNetwork.CloudRegion.ToString();
    //         //textfieldMyNickName.GetComponent<TMP_Text>().text = "Me: " + PhotonNetwork.CurrentLobby.Name.ToString(); // check for name
    //         //textfieldEnemyNickName.GetComponent<TMP_Text>().text = "Enemy: " + PhotonNetwork.CurrentLobby.Name.ToString(); // check for enemy name
    //         // -----------------------------------------------------------------------------
    //         textfieldLobby.GetComponent<TMP_Text>().text = "Lobby: " + PhotonNetwork.CurrentLobby.Name.ToString();
    //         textfieldRoom.GetComponent<TMP_Text>().text = "Room: asdasdasd";
    //         textfieldRegion.GetComponent<TMP_Text>().text = "Region: asdasdasd";
    //         textfieldMyNickName.GetComponent<TMP_Text>().text = "Me: asdasdasd";
    //         textfieldEnemyNickName.GetComponent<TMP_Text>().text = "Enemy: asdasdasd";
    //     } else
    //     {
    //         string _text = "Not connected";
    //         textfieldLobby.GetComponent<TMP_Text>().text = "Lobby: " + _text;
    //         textfieldRoom.GetComponent<TMP_Text>().text = "Room: " + _text;
    //         textfieldRegion.GetComponent<TMP_Text>().text = "Region: " + _text;
    //         textfieldMyNickName.GetComponent<TMP_Text>().text = "Me: " + _text; 
    //         textfieldEnemyNickName.GetComponent<TMP_Text>().text = "Enemy: " + _text;
    //     }
    // }
}
