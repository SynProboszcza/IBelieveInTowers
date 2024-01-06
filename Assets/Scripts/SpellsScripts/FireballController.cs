using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireballController : MonoBehaviour
{
    // Reference to the fireball GameObject (assign this in the Inspector)
    public GameObject fireballObject;

    void GetFireballPosition()
    {
        // Access the Transform component of the fireball GameObject
        Transform fireballTransform = fireballObject.GetComponent<Transform>();

        if (fireballTransform != null)
        {
            // Retrieve the position of the fireball from its Transform component
            Vector3 fireballPosition = fireballTransform.position;
            Debug.Log("Fireball position: " + fireballPosition);
        }
        else
        {
            Debug.LogError("Fireball Transform component is null!");
        }
    }
}