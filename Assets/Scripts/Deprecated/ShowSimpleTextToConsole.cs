using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowSimpleTextToConsole : MonoBehaviour
{
    public void AAAAAAAShowTextToConsole()
    {
        print("i am working from " + gameObject.name);
    }

    private void OnMouseDown()
    {
        print("i am working from " + gameObject.name);
    }
}
