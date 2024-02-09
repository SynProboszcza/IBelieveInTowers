using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HostGameButton : MonoBehaviour
{
    public void ShowHostGameScene()
    {
        SceneManager.LoadScene("HostGame");
    }
}
