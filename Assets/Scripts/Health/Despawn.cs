using Photon.Pun;
using UnityEngine;

public class Despawn : MonoBehaviour
{
    public GameObject mainGame;

    private void Start()
    {
        // Singleplayer and multiplayer maingameloops are tagged this
        // Just a fallback, it should be set in editor
        if (mainGame == null)
        {
            mainGame = GameObject.FindWithTag("SingleTagForMainGameLoop");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (mainGame.GetComponent<MainGameLoop>() != null) // Had to add this check for main menu enemies
        {
            if (mainGame.TryGetComponent<MainMenuLoop>(out _))
            {
                // Main Menu
                Destroy(collision.gameObject);
            }
            else
            {
                // Singleplayer
                mainGame.GetComponent<MainGameLoop>().TakePlayerDamage(collision.gameObject.GetComponent<Enemy>().GetDamage());
                Destroy(collision.gameObject);
            }
        }
        else if (mainGame.GetComponent<MultiplayerMainGameLoop>() != null)
        {
            // Multiplayer
            if (CrossSceneManager.instance.amIMaster)
            {
                if (collision.TryGetComponent<MultiplayerEnemy>(out _))
                {
                    //print("Despawning: " + collision.gameObject.name);
                    int _dmg = collision.gameObject.GetComponent<MultiplayerEnemy>().GetDamage();
                    CrossSceneManager.instance.TakeDefenderDamageAndCheckIfDied(_dmg);
                    PhotonNetwork.Destroy(collision.gameObject);
                }
            }
        }
    }
}
