using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MultiplayerMainGameLoop : MonoBehaviourPunCallbacks, IPunObservable
{
    // Public instance so we can enable the correct one
    // Every scene(map) has two parts for simplicity
    public GameObject attackerPart;
    public GameObject defenderPart;

    // Public instance so we can enable the correct one
    // Every scene(map) has two parts for simplicity
    public GameObject attackerMatchResults;
    public GameObject defenderMatchResults;

    public GameObject nodePrefab;
    [HideInInspector]
    public List<GameObject> nodes = new List<GameObject>();
    [HideInInspector]
    public GameObject shopNodesCollection;

    // playArenaCorners is an object that has 4 properly named child gameObjects
    // Their transform.position 's are corners of play arena
    public Transform playArenaCorners;
    public GameObject msgToPlayerCanvas;
    public Transform[] waypoints;
    public Transform[] obstacles;
    public float roundTimeSeconds = 180;
    public float currentTime;
    private bool isTimerRunning = false;
    [Header("Attacker parts")]
    public TMP_Text a_timer;
    public TMP_Text a_enemyHealthTextField;
    public TMP_Text a_playerMoneyTextField;
    public TMP_Text a_playerManaTextField;
    public int a_moneyPerSecond = 10;
    [Header("Defender parts")]
    public TMP_Text d_timer;
    public TMP_Text d_enemyHealthTextField;
    public TMP_Text d_playerMoneyTextField;
    public TMP_Text d_playerManaTextField;
    public Vector2 topLeft;
    public Vector2 topRight;   // Despite what VS says, they are used in
    public Vector2 bottomLeft; // GetPlayArenaCorners() 
    public Vector2 bottomRight;
    [HideInInspector]
    public int nodesInstantiated = 0;
    [HideInInspector]
    public int nodesDestroyed = 0;
    public int secondsToWaitAfterGameEnd = 3;
    //public int playerMoney = 0;
    //public int playerMana = 0;
    //public int defenderHealth;
    [Tooltip("Specifies an offset on a Z axis. 0 is default map, camera is -15, positive will be hidden. This is mainly for colliders to work properly")]
    public static float shopNodesZOffset = -2;
    public bool isDebugging = false;
    [HideInInspector]
    public bool isShopOpen = false;
    //private bool isAllowedInstantiating = true;
    [HideInInspector]
    public bool amIMaster;
    [HideInInspector]
    public bool amIDefender;
    [HideInInspector]
    private bool matchResultsShown = false; // Flag so it gets run only once
    public float defenderHealthToSync;
    private bool addingMoneySet = false;
    private Coroutine moneyPerSecond;

    // FPS limit and SIMPLEConnect
    void Awake()
    {
        // Doing this because difference in framerate between Editor and 
        // an application causes differences in projectile speed
        //
        // This should be copied inside MainMenuLoop and MainGameLoop
        // (i think(?)(and maybe every scene?))
        QualitySettings.vSyncCount = 0;  // VSync must be disabled
        Application.targetFrameRate = 60;
        if (GameObject.Find("SIMPLEConnect") != null && GameObject.Find("SIMPLEConnect").activeSelf)
        {
            print("Disabling MainGame for SIMPLEConnect to be enabled when connection is established");
            gameObject.SetActive(false);
        }
    }

    // Parts de- and activating, setting match dur.
    void Start()
    {
        // Check if defending, then activate proper part
        attackerPart.gameObject.SetActive(false);
        defenderPart.gameObject.SetActive(false);
        attackerMatchResults.gameObject.SetActive(false);
        attackerMatchResults.transform.Find("Win").gameObject.SetActive(false);
        attackerMatchResults.transform.Find("Loose").gameObject.SetActive(false);
        defenderMatchResults.gameObject.SetActive(false);
        defenderMatchResults.transform.Find("Win").gameObject.SetActive(false);
        defenderMatchResults.transform.Find("Loose").gameObject.SetActive(false);
        amIMaster = CrossSceneManager.instance.amIMaster;
        amIDefender = CrossSceneManager.instance.amIDefender;
        defenderHealthToSync = CrossSceneManager.instance.defenderHealth;
        GetPlayArenaCorners();
        //playerMoney = CrossSceneManager.instance.playerMoney;
        //playerMana = CrossSceneManager.instance.playerMana;
        if (amIDefender)
        {
            defenderPart.gameObject.SetActive(true);
            shopNodesCollection = new GameObject("ShopNodesCollection");
            GenerateAndConfigureNodeMesh(topLeft, bottomRight);
            // TODO: ShopNode needs to show spells
        }
        else
        {
            attackerPart.gameObject.SetActive(true);
            // defenderHealth = CrossSceneManager.instance.defenderHealth;
        }

        // =====================================================================================================================================================
        // Temporarily disabled - for testing
        // HACK: comment roundtimeseconds to use SIMPLEConnect
        // =====================================================================================================================================================
        //roundTimeSeconds = CrossSceneManager.instance.currentMatchMaxTime;
        currentTime = roundTimeSeconds;

        // start the timer, syncing is done by sending current time and compensating lag
        isTimerRunning = true;

    }

    void Update()
    {
        defenderHealthToSync = CrossSceneManager.instance.defenderHealth;
        UpdatePlayerStats();
        // Check if defending when ded
        //  checking is done when dealing damage
        if (CrossSceneManager.instance.hasDefenderDied && !matchResultsShown)
        {
            RoundEnd(amIDefender, true);
            matchResultsShown = true;
        }
        // -----------------------------------------------------------------------
        // Timer logic
        // -----------------------------------------------------------------------
        if (isTimerRunning)
        {
            if (currentTime > 0)
            {
                currentTime -= Time.deltaTime;
                if (amIDefender)
                {
                    DisplayTime(d_timer, currentTime);
                }
                else
                {
                    if (!addingMoneySet)
                    {
                        moneyPerSecond = StartCoroutine(AddMoneyPerSecond(a_moneyPerSecond));
                        addingMoneySet = true;
                    }
                    DisplayTime(a_timer, currentTime);
                }
            }
            else
            {
                // Time has passed!
                currentTime = -1f; // DisplayTime adds 1, so the result is 0:00
                if (amIDefender)
                {
                    DisplayTime(d_timer, currentTime);
                }
                else
                {
                    DisplayTime(a_timer, currentTime);
                }
                RoundEnd(amIDefender, false);
                isTimerRunning = false;
            }
        }

        //synchronize with host/client
        //maybe cheats?
        //wait for escape menu
        //maybe listen for quitting? or add button for it

    }

    private void DisplayTime(TMP_Text timer, float time)
    {
        time += 1; // this is so it does not show 0 for whole last second
        int seconds = Mathf.FloorToInt(time % 60);
        int minutes = Mathf.FloorToInt(time / 60);
        timer.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void RoundEnd(bool amIDefending, bool didDefenderDie)
    {
        isTimerRunning = false;
        ExitGames.Client.Photon.Hashtable roundEndCleaning = new ExitGames.Client.Photon.Hashtable();
        if (moneyPerSecond != null)
        {
            StopCoroutine(moneyPerSecond);
        }
        // TODO: Decide if load next map, or main menu on finish
        CrossSceneManager.instance.didDefenderWin.Add(!didDefenderDie);
        string sceneName = "HostGame";
        if (CrossSceneManager.instance.didDefenderWin.Count < 3)
        {
            if (CrossSceneManager.instance.didDefenderWin.Count == 2 && (CrossSceneManager.instance.didDefenderWin[0] == CrossSceneManager.instance.didDefenderWin[1]))
            {
                // somebody won by 2:0
                if (amIDefending)
                {
                    if (didDefenderDie)
                    {
                        // i defender died
                        GameObject.Find("CanvasLeaveAndFinish").transform.Find("Loose").gameObject.SetActive(true);
                        ChangeSceneAfterNSeconds(secondsToWaitAfterGameEnd, "MainMenu");
                    }
                    else
                    {
                        // i defender won by time
                        GameObject.Find("CanvasLeaveAndFinish").transform.Find("Win").gameObject.SetActive(true);
                        ChangeSceneAfterNSeconds(secondsToWaitAfterGameEnd, "MainMenu");
                    }
                } else
                {
                    if (didDefenderDie)
                    { 
                        // i attacker won by killing defender
                        GameObject.Find("CanvasLeaveAndFinish").transform.Find("Win").gameObject.SetActive(true);
                        ChangeSceneAfterNSeconds(secondsToWaitAfterGameEnd, "MainMenu");
                    } else
                    {
                        // i attacker lost by time
                        GameObject.Find("CanvasLeaveAndFinish").transform.Find("Loose").gameObject.SetActive(true);
                        ChangeSceneAfterNSeconds(secondsToWaitAfterGameEnd, "MainMenu");
                    }
                }
            }
            sceneName = "InBetweenScene";
        } else if (CrossSceneManager.instance.didDefenderWin.Count >= 3)
        {
            sceneName = "MainMenu";
        }
        roundEndCleaning.Add("isMasterReady", false);
        roundEndCleaning.Add("isJoinedReady", false);
        PhotonNetwork.CurrentRoom.SetCustomProperties(roundEndCleaning);
        if (amIDefending)
        {
            if (didDefenderDie)
            {
                // i defender died
                // We don't activate attacker/defender parts, we expect the proper one to be active
                defenderMatchResults.SetActive(true);
                defenderMatchResults.transform.Find("Win").gameObject.SetActive(false); // fallback
                defenderMatchResults.transform.Find("Loose").gameObject.SetActive(true);
                print("I lost by deadly death, going back to host game after " + secondsToWaitAfterGameEnd + " seconds");
                ChangeSceneAfterNSeconds(secondsToWaitAfterGameEnd, sceneName);
            }
            else
            {
                // i defender won by time
                // We don't activate attacker/defender parts, we expect the proper one to be active
                defenderMatchResults.SetActive(true);
                defenderMatchResults.transform.Find("Win").gameObject.SetActive(true); // fallback
                defenderMatchResults.transform.Find("Loose").gameObject.SetActive(false);
                print("I won by surviving, going back to host game after " + secondsToWaitAfterGameEnd + " seconds");
                ChangeSceneAfterNSeconds(secondsToWaitAfterGameEnd, sceneName);
            }
        }
        else
        {
            if (didDefenderDie)
            {
                // i attacker won by killing defender
                attackerMatchResults.SetActive(true);
                attackerMatchResults.transform.Find("Loose").gameObject.SetActive(false); // fallback
                attackerMatchResults.transform.Find("Win").gameObject.SetActive(true);
                print("I won by killing defender, going back to host game after " + secondsToWaitAfterGameEnd + " seconds");
                ChangeSceneAfterNSeconds(secondsToWaitAfterGameEnd, sceneName);
            }
            else
            {
                // i attacker lost by time passing
                attackerMatchResults.SetActive(true);
                attackerMatchResults.transform.Find("Loose").gameObject.SetActive(true); // fallback
                attackerMatchResults.transform.Find("Win").gameObject.SetActive(false);
                print("I lost by time, going back to host game after " + secondsToWaitAfterGameEnd + " seconds");
                ChangeSceneAfterNSeconds(secondsToWaitAfterGameEnd, sceneName);
            }
        }
        if (CrossSceneManager.instance.didDefenderWin.Count >= 3)
        {
            print("Game over, going back to main menu");
            ChangeSceneAfterNSeconds(5, "MainMenu");
        }
        CrossSceneManager.instance.ResetInBetweenRounds();
    }

    private void ChangeSceneAfterNSeconds(int seconds, string sceneName)
    {
        StartCoroutine(ChangeSceneDelayed(seconds, sceneName));
    }

    private IEnumerator AddMoneyPerSecond(int amount)
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            CrossSceneManager.instance.AddMoney(amount, topRight);
        }
    }

    private IEnumerator ChangeSceneDelayed(int seconds, string sceneName)
    {
        yield return new WaitForSeconds(seconds);
        //PhotonNetwork.LeaveRoom();
        PhotonNetwork.LoadLevel(sceneName);
    }

    private void UpdatePlayerStats()
    {
        // Both players have their own gold and mana, but only defender has health
        // We need to show defender health to both players, but attacker needs to see it as "Enemy health"
        if (amIDefender)
        {
            if (CrossSceneManager.instance.isMoneyInfinite)
            {
                string playerMoneyTextTemplate = "Gold: inf";
                d_playerMoneyTextField.text = playerMoneyTextTemplate;

            }
            else
            {
                string playerMoneyTextTemplate = "Gold: " + CrossSceneManager.instance.playerMoney;
                d_playerMoneyTextField.text = playerMoneyTextTemplate;
            }
            // ------------------------------------------------------------------------------------
            if (CrossSceneManager.instance.isManaInfinite)
            {
                string playerManaTextTemplate = "Mana: Inf";
                d_playerManaTextField.text = playerManaTextTemplate;
            }
            else
            {
                string playerManaTextTemplate = "Mana: " + CrossSceneManager.instance.playerMana;
                d_playerManaTextField.text = playerManaTextTemplate;
            }
            // ------------------------------------------------------------------------------------
            string enemyHealthTextTemplate = "Health: " + CrossSceneManager.instance.defenderHealth;
            d_enemyHealthTextField.text = enemyHealthTextTemplate;
        }
        else
        {
            if (CrossSceneManager.instance.isMoneyInfinite)
            {
                string playerMoneyTextTemplate = "Gold: inf";
                a_playerMoneyTextField.text = playerMoneyTextTemplate;

            }
            else
            {
                string playerMoneyTextTemplate = "Gold: " + CrossSceneManager.instance.playerMoney;
                a_playerMoneyTextField.text = playerMoneyTextTemplate;
            }
            if (CrossSceneManager.instance.isManaInfinite)
            {
                string playerManaTextTemplate = "Mana: Inf";
                a_playerManaTextField.text = playerManaTextTemplate;
            }
            else
            {
                string playerManaTextTemplate = "Mana: " + CrossSceneManager.instance.playerMana;
                a_playerManaTextField.text = playerManaTextTemplate;
            }
            string enemyHealthTextTemplate = "Enemy health: " + CrossSceneManager.instance.defenderHealth;
            a_enemyHealthTextField.text = enemyHealthTextTemplate;
        }
    }

    [PunRPC]
    public void AddResourcesShowAtMouse(bool forDefender, bool isMoney, int amount)
    {
        if (CrossSceneManager.instance.amIDefender == forDefender)
        {
            if (isMoney)
            {
                CrossSceneManager.instance.AddMoney(amount);
            }
            else
            {
                CrossSceneManager.instance.AddMana(amount);
            }
        }
        else
        {
            // Do nothing
        }
    }

    [PunRPC]
    public void AddResourcesShowAtSpecifiedPoint(bool forDefender, bool isMoney, int amount, Vector2 fromWhere)
    {
        if (CrossSceneManager.instance.amIDefender == forDefender)
        {
            if (isMoney)
            {
                CrossSceneManager.instance.AddMoney(amount, fromWhere);
            }
            else
            {
                CrossSceneManager.instance.AddMana(amount, fromWhere); //ShowManaChange is not implemented yet
            }
        }
        else
        {
            // Do nothing
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        isTimerRunning = false;
        msgToPlayerCanvas.gameObject.SetActive(true);
        msgToPlayerCanvas.transform.Find("EnemyLeft").gameObject.SetActive(true);
        PhotonNetwork.LeaveRoom();
        StartCoroutine(GoBackAfterEnemyPlayerLeaves(3));
        base.OnPlayerLeftRoom(otherPlayer);
    }

    System.Collections.IEnumerator GoBackAfterEnemyPlayerLeaves(int seconds)
    {
        yield return new WaitForSeconds(seconds);
        SceneManager.LoadScene("HostGame");
    }

    public void LeaveMatchButton()
    {
        isTimerRunning = false;
        msgToPlayerCanvas.gameObject.SetActive(true);
        msgToPlayerCanvas.transform.Find("EnemyLeft").gameObject.SetActive(true);
        msgToPlayerCanvas.transform.Find("EnemyLeft").gameObject.GetComponent<TMP_Text>().text = "You left";
        PhotonNetwork.LeaveRoom();
        StartCoroutine(GoBackButton(3));
    }

    System.Collections.IEnumerator GoBackButton(int seconds)
    {
        yield return new WaitForSeconds(seconds);
        SceneManager.LoadScene("HostGame");
    }

    // ----------------------------------------------------
    private void DestroyNode(GameObject node)
    {
        Destroy(node);
        nodesDestroyed++;
    }

    private void GenerateAndConfigureNodeMesh(Vector2 topleft, Vector2 bottomright)
    {
        nodesInstantiated = 0;
        if (nodePrefab != null)
        {
            Vector2 distance = (new Vector2(Mathf.Abs(topleft.x), Mathf.Abs(topleft.y))) - new Vector2(Mathf.Abs(bottomright.x), Mathf.Abs(bottomright.y));
            distance = new Vector2(Mathf.Abs(distance.x), Mathf.Abs(distance.y));
            for (float x = topleft.x; x <= distance.x + topleft.x; x++)
            {
                for (float y = bottomright.y; y <= bottomright.y + distance.y; y++)
                {
                    //it would be better to check if node is on
                    //forbidden tile, then skip that tile
                    GameObject singleNode = Instantiate(nodePrefab, new Vector3(x, y, shopNodesZOffset), Quaternion.identity);
                    singleNode.transform.SetParent(shopNodesCollection.transform);
                    //configure node right here and right now
                    //reduce number of nodes - we cant place them on:
                    //paths, obstacles, spawners, despawners,
                    //other turrets need to be configured to not let any more
                    //or they need to be upgradable
                    nodesInstantiated++;
                    nodes.Add(singleNode);
                }
            }


            // ---------------------------------------------------------------------------------------------
            // Remove nodes from obstacles
            // ---------------------------------------------------------------------------------------------
            //print("trying to remove nodes");
            //print(nodes);
            //print(obstacles[0].transform.position);
            foreach (GameObject node in nodes)
            {
                foreach (Transform obstacle in obstacles)
                {
                    //if(node.transform.position == obstacle.transform.position)
                    if (Mathf.Approximately(node.transform.position.x, obstacle.transform.position.x)
                        && Mathf.Approximately(node.transform.position.y, obstacle.transform.position.y))
                    {
                        DestroyNode(node);
                    }
                }
            }
        }
        else
        {
            Debug.Log("Node prefabs were not set");
        }
        Debug.Log("shopNodes instantiated: " + nodesInstantiated + ", destroyed: " + nodesDestroyed + ",  left: " + (nodesInstantiated - nodesDestroyed));
    }

    private void GetPlayArenaCorners()
    {
        foreach (Transform child in playArenaCorners)
        {
            if (child.name == "topLeft")
            {
                if (isDebugging) { Debug.Log("got topleft:" + child.transform.position); }
                topLeft = child.transform.position;
            }
            else if (child.name == "topRight")
            {
                if (isDebugging) { Debug.Log("got topright"); }
                topRight = child.transform.position;
            }
            else if (child.name == "bottomLeft")
            {
                if (isDebugging) { Debug.Log("got botleft"); }
                bottomLeft = child.transform.position;
            }
            else if (child.name == "bottomRight")
            {
                if (isDebugging) { Debug.Log("got botright" + child.transform.position); }
                bottomRight = child.transform.position;
            }
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(this.currentTime);
            stream.SendNext(defenderHealthToSync);
            //print("Sent time and health: " + defenderHealthToSync);
        }
        else
        {
            float _currentTime = (float)stream.ReceiveNext();
            // Basic lag compensation
            float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
            this.currentTime = _currentTime - lag;
            CrossSceneManager.instance.defenderHealth = Mathf.FloorToInt((float)stream.ReceiveNext());
            //print("Received time and health: " + defenderHealthToSync);
        }
    }

}
