using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Despawn : MonoBehaviour
{
    public GameObject mainGame;

    private void Start()
    {
        // Singleplayer and multiplayer maingameloops are tagged this
        // Just a fallback, it should be set in editor
        if(mainGame == null)
        {
            mainGame = GameObject.FindWithTag("SingleTagForMainGameLoop");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (mainGame.GetComponent<MainGameLoop>() != null) // Had to add this check for main menu enemies
        {
            // Singleplayer
            mainGame.GetComponent<MainGameLoop>().TakePlayerDamage(collision.gameObject.GetComponent<Enemy>().GetDamage());
        Destroy(collision.gameObject);
        } else if (mainGame.GetComponent<MultiplayerMainGameLoop>() != null)
        {
            // Multiplayer
            //mainGame.GetComponent<MultiplayerMainGameLoop>().TakeDefenderDamage();
            print("Despawning: " + collision.gameObject.name);
            CrossSceneManager.instance.TakeDefenderDamageAndCheckIfDied(collision.gameObject.GetComponent<MultiplayerEnemy>().GetDamage());
            PhotonNetwork.Destroy(collision.gameObject);
        }
    }
}
