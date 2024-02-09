using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetDefaultResolution : MonoBehaviour
{
    public void SetDefaultResolutionOnClick()
    {
        Screen.SetResolution(1920, 1080, false);
    }
}
