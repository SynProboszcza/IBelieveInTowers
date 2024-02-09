using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ShowCredits : MonoBehaviour
{
    public void ShowCreditsScene()
    {
        SceneManager.LoadScene("Credits");
    }
}
