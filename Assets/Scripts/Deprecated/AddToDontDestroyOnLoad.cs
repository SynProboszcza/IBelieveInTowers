using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddToDontDestroyOnLoad : MonoBehaviour
{
    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }
}
