using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveMyselfWhenNoChildren : MonoBehaviour
{
    void Update()
    {
        if (transform.childCount == 0)
        {
            Destroy(gameObject);
        }
    }
}
