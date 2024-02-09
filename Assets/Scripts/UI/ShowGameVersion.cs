using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShowGameVersion : MonoBehaviour
{
    void Start()
    {
        gameObject.GetComponent<TMP_Text>().text += Application.version;
    }
}
