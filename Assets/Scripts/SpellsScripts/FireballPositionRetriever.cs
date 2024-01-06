using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireballPositionRetriever : MonoBehaviour
{ 
    public GameObject fireballObject;  
    Vector3 GetFireballPosition()
    {
        // Ensure the fireballObject reference is assigned
        if (fireballObject != null)
        {
            // Access the Transform component of the fireball GameObject
            Transform fireballTransform = fireballObject.transform;

            // Retrieve the position of the fireball from its Transform component
            Vector3 fireballPosition = fireballTransform.position;
            return fireballPosition;
        }
        else
        {
            Debug.LogError("Fireball GameObject reference is null!");
            return Vector3.zero; // Return a default position if the reference is null
        }
    }
}