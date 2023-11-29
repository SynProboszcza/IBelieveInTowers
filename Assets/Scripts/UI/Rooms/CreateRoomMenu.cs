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
    private TMP_Text _roomName;
    // Temporary only, delete after debugging
    public GameObject showConnection;


    public void OnClickCreateRoom()
    {
        if (!PhotonNetwork.IsConnected)
        {
            Debug.Log("Not Connected, aborting creating room");

            // Temporary only, delete after debugging
            showConnection.GetComponent<RawImage>().color = Color.red;
            return;
        }
        RoomOptions options = new RoomOptions();
        options.MaxPlayers = 2;
        if (_roomName == null || _roomName.text == "")
        {
            _roomName.text = "DefaultRoomName";
        }
        PhotonNetwork.CreateRoom(_roomName.text, options, TypedLobby.Default);
    }

    public override void OnCreatedRoom()
    {
        // TODO: change scene to appropriate room
        Debug.Log("Created room succesfully", this);
        showConnection.GetComponent<RawImage>().color = Color.green;

        base.OnCreatedRoom();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);
    }
}
