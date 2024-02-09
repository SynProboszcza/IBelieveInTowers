using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletPool : MonoBehaviour
{
    public static BulletPool instance;

    public int amountToPoolEach = 50;
    public GameObject[] boltgunBullets;
    public GameObject[] smgBullets;
    public GameObject[] gatlingBullets;
    public GameObject[] rocketLauncherBullets;
    public GameObject[] shotgunBullets;
    public GameObject[] superShotgunBullets;
    private List<GameObject> pooledBoltgunBullets = new();
    private List<GameObject> pooledSmgBullets = new();
    private List<GameObject> pooledGatlingBullets = new();
    private List<GameObject> pooledRocketLauncherBullets = new();
    private List<GameObject> pooledShotgunBullets = new();
    private List<GameObject> pooledSuperShotgunBullets = new();

    // // Lists of lists of bullets of each level
    // // ============================================================================================
    // private List<List<GameObject>> boltgunBulletsAllLevels = new List<List<GameObject>>();
    // private List<List<GameObject>> smgBulletsAllLevels = new List<List<GameObject>>();
    // private List<List<GameObject>> gatlingBulletsAllLevels = new List<List<GameObject>>();
    // private List<List<GameObject>> rocketLauncherBulletsAllLevels = new List<List<GameObject>>();
    // private List<List<GameObject>> shotgunBulletsAllLevels = new List<List<GameObject>>();
    // private List<List<GameObject>> superShotgunBulletsAllLevels = new List<List<GameObject>>();
    // ============================================================================================
    private List<GameObject> boltgunBulletsLevelOne   = new();
    private List<GameObject> boltgunBulletsLevelTwo   = new();
    private List<GameObject> boltgunBulletsLevelThree = new();
    private List<GameObject> boltgunBulletsLevelFour  = new();
    // ============================================================================================
    private List<GameObject> smgBulletsLevelOne   = new();
    private List<GameObject> smgBulletsLevelTwo   = new();
    private List<GameObject> smgBulletsLevelThree = new();
    private List<GameObject> smgBulletsLevelFour  = new();
    // ============================================================================================
    private List<GameObject> gatlingBulletsLevelOne   = new();
    private List<GameObject> gatlingBulletsLevelTwo   = new();
    private List<GameObject> gatlingBulletsLevelThree = new();
    private List<GameObject> gatlingBulletsLevelFour  = new();
    // ============================================================================================
    private List<GameObject> rocketLauncherBulletsLevelOne   = new();
    private List<GameObject> rocketLauncherBulletsLevelTwo   = new();
    private List<GameObject> rocketLauncherBulletsLevelThree = new();
    private List<GameObject> rocketLauncherBulletsLevelFour  = new();
    // ============================================================================================
    private List<GameObject> shotgunBulletsLevelOne   = new();
    private List<GameObject> shotgunBulletsLevelTwo   = new();
    private List<GameObject> shotgunBulletsLevelThree = new();
    private List<GameObject> shotgunBulletsLevelFour  = new();
    // ============================================================================================
    private List<GameObject> superShotgunBulletsLevelOne   = new();
    private List<GameObject> superShotgunBulletsLevelTwo   = new();
    private List<GameObject> superShotgunBulletsLevelThree = new();
    private List<GameObject> superShotgunBulletsLevelFour  = new();


    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogError("found other bullet pool, destroying myself");
            Destroy(this);
        }
        else
        {
            Debug.LogWarning("not found other bulletpools, im good");
            instance = this;
        }
    }

    void Start()
    {
        /*
        // boltgunBulletsAllLevels.Add(boltgunBulletsLevelOne);
        // boltgunBulletsAllLevels.Add(boltgunBulletsLevelTwo);
        // boltgunBulletsAllLevels.Add(boltgunBulletsLevelThree);
        // boltgunBulletsAllLevels.Add(boltgunBulletsLevelFour);
        // // =======================================
        // smgBulletsAllLevels.Add(smgBulletsLevelOne);
        // smgBulletsAllLevels.Add(smgBulletsLevelTwo);
        // smgBulletsAllLevels.Add(smgBulletsLevelThree);
        // smgBulletsAllLevels.Add(smgBulletsLevelFour);
        // // =======================================
        // gatlingBulletsAllLevels.Add(gatlingBulletsLevelOne);
        // gatlingBulletsAllLevels.Add(gatlingBulletsLevelTwo);
        // gatlingBulletsAllLevels.Add(gatlingBulletsLevelThree);
        // gatlingBulletsAllLevels.Add(gatlingBulletsLevelFour);
        // // =======================================
        // rocketLauncherBulletsAllLevels.Add(rocketLauncherBulletsLevelOne);
        // rocketLauncherBulletsAllLevels.Add(rocketLauncherBulletsLevelTwo);
        // rocketLauncherBulletsAllLevels.Add(rocketLauncherBulletsLevelThree);
        // rocketLauncherBulletsAllLevels.Add(rocketLauncherBulletsLevelFour);
        // // =======================================
        // shotgunBulletsAllLevels.Add(shotgunBulletsLevelOne);
        // shotgunBulletsAllLevels.Add(shotgunBulletsLevelTwo);
        // shotgunBulletsAllLevels.Add(shotgunBulletsLevelThree);
        // shotgunBulletsAllLevels.Add(shotgunBulletsLevelFour);
        // // =======================================
        // superShotgunBulletsAllLevels.Add(superShotgunBulletsLevelOne);
        // superShotgunBulletsAllLevels.Add(superShotgunBulletsLevelTwo);
        // superShotgunBulletsAllLevels.Add(superShotgunBulletsLevelThree);
        // superShotgunBulletsAllLevels.Add(superShotgunBulletsLevelFour);
        // =======================================
        // TODO: make new bullets and a rocket
        // Make 5 different child objects for each bullet
        // Set those children as parents for each pool of bullets
        // So the hierarchy is /kind of/ usable with them
        // BulletPool
        //  GemBulletPool
        //  RocketBulletPool
        //  ArrowBulletPool
        //  ShotgunBulletPool
        //  BoltBulletPool
        //  NormalBulletPool
        // And each of these has 500 or sth bullets stored
        // Maybe less than 500, maybe 100..150 will be enough
        // Then make GetBulletFromPool take a type argument or string id
        // Or make 5 differents Getters, since every bullet derives from Bullet.cs
        // Potentially make 5 different types that derive from Bullet.cs

        // public GameObject[] boltgun Bullets;
        // public GameObject[] smg Bullets;
        // public GameObject[] gatling Bullets;
        // public GameObject[] rocketLauncher Bullets;
        // public GameObject[] shotgun Bullets;
        // public GameObject[] superShotgun Bullets;

        // Structure of the GO
        // Start() expects it as it is saved
        // Every first child has 4 childs 1..4
        // And every 1..4 has <amountToPoolEach> pooled bullets

        // MainParent
        // > boltgunBullets
        // > smgBullets
        // > gatlingBullets
        // > rocketLauncherBullets
        // > shotgunBullets
        // > superShotgunBullets
        //    > 1
        //    > 2
        //    > 3
        //    > 4
        */
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform bulletType = transform.GetChild(i);
            print("bulletType: " + bulletType + "how many: " + bulletType.childCount);
            for (int j = 0; j < bulletType.childCount; j++)
            {
                Transform bulletLevel = bulletType.GetChild(j);
                print("bulletType: " + bulletType + " bulletLevel: " + bulletLevel);
                for (int k = 0; k < amountToPoolEach; k++)
                {
                    /// *
                    /// i = bullet type
                    /// j = bullet level
                    /// k = amount to pool each
                    /// ///
                    /// pooledBoltgunBullets
                    /// pooledSmgBullets
                    /// pooledGatlingBullets 
                    /// pooledRocketLauncherBullets
                    /// pooledShotgunBullets
                    /// pooledSuperShotgunBullets
                    /// */
                    switch (bulletType.gameObject.name)
                    {
                        case "boltgunBullets":
                            GameObject bolt = Instantiate(boltgunBullets[j], bulletLevel);
                            bolt.name = bolt.name.Replace("(Clone)", "");
                            switch (j)
                            {
                                case 1:
                                    //
                                    boltgunBulletsLevelOne.Add(bolt);
                                    break;
                                case 2:
                                    //
                                    boltgunBulletsLevelTwo.Add(bolt);
                                    break;
                                case 3:
                                    //
                                    boltgunBulletsLevelThree.Add(bolt);
                                    break;
                                case 4:
                                    //
                                    boltgunBulletsLevelFour.Add(bolt);
                                    break;
                                case < 0:
                                    Debug.LogError("unknown level during inst.:" + j);
                                    break;
                                case > 0:
                                    Debug.LogError("unknown level during inst.:" + j);
                                    break;
                            }
                            bolt.SetActive(false);
                            break;
                        case "smgBullets":
                            GameObject ninemm = Instantiate(smgBullets[j], bulletLevel);
                            ninemm.name = ninemm.name.Replace("(Clone)", "");
                            switch (j)
                            {
                                case 1:
                                    //
                                    smgBulletsLevelOne.Add(ninemm);
                                    break;
                                case 2:
                                    //
                                    smgBulletsLevelTwo.Add(ninemm);
                                    break;
                                case 3:
                                    //
                                    smgBulletsLevelThree.Add(ninemm);
                                    break;
                                case 4:
                                    //
                                    smgBulletsLevelFour.Add(ninemm);
                                    break;
                                case < 0:
                                    Debug.LogError("unknown level during inst.:" + j);
                                    break;
                                case > 0:
                                    Debug.LogError("unknown level during inst.:" + j);
                                    break;
                            }
                            ninemm.SetActive(false);
                            break;
                        case "gatlingBullets":
                            GameObject fivefivesix = Instantiate(gatlingBullets[j], bulletLevel);
                            fivefivesix.name = fivefivesix.name.Replace("(Clone)", "");
                            switch (j)
                            {
                                case 1:
                                    //
                                    gatlingBulletsLevelOne.Add(fivefivesix);
                                    break;
                                case 2:
                                    //
                                    gatlingBulletsLevelTwo.Add(fivefivesix);
                                    break;
                                case 3:
                                    //
                                    gatlingBulletsLevelThree.Add(fivefivesix);
                                    break;
                                case 4:
                                    //
                                    gatlingBulletsLevelFour.Add(fivefivesix);
                                    break;
                                case < 0:
                                    Debug.LogError("unknown level during inst.:" + j);
                                    break;
                                case > 0:
                                    Debug.LogError("unknown level during inst.:" + j);
                                    break;
                            }
                            fivefivesix.SetActive(false);
                            break;
                        case "rocketLauncherBullets":
                            GameObject rocket = Instantiate(rocketLauncherBullets[j], bulletLevel);
                            rocket.name = rocket.name.Replace("(Clone)", "");
                            switch (j)
                            {
                                case 1:
                                    //
                                    rocketLauncherBulletsLevelOne.Add(rocket);
                                    break;
                                case 2:
                                    //
                                    rocketLauncherBulletsLevelTwo.Add(rocket);
                                    break;
                                case 3:
                                    //
                                    rocketLauncherBulletsLevelThree.Add(rocket);
                                    break;
                                case 4:
                                    //
                                    rocketLauncherBulletsLevelFour.Add(rocket);
                                    break;
                                case < 0:
                                    Debug.LogError("unknown level during inst.:" + j);
                                    break;
                                case > 0:
                                    Debug.LogError("unknown level during inst.:" + j);
                                    break;
                            }
                            rocket.SetActive(false);
                            break;
                        case "shotgunBullets":
                            GameObject pellet = Instantiate(shotgunBullets[j], bulletLevel);
                            pellet.name = pellet.name.Replace("(Clone)", "");
                            switch (j)
                            {
                                case 1:
                                    //
                                    shotgunBulletsLevelOne.Add(pellet);
                                    break;
                                case 2:
                                    //
                                    shotgunBulletsLevelTwo.Add(pellet);
                                    break;
                                case 3:
                                    //
                                    shotgunBulletsLevelThree.Add(pellet);
                                    break;
                                case 4:
                                    //
                                    shotgunBulletsLevelFour.Add(pellet);
                                    break;
                                case < 0:
                                    Debug.LogError("unknown level during inst.:" + j);
                                    break;
                                case > 0:
                                    Debug.LogError("unknown level during inst.:" + j);
                                    break;
                            }
                            pellet.SetActive(false);
                            break;
                        case "superShotgunBullets":
                            GameObject superPellet = Instantiate(superShotgunBullets[j], bulletLevel);
                            superPellet.name = superPellet.name.Replace("(Clone)", "");
                            switch (j)
                            {
                                case 1:
                                    //
                                    superShotgunBulletsLevelOne.Add(superPellet);
                                    break;
                                case 2:
                                    //
                                    superShotgunBulletsLevelTwo.Add(superPellet);
                                    break;
                                case 3:
                                    //
                                    superShotgunBulletsLevelThree.Add(superPellet);
                                    break;
                                case 4:
                                    //
                                    superShotgunBulletsLevelFour.Add(superPellet);
                                    break;
                                case < 0:
                                    Debug.LogError("unknown level during inst.:" + j);
                                    break;
                                case > 0:
                                    Debug.LogError("unknown level during inst.:" + j);
                                    break;
                            }
                            superPellet.SetActive(false);
                            break;
                        case null:
                            Debug.LogError("got something to pool that i dont have");
                            break;
                    }
                }
            }
        }
    }


    public GameObject GetBulletFromPool(string name = "smgBullets", int level = 3)
    {
        switch (name)
        {
            case "boltgunBullets":
                switch (level)
                {
                    case 1:
                        for (int i = 0; i < boltgunBulletsLevelOne.Count; i++)
                        {
                            if (!boltgunBulletsLevelOne[i].activeInHierarchy)
                            {
                                return boltgunBulletsLevelOne[i];
                            }
                        }
                        break;
                    case 2:
                        for (int i = 0; i < boltgunBulletsLevelTwo.Count; i++)
                        {
                            if (!boltgunBulletsLevelTwo[i].activeInHierarchy)
                            {
                                return boltgunBulletsLevelTwo[i];
                            }
                        }
                        break;
                    case 3:
                        for (int i = 0; i < boltgunBulletsLevelThree.Count; i++)
                        {
                            if (!boltgunBulletsLevelThree[i].activeInHierarchy)
                            {
                                return boltgunBulletsLevelThree[i];
                            }
                        }
                        break;
                    case 4:
                        for (int i = 0; i < boltgunBulletsLevelFour.Count; i++)
                        {
                            if (!boltgunBulletsLevelFour[i].activeInHierarchy)
                            {
                                return boltgunBulletsLevelFour[i];
                            }
                        }
                        break;
                    case < 0:
                        Debug.LogError("unsupported level: " + level);
                        return null;
                    case > 4:
                        Debug.LogError("unsupported level: " + level);
                        return null;
                }
                break;
            case "smgBullets":
                switch (level)
                {
                    case 1:
                        for (int i = 0; i < smgBulletsLevelOne.Count; i++)
                        {
                            if (!smgBulletsLevelOne[i].activeInHierarchy)
                            {
                                return smgBulletsLevelOne[i];
                            }
                        }
                        break;
                    case 2:
                        for (int i = 0; i < smgBulletsLevelTwo.Count; i++)
                        {
                            if (!smgBulletsLevelTwo[i].activeInHierarchy)
                            {
                                return smgBulletsLevelTwo[i];
                            }
                        }
                        break;
                    case 3:
                        for (int i = 0; i < smgBulletsLevelThree.Count; i++)
                        {
                            if (!smgBulletsLevelThree[i].activeInHierarchy)
                            {
                                return smgBulletsLevelThree[i];
                            }
                        }
                        break;
                    case 4:
                        for (int i = 0; i < smgBulletsLevelFour.Count; i++)
                        {
                            if (!smgBulletsLevelFour[i].activeInHierarchy)
                            {
                                return smgBulletsLevelFour[i];
                            }
                        }
                        break;
                    case < 0:
                        Debug.LogError("unsupported level: " + level);
                        return null;
                    case > 4:
                        Debug.LogError("unsupported level: " + level);
                        return null;
                }
                break;
            case "gatlingBullets":
                switch (level)
                {
                    case 1:
                        for (int i = 0; i < gatlingBulletsLevelOne.Count; i++)
                        {
                            if (!gatlingBulletsLevelOne[i].activeInHierarchy)
                            {
                                return gatlingBulletsLevelOne[i];
                            }
                        }
                        break;
                    case 2:
                        for (int i = 0; i < gatlingBulletsLevelTwo.Count; i++)
                        {
                            if (!gatlingBulletsLevelTwo[i].activeInHierarchy)
                            {
                                return gatlingBulletsLevelTwo[i];
                            }
                        }
                        break;
                    case 3:
                        for (int i = 0; i < gatlingBulletsLevelThree.Count; i++)
                        {
                            if (!gatlingBulletsLevelThree[i].activeInHierarchy)
                            {
                                return gatlingBulletsLevelThree[i];
                            }
                        }
                        break;
                    case 4:
                        for (int i = 0; i < gatlingBulletsLevelFour.Count; i++)
                        {
                            if (!gatlingBulletsLevelFour[i].activeInHierarchy)
                            {
                                return gatlingBulletsLevelFour[i];
                            }
                        }
                        break;
                    case < 0:
                        Debug.LogError("unsupported level: " + level);
                        return null;
                    case > 4:
                        Debug.LogError("unsupported level: " + level);
                        return null;
                }
                break;
            case "rocketLauncherBullets":
                switch (level)
                {
                    case 1:
                        for (int i = 0; i < rocketLauncherBulletsLevelOne.Count; i++)
                        {
                            if (!rocketLauncherBulletsLevelOne[i].activeInHierarchy)
                            {
                                return rocketLauncherBulletsLevelOne[i];
                            }
                        }
                        break;
                    case 2:
                        for (int i = 0; i < rocketLauncherBulletsLevelTwo.Count; i++)
                        {
                            if (!rocketLauncherBulletsLevelTwo[i].activeInHierarchy)
                            {
                                return rocketLauncherBulletsLevelTwo[i];
                            }
                        }
                        break;
                    case 3:
                        for (int i = 0; i < rocketLauncherBulletsLevelThree.Count; i++)
                        {
                            if (!rocketLauncherBulletsLevelThree[i].activeInHierarchy)
                            {
                                return rocketLauncherBulletsLevelThree[i];
                            }
                        }
                        break;
                    case 4:
                        for (int i = 0; i < rocketLauncherBulletsLevelFour.Count; i++)
                        {
                            if (!rocketLauncherBulletsLevelFour[i].activeInHierarchy)
                            {
                                return rocketLauncherBulletsLevelFour[i];
                            }
                        }
                        break;
                    case < 0:
                        Debug.LogError("unsupported level: " + level);
                        return null;
                    case > 4:
                        Debug.LogError("unsupported level: " + level);
                        return null;
                }
                break;
            case "shotgunBullets":
                switch (level)
                {
                    case 1:
                        for (int i = 0; i < shotgunBulletsLevelOne.Count; i++)
                        {
                            if (!shotgunBulletsLevelOne[i].activeInHierarchy)
                            {
                                return shotgunBulletsLevelOne[i];
                            }
                        }
                        break;
                    case 2:
                        for (int i = 0; i < shotgunBulletsLevelTwo.Count; i++)
                        {
                            if (!shotgunBulletsLevelTwo[i].activeInHierarchy)
                            {
                                return shotgunBulletsLevelTwo[i];
                            }
                        }
                        break;
                    case 3:
                        for (int i = 0; i < shotgunBulletsLevelThree.Count; i++)
                        {
                            if (!shotgunBulletsLevelThree[i].activeInHierarchy)
                            {
                                return shotgunBulletsLevelThree[i];
                            }
                        }
                        break;
                    case 4:
                        for (int i = 0; i < shotgunBulletsLevelFour.Count; i++)
                        {
                            if (!shotgunBulletsLevelFour[i].activeInHierarchy)
                            {
                                return shotgunBulletsLevelFour[i];
                            }
                        }
                        break;
                    case < 0:
                        Debug.LogError("unsupported level: " + level);
                        return null;
                    case > 4:
                        Debug.LogError("unsupported level: " + level);
                        return null;
                }
                break;
            case "superShotgunBullets":
                switch (level)
                {
                    case 1:
                        for (int i = 0; i < superShotgunBulletsLevelOne.Count; i++)
                        {
                            if (!superShotgunBulletsLevelOne[i].activeInHierarchy)
                            {
                                return superShotgunBulletsLevelOne[i];
                            }
                        }
                        break;
                    case 2:
                        for (int i = 0; i < superShotgunBulletsLevelTwo.Count; i++)
                        {
                            if (!superShotgunBulletsLevelTwo[i].activeInHierarchy)
                            {
                                return superShotgunBulletsLevelTwo[i];
                            }
                        }
                        break;
                    case 3:
                        for (int i = 0; i < superShotgunBulletsLevelThree.Count; i++)
                        {
                            if (!superShotgunBulletsLevelThree[i].activeInHierarchy)
                            {
                                return superShotgunBulletsLevelThree[i];
                            }
                        }
                        break;
                    case 4:
                        for (int i = 0; i < superShotgunBulletsLevelFour.Count; i++)
                        {
                            if (!superShotgunBulletsLevelFour[i].activeInHierarchy)
                            {
                                return superShotgunBulletsLevelFour[i];
                            }
                        }
                        break;
                    case < 0:
                        Debug.LogError("unsupported level: " + level);
                        return null;
                    case > 4:
                        Debug.LogError("unsupported level: " + level);
                        return null;
                }
                break;
            case null:
                Debug.LogError("got something to pool that i dont have");
                return null;
                //break;
        }
        /*
        for (int i = 0; i < pooledBoltgunBullets.Count; i++)
        {
            if (!pooledBoltgunBullets[i].activeInHierarchy)
            {
                return pooledBoltgunBullets[i];
            }
        }
        */
        // Base case when none are ready
        Debug.LogError("Tried to pull a bullet, but none was ready");
        return null;
    }

}
