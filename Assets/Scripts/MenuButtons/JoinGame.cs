using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class JoinGame : MonoBehaviour
{
    public void ShowJoinGameScene()
    {
        SceneManager.LoadScene("JoinGame");
    }
}
