using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.SocialPlatforms.GameCenter;

// Publicly accesible Singleton
// Add fields that need to be kept inbetween scenes
public class CrossSceneManager : MonoBehaviour
{
    public static CrossSceneManager instance;
    [SerializeField]
    public string gameVersion
    {
        get
        {
            return gameVersion;
        }
        private set
        {
            this.gameVersion = Application.version;
        }
    }
    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    // Make sure there is only one instance
    private void Awake()
    {
        if(instance != null && instance != this)
        {
            Destroy(this);
        } else
        {
            instance = this;
        }
    }
}
