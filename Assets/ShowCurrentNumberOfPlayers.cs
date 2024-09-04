using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShowCurrentNumberOfPlayers : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(WaitAndTryAgain());
    }
    IEnumerator WaitAndTryAgain()
    {
        while (!PhotonNetwork.IsConnectedAndReady)
        {
            //Debug.Log("not yet connected, trying again...");
            yield return new WaitForSeconds(1);
        }
        gameObject.GetComponent<TMP_Text>().text = "Current players: " + PhotonNetwork.CountOfPlayers;
        Debug.Log("current players: " + PhotonNetwork.CountOfPlayers);
        while (PhotonNetwork.IsConnectedAndReady)
        {
            yield return new WaitForSeconds(1); // Event is sent every 5 seconds
            gameObject.GetComponent<TMP_Text>().text = "Current players: " + PhotonNetwork.CountOfPlayers;
            Debug.Log("current players update: " + PhotonNetwork.CountOfPlayers);
        }
    }
}
